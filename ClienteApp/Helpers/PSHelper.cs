using System;
using System.Diagnostics;

namespace ClienteApp.Helpers
{
    public static class PowerShellHelper
    {
        public static string? GetWorker()
        {
            using Process process = new();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "-Command \"hostname\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                process.Start();
                string nombreEquipo = process.StandardOutput.ReadToEnd().Trim(); // Eliminar espacios en blanco
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine("\nErrores al obtener el nombre del equipo:");
                    Console.WriteLine(error);
                    return null;
                }

                return nombreEquipo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al ejecutar PowerShell: {ex.Message}");
                return null;
            }
        }

        public static async Task ProcessApps(Models.Data data)
        {
            if (data?.grupo?.apps == null || data.grupo.apps.Length == 0)
            {
                Console.WriteLine("No hay aplicaciones para procesar.");
                return;
            }

            Models.Data? existingData = await PersistenceHelper.Load<Models.Data>();

            //Unistall Apps
            foreach (Models.App app in existingData?.grupo?.apps ?? [])
            {
                Console.WriteLine($"Procesando aplicación para desinstalación: {app.name} (Versión: {app.version})");
                bool isDifferentGroup = existingData != null && existingData.grupo != null && existingData.grupo.name != data.grupo.name;
                bool isAppNotInNewData = data.grupo.apps.All(newApp =>
                    newApp.name != app.name ||
                    (newApp.name == app.name && newApp.version != app.version));

                if (isDifferentGroup || isAppNotInNewData)
                {
                    DesinstalarAplicacion(app);
                }
            }

            // Install Apps
            foreach (Models.App app in data.grupo.apps)
            {
                Console.WriteLine($"Procesando aplicación para instalación: {app.name} (Versión: {app.version})");
                bool install = true;
                bool isAppAlreadyInstalled = existingData != null && existingData.grupo != null && existingData.grupo.name == data.grupo.name &&
                                             existingData.grupo.apps != null && existingData.grupo.apps.Any(existingApp => existingApp.name == app.name && existingApp.version == app.version) == true;
                if (isAppAlreadyInstalled)
                {
                    install = false;
                    Console.WriteLine($"La aplicación {app.name} ya se encuentra en la data.");
                }

                if (install)
                {
                    InstalarAplicacion(app);
                }
            }
        }

        private static void InstalarAplicacion(Models.App app)
        {
            if (string.IsNullOrEmpty(app.name) || string.IsNullOrEmpty(app.version) || string.IsNullOrEmpty(app.path))
            {
                return;
            }

            string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(app.path));

            try
            {
                File.Copy(app.path, tempFilePath, true);

                using Process process = new();
                process.StartInfo.FileName = "msiexec";
                process.StartInfo.Arguments = $"/i \"{tempFilePath}\" /quiet /norestart";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false; // Para redirigir la salida
                process.StartInfo.CreateNoWindow = true;   // No mostrar ventanas
                process.StartInfo.Verb = "runas";         // Ejecutar como administrador

                Console.WriteLine($"Instalando la aplicación {app.name}...");
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine($"Error al instalar la aplicación {app.name}: {error}");
                }
                else
                {
                    Console.WriteLine($"Aplicación {app.name} instalada correctamente.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al instalar la aplicación {app.name}: {ex.Message}");
            }
            finally
            {
                // Intentar eliminar el archivo temporal después de la instalación
                try
                {
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar el archivo temporal: {ex.Message}");
                }
            }
        }

        private static void DesinstalarAplicacion(Models.App app)
        {
            if (string.IsNullOrEmpty(app.name) || string.IsNullOrEmpty(app.version) || string.IsNullOrEmpty(app.path))
            {
                return;
            }
            string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(app.path));
            try
            {
                // Copiar el archivo desde la carpeta compartida a la ubicación temporal
                File.Copy(app.path, tempFilePath, true);

                using Process process = new();
                process.StartInfo.FileName = "msiexec";
                process.StartInfo.Arguments = $"/x \"{tempFilePath}\" /quiet /norestart";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false; // Para redirigir la salida
                process.StartInfo.CreateNoWindow = true;   // No mostrar ventanas
                process.StartInfo.Verb = "runas";         // Ejecutar como administrador

                Console.WriteLine($"Desinstalando la aplicación {app.name}...");
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine($"Error al desinstalar la aplicación {app.name}: {error}");
                }
                else
                {
                    Console.WriteLine($"Aplicación {app.name} desinstalada correctamente.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al desinstalar la aplicación {app.name}: {ex.Message}");
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                        Console.WriteLine("Archivo temporal eliminado correctamente.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar el archivo temporal: {ex.Message}");
                }
            }
        }
    }
}