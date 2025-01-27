using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClienteApp.Helpers
{
    public static class ApiHelper
    {
        private const string backendUrl = $"http://192.168.20.199:3004";
        public static async Task Liveness(string worker)
        {
            using var client = new HttpClient();
            try
            {
                string apiUrl = $"{backendUrl}/workers/liveness/{worker}";
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Liveness: true");
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
                string apiUrl = $"{backendUrl}/workers/{worker}";
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
            public Worker? worker { get; set; }
            public Grupo? grupo { get; set; }
        }

        public class Grupo
        {
            public string? name { get; set; }
            public string? description { get; set; }
            public App[]? apps { get; set; }
        }

        public class Worker
        {
            public string? name { get; set; }
            public string? status { get; set; }
            public string? statusApp { get; set; }
        }

        public class App
        {
            public string? name { get; set; }
            public string? version{ get; set; }
            public string? path{ get; set; }
        }

    }

}