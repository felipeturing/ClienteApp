using System.Text.Json;

namespace ClienteApp.Helpers
{
    public static class PersistenceHelper
    {
        private static readonly string BaseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClienteApp"
        );
        private static readonly string RutaPayloadActual = Path.Combine(BaseDirectory, "data.json");

        static PersistenceHelper()
        {
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
            }
        }

        public static async Task Save(object? data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data);
                await File.WriteAllTextAsync(RutaPayloadActual, json);
                Console.WriteLine("Payload actual guardado correctamente.");
                Console.WriteLine($"Ruta del archivo actual: {RutaPayloadActual}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el payload actual: {ex.Message}");
            }
        }

        public static async Task<ApiHelper.Data?> Load<T>()
        {
            try
            {
                if (File.Exists(RutaPayloadActual))
                {
                    string json = await File.ReadAllTextAsync(RutaPayloadActual);
                    ApiHelper.Data? data = JsonSerializer.Deserialize<ApiHelper.Data>(json);
                    Console.WriteLine("Datos cargados correctamente.");
                    return data;
                }
                else
                {
                    Console.WriteLine("El archivo data.json no existe.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar los datos: {ex.Message}");
                return null;
            }
        }

    }
}
