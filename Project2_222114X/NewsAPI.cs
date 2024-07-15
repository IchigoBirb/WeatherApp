using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Project2_222114X
{
    internal class NewsAPI
    {
        private const string APIKEY2 = "APIKEY";

        public async static Task<Root> GetNews()
        {
            try
            {
                using (var http = new HttpClient())
                {
                    // Set User-Agent header to identify your application
                    http.DefaultRequestHeaders.Add("User-Agent", "VisualStudio2022");

                    var url = $"https://newsapi.org/v2/top-headlines?country=US&apiKey={APIKEY2}";
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
        public class Article
        {
            [DataMember(Name = "source")]
            public Source Source { get; set; }

            [DataMember(Name = "author")]
            public string Author { get; set; }

            [DataMember(Name = "title")]
            public string Title { get; set; }

            [DataMember(Name = "description")]
            public string Description { get; set; }

            [DataMember(Name = "url")]
            public string Url { get; set; }

            [DataMember(Name = "urlToImage")]
            public string UrlToImage { get; set; }

            [DataMember(Name = "publishedAt")]
            public DateTime PublishedAt { get; set; }

            [DataMember(Name = "content")]
            public string Content { get; set; }
        }

        [DataContract]
        public class Root
        {
            [DataMember(Name = "status")]
            public string Status { get; set; }

            [DataMember(Name = "totalResults")]
            public int TotalResults { get; set; }

            [DataMember(Name = "articles")]
            public List<Article> Articles { get; set; }
        }

        [DataContract]
        public class Source
        {
            [DataMember(Name = "id")]
            public string Id { get; set; }

            [DataMember(Name = "name")]
            public string Name { get; set; }
        }
    }
}
