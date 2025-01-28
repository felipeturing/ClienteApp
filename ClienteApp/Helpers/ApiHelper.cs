using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClienteApp.Models;

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
                        await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.GetApps: true", worker);
                        var data = System.Text.Json.JsonSerializer.Deserialize<Models.Data>(responseBody);
                        if (data?.worker != null || data?.worker?.name != null)
                        {
                            await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.GetApps: Datos serializados correctamente", worker);
                        }
                        else
                        {
                            await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.GetApps: El objeto 'worker o worker.name' está vacío o nulo en los datos recibidos - {responseBody}.");
                            return null;
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

        public static async Task UpdateWorkerStatus(string? workername, Models.WorkerStatusApp? status)
        {
            if(status == null || workername == null)
            {
                await PersistenceHelper.WriteLog("ClienteApp.Helpers.ApiHelper.UpdateWorkerStatus (Error): Uno o más parámetros son nulos", workername);
                return;
            }

            using var client = new HttpClient();
            try
            {
                string apiUrl = $"{backendUrl}/workers/{workername}/status/{status}";
                var response = await client.PutAsync(apiUrl, null); // sin cuerpo

                if (response.IsSuccessStatusCode)
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.UpdateWorkerStatus: Estado actualizado correctamente: {status}", workername);
                }
                else
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.UpdateWorkerStatus (Error): Código de estado HTTP {response.StatusCode}", workername);
                }
            }
            catch (Exception ex)
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.UpdateWorkerStatus (Error): Excepción al enviar datos a la API: {ex.Message}", workername);
            }
        }

        public static async Task ReportAppError(string? workerName, string? error, Models.ErrorType? tipo, string? appName)
        {
            if(workerName == null || error == null || tipo == null || appName == null)
            {
                await PersistenceHelper.WriteLog("ClienteApp.Helpers.ApiHelper.ReportAppError (Error): Uno o más parámetros son nulos", workerName);
                return;
            }

            using var client = new HttpClient();
            try
            {
                string apiUrl = $"{backendUrl}/apps/error";
                var payload = new
                {
                    workerName,
                    error,
                    tipo,
                    appName,
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.ReportAppError: Error reportado correctamente", workerName);
                }
                else
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.ReportAppError (Error): Código de estado HTTP {response.StatusCode}", workerName);
                }
            }
            catch (Exception ex)
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.ApiHelper.ReportAppError (Error): Excepción al enviar error a la API: {ex.Message}", workerName);
            }
        }
    }
}