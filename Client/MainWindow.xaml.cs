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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        private bool isMaximized = false;

        public MainWindow()
        {
            InitializeComponent();

            // Source for data grid

            informationCardsDataGrid.ItemsSource = cardsReadable;
        }

        #region User Interaction with Form


        // Form scrin methods

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Click on the form 2 times

            if (e.ClickCount == 2)
            {
                // Set normal scrin size
                if (isMaximized)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;

                    isMaximized = false;
                }
                // Set maximum scrin size
                else
                {
                    this.WindowState = WindowState.Maximized;

                    isMaximized = true;
                }
            }
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }


        // Left menu buttons

        private void Dashboard_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Hide all panels

            addingNewCard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Hidden;
            connection_section.Visibility = Visibility.Hidden;

            // Make deshboard panel visible

            dashboard_section.Visibility = Visibility.Visible;
        }
        private void AddNewCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Hide all panels

            dashboard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Hidden;
            connection_section.Visibility = Visibility.Hidden;

            // Make AddNewCard panel visible

            addingNewCard_section.Visibility = Visibility.Visible;
        }
        private void UpdateCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Add counters for items wich user has selected in datagrid

            int counterOfSelectedItems = 0;
            int selectedItemIndex = 0;

            // Count selected items

            foreach (InformationCardReadDto card in informationCardsDataGrid.ItemsSource)
            {
                if (card.IsSelected == true)
                {
                    counterOfSelectedItems++;
                    selectedItemIndex = card.Index;
                }
            }

            // If user didnt select or selected more then 1 item in datagrid -> unable to update

            if (counterOfSelectedItems > 1 || counterOfSelectedItems == 0)
            {
                MessageBox.Show("Please, select only one item!", "Instruction", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // If everything is correct -> get selected item data

            InformationCardReadDto existingCard = cardsReadable.FirstOrDefault(card => card.Index == selectedItemIndex);

            // Hide other panels

            dashboard_section.Visibility = Visibility.Hidden;
            addingNewCard_section.Visibility= Visibility.Hidden;
            connection_section.Visibility = Visibility.Hidden;

            // Make an update panel visible

            updatingExistingCard_section.Visibility = Visibility.Visible;

            // Show existing card data to the user via form components (index is a hidden field)

            existingCardIndex_TextBlock.Text = Convert.ToString(existingCard.Index);
            textboxExistingCardName.Text = existingCard.Name;
            existingInformationCardImage.Source = new ImageSourceConverter().ConvertFromString(cards[existingCard.Index - 1].Image) as ImageSource;

            // Unsort datagrid items (information cards)

            Unsort();
        }
        private async void DeleteCard_Btn_Click_1(object sender, RoutedEventArgs e)
        {
            // Create a variable to store all items wich need to be deleted

            var selectedCards = new List<InformationCardReadDto>();

            // Add all items wich user has selected in the datagrid

            foreach (InformationCardReadDto card in informationCardsDataGrid.ItemsSource)
                if (card.IsSelected == true)
                    selectedCards.Add(card);

            // If there is no items user has selected -> Do nothing

            if (selectedCards.Count == 0)
            {
                MessageBox.Show("Select at list 1 item to be deleted!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // If everything is alright -> getting ids and store then in the list

            List<int> ids = new List<int>();

            for (int i = selectedCards.Count - 1; i >= 0; i--)
                ids.Add(cards[selectedCards[i].Index - 1].Id);

            // Start method wich will delete all models with provided id on the server side

            await DeleteMultipleInformationCardsFromTheServer(ids);

        }
        private void Connection_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Hide all panels

            dashboard_section.Visibility = Visibility.Hidden;
            addingNewCard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Hidden;

            // Make connection panel visible

            connection_section.Visibility = Visibility.Visible;
        }
        private async void Logout_Btn_Click(object sender, RoutedEventArgs e)
        {
            //Delete all not used local images

            await DeleteAllNotUsedImages();

            //Exit the application

            Application.Current.Shutdown();
        }


        // Dashboard pannel buttons

        private void sortedByName_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Sort existing in data grid items

            SortByName();
        }
        private void unsorted_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Unort existing in data grid items

            Unsort();
        }
        private void selectAll_CheckBox_ClickChecked(object sender, RoutedEventArgs e)
        {
            // Click on checkbox when it's not selected -> select all items in the datagrid 

            foreach (InformationCardReadDto card in informationCardsDataGrid.ItemsSource)
                card.IsSelected = true;
        }
        private void selectAll_CheckBox_ClickUnchecked(object sender, RoutedEventArgs e)
        {
            // Click on checkbox when it's not selected -> unselect all items in the datagrid

            foreach (InformationCardReadDto card in informationCardsDataGrid.ItemsSource)
                card.IsSelected = false;

        }

        // Datagrid buttons
        private void SelectCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected item data

            InformationCardReadDto card = (InformationCardReadDto)informationCardsDataGrid.SelectedItem;

            // Change it's isSelected property

            if (card.IsSelected == true)
                card.IsSelected = false;
            else
                card.IsSelected = true;
        }
        private async void DeleteCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected item data

            InformationCardReadDto? card = (InformationCardReadDto)informationCardsDataGrid.SelectedItem;

            // Getting item id

            int id = cards[card.Index - 1].Id;

            // Delete Information Card data on the server side

            await DeleteInformationCardFromTheServer(id);
        }
        private void EditCard_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected item data

            InformationCardReadDto existingCard = (InformationCardReadDto)informationCardsDataGrid.SelectedItem;

            // Hide dashboard panel and make edit panel visible

            dashboard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Visible;

            // Show existing card data to the user via form components (index is hidden)

            existingCardIndex_TextBlock.Text = Convert.ToString(existingCard.Index);
            textboxExistingCardName.Text = existingCard.Name;
            existingInformationCardImage.Source = new ImageSourceConverter().ConvertFromString(cards[existingCard.Index - 1].Image) as ImageSource;

            // Unsort all datagrid items

            Unsort();
        }


        // AddNewCard panel buttons

        private void insertImage_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Getting the image

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.bmp;*.jpg;*.png";
            openFileDialog.FilterIndex = 1;

            // If image was selected successully

            if (openFileDialog.ShowDialog() == true)
            {
                // Hide mock image

                mockImage.Visibility = Visibility.Hidden;

                // Change the source for the form item and show selected image to the user 

                informationCardImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }
        private async void SaveInformationCard_Btn_Click(object sender, RoutedEventArgs e)  // This method calls POST method
        {
            // If user didnt provide both name and image for the information card -> show the error and do nothing

            if (textboxCardName.Text == string.Empty || textboxCardName.Text.Length > 50 || informationCardImage.Source == null)
            {
                MessageBox.Show("Both Name and Image are required! Name must be not more then 50 symbols length!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // If everything is alright -> get selected image local path

            string source = informationCardImage.Source.ToString();
            string filePath = (informationCardImage.Source as BitmapImage).UriSource.LocalPath;

            // Generate new name for the selected image file

            string newFileName = $"{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}_" + System.IO.Path.GetFileName(source);

            // Generate new file path where our image will be stored 

            string destination = System.IO.Path.Combine(@"../../../Data/Images/", newFileName);

            // Copy image file to the client project directory

            File.Copy(filePath, destination, true);

            // Get back to the dashboard panel

            Dashboard_Btn_Click(sender, e);

            // Send POST request to the server

            await SendInformationCardToTheServer(new InformationCardCreateDto
            {
                Name = textboxCardName.Text,
                Image = destination
            });

            // Get back AddNewCard panel to the default state

            SetAddNewCardPageToTheDefaultState();
        }


        // UpdateExistingCard panel buttons

        private void insertNewImage_Btn_Click(object sender, RoutedEventArgs e)
        {
            // Getting the image

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.bmp;*.jpg;*.png";
            openFileDialog.FilterIndex = 1;

            // If image was selected successully -> change source of the current image wich is shown to the user

            if (openFileDialog.ShowDialog() == true)
                existingInformationCardImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
        }
        private async void updateInformationCard_Btn_Click(object sender, RoutedEventArgs e)  // This method calls PUT method
        {
            // If user didnt provide both name and image for the information card -> show the error and do nothing

            if (textboxExistingCardName.Text == string.Empty || textboxCardName.Text.Length > 50 || existingInformationCardImage.Source == null)
            {
                MessageBox.Show("Name field is required and must not be empty!  Name must be not more then 50 symbols length!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // If everything is alright -> getting the update card index from a hidden field

            int indexOfUpdatingCard = int.Parse(existingCardIndex_TextBlock.Text);

            // Getting image source

            string source = existingInformationCardImage.Source.ToString();
            string destination = string.Empty;

            // If new image was selected -> save it

            if (!source.Contains("../../../Data/Images"))
            {
                // Getting new image local path

                string filePath = (existingInformationCardImage.Source as BitmapImage).UriSource.LocalPath;

                // Generate new image file name 

                string newFileName = $"{DateTime.Now.Hour}_{DateTime.Now.Minute}_{DateTime.Now.Second}_" + System.IO.Path.GetFileName(source);

                // Path where new image must be stored

                destination = System.IO.Path.Combine(@"../../../Data/Images/", newFileName);

                // Save new image on the client side

                File.Copy(filePath, destination, true);
            }
            else
                destination = source;

            // Get back to the dashboar panel

            Dashboard_Btn_Click(sender, e);

            // Send PUT method to the server, where we provide id of the updating card and new data

            await UpdateInformationCardDataOnTheServer(cards[indexOfUpdatingCard-1].Id, new InformationCardUpdateDto
            {
                Name = textboxExistingCardName.Text,
                Image = destination
            });

            // Set EditCard panel to the default state (clear all the components)

            SetEditExistingCardPageToTheDefaultState();
        }


        // Connection panel buttons

        private async void Connect_Btn_Click(object sender, RoutedEventArgs e) // This method calls GET method
        {
            // Try to get information from the server. Sending GET request to the server

            await GetInformationCardsFromTheServer();
        }

        #endregion

        #region Control Methods

        private void RefreshIndexes()
        {
            // Give default indexes to the cardRead variables

            int index = 1;
            foreach (var card in cardsReadable)
            {
                card.Index = index;
                index++;
            }
        }
        private void RefreshInformationCardsCounter()
        {
            // Set dashboard counter value which shows current number of cards

            string result = cards.Count == 0 ? "No Any Cards Available" : $"{cards.Count} Cards Available";
            Counter_Textbox.Text = result;
        }

        private void SortByName()
        {
            // Create list of sorted cards

            var orderedList = cardsReadable.OrderBy(x => x.Name).ToList();

            // Refresh our main collection of cards with sorted items

            int counter = 0;
            foreach (var card in orderedList)
            {
                cardsReadable[counter] = card;
                counter++;
            }
        }
        private void Unsort()
        {
            // Create list of unsorted cards

            var unorderedList = cardsReadable.OrderBy(x => x.Index).ToList();

            // Refresh our main collection of cards with sorted items

            int counter = 0;
            foreach (var card in unorderedList)
            {
                cardsReadable[counter] = card;
                counter++;
            }
        }

        private void SetAddNewCardPageToTheDefaultState()
        {
            // Clear all panel components and set them to the default state

            textboxCardName.Text = string.Empty;
            mockImage.Visibility = Visibility.Visible;
            informationCardImage.Source = null;
        }
        private void SetEditExistingCardPageToTheDefaultState()
        {
            // Set EditCard panel counter value which shows current number of cards

            existingCardIndex_TextBlock.Text = string.Empty;
            textboxExistingCardName.Text = string.Empty;
            existingInformationCardImage.Source = null;
        }

        private void ActivateAppFunctionalityAfterSuccessConnectionWereSet()
        {
            // Enable left menu buttons after successful connection were set

            Dashboard_Btn.IsEnabled = true;
            AddNewCard_Btn.IsEnabled = true;
            UpdateCard_Btn.IsEnabled = true;
            DeleteCard_Btn.IsEnabled = true;

            // Disable connect button on the connection panel

            Connect_Btn.IsEnabled = false;
        }
        private void RedirectToTheConnectionPageWithAnException(HttpRequestException ex) // Exception happened with server
        {
            connectedToTheServer = false;

            // Disable all the left menu buttons wich allow user to work with data wich stores on the server side

            Dashboard_Btn.IsEnabled = false;
            AddNewCard_Btn.IsEnabled = false;
            UpdateCard_Btn.IsEnabled = false;
            DeleteCard_Btn.IsEnabled = false;

            // Enable connection button

            Connect_Btn.IsEnabled = true;

            // Set the connection status and show exception to the user

            connection_StatusCode.Foreground = Brushes.Red;
            connection_StatusCode.Text = "503";

            connection_StatusHeader.Foreground = Brushes.Red;
            connection_StatusHeader.Text = ex.Message;

            // Hide all other panels

            dashboard_section.Visibility = Visibility.Hidden;
            addingNewCard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Hidden;

            // Make connection panel visible

            connection_section.Visibility = Visibility.Visible;

            // Kill all the processes wich can cause data leak 

            System.Diagnostics.Process.GetCurrentProcess().Kill();

            // Show error message box to the user

            MessageBox.Show("Error occured when sending request to the server.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void RedirectToTheConnectionPage(string responseCode, string responseHeader) // Exception happened with processing server response
        {
            connectedToTheServer = false;

            // Disable all the left menu buttons wich allow user to work with data wich stores on the server side

            Dashboard_Btn.IsEnabled = false;
            AddNewCard_Btn.IsEnabled = false;
            UpdateCard_Btn.IsEnabled = false;
            DeleteCard_Btn.IsEnabled = false;

            // Enable connection button

            Connect_Btn.IsEnabled = true;

            // Set the connection status and show exception to the user

            connection_StatusCode.Foreground = Brushes.Red;
            connection_StatusCode.Text = responseCode;

            connection_StatusHeader.Foreground = Brushes.Red;
            connection_StatusHeader.Text = responseHeader;

            // Hide all other panels

            dashboard_section.Visibility = Visibility.Hidden;
            addingNewCard_section.Visibility = Visibility.Hidden;
            updatingExistingCard_section.Visibility = Visibility.Hidden;

            // Make connection panel visible

            connection_section.Visibility = Visibility.Visible;

            // Kill all the processes wich can cause data leak 

            System.Diagnostics.Process.GetCurrentProcess().Kill();

            // Show error message box to the user

            MessageBox.Show("Error occured when processing server response.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private Task DeleteAllNotUsedImages()
        {
            // If we didnt connected to the server and dont know anything about models wich server stores -> dont touch local images
            if (!connectedToTheServer)
                return Task.CompletedTask;

            // Otherwise -> make an array of all images paths with names 

            string dirPath = @"../../../Data/Images";
            string[] fileNames = Directory.GetFiles(dirPath);

            for (int i = 0; i < fileNames.Length; i++)
            {
                // If there is no such image in the card models -> try to delete this image

                if (cards.FirstOrDefault(card => card.Image == $"{dirPath}/{fileNames[i]}") == null)
                {
                    try
                    {
                        File.Delete($"{dirPath}/Client/{fileNames[i]}");
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            return Task.CompletedTask;
        }

        #endregion

        #region Connect To The Server Methods

        // POST method (Create)
        private async Task SendInformationCardToTheServer(InformationCardCreateDto cardCreateDto)
        {
            using (HttpClient client = new HttpClient())
            {
                // Create a POST method uri, in wich include our createDto model

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

                // After we got response

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    // Data was saved successfully, get data from the server 

                    await GetInformationCardsFromTheServer();
                }
                else
                {
                    // If we received error response

                    RedirectToTheConnectionPage(response.StatusCode.ToString(), response.Headers.ToString());
                }
            }

        }

        // GET method (Read)
        private async Task GetInformationCardsFromTheServer()
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage? response = null;
                try
                {
                    // Sending the request; waiting for the response

                    response = await client.GetAsync("http://localhost:63697/api/InformationCards");
                }
                catch (HttpRequestException ex)
                {
                    // If server is not running yet or wrong uri was set

                    // Changing response textboxes colors & texts which will show the error to the user & redirect

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

                    // Setting up counters

                    int counter = 1;
                    int j = 0;

                    // Get all local stored images paths

                    string dirPath = "../../../Data/Images/";

                    DirectoryInfo di = new DirectoryInfo($"{dirPath}");
                    FileInfo[] files = di.GetFiles();
                    List<string> images = new List<string>();
                    foreach (FileInfo file in files)
                    {
                        images.Add($"{dirPath}{file.Name}");
                    }

                    // Rewriting data from the response to the local storage

                    foreach (var item in cardsFromServer)
                    {
                        // If we dont have an image on the client side for one of the server response models -> move to the next one

                        if (!images.Contains(item.Image))
                            continue;

                        //Otherwise

                        // Adding data into normal card models collection
                        cards.Add(new InformationCard
                        {
                            Id = item.Id,
                            Name = item.Name,
                            Image = item.Image
                        });

                        // Generating brush image

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

                    // Calling methods wich will allow user to work with app

                    ActivateAppFunctionalityAfterSuccessConnectionWereSet();
                    informationCardsDataGrid.ItemsSource = cardsReadable;

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

            // Refresh data

            RefreshIndexes();
            RefreshInformationCardsCounter();
            informationCardsDataGrid.Items.Refresh();
        }

        // PUT method (Update)
        private async Task UpdateInformationCardDataOnTheServer(int id, InformationCardUpdateDto cardUpdateDto)
        {
            using (HttpClient client = new HttpClient())
            {
                // Create a PUT method uri, in wich include our updateDto model

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

                // After we got response

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Model was updated successfully, get data from the server 

                    await GetInformationCardsFromTheServer();
                }
                else
                {
                    // If we received error response

                    RedirectToTheConnectionPage(response.StatusCode.ToString(), response.Headers.ToString());
                }
            }
        }

        // DELETE single item method (Delete)
        private async Task DeleteInformationCardFromTheServer(int id)
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

                // After we got response

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Model was deleted successfully, get data from the server 

                    await GetInformationCardsFromTheServer();
                }
                else
                {
                    // If we received error response

                    RedirectToTheConnectionPage(response.StatusCode.ToString(), response.Headers.ToString());
                }
            }
        }

        // DELETE multiple items method (Delete)
        private async Task DeleteMultipleInformationCardsFromTheServer(List<int> ids)
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

                    // After we got response

                    response.EnsureSuccessStatusCode();

                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // If it was successful -> move to the next model

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

            // Models were deleted successfully, get data from the server 

            await GetInformationCardsFromTheServer();
        }

        #endregion
    }
}
