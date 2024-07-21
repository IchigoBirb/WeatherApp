using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Project2_222114X
{
    internal class Trivia
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

        public async static Task<Root> GetTrivia()
        {
            try
            {
                using (var http = new HttpClient())
                {
                    // Set User-Agent header to identify your application
                    http.DefaultRequestHeaders.Add("User-Agent", "VisualStudio2022");

                    var url = $"https://opentdb.com/api.php?amount=10&category=9&difficulty=medium&type=multiple\r\n";
                    var response = await http.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"HTTP error: {response.StatusCode} - {response.ReasonPhrase}");
                        return null;
                    }

                    var result = await response.Content.ReadAsStringAsync();

                    // Configure serializer settings to handle DateTime format
                    var settings = new DataContractJsonSerializerSettings
                    {
                        DateTimeFormat = new DateTimeFormat("yyyy-MM-ddTHH:mm:ssZ")
                    };

                    var serializer = new DataContractJsonSerializer(typeof(Root), settings);

                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                    {
                        var data = (Root)serializer.ReadObject(ms);
                        return data;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HTTP error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }



        [DataContract]
        public class Result
        {
            [DataMember]
            public string type { get; set; }

            [DataMember]
            public string difficulty { get; set; }

            [DataMember]
            public string category { get; set; }

            [DataMember]
            public string question { get; set; }

            [DataMember]
            public string correct_answer { get; set; }

            [DataMember]
            public List<string> incorrect_answers { get; set; }
        }

        [DataContract]
        public class Root
        {
            [DataMember]
            public int response_code { get; set; }

            [DataMember]
            public List<Result> results { get; set; }
        }


    }
}
