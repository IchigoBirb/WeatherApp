using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Xml.Linq;
using System.Diagnostics;
using System.Reflection;
using Windows.UI;
using Windows.Media.SpeechSynthesis;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections;
using Windows.ApplicationModel.Contacts;
using System.Threading;
using Windows.UI.Core;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Controls.Maps;
using static Project2_222114X.NewsAPI;
using static System.Net.WebRequestMethods;
using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
namespace Project2_222114X
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    //https://learn.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.canvas?view=winrt-22621
    public sealed partial class AdditionalFeatures : Page
    {
        SpeechSynthesizer speech;
        MediaElement mediaElement;
        int index = 0; 
        bool DelImmunity;
        double newX, newY;
        Image imgElement;
        int Counter;
        double result;
        double totalwidth;

        private Image lastDeletedImage;
        private UIElement lastDeletedNews;
        private Button lastDeletedLike;
        public AdditionalFeatures()
        {
            this.InitializeComponent();
            speech = new SpeechSynthesizer();
            this.KeyDown += Page_KeyDown;
        }
     
        private void imgPlayer_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            ((Image)sender).Opacity = 0.7;
        }
        private void UpdateTextBlockPosition(Image imgElement, double newX, double newY)
        {
            try
            {
                Border News = mainCanvas.FindName("News" + imgElement.Name[9]) as Border;
                Button Like = mainCanvas.FindName("Like" + imgElement.Name[9]) as Button;
                Canvas.SetLeft(News, Canvas.GetLeft(imgElement) + newX);
                Canvas.SetLeft(Like, Canvas.GetLeft(imgElement) + newX);
                Canvas.SetTop(News, Canvas.GetTop(imgElement) + newY + imgElement.ActualHeight);
                Canvas.SetTop(Like, Canvas.GetTop(imgElement) + newY + imgElement.ActualHeight-50);

            }
            catch{}//if its deleted
        }
        private async void imgPlayer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            imgElement = (Image)sender;
            var transform = ((MatrixTransform)imgElement.RenderTransform).Matrix;

            newX = transform.OffsetX + e.Delta.Translation.X;
            newY = transform.OffsetY + e.Delta.Translation.Y;

            // Clamp the new position within the bounds of the canvas
            newX = Math.Max(0, Math.Min(mainCanvas.ActualWidth - imgElement.ActualWidth, newX));
            newY = Math.Max(0, Math.Min(mainCanvas.ActualHeight - imgElement.ActualHeight, newY));

            transform.OffsetX = newX;
            transform.OffsetY = newY;

            Border News = mainCanvas.FindName("News" + imgElement.Name[9]) as Border;
            TextBlock NewsT = mainCanvas.FindName("NewsT" + imgElement.Name[9]) as TextBlock;
            Button Like = mainCanvas.FindName("Like" + imgElement.Name[9]) as Button;
            News.Background = new SolidColorBrush(Colors.MediumPurple);

            imgElement.RenderTransform = new MatrixTransform { Matrix = transform };

            //Debug.WriteLine($"New X: {newX}, New Y: {newY}");
            Counter++;
            Canvas.SetZIndex(imgElement, 10+Counter);
            Canvas.SetZIndex(NewsT, 10+Counter);
            Canvas.SetZIndex(News, 10+Counter);
            Canvas.SetZIndex(Like, 10 + Counter);

            speech = new SpeechSynthesizer();

            Canvas.SetLeft(News, Canvas.GetLeft(imgElement) + transform.OffsetX);
            Canvas.SetTop(News, Canvas.GetTop(imgElement) + transform.OffsetY + imgElement.ActualHeight);

            NewsT.Visibility = Visibility.Visible;

            SpeechSynthesisStream stream = await speech.SynthesizeTextToStreamAsync(NewsT.Text.ToString());
            mePlayer.SetSource(stream, string.Empty);

            UpdateTextBlockPosition(imgElement,newX,newY);
            mePlayer.Play();

            var trashBinBounds = new Windows.Foundation.Rect(Canvas.GetLeft(TrashBin), Canvas.GetTop(TrashBin), TrashBin.ActualWidth, TrashBin.ActualHeight);
            var elementBounds = new Windows.Foundation.Rect(Canvas.GetLeft(imgElement) + transform.OffsetX, Canvas.GetTop(imgElement) + transform.OffsetY, imgElement.ActualWidth, imgElement.ActualHeight);

            if (IsIntersecting(trashBinBounds, elementBounds))
            {
                if(DelImmunity!=false)
                {
                    Thread.Sleep(800);
                    DelImmunity = false;
                    return; // Exit the method 
                }
                speech = new SpeechSynthesizer();
                SpeechSynthesisStream streams = await speech.SynthesizeTextToStreamAsync("Deleted");
                mePlayer.SetSource(streams, string.Empty);
                mePlayer.Play();

                mainCanvas.Children.Remove(imgElement);
                mainCanvas.Children.Remove(News);
                mainCanvas.Children.Remove(Like);
                lastDeletedImage = imgElement;
                lastDeletedNews = News;
                lastDeletedLike = Like;
            }
        }
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }
        private bool IsIntersecting(Rect rect1, Rect rect2)
        {
            return (rect2.Right > rect1.Left && rect2.Bottom > rect1.Top);
        }
        private void imgPlayer_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            ((Image)sender).Opacity = 1;
        }
        private async void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            speech = new SpeechSynthesizer();
            SpeechSynthesisStream stream = await speech.SynthesizeTextToStreamAsync("Undo");
            mePlayer.SetSource(stream, string.Empty);
            mePlayer.Play();

            if (lastDeletedImage != null && lastDeletedNews != null && lastDeletedLike != null)
            {
                DelImmunity = true;
                mainCanvas.Children.Add(lastDeletedNews);   
                mainCanvas.Children.Add(lastDeletedImage);
                mainCanvas.Children.Add(lastDeletedLike);

                lastDeletedImage.SetValue(Canvas.LeftProperty, 0);
                lastDeletedImage.SetValue(Canvas.TopProperty, 0);

                lastDeletedNews.SetValue(Canvas.LeftProperty, 0);
                lastDeletedNews.SetValue(Canvas.TopProperty, lastDeletedImage.ActualHeight);

                lastDeletedLike.SetValue(Canvas.LeftProperty, 0);
                lastDeletedLike.SetValue(Canvas.TopProperty, lastDeletedImage.ActualHeight);
            }

            lastDeletedImage = null;
            lastDeletedNews = null;
            lastDeletedLike = null;
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            speech = new SpeechSynthesizer();
            SpeechSynthesisStream streamsss = await speech.SynthesizeTextToStreamAsync("Adding More News");
            mePlayer.SetSource(streamsss, string.Empty);
            mePlayer.Play();

            Create(sender, e);
        }

        private async void Create(object sender, RoutedEventArgs e)
        {

            if (index < App.myList.Count)
            {
                try
                {
                    //Debug.WriteLine("Found index:"+ index);//0,1
                    string News = App.myList[index + 1];
                    var data = News.Split("split");

                    //Debug.WriteLine("Image source: " + data[1]);
                    var ImageSrc = data[1];
                    var NewsText = data[0];
                    var Widths = data[2];
                    double result;
                    double.TryParse(Widths, out result);
                    Debug.WriteLine("tried");
                    if (!string.IsNullOrEmpty(ImageSrc))
                    {
                        Uri uri = new Uri(ImageSrc, UriKind.RelativeOrAbsolute);
                        BitmapImage bitmapImage = new BitmapImage(uri);

                        Image imgPlayer = new Image()
                        {
                            Source = bitmapImage,
                            Name = "imgPlayer" + (index + 1),
                            ManipulationMode = ManipulationModes.All,
                            Width = result,
                        };
                        Image prevImgPlayer = mainCanvas.FindName("imgPlayer" + index) as Image;
                        //Canvas.SetTop(imgPlayer, 110 + (double)(index / 3) * 150);
                        imgPlayer.ManipulationStarted += imgPlayer_ManipulationStarted;
                        imgPlayer.ManipulationDelta += imgPlayer_ManipulationDelta;
                        imgPlayer.ManipulationCompleted += imgPlayer_ManipulationCompleted;

                        Border border = new Border()
                        {
                            Name = "News" + (index + 1),
                            Padding = new Thickness(5),
                        };

                        NewsText = NewsText.Replace("&#x0a;", Environment.NewLine);
                        //Debug.WriteLine("Text: " + NewsText);
                        TextBlock textBlock = new TextBlock()
                        {
                            Name = "NewsT" + (index + 1),
                            Text = NewsText,
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 16,
                            Foreground = new SolidColorBrush(Colors.Black)
                        };
                        Button Likebutton = new Button()
                        {
                            FontFamily = new FontFamily("Segoe MDL2 Assets"),
                            FontSize = 18,
                            Content = "\uE8E1", // Unicode character for the "Like" icon
                            Width = 50,
                            Height = 50,
                            Background = new SolidColorBrush(Colors.MediumPurple),
                            Foreground = new SolidColorBrush(Colors.LightCyan),
                            Name = "Like" + (index + 1)
                        };
                        Likebutton.Click += Likebutton_Click;
                        border.Child = textBlock;

                        mainCanvas.Children.Add(imgPlayer);
                        mainCanvas.Children.Add(border);
                        mainCanvas.Children.Add(Likebutton);
                        if ((index % 3) != 0)
                        {
                            imgPlayer.ImageOpened += (senderImage, args) =>
                            {
                                Canvas.SetLeft(border, totalwidth);
                                Canvas.SetLeft(Likebutton, totalwidth);
                                Canvas.SetTop(border, 110 + ((double)((index - 1) / 3) * 150) + imgPlayer.ActualHeight);
                                Canvas.SetTop(Likebutton, 110 + ((double)((index - 1) / 3) * 150) + imgPlayer.ActualHeight - 50);

                            };
                        }
                        else
                        {
                            imgPlayer.ImageOpened += (senderImage, args) =>
                            {
                                Canvas.SetLeft(border, 10);
                                Canvas.SetLeft(Likebutton, 10);
                                Canvas.SetTop(border, 110 + ((double)((index) / 3) * 150) + imgPlayer.ActualHeight);
                                Canvas.SetTop(Likebutton, 110 + ((double)((index) / 3) * 150) + imgPlayer.ActualHeight - 50);
                            };
                        }
                        index++;
                        //Debug.WriteLine($"Added image and border: {imgPlayer.Name}, {border.Name}");

                        border.Background = new SolidColorBrush(Colors.MediumPurple);
                    }
                    else
                    {
                        Debug.WriteLine("url is null");
                        index++;
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                speech = new SpeechSynthesizer();
                SpeechSynthesisStream streamsss = await speech.SynthesizeTextToStreamAsync("Free subscription is only limited to 4 feeds , subscribe to our premium plan for more");
                mePlayer.SetSource(streamsss, string.Empty);
                mePlayer.Play();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
        private async void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Add_Click(sender,e);
            }
            else if (e.Key == Windows.System.VirtualKey.Back)
            {
                Back_Click(sender, e);
            }
            else if (e.Key == Windows.System.VirtualKey.Z && Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
            {
                UndoButton_Click(sender,e);
            }
        }
        private void Likebutton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SolidColorBrush brush = button.Background as SolidColorBrush;

            if (brush.Color == Colors.MediumPurple) // Assume gray color indicates selected
            {
                button.Background = new SolidColorBrush(Colors.LightCyan);
                button.Foreground = new SolidColorBrush(Colors.MediumPurple);
            }
            else
            {
                button.Background = new SolidColorBrush(Colors.MediumPurple);
                button.Foreground = new SolidColorBrush(Colors.LightCyan);
            }
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Root myNews = await NewsAPI.GetNews();
            Debug.WriteLine($"Title of the first article: {myNews.Articles[1].UrlToImage}");
            
            for (int i = 0; i < myNews.Articles.Count; i++)
            {
                Debug.WriteLine($"Article{i}added");
                StringBuilder sb = new StringBuilder();

                int charCount = 0;
                foreach (char c in myNews.Articles[i].Title)
                {
                    sb.Append(c);
                    charCount++;

                    // Insert newline every 30 characters
                    if (charCount % 30 == 0)
                    {
                        sb.Append("\n");
                    }
                }
                // Check if the article has a valid UrlToImage and other conditions before adding
                App.myList.Add($"{sb}split{myNews.Articles[i].UrlToImage}split233");
                /* Source = "ms-appx:///images/train.png"
                Scheme(ms - appx): This indicates that the URI is referring to an application resource.
                Authority (//): This separates the scheme from the rest of the URI.
                Path(/ path / to / resource): This is the path to the resource within the app's package. 
            It typically starts with a forward slash (/) and specifies the directory structure and filename of the resource.
            */
            }
            for (int i = 0; i < 3; i++)
            {
                string News = App.myList[i];
                var data = News.Split("split");

                // Extract data from the split array
                var NewsText = data[0];
                var ImageSrc = data[1];
                var Widths = data[2];
                if (!string.IsNullOrEmpty(ImageSrc))
                {

                    double result;
                    double.TryParse(Widths, out result);

                    Uri uri;
                    uri = new Uri(ImageSrc, UriKind.Absolute);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.UriSource = uri;
                    Image imgPlayer = new Image()
                    {
                        Source = bitmapImage,
                        Name = "imgPlayer" + (index + 1),
                        ManipulationMode = ManipulationModes.All,
                        Width = result,
                    };

                    Debug.WriteLine("H2");

                    Image prevImgPlayer = mainCanvas.FindName("imgPlayer" + index) as Image;
                    Debug.WriteLine("H3");
                    if ((index % 3) != 0)
                    {
                        Canvas.SetLeft(imgPlayer, totalwidth);
                        Debug.WriteLine(totalwidth);
                        totalwidth += imgPlayer.ActualWidth + (5 * ((index + 1) % 3));
                        Debug.WriteLine(totalwidth);
                    }
                    else
                    {
                        Canvas.SetLeft(imgPlayer, 10);

                        Debug.WriteLine(totalwidth);
                        totalwidth += imgPlayer.ActualWidth + 20;
                        Debug.WriteLine(totalwidth);
                    }
                    //Canvas.SetTop(imgPlayer, 110 + (double)(index / 3) * 150);
                    imgPlayer.ManipulationStarted += imgPlayer_ManipulationStarted;
                    imgPlayer.ManipulationDelta += imgPlayer_ManipulationDelta;
                    imgPlayer.ManipulationCompleted += imgPlayer_ManipulationCompleted;

                    Border border = new Border()
                    {
                        Name = "News" + (index + 1),
                        Padding = new Thickness(5),
                    };

                    NewsText = NewsText.Replace("&#x0a;", Environment.NewLine);
                    //Debug.WriteLine("Text: " + NewsText);
                    TextBlock textBlock = new TextBlock()
                    {
                        Name = "NewsT" + (index + 1),
                        Text = NewsText,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 16,
                        Foreground = new SolidColorBrush(Colors.Black)
                    };
                    Button Likebutton = new Button()
                    {
                        FontFamily = new FontFamily("Segoe MDL2 Assets"),
                        FontSize = 18,
                        Content = "\uE8E1", // Unicode character for the "Like" icon
                        Width = 50,
                        Height = 50,
                        Background = new SolidColorBrush(Colors.MediumPurple),
                        Foreground = new SolidColorBrush(Colors.LightCyan),
                        Name = "Like" + (index + 1)
                    };
                    Likebutton.Click += Likebutton_Click;
                    border.Child = textBlock;

                    mainCanvas.Children.Add(imgPlayer);
                    mainCanvas.Children.Add(border);
                    mainCanvas.Children.Add(Likebutton);
                    if ((index % 3) != 0)
                    {
                        imgPlayer.ImageOpened += (senderImage, args) =>
                        {
                            Canvas.SetLeft(border, (double)((index - 1) % 3) * prevImgPlayer.ActualWidth + (10 * (index % 3)));
                            Canvas.SetLeft(Likebutton, (double)((index - 1) % 3) * prevImgPlayer.ActualWidth + (10 * (index % 3)));
                            Canvas.SetTop(border, 110 + ((double)((index - 1) / 3) * 150) + imgPlayer.ActualHeight);
                            Canvas.SetTop(Likebutton, 110 + ((double)((index - 1) / 3) * 150) + imgPlayer.ActualHeight - 50);
                            //0 * prevWidth + 10

                        };
                    }
                    else
                    {
                        imgPlayer.ImageOpened += (senderImage, args) =>
                        {
                            Canvas.SetLeft(border, 10);
                            Canvas.SetLeft(Likebutton, 10);
                            Canvas.SetTop(border, 110 + ((double)((index) / 3) * 150) + imgPlayer.ActualHeight);
                            Canvas.SetTop(Likebutton, 110 + ((double)((index) / 3) * 150) + imgPlayer.ActualHeight - 50);
                        };
                    }
                    index++;
                    //Debug.WriteLine($"Added image and border: {imgPlayer.Name}, {border.Name}");

                    border.Background = new SolidColorBrush(Colors.MediumPurple);
                }

            }

        }
        
    }
}
