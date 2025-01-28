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

        public static async Task<Models.Data?> GetApps(string worker)
        {
            using var client = new HttpClient();

            try
            {
                string apiUrl = $"{backendUrl}/workers/{worker}";
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var data = System.Text.Json.JsonSerializer.Deserialize<Models.Data>(responseBody);
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

    }

}