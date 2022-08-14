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
        public MainWindow()
        {
            InitializeComponent();

            //var converter = new BrushConverter();

            //cards.Add(new InformationCard { Id = 0, Name = "Cat", Image = new byte[] { 1, 2, 3 } });
            //cardsReadable.Add(new InformationCardReadDto { Index = 1, Name = "Cat", Image = (Brush)converter.ConvertFromString("#1098ad")});
            //cards.Add(new InformationCard { Id = 1, Name = "Cat", Image = new byte[] { 1, 2, 3 } });
            //cardsReadable.Add(new InformationCardReadDto { Index = 2, Name = "Abort", Image = (Brush)converter.ConvertFromString("#1098bc") });
            //cards.Add(new InformationCard { Id = 2, Name = "Cat", Image = new byte[] { 1, 2, 3 } });
            //cardsReadable.Add(new InformationCardReadDto { Index = 3, Name = "Baby", Image = (Brush)converter.ConvertFromString("#1098ef") });
            //cards.Add(new InformationCard { Id = 3, Name = "Cat", Image = new byte[] { 1, 2, 3 } });
            //cardsReadable.Add(new InformationCardReadDto { Index = 4, Name = "Eagle", Image = (Brush)converter.ConvertFromString("#1098ad") });
            //cards.Add(new InformationCard { Id = 4, Name = "Cat", Image = new byte[] { 1, 2, 3 } });
            //cardsReadable.Add(new InformationCardReadDto { Index = 5, Name = "Doctor", Image = (Brush)converter.ConvertFromString("#1098bc") });

            //Create DataGrid Items Info

            informationCardsDataGrid.ItemsSource = cardsReadable;

            //RefreshInformationCardsCounter();
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
        private void Logout_Btn_Click(object sender, RoutedEventArgs e)
        {
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
        private void SaveInformationCard_Btn_Click(object sender, RoutedEventArgs e)  // Method needs to send request to the server
        {
            SendInformationCardToTheServer(new InformationCardCreateDto
            {
                Name = textboxCardName.Text,
                //Image = Encoding.Default.GetString(ConvertImageToBytes((BitmapImage)informationCardImage.Source))
                Image = Convert.ToBase64String(ConvertImageToBytes((BitmapImage)informationCardImage.Source))
            });

            //cards.Add(new InformationCard
            //{
            //    Id = cards.LastOrDefault() == null ? 0 : cards.LastOrDefault().Id + 1,
            //    Name = textboxCardName.Text,
            //    Image = ConvertImageToBytes((BitmapImage)informationCardImage.Source)
            //});

            //ImageBrush imageBrush = new ImageBrush();
            //imageBrush.ImageSource = GenerateImageFromBytes(cards.Last().Image);

            //cardsReadable.Add(new InformationCardReadDto
            //{
            //    Index = cardsReadable.LastOrDefault() == null ? 1 : GetMaxIndex() + 1,
            //    Name = cards.LastOrDefault().Name,
            //    Image = imageBrush,
            //});

            RefreshInformationCardsCounter();
            informationCardsDataGrid.Items.Refresh();
            SetAddNewCardPageToTheDefaultState();
            Unsort();
            Dashboard_Btn_Click(sender, e);

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
        private void DeleteCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            InformationCardReadDto card = (InformationCardReadDto)informationCardsDataGrid.SelectedItem;

            cards.RemoveAt(card.Index-1);
            cardsReadable.Remove(card);

            RefreshIndexes();
            RefreshInformationCardsCounter();
            informationCardsDataGrid.Items.Refresh();
        }
        private void DeleteCard_Btn_Click_1(object sender, RoutedEventArgs e)
        {
            var selectedCards = new List<InformationCardReadDto>();

            foreach (InformationCardReadDto card in informationCardsDataGrid.ItemsSource)
                if(card.IsSelected == true)
                    selectedCards.Add(card);

            if (selectedCards.Count == 0)
                return;


            for (int i = selectedCards.Count - 1; i >= 0; i--)
            {
                cards.RemoveAt(selectedCards[i].Index-1);
                cardsReadable.Remove(selectedCards[i]);
            }

            RefreshIndexes();
            RefreshInformationCardsCounter();
            informationCardsDataGrid.Items.Refresh();
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
            existingInformationCardImage.Source = GenerateImageFromBytes(cards[existingCard.Index-1].Image);

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
        private void updateInformationCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            int indexOfUpdatingCard = int.Parse(existingCardIndex_TextBlock.Text);

            //Update main card model
            cards[indexOfUpdatingCard - 1].Name = textboxExistingCardName.Text;
            cards[indexOfUpdatingCard - 1].Image = ConvertImageToBytes((BitmapImage)existingInformationCardImage.Source);

            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = GenerateImageFromBytes(cards[indexOfUpdatingCard - 1].Image);

            //Update cardReadDTO model
            cardsReadable[indexOfUpdatingCard - 1].Name = textboxExistingCardName.Text;
            cardsReadable[indexOfUpdatingCard - 1].Image = imageBrush;

            SetEditExistingCardPageToTheDefaultState();
            informationCardsDataGrid.Items.Refresh();
            Dashboard_Btn_Click(sender, e);

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
            existingInformationCardImage.Source = GenerateImageFromBytes(cards[existingCard.Index - 1].Image);

            Unsort();


        }
        private void Connection_Btn_Click(object sender, RoutedEventArgs e)
        {
            dashboard_section.Visibility = Visibility.Hidden;
            addingNewCard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Hidden;

            connection_section.Visibility = Visibility.Visible;
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

        //#endregion


        #region Connect To The Server Methods

        private async void GetInformationCardsFromTheServer(HttpClient clientt)
        {
            using (HttpClient client = new HttpClient())
            {


                HttpResponseMessage? response = null;
                try
                {
                    // Sending for the request; waiting for the response

                    response = await client.GetAsync("http://localhost:63697/api/InformationCards/AsAString");
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

                            Image = Convert.FromBase64String(item.Image),
                        });

                        // Generating brush image from the byte array

                        ImageBrush imageBrush = new ImageBrush();
                        imageBrush.ImageSource = GenerateImageFromBytes(cards.Last().Image);

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
        }

        private void Connect_Btn_Click(object sender, RoutedEventArgs e)
        {
            using (HttpClient client = new HttpClient())
            {
                GetInformationCardsFromTheServer(client);
            }
        }

        private async void SendInformationCardToTheServer(InformationCardCreateDto cardCreateDto)
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
                    GetInformationCardsFromTheServer(client);
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
