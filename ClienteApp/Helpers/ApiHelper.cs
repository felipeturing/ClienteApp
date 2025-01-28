using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClienteApp.Helpers
{
    public static class ApiHelper
    {
        private const string backendUrl = $"http://localhost:3004";
        public static async Task Liveness(string worker)
        {
            using var client = new HttpClient();
            try
            {
                string apiUrl = $"{backendUrl}/workers/liveness/{worker}";
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    await PersistenceHelper.WriteLog("ClienteApp.Helpers.ApiHelper.Liveness: true", worker);
                }
                else
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.Liveness (Error): Código de estado HTTP {response.StatusCode}", worker);
                }
            }
            catch (Exception ex)
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.Liveness (Error): Excepción al enviar datos a la API: {ex.Message}", worker);
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
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.GetApps: true {responseBody}", worker);
                    return data;
                }
                else
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.GetApps (Error): Código de estado HTTP {response.StatusCode}", worker);
                    return null;
                }
            }
            catch (Exception ex)
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.GetApps (Error): Excepción al consultar aplicaciones en la API: {ex.Message}", worker);
                return null;
            }
        }

    }

}