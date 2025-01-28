using System.Text.Json;

namespace ClienteApp.Helpers
{
    public static class PersistenceHelper
    {
        private static readonly string BaseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClienteApp"
        );
        private static readonly string ruta = Path.Combine(BaseDirectory, "data.json");

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
                await File.WriteAllTextAsync(ruta, json);
                Console.WriteLine("Data guardada correctamente.");
                Console.WriteLine($"Ruta de la data: {ruta}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el payload actual: {ex.Message}");
            }
        }

        public static async Task<Models.Data?> Load<T>()
        {
            try
            {
                if (File.Exists(ruta))
                {
                    string json = await File.ReadAllTextAsync(ruta);
                    Models.Data? data = JsonSerializer.Deserialize<Models.Data>(json);
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

        public static string GetDirectorioDeApps()
        {
            string carpetaDestino = @"C:\AppsSistemas";
            if (!Directory.Exists(carpetaDestino))
            {
                Directory.CreateDirectory(carpetaDestino);
                Console.WriteLine($"Carpeta creada en: {carpetaDestino}");
            }
            else
            {
                Console.WriteLine($"La carpeta ya existe en: {carpetaDestino}");
            }

            return carpetaDestino;
        }

    }
}
