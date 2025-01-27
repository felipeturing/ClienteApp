using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClienteApp.Helpers
{
    public static class ApiHelper
    {
        public static async Task Liveness(string worker)
        {
            using var client = new HttpClient();
            try
            {
                string apiUrl = "http://localhost:3002/certificados/test";
                var body = new { host = worker };
                string jsonBody = System.Text.Json.JsonSerializer.Serialize(body);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("liveness: " + responseBody);
                }
                else
                {
                    Console.WriteLine($"Error: Código de estado HTTP {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al enviar datos a la API: {ex.Message}");
            }
        }

        public static async Task<Data?> GetApps(string worker)
        {
            using var client = new HttpClient();

            try
            {
                string apiUrl = $"http://localhost:3002/certificados/test/{worker}";
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonSerializer.Deserialize<Data>(responseBody);
                    return data;
                }
                else
                {
                    Console.WriteLine($"Error: Código de estado HTTP {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al consultar aplicaciones en la API: {ex.Message}");
                return null;
            }
        }

        public class Data
        {
            public string? host { get; set; }
            public string? group { get; set; }
            public App[]? apps { get; set; }
        }

        public class App
        {
            public string? name { get; set; }
            public string? version{ get; set; }
        }

    }

}