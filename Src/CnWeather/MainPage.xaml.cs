using MiniJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace CnWeather
{
    public sealed partial class MainPage : Page
    {
        private string primaryKey = "230a5edfb7944cfd8a9125840252708";
        private string backupKey = "75d5be0163b3427198c133554252708";

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (!localSettings.Values.ContainsKey("SavedCity"))
            {
                Frame.Navigate(typeof(RegionPage));
            }
            else
            {
                string city = localSettings.Values["SavedCity"] as string;
                await FetchWeather(city);
                await FetchForecast(city);
                await FetchMood(city);
            }
        }

        private async void ChangeRegion_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Select Region",
                Content = "Please select a valid region from the list.",
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
            Frame.Navigate(typeof(RegionPage));
        }

        private async System.Threading.Tasks.Task FetchWeather(string city)
        {
            string result = await FetchWithFallback($"http://api.weatherapi.com/v1/current.json?key={primaryKey}&q={city}", 
                () => $"http://api.weatherapi.com/v1/current.json?key={backupKey}&q={city}");
            
            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    Dictionary<string, object> dictionary1 = Json.Deserialize(result) as Dictionary<string, object>;
                    Dictionary<string, object> dictionary2 = dictionary1["location"] as Dictionary<string, object>;
                    Dictionary<string, object> dictionary3 = dictionary1["current"] as Dictionary<string, object>;
                    Dictionary<string, object> dictionary4 = dictionary3["condition"] as Dictionary<string, object>;
                    string temp = dictionary3["temp_c"].ToString();
                    string condition = dictionary4["text"].ToString();
                    string humidity = dictionary3["humidity"].ToString();
                    string iconUrl = "http:" + dictionary4["icon"].ToString();
                    
                    CityText.Text = dictionary2["name"].ToString() + ", " + dictionary2["region"];
                    TempText.Text = "Temperature: " + temp + " °C";
                    ConditionText.Text = "Condition: " + condition;
                    HumidityText.Text = "Humidity: " + humidity + "%";
                    WeatherIcon.Source = new BitmapImage(new Uri(iconUrl, UriKind.Absolute));
                }
                catch
                {
                    CityText.Text = "Weather unavailable";
                    TempText.Text = "";
                    ConditionText.Text = "";
                    HumidityText.Text = "";
                    WeatherIcon.Source = null;
                }
            }
            else
            {
                CityText.Text = "Weather unavailable";
                TempText.Text = "";
                ConditionText.Text = "";
                HumidityText.Text = "";
                WeatherIcon.Source = null;
            }
        }

        private async System.Threading.Tasks.Task FetchForecast(string city)
        {
            string result = await FetchWithFallback($"http://api.weatherapi.com/v1/forecast.json?key={primaryKey}&q={city}&days=3",
                () => $"http://api.weatherapi.com/v1/forecast.json?key={backupKey}&q={city}&days=3}}");
            
            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    List<object> objectList = ((Json.Deserialize(result) as Dictionary<string, object>)["forecast"] as Dictionary<string, object>)["forecastday"] as List<object>;
                    for (int index = 0; index < 3; ++index)
                    {
                        Dictionary<string, object> dictionary1 = objectList[index] as Dictionary<string, object>;
                        string date = dictionary1["date"].ToString();
                        Dictionary<string, object> dictionary2 = dictionary1["day"] as Dictionary<string, object>;
                        Dictionary<string, object> dictionary3 = dictionary2["condition"] as Dictionary<string, object>;
                        string avgTemp = dictionary2["avgtemp_c"].ToString();
                        string condition = dictionary3["text"].ToString();
                        string iconUrl = "http:" + dictionary3["icon"].ToString();
                        
                        switch (index)
                        {
                            case 0:
                                ForecastDate1.Text = date;
                                ForecastTemp1.Text = "Avg Temp: " + avgTemp + " °C";
                                ForecastText1.Text = condition;
                                ForecastIcon1.Source = new BitmapImage(new Uri(iconUrl, UriKind.Absolute));
                                break;
                            case 1:
                                ForecastDate2.Text = date;
                                ForecastTemp2.Text = "Avg Temp: " + avgTemp + " °C";
                                ForecastText2.Text = condition;
                                ForecastIcon2.Source = new BitmapImage(new Uri(iconUrl, UriKind.Absolute));
                                break;
                            case 2:
                                ForecastDate3.Text = date;
                                ForecastTemp3.Text = "Avg Temp: " + avgTemp + " °C";
                                ForecastText3.Text = condition;
                                ForecastIcon3.Source = new BitmapImage(new Uri(iconUrl, UriKind.Absolute));
                                break;
                        }
                    }
                }
                catch
                {
                    ForecastDate1.Text = "Forecast unavailable";
                    ForecastTemp1.Text = "";
                    ForecastText1.Text = "";
                    ForecastIcon1.Source = null;
                    ForecastDate2.Text = "";
                    ForecastTemp2.Text = "";
                    ForecastText2.Text = "";
                    ForecastIcon2.Source = null;
                    ForecastDate3.Text = "";
                    ForecastTemp3.Text = "";
                    ForecastText3.Text = "";
                    ForecastIcon3.Source = null;
                }
            }
            else
            {
                ForecastDate1.Text = "Forecast unavailable";
                ForecastTemp1.Text = "";
                ForecastText1.Text = "";
                ForecastIcon1.Source = null;
                ForecastDate2.Text = "";
                ForecastTemp2.Text = "";
                ForecastText2.Text = "";
                ForecastIcon2.Source = null;
                ForecastDate3.Text = "";
                ForecastTemp3.Text = "";
                ForecastText3.Text = "";
                ForecastIcon3.Source = null;
            }
        }

        private async System.Threading.Tasks.Task FetchMood(string city)
        {
            string result = await FetchWithFallback($"http://api.weatherapi.com/v1/current.json?key={primaryKey}&q={city}", 
                () => $"http://api.weatherapi.com/v1/current.json?key={backupKey}&q={city}");
            
            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    string lower = (((Json.Deserialize(result) as Dictionary<string, object>)["current"] as Dictionary<string, object>)["condition"] as Dictionary<string, object>)["text"].ToString().ToLower();
                    string mood = "Enjoy your day :3";
                    if (lower.Contains("sun") || lower.Contains("clear"))
                        mood = "Sun's out — perfect for a walk ";
                    else if (lower.Contains("cloud"))
                        mood = "Cloudy skies? Cozy up with a book ";
                    else if (lower.Contains("rain"))
                        mood = "Rainy mood — time for tea and tunes ";
                    else if (lower.Contains("snow"))
                        mood = "Snowy vibes — grab a blanket ";
                    else if (lower.Contains("storm"))
                        mood = "Stormy feels — stay safe indoors ";
                    MoodOutput.Text = mood;
                }
                catch
                {
                    MoodOutput.Text = "Mood unavailable.";
                }
            }
            else
            {
                MoodOutput.Text = "Mood unavailable.";
            }
        }

        private async System.Threading.Tasks.Task<string> FetchWithFallback(string primaryUrl, Func<string> backupUrlFunc)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    string result = await client.GetStringAsync(primaryUrl);
                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
            }
            catch
            {
                // If primary fails, try backup
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    string result = await client.GetStringAsync(backupUrlFunc());
                    if (!string.IsNullOrEmpty(result))
                        return result;
                }
            }
            catch
            {
                // Both failed
            }

            return null;
        }
    }
}
