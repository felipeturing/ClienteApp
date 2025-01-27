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
        //private static readonly string RutaPayloadAnterior = Path.Combine(BaseDirectory, "data_anterior.json");

        static PersistenceHelper()
        {
            // Crear la carpeta si no existe
            if (!Directory.Exists(BaseDirectory))
            {
                Directory.CreateDirectory(BaseDirectory);
            }
        }

        // Guardar el payload actual en un archivo JSON
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
                    return default;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar los datos: {ex.Message}");
                return default;
            }
        }

        // Comparar datos entrantes con los datos existentes
        public static async Task<(int Codigo, ApiHelper.Data? Data)> Compare(ApiHelper.Data incomingData)
        {
            try
            {
                var existingData = await Load<ApiHelper.Data>();
                if (existingData == null)
                {
                    Console.WriteLine("No hay datos existentes para comparar.");
                    return (0, incomingData); // Código 0: No hay datos para comparar
                }

                var incomingJson = JsonSerializer.Serialize(incomingData);
                var existingJson = JsonSerializer.Serialize(existingData);

                var incomingGroup = JsonSerializer.Deserialize<ApiHelper.Data>(incomingJson)?.group;
                var existingGroup = existingData.group;

                if (incomingGroup != existingGroup)
                {
                    Console.WriteLine("Diferencia detectada en el grupo.");
                    return (1, incomingData); // Código 1: Diferencia en el grupo
                }

                var incomingApps = JsonSerializer.Deserialize<ApiHelper.Data>(incomingJson)?.apps;
                var existingApps = existingData.apps;

                if (!CompareApps(incomingApps, existingApps))
                {
                    Console.WriteLine("Diferencia detectada en las aplicaciones.");
                    return (2, incomingData); // Código 2: Diferencia en las aplicaciones
                }

                Console.WriteLine("No se encontraron diferencias.");
                return (0, null); // Código 0: No hay diferencias
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al comparar los datos: {ex.Message}");
                return (-1, null); // Código -1: Error en la comparación
            }
        }

        // Método auxiliar para comparar listas de aplicaciones
        private static bool CompareApps(dynamic? incomingApps, dynamic? existingApps)
        {
            try
            {
                var incomingAppsList = JsonSerializer.Serialize(incomingApps);
                var existingAppsList = JsonSerializer.Serialize(existingApps);

                return incomingAppsList == existingAppsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al comparar las aplicaciones: {ex.Message}");
                return false;
            }
        }
    }
}
