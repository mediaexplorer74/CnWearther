//using MiniJSON;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace CnWeatherAgent
{
    public sealed class WeatherAgent : IBackgroundTask
    {
        private string primaryKey = "230a5edfb7944cfd8a9125840252708";
        private string backupKey = "75d5be0163b3427198c133554252708";

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            
            try
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                string city = "Hong Kong";
                
                if (localSettings.Values.ContainsKey("SavedCity"))
                {
                    city = localSettings.Values["SavedCity"] as string;
                }

                string result = await FetchWithFallback($"http://api.weatherapi.com/v1/current.json?key={primaryKey}&q={city}",
                    () => $"http://api.weatherapi.com/v1/current.json?key={backupKey}&q={city}");

                if (!string.IsNullOrEmpty(result))
                {
                    UpdateTileFromJson(result, city);
                    ShowToast(city, result);
                }
                else
                {
                    UpdateTile("Weather unavailable");
                }
            }
            catch
            {
                UpdateTile("Weather unavailable");
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async Task<string> FetchWithFallback(string primaryUrl, Func<string> backupUrlFunc)
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

        private void ShowToast(string city, string json)
        {
            try
            {
                Dictionary<string, object> dictionary1 = (Json.Deserialize(json) as Dictionary<string, object>)["current"] as Dictionary<string, object>;
                Dictionary<string, object> dictionary2 = dictionary1["condition"] as Dictionary<string, object>;
                string temp = dictionary1["temp_c"].ToString();
                string emoji = GetEmoji(dictionary2["text"].ToString());

                string toastXml = $@"
                <toast>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>CnWeather</text>
                            <text>{city}: {temp}°C {emoji}</text>
                        </binding>
                    </visual>
                </toast>";

                XmlDocument toastDoc = new XmlDocument();
                toastDoc.LoadXml(toastXml);

                var toast = new ToastNotification(toastDoc);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }
            catch
            {
                // If toast fails, continue silently
            }
        }

        private void UpdateTileFromJson(string json, string city)
        {
            try
            {
                Dictionary<string, object> dictionary1 = (Json.Deserialize(json) as Dictionary<string, object>)["current"] as Dictionary<string, object>;
                Dictionary<string, object> dictionary2 = dictionary1["condition"] as Dictionary<string, object>;
                string temp = dictionary1["temp_c"].ToString();
                string emoji = GetEmoji(dictionary2["text"].ToString());
                
                string content = $"{city}\n{temp}°C {emoji}";
                UpdateTile(content);
            }
            catch
            {
                UpdateTile("Weather unavailable");
            }
        }

        private string GetEmoji(string condition)
        {
            condition = condition.ToLower();
            if (condition.Contains("sun") || condition.Contains("clear"))
                return "☀️";
            if (condition.Contains("cloud"))
                return "☁️";
            if (condition.Contains("rain"))
                return "🌧️";
            if (condition.Contains("snow"))
                return "❄️";
            return condition.Contains("storm") ? "⛈️" : "🌡️";
        }

        private void UpdateTile(string content)
        {
            try
            {
                string tileXml = $@"
                <tile>
                    <visual>
                        <binding template='TileSquare150x150Text01'>
                            <text id='1'>CnWeather</text>
                            <text id='2'>Live Weather</text>
                            <text id='3'>{content}</text>
                        </binding>
                        <binding template='TileWide310x150Text01'>
                            <text id='1'>CnWeather</text>
                            <text id='2'>Live Weather</text>
                            <text id='3'>{content}</text>
                        </binding>
                    </visual>
                </tile>";

                XmlDocument tileDoc = new XmlDocument();
                tileDoc.LoadXml(tileXml);

                var tileNotification = new TileNotification(tileDoc);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            }
            catch
            {
                // If tile update fails, continue silently
            }
        }
    }
}
