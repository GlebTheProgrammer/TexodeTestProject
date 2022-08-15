using Client.DTOs;
using Client.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Server.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<InformationCard> cards = new ObservableCollection<InformationCard>();
        ObservableCollection<InformationCardReadDto> cardsReadable = new ObservableCollection<InformationCardReadDto>();
        bool connectedToTheServer = false;
        public MainWindow()
        {
            InitializeComponent();

            //Create DataGrid Items Info

            informationCardsDataGrid.ItemsSource = cardsReadable;
        }

        private bool isMaximized = false;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2)
            {
                if(isMaximized)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;

                    isMaximized = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;

                    isMaximized = true;
                }
            }
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private async void Logout_Btn_Click(object sender, RoutedEventArgs e)
        {
            await DeleteAllNotUsedImages();
            Application.Current.Shutdown();
        }
        private void insertImage_Btn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.bmp;*.jpg;*.png";
            openFileDialog.FilterIndex = 1;
            if(openFileDialog.ShowDialog() == true)
            {
                mockImage.Visibility = Visibility.Hidden;
                informationCardImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }
        private void Dashboard_Btn_Click(object sender, RoutedEventArgs e)
        {
            addingNewCard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Hidden;
            connection_section.Visibility = Visibility.Hidden;

            dashboard_section.Visibility = Visibility.Visible;
        }
        private void AddNewCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            dashboard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Hidden;
            connection_section.Visibility = Visibility.Hidden;

            addingNewCard_section.Visibility = Visibility.Visible;
        }
        private async void SaveInformationCard_Btn_Click(object sender, RoutedEventArgs e)  // Method needs to send request to the server
        {
            if(textboxCardName.Text == string.Empty || informationCardImage.Source == null)
            {
                MessageBox.Show("Both Name and Image are required!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string source = informationCardImage.Source.ToString();

            string filePath = (informationCardImage.Source as BitmapImage).UriSource.LocalPath;

            string newFileName = $"{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}_" + System.IO.Path.GetFileName(source);

            string destination = System.IO.Path.Combine(@"../../../Data/Images/", newFileName);

            File.Copy(filePath, destination, true);

            Dashboard_Btn_Click(sender, e);

            await SendInformationCardToTheServer(new InformationCardCreateDto
            {
                Name = textboxCardName.Text,
                Image = destination
            });

            informationCardsDataGrid.Items.Refresh();
            RefreshInformationCardsCounter();
            Unsort();

            SetAddNewCardPageToTheDefaultState();

            return;
        }
        private void sortedByName_Btn_Click(object sender, RoutedEventArgs e)
        {
            SortByName();
        }
        private void unsorted_Btn_Click(object sender, RoutedEventArgs e)
        {
            Unsort();
        }
        private void selectAll_CheckBox_ClickChecked(object sender, RoutedEventArgs e)
        {
            foreach (InformationCardReadDto card in informationCardsDataGrid.ItemsSource)
                card.IsSelected = true;
        }
        private void selectAll_CheckBox_ClickUnchecked(object sender, RoutedEventArgs e)
        {
            foreach (InformationCardReadDto card in informationCardsDataGrid.ItemsSource)
                card.IsSelected = false;
                
        }
        private async void DeleteCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            InformationCardReadDto? card = (InformationCardReadDto)informationCardsDataGrid.SelectedItem;

            int id = cards[card.Index - 1].Id;
            string imagePath = cards[card.Index - 1].Image;

            await DeleteInformationCardFromTheServer(id, imagePath);
        }
        private async void DeleteCard_Btn_Click_1(object sender, RoutedEventArgs e)
        {
            var selectedCards = new List<InformationCardReadDto>();

            foreach (InformationCardReadDto card in informationCardsDataGrid.ItemsSource)
                if(card.IsSelected == true)
                    selectedCards.Add(card);

            if (selectedCards.Count == 0)
                return;

            List<int> ids = new List<int>();
            List<string> imagePaths = new List<string>();
            for (int i = selectedCards.Count - 1; i >= 0; i--)
            {
                ids.Add(cards[selectedCards[i].Index - 1].Id);
                imagePaths.Add(cards[selectedCards[i].Index - 1].Image);
            }

            await DeleteMultipleInformationCardsFromTheServer(ids);

        }
        private void SelectCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            InformationCardReadDto card = (InformationCardReadDto)informationCardsDataGrid.SelectedItem;

            if (card.IsSelected == true)
                card.IsSelected = false;
            else
                card.IsSelected = true;
        }
        private void EditCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            InformationCardReadDto existingCard = (InformationCardReadDto)informationCardsDataGrid.SelectedItem;

            dashboard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Visible;

            existingCardIndex_TextBlock.Text =  Convert.ToString(existingCard.Index);
            textboxExistingCardName.Text = existingCard.Name;
            existingInformationCardImage.Source = new ImageSourceConverter().ConvertFromString(cards[existingCard.Index - 1].Image) as ImageSource;

            Unsort();
        }
        private void insertNewImage_Btn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.bmp;*.jpg;*.png";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == true)
                existingInformationCardImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
        }
        private async void updateInformationCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (textboxExistingCardName.Text == string.Empty || existingInformationCardImage.Source == null)
            {
                MessageBox.Show("Name field is required and must not be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int indexOfUpdatingCard = int.Parse(existingCardIndex_TextBlock.Text);

            string source = existingInformationCardImage.Source.ToString();
            string filePath = (existingInformationCardImage.Source as BitmapImage).UriSource.LocalPath;
            string newFileName = $"{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}_" + System.IO.Path.GetFileName(source);

            string destination = System.IO.Path.Combine(@"../../../Data/Images/", newFileName);

            File.Copy(filePath, destination, true);

            Dashboard_Btn_Click(sender, e);

            await UpdateInformationCardDataOnTheServer(cards[indexOfUpdatingCard-1].Id, new InformationCardUpdateDto
            {
                Name = textboxExistingCardName.Text,
                Image = destination
            });

            informationCardsDataGrid.Items.Refresh();
            Unsort();

            SetEditExistingCardPageToTheDefaultState();

            return;
        }
        private void UpdateCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            int counterOfSelectedItems = 0;
            int selectedItemIndex = 0;

            foreach (InformationCardReadDto card in informationCardsDataGrid.ItemsSource)
            {
                if (card.IsSelected == true)
                {
                    counterOfSelectedItems++;
                    selectedItemIndex = card.Index;
                }
            }

            if (counterOfSelectedItems > 1 || counterOfSelectedItems == 0)
                return;

            InformationCardReadDto existingCard = cardsReadable.FirstOrDefault(card => card.Index == selectedItemIndex);

            dashboard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Visible;

            existingCardIndex_TextBlock.Text = Convert.ToString(existingCard.Index);
            textboxExistingCardName.Text = existingCard.Name;
            existingInformationCardImage.Source = new ImageSourceConverter().ConvertFromString(cards[existingCard.Index - 1].Image) as ImageSource;

            Unsort();


        }
        private void Connection_Btn_Click(object sender, RoutedEventArgs e)
        {
            dashboard_section.Visibility = Visibility.Hidden;
            addingNewCard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Hidden;

            connection_section.Visibility = Visibility.Visible;
        }
        private async void Connect_Btn_Click(object sender, RoutedEventArgs e)
        {
            await GetInformationCardsFromTheServer();
        }

        //#region Control Methods
        private void RefreshInformationCardsCounter()
        {
            string result = cards.Count == 0 ? "No Any Cards Available" : $"{cards.Count} Cards Available";
            Counter_Textbox.Text = result;

            return;
        }

        private byte[] ConvertImageToBytes(BitmapImage image)
        {
            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }

            return data;
        }

        private BitmapImage GenerateImageFromBytes(byte[] imageData)
        {
            if(imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();

            using (MemoryStream stream = new MemoryStream(imageData))
            {
                stream.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = stream;
                image.EndInit();
            }

            image.Freeze();

            return image;
        }

        private void SetAddNewCardPageToTheDefaultState()
        {
            textboxCardName.Text = string.Empty;
            mockImage.Visibility = Visibility.Visible;
            informationCardImage.Source = null;
        }

        private int GetMaxIndex()
        {
            return cardsReadable.Max(card => card.Index);
        }

        private void SortByName()
        {
            var orderedList = cardsReadable.OrderBy(x => x.Name).ToList();

            int counter = 0;
            foreach (var card in orderedList)
            {
                cardsReadable[counter] = card;
                counter++;
            }

            return;
        }

        private void Unsort()
        {
            var unorderedList = cardsReadable.OrderBy(x => x.Index).ToList();

            int counter = 0;
            foreach (var card in unorderedList)
            {
                cardsReadable[counter] = card;
                counter++;
            }

            return;
        }

        private void RefreshIndexes()
        {
            int index = 1;
            foreach (var card in cardsReadable)
            {
                card.Index = index;
                index++;
            }
        }

        private void SetEditExistingCardPageToTheDefaultState()
        {
            existingCardIndex_TextBlock.Text = string.Empty;
            textboxExistingCardName.Text = string.Empty;
            existingInformationCardImage.Source = null;
        }

        private void ActivateAppFunctionalityAfterSuccessConnectionWereSet()
        {
            Dashboard_Btn.IsEnabled = true;
            AddNewCard_Btn.IsEnabled = true;
            UpdateCard_Btn.IsEnabled = true;
            DeleteCard_Btn.IsEnabled = true;

            Connect_Btn.IsEnabled = false;

            RefreshInformationCardsCounter();
            informationCardsDataGrid.Items.Refresh();
        }

        private void RedirectToTheConnectionPageWithAnException(HttpRequestException ex)
        {
            Dashboard_Btn.IsEnabled = false;
            AddNewCard_Btn.IsEnabled = false;
            UpdateCard_Btn.IsEnabled = false;
            DeleteCard_Btn.IsEnabled = false;

            Connect_Btn.IsEnabled = true;

            connection_StatusCode.Foreground = Brushes.Red;
            connection_StatusCode.Text = "503";

            connection_StatusHeader.Foreground = Brushes.Red;
            connection_StatusHeader.Text = ex.Message;

            dashboard_section.Visibility = Visibility.Hidden;
            addingNewCard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Hidden;

            connection_section.Visibility = Visibility.Visible;
        }

        private void RedirectToTheConnectionPage(string responseCode, string responseHeader)
        {
            Dashboard_Btn.IsEnabled = false;
            AddNewCard_Btn.IsEnabled = false;
            UpdateCard_Btn.IsEnabled = false;
            DeleteCard_Btn.IsEnabled = false;

            Connect_Btn.IsEnabled = true;

            connection_StatusCode.Foreground = Brushes.Red;
            connection_StatusCode.Text = responseCode;

            connection_StatusHeader.Foreground = Brushes.Red;
            connection_StatusHeader.Text = responseHeader;

            dashboard_section.Visibility = Visibility.Hidden;
            addingNewCard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Hidden;

            connection_section.Visibility = Visibility.Visible;
        }

        private void DeleteImagesLocally(List<string> imagePaths, int NumberOfRetries, int DelayOnRetryIn_ms)
        {
            foreach (string imagePath in imagePaths)
            {
                for (int i = 1; i <= NumberOfRetries; ++i)
                {
                    try
                    {
                        // Do stuff with file
                        File.Delete(imagePath);
                        break; // When done we can break loop
                    }
                    catch (IOException) when (i <= NumberOfRetries)
                    {
                        // You may check error code to filter some exceptions, not every error
                        // can be recovered.
                        Thread.Sleep(DelayOnRetryIn_ms);
                    }
                }
            }
        }

        private Task DeleteAllNotUsedImages()
        {
            if (!connectedToTheServer)
                return Task.CompletedTask;

            string dirPath = @"../../../Data/Images";
            string[] fileNames = Directory.GetFiles(dirPath);

            for (int i = 0; i < fileNames.Length; i++)
            {
                if (cards.FirstOrDefault(card => card.Image == $"{dirPath}/{fileNames[i]}") == null)
                {
                    try
                    {
                        File.Delete($"{dirPath}/Client/{fileNames[i]}");
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                }
            }
            return Task.CompletedTask;
        }

        //#endregion


        #region Connect To The Server Methods

        private async Task GetInformationCardsFromTheServer()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage? response = null;
                try
                {
                    // Sending for the request; waiting for the response

                    //response = await client.GetAsync("http://localhost:63697/api/InformationCards/AsAString");
                    response = await client.GetAsync("http://localhost:63697/api/InformationCards");
                }
                catch (HttpRequestException ex)
                {
                    // If server is not running yet or wrong uri was set

                    // Changing response textboxes colors & texts which will show the error to the user

                    RedirectToTheConnectionPageWithAnException(ex);

                    return;
                }

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    // If we received successful response

                    // Refreshing cards data if it is exists locally

                    cards = new ObservableCollection<InformationCard>();
                    cardsReadable = new ObservableCollection<InformationCardReadDto>();

                    // Changing response textboxes colors and texts to the response attributes

                    connection_StatusCode.Foreground = Brushes.Green;
                    connection_StatusCode.Text = response.StatusCode.ToString();

                    connection_StatusHeader.Foreground = Brushes.Green;
                    connection_StatusHeader.Text = response.Headers.ToString();

                    // Getting data from the response as a string (Models with their fields as a string based on JSON)

                    string jsonData = await response.Content.ReadAsStringAsync();

                    // Saving data to the existing transfer models (Image as a string)

                    var cardsFromServer = JsonConvert.DeserializeObject<List<InformationCardTransferDto>>(jsonData);

                    // If there was no data in the response -> making an empty list as a response

                    if (cardsFromServer == null)
                        cardsFromServer = new List<InformationCardTransferDto>();

                    // Rewriting data from the response to the local storage

                    int counter = 1;

                    foreach (var item in cardsFromServer)
                    {
                        // Adding data into normal card models collection (Image as a byte array)
                        cards.Add(new InformationCard
                        {
                            Id = item.Id,
                            Name = item.Name,

                            // Converting string with bytes into bytes array

                            Image = item.Image
                        });

                        // Generating brush image from the byte array

                        ImageBrush imageBrush = new ImageBrush();
                        imageBrush.ImageSource = new ImageSourceConverter().ConvertFromString(cards.Last().Image) as ImageSource;

                        // Adding data into card read models collection (Image as a brush)

                        cardsReadable.Add(new InformationCardReadDto
                        {
                            Index = counter,
                            Name = item.Name,
                            Image = imageBrush
                        });

                        counter++;
                    }

                    // Calling method wich will allow user to work with app. Also doing some datagrid configuration stuff 

                    informationCardsDataGrid.ItemsSource = cardsReadable;
                    informationCardsDataGrid.Items.Refresh();


                    ActivateAppFunctionalityAfterSuccessConnectionWereSet();
                    connectedToTheServer = true;

                }
                else
                {
                    // If we received error response

                    // Changing response textboxes colors & texts which will show the error with it's specific code

                    connection_StatusCode.Foreground = Brushes.Red;
                    connection_StatusCode.Text = response.StatusCode.ToString();

                    connection_StatusHeader.Foreground = Brushes.Red;
                    connection_StatusHeader.Text = response.Headers.ToString();
                }
            }
            RefreshIndexes();
            RefreshInformationCardsCounter();
            informationCardsDataGrid.Items.Refresh();
        }

        private async Task SendInformationCardToTheServer(InformationCardCreateDto cardCreateDto)
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = $"{{\"{nameof(cardCreateDto.Image)}\": \"{cardCreateDto.Image}\",\"{nameof(cardCreateDto.Name)}\": \"{cardCreateDto.Name}\"}}";
                HttpContent content = new StringContent(uri, Encoding.UTF8, "application/json");


                HttpResponseMessage? response = null;
                try
                {
                    // Sending for the request; waiting for the response

                    response = await client.PostAsync("http://localhost:63697/api/InformationCards", content);
                }
                catch (HttpRequestException ex)
                {
                    // If server was down or wrong uri was set

                    RedirectToTheConnectionPageWithAnException(ex);

                    return;
                }

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    await GetInformationCardsFromTheServer();
                }
                else
                {
                    // If we received error response

                    RedirectToTheConnectionPage(response.StatusCode.ToString(), response.Headers.ToString());
                }
            }
        
        }

        private async Task DeleteInformationCardFromTheServer(int id, string imagePath)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage? response = null;
                try
                {
                    // Sending for the request; waiting for the response

                    response = await client.DeleteAsync($"http://localhost:63697/api/InformationCards/{id}");
                }
                catch (HttpRequestException ex)
                {
                    // If server was down or wrong uri was set

                    RedirectToTheConnectionPageWithAnException(ex);

                    return;
                }

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    await GetInformationCardsFromTheServer();
                    return;
                }
                else
                {
                    // If we received error response

                    RedirectToTheConnectionPage(response.StatusCode.ToString(), response.Headers.ToString());
                }
            }
        }

        private async Task  DeleteMultipleInformationCardsFromTheServer(List<int> ids)
        {
            foreach (var id in ids)
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage? response = null;
                    try
                    {
                        // Sending for the request; waiting for the response

                        response = await client.DeleteAsync($"http://localhost:63697/api/InformationCards/{id}");
                    }
                    catch (HttpRequestException ex)
                    {
                        // If server was down or wrong uri was set

                        RedirectToTheConnectionPageWithAnException(ex);

                        return;
                    }

                    response.EnsureSuccessStatusCode();

                    if (response.IsSuccessStatusCode)
                    {
                        continue;
                    }
                    else
                    {
                        // If we received error response

                        RedirectToTheConnectionPage(response.StatusCode.ToString(), response.Headers.ToString());

                        return;
                    }
                }
            }

            await GetInformationCardsFromTheServer();
           
        }

        private async Task UpdateInformationCardDataOnTheServer(int id,InformationCardUpdateDto cardUpdateDto)
        {
            using (HttpClient client = new HttpClient())
            {
                var uri = $"{{\"{nameof(cardUpdateDto.Image)}\": \"{cardUpdateDto.Image}\",\"{nameof(cardUpdateDto.Name)}\": \"{cardUpdateDto.Name}\"}}";
                HttpContent content = new StringContent(uri, Encoding.UTF8, "application/json");


                HttpResponseMessage? response = null;
                try
                {
                    // Sending for the request; waiting for the response

                    response = await client.PutAsync($"http://localhost:63697/api/InformationCards/{id}", content);
                }
                catch (HttpRequestException ex)
                {
                    // If server was down or wrong uri was set

                    RedirectToTheConnectionPageWithAnException(ex);

                    return;
                }

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    await GetInformationCardsFromTheServer();
                }
                else
                {
                    // If we received error response

                    RedirectToTheConnectionPage(response.StatusCode.ToString(), response.Headers.ToString());
                }
            }
        }


        #endregion
    }
}
