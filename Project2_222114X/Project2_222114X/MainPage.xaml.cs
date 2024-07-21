using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Data;
using System.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.ApplicationModel;
using static Project2_222114X.OpenWeather;
using static Project2_222114X.Trivia;
using System.Security.Claims;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Project2_222114X
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class Choice
    {
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }

    [DataContract]
    public class Message
    {
        [DataMember(Name = "role")]
        public string Role { get; set; }

        [DataMember(Name = "content")]
        public string Content { get; set; }
    }

    [DataContract]
    public class RequestBody
    {
        [DataMember(Name = "messages")]
        public Message[] Messages { get; set; }

        [DataMember(Name = "system_prompt")]
        public string SystemPrompt { get; set; }

        [DataMember(Name = "temperature")]
        public double Temperature { get; set; }

        [DataMember(Name = "top_k")]
        public int TopK { get; set; }

        [DataMember(Name = "top_p")]
        public double TopP { get; set; }

        [DataMember(Name = "max_tokens")]
        public int MaxTokens { get; set; }

        [DataMember(Name = "web_access")]
        public bool WebAccess { get; set; }
    }

    public sealed partial class MainPage : Page
    {
        private List<Trivia.Result> triviaQuestions;
        private int currentQuestionIndex;
        SpeechSynthesizer speech;
        MediaElement mediaElement;
        private Random random = new Random();
        int index; 
        private StackPanel alarmsStackPanel;
        private async void FetchTriviaButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Fetch trivia data
                var triviaData = await Trivia.GetTrivia();

                if (triviaData != null && triviaData.results != null)
                {
                    triviaQuestions = triviaData.results;
                    currentQuestionIndex = 0;

                    // Display the first question
                    DisplayQuestion();
                }
                else
                {
                    Triviatb.Text = "No trivia data available.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching trivia: {ex.Message}");
                Triviatb.Text = "No trivia data available.";
            }
        }
        private async void AIResponse_click(object sender, RoutedEventArgs e)
        {
            string message = InputTextBox.Text;
            if (!string.IsNullOrEmpty(message))
            {
                AddMessage("You", message);
                InputTextBox.Text = "";
                var client = new HttpClient();
                var requestBody = new RequestBody
                {
                    Messages = new[]
                    {
                new Message { Role = "user", Content = message }
            },
                    SystemPrompt = "",
                    Temperature = 0.9,
                    TopK = 5,
                    TopP = 0.9,
                    MaxTokens = 256,
                    WebAccess = false
                };

                var memoryStream = new MemoryStream();
                var serializer = new DataContractJsonSerializer(typeof(RequestBody));
                serializer.WriteObject(memoryStream, requestBody);
                memoryStream.Position = 0;
                var content = new StringContent(Encoding.UTF8.GetString(memoryStream.ToArray()), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://chatgpt-42.p.rapidapi.com/conversationgpt4-2"),
                    Headers =
            {
                { "x-rapidapi-key", "" },
                { "x-rapidapi-host", "chatgpt-42.p.rapidapi.com" }
            },
                    Content = content
                };

                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();

                    // Parse JSON response to extract the result field
                    var jsonResponse = JObject.Parse(body);
                    var result = jsonResponse["result"]?.ToString();

                    Debug.WriteLine(result);

                    string chatbotResponse = result ?? "No result found.";
                    AddMessage("RieAI", chatbotResponse);
                }
            }
        }

        private void DisplayQuestion()
        {
            if (currentQuestionIndex < triviaQuestions.Count)
            {
                var question = triviaQuestions[currentQuestionIndex];
                QuestionTextBlock.Text = question.question;

                // Clear previous choices
                ChoicesPanel.Children.Clear();

                // Create radio buttons for each choice
                var choices = question.incorrect_answers.Select(ans => new Choice { Text = ans, IsCorrect = false }).ToList();
                choices.Add(new Choice { Text = question.correct_answer, IsCorrect = true });
                choices = choices.OrderBy(_ => Guid.NewGuid()).ToList(); // Shuffle the choices

                foreach (var choice in choices)
                {
                    var radioButton = new RadioButton
                    {
                        Content = choice.Text,
                        Tag = choice.IsCorrect,
                        GroupName = "Choices"
                    };
                    ChoicesPanel.Children.Add(radioButton);
                }
            }
            else
            {
                // No more questions
                QuestionTextBlock.Text = "Quiz completed!";
                ChoicesPanel.Children.Clear();
                SubmitButton.IsEnabled = false;
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Find the selected radio button
            var selectedRadioButton = ChoicesPanel.Children.OfType<RadioButton>().FirstOrDefault(rb => rb.IsChecked == true);
            if (selectedRadioButton != null && bool.TryParse(selectedRadioButton.Tag.ToString(), out bool isCorrect))
            {
                Triviatb.Text = isCorrect ? "Correct!" : "Wrong!";
                currentQuestionIndex++;
                DisplayQuestion();
            }
            else
            {
                Triviatb.Text = "Please select an answer.";
            }
        }
    private async void LoadWeatherIcon()
        {
            RootObject myWeather = await OpenWeather.GetWeather();
            var Cicon = myWeather.list[0].weather[0].icon;
            string CiconUrl = $"http://openweathermap.org/img/wn/{Cicon}@2x.png";
            BitmapImage Cbitmap = new BitmapImage(new Uri(CiconUrl, UriKind.Absolute));
            CWeatherIcon.Source = Cbitmap;


            var d1icon = myWeather.list[2].weather[0].icon;
            string d1iconUrl = $"http://openweathermap.org/img/wn/{d1icon}@2x.png";
            BitmapImage d1bitmap = new BitmapImage(new Uri(d1iconUrl, UriKind.Absolute));
            d1WeatherIcon.Source = d1bitmap;


            var d2icon = myWeather.list[8].weather[0].icon;
            string d2iconUrl = $"http://openweathermap.org/img/wn/{d2icon}@2x.png";
            BitmapImage d2bitmap = new BitmapImage(new Uri(d2iconUrl, UriKind.Absolute));
            d2WeatherIcon.Source = d2bitmap;


            var d3icon = myWeather.list[16].weather[0].icon;
            string d3iconUrl = $"http://openweathermap.org/img/wn/{d3icon}@2x.png";
            BitmapImage d3bitmap = new BitmapImage(new Uri(d3iconUrl, UriKind.Absolute));
            d3WeatherIcon.Source = Cbitmap;
        }
        public MainPage()
        {
            this.InitializeComponent();
            LoadWeatherIcon(); // Call method to load weather icon

            ApplicationView.PreferredLaunchViewSize = new Size(800, 480);
            ApplicationView.PreferredLaunchWindowingMode =
            ApplicationViewWindowingMode.PreferredLaunchViewSize;

            ApplicationViewTitleBar titleBar =
                ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar =
            CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            // Prevent the application window from being resized
            var view = ApplicationView.GetForCurrentView();
            var size = new Size(800, 480);
            view.VisibleBoundsChanged += (sender, e) => view.TryResizeView(size);



            App.CBRes.Add("I'm here to help");
            App.CBRes.Add("Let me find the information for you.");
            App.CBRes.Add("I'm processing your request.");
            App.CBRes.Add("No problem");
            App.CBRes.Add("Sorry I'm unable to process that");
            App.CBRes.Add("Is there something else I can help you with");
            App.CBRes.Add("I'm unable to process your request.");
            App.CBRes.Add("Let me provide you with some assistance.");
            App.CBRes.Add("Yes");
            App.CBRes.Add("No");
            App.CBRes.Add("Maybe");
            App.CBRes.Add("Ok");
            App.CBRes.Add("Purging redundencies");
            App.CBRes.Add("Guessing answer");
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {

            StackPanel sp = new StackPanel();
            sp.Background = new SolidColorBrush(Colors.Orange);

            Assembly assembly = Assembly.GetExecutingAssembly();
            var attribute = (AssemblyTitleAttribute)assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true)[0];
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(attribute.Title);
            Clipboard.SetContent(dataPackage);
            FetchTriviaButton_Click(sender,e);

            RootObject myWeather = await OpenWeather.GetWeather();
            //Debug.WriteLine(myWeather.ToString());
            var CurrentDt = DateTime.Now.ToString("dddd, d MMMM") + $" [{myWeather.list[0].main.temp- 273.15}°C]";
            d1Temp.Text = $" [{myWeather.list[2].main.temp - 273.15}°C]";// 6 hr later
            d2Temp.Text = $" [{myWeather.list[8].main.temp - 273.15}°C]";// day 2
            d3Temp.Text = $" [{myWeather.list[16].main.temp - 273.15}°C]";// day 3

            cpressure.Text = ($"Pressure: {myWeather.list[0].main.pressure} hPa");
            chumidity.Text = ($"Humidity: {myWeather.list[0].main.humidity}%");
            cwindspeed.Text = ($"Wind Speed: {myWeather.list[0].wind.speed} m/s");
            cwinddir.Text = ($"Wind Direction: {myWeather.list[0].wind.deg}°");
            crain.Text = ($"Rain Volume: {myWeather.list[0].rain?.volume_3h ?? 0} mm");

            d1pressure.Text = ($"Pressure: {myWeather.list[2].main.pressure} hPa");
            d1humidity.Text = ($"Humidity: {myWeather.list[2].main.humidity}%");
            d1windspeed.Text = ($"Wind Speed: {myWeather.list[2].wind.speed} m/s");
            d1winddir.Text = ($"Wind Direction: {myWeather.list[2].wind.deg}°");
            d1rain.Text = ($"Rain Volume: {myWeather.list[2].rain?.volume_3h ?? 0} mm");

            d2pressure.Text = ($"Pressure: {myWeather.list[8].main.pressure} hPa");
            d2humidity.Text = ($"Humidity: {myWeather.list[8].main.humidity}%");
            d2windspeed.Text = ($"Wind Speed: {myWeather.list[8].wind.speed} m/s");
            d2winddir.Text = ($"Wind Direction: {myWeather.list[8].wind.deg}°");
            d2rain.Text = ($"Rain Volume: {myWeather.list[8].rain?.volume_3h ?? 0} mm");

            d3pressure.Text = ($"Pressure: {myWeather.list[16].main.pressure} hPa");
            d3humidity.Text = ($"Humidity: {myWeather.list[16].main.humidity}%");
            d3windspeed.Text = ($"Wind Speed: {myWeather.list[16].wind.speed} m/s");
            d3winddir.Text = ($"Wind Direction: {myWeather.list[16].wind.deg}°");
            d3rain.Text = ($"Rain Volume: {myWeather.list[16].rain?.volume_3h ?? 0} mm");


            var TodayDt = DateTime.Now.ToString("dddd, d MMMM");
            var NextDayDt = DateTime.Now.AddDays(1).ToString("dddd, d MMMM");
            var DayAfterNextDT = DateTime.Now.AddDays(2).ToString("dddd, d MMMM");
            try
            {
                StorageFolder storageFolder = Package.Current.InstalledLocation;
                StorageFile File = await storageFolder.GetFileAsync("greetings.txt");
                string Greeting = await FileIO.ReadTextAsync(File);
                greet.Text = Greeting;
            }
            catch
            {
                greet.Text = "Hi Jason [Failed to load greeting.txt]";
            }
            var Day = DateTime.Now.ToString("dddd");
            var Date = DateTime.Now.ToString("d");
            var Time = DateTime.Now.ToString("hh:mm");
            var AP = DateTime.Now.ToString("tt");
            Current.Text = CurrentDt;
            TIME.Text = Time;
            M.Text = AP;
            DayTB.Text = Day;
            DateTB.Text = Date;
            Day1.Text = TodayDt;
            Day2.Text = NextDayDt;
            Day3.Text = DayAfterNextDT;

            speech = new SpeechSynthesizer();
            SpeechSynthesisStream stream = await speech.SynthesizeTextToStreamAsync(greet.Text+", The time is now" + Time + AP + "GMT+8");
            mePlayer.SetSource(stream, string.Empty);
            mePlayer.Play();

            string chatbotResponse = "How can I assist you today ?";
            AddMessage("RieAI", chatbotResponse);
        }

        private async void Alarm_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            string checkBoxName = checkBox.Name;
            if (checkBox.IsChecked == true)
            {
                checkBox.Content = "Enabled";

                speech = new SpeechSynthesizer();
                SpeechSynthesisStream stream = await speech.SynthesizeTextToStreamAsync("Enabled");
                mePlayer.SetSource(stream, string.Empty);
                mePlayer.Play();
            }
        }

        private async void Alarm_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            string checkBoxName = checkBox.Name;
            if (checkBox.IsChecked == false)
            {
                checkBox.Content = "Disabled";

                speech = new SpeechSynthesizer();
                SpeechSynthesisStream stream = await speech.SynthesizeTextToStreamAsync("Disabled");
                mePlayer.SetSource(stream, string.Empty);
                mePlayer.Play();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AdditionalFeatures));
        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = InputTextBox.Text;
            if (message != string.Empty)
            {
                AddMessage("You", message);
                InputTextBox.Text = "";
                string chatbotResponse = App.CBRes[random.Next(App.CBRes.Count)];
                AddMessage("RieAI", chatbotResponse);
            }
        }

        private void AddMessage(string sender, string message)
        {
            TextBlock messageBlock = new TextBlock();
            TextBlock TimeMB = new TextBlock();
            if (sender == "You")
            {
                messageBlock.HorizontalAlignment = HorizontalAlignment.Right;
                TimeMB.HorizontalAlignment = HorizontalAlignment.Right;
                messageBlock.Padding = new Thickness(50, 0, 0, 0);

            }
            else
            {
                messageBlock.HorizontalAlignment = HorizontalAlignment.Left;
                TimeMB.HorizontalAlignment = HorizontalAlignment.Left;
                messageBlock.Padding = new Thickness(0, 0, 50, 0);

            }
            messageBlock.Text = $"{sender}: {message}";
            messageBlock.Margin = new Thickness(5, 5, 5, 0);
            messageBlock.TextWrapping = TextWrapping.Wrap;
            messageBlock.MaxWidth = 300;
            messageBlock.Foreground = new SolidColorBrush(Windows.UI.Colors.WhiteSmoke);
            ChatStackPanel.Children.Add(messageBlock);

            TimeMB.Text = DateTime.Now.ToString("HH:mm");
            TimeMB.FontSize = 10;
            TimeMB.Margin = new Thickness(5, 0, 5, 5);
            TimeMB.Foreground = new SolidColorBrush(Windows.UI.Colors.WhiteSmoke);
            ChatStackPanel.Children.Add(TimeMB);

            ChatScrollViewer.ChangeView(null, ChatScrollViewer.ScrollableHeight, null);
        }
        private void DeleteAlarmButton_Click(object sender, RoutedEventArgs e)
        {
            Button deleteButton = (Button)sender;
            Grid alarmGrid = (Grid)deleteButton.Parent;
            Border alarmEntry = (Border)alarmGrid.Parent;
            StackPanel alarmsStackPanel = (StackPanel)this.FindName("AlarmHolder");

            alarmsStackPanel.Children.Remove(alarmEntry);
        }

        private void DateButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SolidColorBrush brush = button.Background as SolidColorBrush;

            if (brush.Color == Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF)) // Assume gray color indicates selected
            {
                button.Background = new SolidColorBrush(Colors.LightCyan);
                button.Foreground = new SolidColorBrush(Colors.MediumPurple);
            }
            else
            {
                button.Background = new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));
                button.Foreground = new SolidColorBrush(Colors.White);
            }
            Grid alarmGrid = (Grid)button.Parent;
            int index;
            if (alarmGrid.Name.StartsWith("AlarmGrid"))
            {
                bool Mbool = false, Tbool = false, Wbool = false, T1bool = false, Fbool = false, Sbool = false;
                string indexString = alarmGrid.Name.Substring("AlarmGrid".Length);
                if (int.TryParse(indexString, out index))
                {
                    TextBlock dateTimeTextBlock = alarmGrid.FindName("dateTextBlock" + index) as TextBlock;
                    //Debug.WriteLine($"Alarm Index: {index}");
                    Button dateMButton = alarmGrid.FindName("DATEM" + index) as Button;
                    Button dateTButton = alarmGrid.FindName("DATET" + index) as Button;
                    Button dateWButton = alarmGrid.FindName("DATEW" + index) as Button;
                    Button dateT1Button = alarmGrid.FindName("DATET1" + index) as Button;
                    Button dateFButton = alarmGrid.FindName("DATEF" + index) as Button;
                    Button dateSButton = alarmGrid.FindName("DATES" + index) as Button;
                    Button dateS1Button = alarmGrid.FindName("DATES1" + index) as Button;
                    if (dateMButton != null && dateMButton.Background is SolidColorBrush solidColorBrushM && solidColorBrushM.Color == Colors.LightCyan &&Mbool != true)
                    {
                        dateTimeTextBlock.Text = "Every MON";
                        Mbool = true;
                    }

                    if (dateTButton != null && dateTButton.Background is SolidColorBrush solidColorBrushT && solidColorBrushT.Color == Colors.LightCyan)
                    {
                        if (Mbool == true)
                        {
                            dateTimeTextBlock.Text += ",TUE";
                            Tbool = true;
                        }
                        else
                        {
                            dateTimeTextBlock.Text = "Every TUE";
                            Tbool = true;
                        }
                    }
                    if (dateWButton.Background is SolidColorBrush solidColorBrushW && solidColorBrushW.Color == Colors.LightCyan)
                    {
                        if (Mbool == true || Tbool == true)
                        {
                            dateTimeTextBlock.Text += ",WED";
                            Wbool = true;
                        }
                        else
                        {
                            dateTimeTextBlock.Text = "Every WED";
                            Wbool = true;
                        }
                    }
                    if (dateT1Button.Background is SolidColorBrush solidColorBrushT1 && solidColorBrushT1.Color == Colors.LightCyan)
                    {
                        if (Mbool || Tbool || Wbool)
                        {
                            dateTimeTextBlock.Text += ",THU";
                            T1bool = true;
                        }
                        else
                        {
                            dateTimeTextBlock.Text = "Every THU";
                            T1bool = true;
                        }
                    }
                    if (dateFButton.Background is SolidColorBrush solidColorBrushF && solidColorBrushF.Color == Colors.LightCyan)
                    {
                        if (Mbool && Tbool && Wbool && T1bool)
                        {
                            dateTimeTextBlock.Text = "Every Weekday";
                            Fbool = true;
                        }
                        else if (Mbool || Tbool || Wbool || T1bool)
                        {
                            dateTimeTextBlock.Text += ",FRI";
                            Fbool = true;
                        }
                        else
                        {
                            dateTimeTextBlock.Text = "Every FRI";
                            Fbool = true;
                        }
                    }
                    if (dateSButton.Background is SolidColorBrush solidColorBrushS && solidColorBrushS.Color == Colors.LightCyan)
                    {
                        if (Mbool || Tbool || Wbool || T1bool || Fbool)
                        {
                            dateTimeTextBlock.Text += ",SAT";
                            Sbool = true;
                        }
                        else
                        {
                            dateTimeTextBlock.Text = "Every SAT";
                            Sbool = true;
                        }
                    }
                    if (dateS1Button.Background is SolidColorBrush solidColorBrushS1 && solidColorBrushS1.Color == Colors.LightCyan)
                    {
                        if (Mbool && Tbool && Wbool && T1bool && Fbool && Sbool)
                        {
                            dateTimeTextBlock.Text = "Everyday";
                        }
                        else if (!Mbool && !Tbool && !Wbool && !T1bool && !Fbool && Sbool)
                        {
                            dateTimeTextBlock.Text = "Every Weekends";
                        }
                        else if (Mbool || Tbool || Wbool || T1bool || Fbool || Sbool)
                        {
                            dateTimeTextBlock.Text += ",SUN";
                        }
                        else
                        {
                            dateTimeTextBlock.Text = "Every SUN";
                        }
                    }
                    /*Debug.WriteLine("Mbool:" + Mbool.ToString());
                    Debug.WriteLine("Tbool:" + Tbool.ToString());
                    Debug.WriteLine("Wbool:" + Wbool.ToString());
                    Debug.WriteLine("T1bool:" + T1bool.ToString());
                    Debug.WriteLine("Fbool:" + Fbool.ToString());
                    Debug.WriteLine("Sbool:" + Sbool.ToString());*/
                }
            }

        }

        private async void AddAlarmButton_Click(object sender, RoutedEventArgs e)
        {
            index++;

            DateTimeOffset selectedDate = DateTimePicker.Date;
            DateTime date = selectedDate.Date;
            TimeSpan selectedTime = AlarmTimePicker.Time;

            string selectedDateString = date.ToString("dd/MM/yyyy");
            string selectedTimeString = selectedTime.ToString(@"hh\:mm");

            Border newAlarmEntry = new Border
            {
                BorderThickness = new Thickness(1.2),
                Margin = new Thickness(2),
                Height = 200,
                CornerRadius = new CornerRadius(12),
                Background = new SolidColorBrush(Colors.MediumPurple),
                Width = 450,
                Name = "Alarm" + index,
            };
            DatePicker datePicker = new DatePicker
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Name = "DatePicker" + index
            };

            TimePicker timePicker = new TimePicker
            {
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Name = "TimePicker" + index
            };

            TextBlock Time = new TextBlock
            {
                Text = DateTime.Parse(selectedTimeString).ToString("hh:mm"),
                Padding = new Thickness(10, 2, 2, 0),
                FontSize = 80,
            };

            TextBlock AMPM = new TextBlock
            {
                Text = DateTime.Parse(selectedTimeString).ToString("tt"),
                Padding = new Thickness(0, 30, 0, 0),
            };

            Button DATEM = new Button
            {
                Content = "M",
                FontSize = 15,
                Margin = new Thickness(10, 0, 0, 20),
                Width = 35,
                Name = "DATEM" + index // Assign a unique name
            };
            Button DATET = new Button
            {
                Content = "T",
                FontSize = 15,
                Margin = new Thickness(55, 0, 0, 20),
                Width = 35,
                Name = "DATET" + index // Assign a unique name
            };
            Button DATEW = new Button
            {
                Content = "W",
                FontSize = 15,
                Margin = new Thickness(100, 0, 0, 20),
                Width = 35,
                Name = "DATEW" + index // Assign a unique name
            };
            Button DATET1 = new Button
            {
                Content = "T",
                FontSize = 15,
                Margin = new Thickness(145, 0, 0, 20),
                Width = 35,
                Name = "DATET1" + index // Assign a unique name
            };
            Button DATEF = new Button
            {
                Content = "F",
                FontSize = 15,
                Margin = new Thickness(-10, 0, 0, 20),
                Width = 35,
                Name = "DATEF" + index // Assign a unique name
            };
            Button DATES = new Button
            {
                Content = "S",
                FontSize = 15,
                Margin = new Thickness(35, 0, 0, 20),
                Width = 35,
                Name = "DATES" + index // Assign a unique name
            };
            Button DATES1 = new Button
            {
                Content = "S",
                FontSize = 15,
                Margin = new Thickness(80, 0, 0, 20),
                Width = 35,
                Name = "DATES1" + index // Assign a unique name
            };
            DATEM.Click += DateButton_Click;
            DATET.Click += DateButton_Click;
            DATEW.Click += DateButton_Click;
            DATET1.Click += DateButton_Click;
            DATEF.Click += DateButton_Click;
            DATES.Click += DateButton_Click;
            DATES1.Click += DateButton_Click;

            string imagePath = "ms-appx:///images/Delete.png";
            var uri = new Uri(imagePath);

            Button deleteButton = new Button
            {
                Content = new Windows.UI.Xaml.Controls.Image // Specify the namespace
                {
                    Source = new BitmapImage(uri),
                    Width = 30, // Adjust the width and height as needed
                    Height = 30,
                },
                Background = new SolidColorBrush(Colors.Transparent),
                Margin = new Thickness(20, 0, 20, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Name = "Del" + index.ToString(),
            };

            CheckBox checkBox = new CheckBox
            {
                Content = "Disabled",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                Name = index.ToString(),
            };
            TextBlock dateTextBlock = new TextBlock
            {
                Text = selectedDateString,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                Name = "dateTextBlock" + index.ToString(),
            };

            Grid grid = new Grid
            {
                Name = "AlarmGrid" + index
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) }); // Time
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // AMPM
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // DatePicker
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // CheckBox
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Delete

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Time
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // DATE
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // DATE

            Grid.SetRow(DATEM, 1);
            Grid.SetRow(DATET, 1);
            Grid.SetRow(DATEW, 1);
            Grid.SetRow(DATET1, 1);
            Grid.SetRow(DATEF, 1);
            Grid.SetRow(DATES, 1);
            Grid.SetRow(DATES1, 1);
            Grid.SetRow(Time, 0);
            Grid.SetRow(AMPM, 0);
            Grid.SetRow(checkBox, 0);
            Grid.SetRow(deleteButton, 1);
            Grid.SetRow(dateTextBlock, 2);

            Grid.SetColumn(DATEF, 1);
            Grid.SetColumn(DATES, 1);
            Grid.SetColumn(DATES1, 1);
            Grid.SetColumn(Time, 0);
            Grid.SetColumn(AMPM, 1);
            Grid.SetColumn(checkBox, 2);
            Grid.SetColumn(deleteButton, 2);
            Grid.SetColumnSpan(dateTextBlock, 2);


            grid.Children.Add(dateTextBlock);
            grid.Children.Add(Time);
            grid.Children.Add(AMPM);
            grid.Children.Add(DATEM);
            grid.Children.Add(DATET);
            grid.Children.Add(DATEW);
            grid.Children.Add(DATET1);
            grid.Children.Add(DATEF);
            grid.Children.Add(DATES);
            grid.Children.Add(DATES1);
            grid.Children.Add(checkBox);
            grid.Children.Add(deleteButton);    

            newAlarmEntry.Child = grid;
            StackPanel alarmsStackPanel = (StackPanel)this.FindName("AlarmHolder");
            alarmsStackPanel.Children.Insert(alarmsStackPanel.Children.Count, newAlarmEntry);


            CheckBox foundCheckBox = alarmsStackPanel.FindName(index.ToString()) as CheckBox;
            foundCheckBox.Unchecked += Alarm_Unchecked;
            foundCheckBox.Checked += Alarm_Checked;

            Button foundDelete = alarmsStackPanel.FindName("Del" + index.ToString()) as Button;
            foundDelete.Click += DeleteAlarmButton_Click;
        }
    }
}