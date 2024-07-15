using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Project2_222114X
{
    internal class OpenWeather
    {
        public async static Task<RootObject> GetWeather()
        {
            var APIKEY = "APIKEY";
            var http = new HttpClient();
            var response = await http.GetAsync($"http://api.openweathermap.org/data/2.5/forecast?lat=1.2897&lon=103.8501&APPID={APIKEY}");

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(RootObject));

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(result)))
            {
                var data = (RootObject)serializer.ReadObject(ms);
                return data;
            }
        }

        [DataContract]
        public class RootObject
        {
            [DataMember]
            public string cod { get; set; }

            [DataMember]
            public int message { get; set; }

            [DataMember]
            public int cnt { get; set; }

            [DataMember]
            public List<ListItem> list { get; set; }

            [DataMember]
            public City city { get; set; }

            public override string ToString() //Debug statment ToString() default just returns an object
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"OpenWeather API Response:");
                sb.AppendLine($"--------------------------");
                sb.AppendLine($"Cod: {cod}");
                sb.AppendLine($"Message: {message}");
                sb.AppendLine($"Count: {cnt}");

                if (city != null)
                {
                    sb.AppendLine($"City: {city.name}, {city.country}");
                    sb.AppendLine($"Population: {city.population}");
                    sb.AppendLine($"Timezone: {city.timezone}");
                    sb.AppendLine($"Sunrise: {UnixTimeStampToDateTime(city.sunrise).ToLocalTime()}");
                    sb.AppendLine($"Sunset: {UnixTimeStampToDateTime(city.sunset).ToLocalTime()}");
                }

                if (list != null && list.Count > 0)
                {
                    sb.AppendLine($"Weather Forecasts:");

                    foreach (var item in list)
                    {
                        sb.AppendLine($"  Date/Time: {item.dt_txt}");
                        sb.AppendLine($"  Temperature: {item.main.temp - 273.15:F2}°C");
                        sb.AppendLine($"  Feels Like: {item.main.feels_like - 273.15:F2}°C");
                        sb.AppendLine($"  Min Temperature: {item.main.temp_min - 273.15:F2}°C");
                        sb.AppendLine($"  Max Temperature: {item.main.temp_max - 273.15:F2}°C");
                        sb.AppendLine($"  Pressure: {item.main.pressure} hPa");
                        sb.AppendLine($"  Humidity: {item.main.humidity}%");
                        sb.AppendLine($"  Weather Description: {item.weather[0].description}");
                        sb.AppendLine($"  Cloud Coverage: {item.clouds.all}%");
                        sb.AppendLine($"  Wind Speed: {item.wind.speed} m/s");
                        sb.AppendLine($"  Wind Direction: {item.wind.deg}°");
                        sb.AppendLine($"  Rain Volume (last 3 hours): {item.rain?.volume_3h ?? 0} mm");

                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            }

            // Helper method to convert Unix timestamp to DateTime
            private DateTime UnixTimeStampToDateTime(int unixTimeStamp)
            {
                // Unix timestamp is seconds past epoch
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
                return dtDateTime;
            }
        }

        [DataContract]
        public class ListItem
        {
            [DataMember]
            public int dt { get; set; }

            [DataMember]
            public Main main { get; set; }

            [DataMember]
            public List<Weather> weather { get; set; }

            [DataMember]
            public Clouds clouds { get; set; }

            [DataMember]
            public Wind wind { get; set; }

            [DataMember]
            public int visibility { get; set; }

            [DataMember]
            public double pop { get; set; }

            [DataMember]
            public Sys sys { get; set; }

            [DataMember]
            public string dt_txt { get; set; }

            [DataMember(EmitDefaultValue = false)]
            public Rain rain { get; set; } // Optional field
        }

        [DataContract]
        public class Main
        {
            [DataMember]
            public double temp { get; set; }

            [DataMember]
            public double feels_like { get; set; }

            [DataMember]
            public double temp_min { get; set; }

            [DataMember]
            public double temp_max { get; set; }

            [DataMember]
            public int pressure { get; set; }

            [DataMember]
            public int sea_level { get; set; }

            [DataMember]
            public int grnd_level { get; set; }

            [DataMember]
            public int humidity { get; set; }

            [DataMember]
            public double temp_kf { get; set; }
        }

        [DataContract]
        public class Weather
        {
            [DataMember]
            public int id { get; set; }

            [DataMember]
            public string main { get; set; }

            [DataMember]
            public string description { get; set; }

            [DataMember]
            public string icon { get; set; }
        }

        [DataContract]
        public class Clouds
        {
            [DataMember]
            public int all { get; set; }
        }

        [DataContract]
        public class Wind
        {
            [DataMember]
            public double speed { get; set; }

            [DataMember]
            public int deg { get; set; }

            [DataMember]
            public double gust { get; set; }
        }

        [DataContract]
        public class Sys
        {
            [DataMember]
            public string pod { get; set; }
        }

        [DataContract]
        public class Rain
        {
            [DataMember(Name = "3h")]
            public double volume_3h { get; set; }
        }

        [DataContract]
        public class City
        {
            [DataMember]
            public int id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public Coord coord { get; set; }

            [DataMember]
            public string country { get; set; }

            [DataMember]
            public int population { get; set; }

            [DataMember]
            public int timezone { get; set; }

            [DataMember]
            public int sunrise { get; set; }

            [DataMember]
            public int sunset { get; set; }
        }

        [DataContract]
        public class Coord
        {
            [DataMember]
            public double lat { get; set; }

            [DataMember]
            public double lon { get; set; }
        }
    }
}
