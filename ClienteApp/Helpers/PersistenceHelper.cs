using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ClienteApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClienteApp.Helpers
{
    public static class PersistenceHelper
    {
        private static readonly string BaseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClienteApp"
        );
        private static readonly string dataRuta = Path.Combine(BaseDirectory, "data.json");
        private static readonly string logRuta = Path.Combine(BaseDirectory, "clienteapp.log");

        static PersistenceHelper()
        {
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
            }
        }

        public static async Task Save(Models.Data? data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data);
                await File.WriteAllTextAsync(dataRuta, json);
                await WriteLog($"ClienteApp.Helpers.PersistenceHelper.Save: Data guardada correctamente en {dataRuta} ", data?.worker?.name);
            }
            catch (Exception ex)
            {
                await WriteLog($"ClienteApp.Helpers.PersistenceHelper.Save (Error): Error al guardar la data actual: {ex.Message}", data?.worker?.name);
            }
        }

        public static async Task<Models.Data?> Load<T>()
        {
            try
            {
                if (File.Exists(dataRuta))
                {
                    string json = await File.ReadAllTextAsync(dataRuta);
                    Models.Data? data = JsonSerializer.Deserialize<Models.Data>(json);
                    await WriteLog($"ClienteApp.Helpers.PersistenceHelper.Load: Datos cargados correctamente");
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                await WriteLog($"ClienteApp.Helpers.PersistenceHelper.Load (Error): Error al cargar la data actual: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Escribe un mensaje de log en el archivo clienteapp.log con la fecha, hora, y el nombre del host.
        /// </summary>
        /// <param name="message">Mensaje del log.</param>
        /// <param name="hostName">Nombre del host.</param>
        public static async Task WriteLog(string message, string? worker = "worker")
        {
            try
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {worker ?? "worker"} | {message}{Environment.NewLine}";
                await File.AppendAllTextAsync(logRuta, logMessage);
            }
            catch (Exception)
            {   
                return;
            }
        }

    }
}
