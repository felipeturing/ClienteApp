using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClienteApp.Helpers
{
    public static class ApiHelper
    {
        private const string backendUrl = $"http://192.168.20.69:3004";
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

                    if (string.IsNullOrWhiteSpace(responseBody))
                    {
                        await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.GetApps:Respuesta vacía del servidor", worker);
                        return null;
                    }

                    try {
                        await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.GetApps: true {responseBody}", worker);
                        var data = System.Text.Json.JsonSerializer.Deserialize<Models.Data>(responseBody);
                        if (data?.worker != null)
                        {
                            await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.GetApps: Datos serializados correctamente: {responseBody}", worker);
                        }
                        else
                        {
                            await PersistenceHelper.WriteLog("ClienteApp.Helpers.ApiHelper.GetApps: El objeto 'worker' está vacío o nulo en los datos recibidos.", worker);
                        }
                        return data;
                    } catch (JsonException ex)
                    {
                        await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.GetApps: Error en la deserialización: {ex.Message}. Respuesta: {responseBody}", worker);
                        return null;
                    }

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