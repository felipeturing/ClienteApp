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
                string nombreEquipo = process.StandardOutput.ReadToEnd().Trim();
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Procesando aplicación para desinstalación: {app.name} (Versión: {app.version})");
                Console.ResetColor();
                bool isDifferentGroup = existingData != null && existingData.grupo != null && existingData.grupo.name != data.grupo.name;
                bool isAppNotInNewData = data.grupo.apps.All(newApp =>
                    newApp.name != app.name ||
                    (newApp.name == app.name && newApp.version != app.version));

                if (isDifferentGroup || isAppNotInNewData)
                {
                    DesInstalarAplicacion(app);
                }
            }

            // Install Apps
            foreach (Models.App app in data.grupo.apps)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Procesando aplicación para instalación: {app.name} (Versión: {app.version})");
                Console.ResetColor();
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

            string tempFolder = Path.Combine(Path.GetTempPath(), "ClientApp");
            string tempFilePath = Path.Combine(tempFolder, Path.GetFileName(app.path));
            try
            {
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                if (!File.Exists(tempFilePath))
                {
                    Console.WriteLine($"Archivo no encontrado en la carpeta temporal. Copiando...");
                    File.Copy(app.path, tempFilePath, true);
                }
                else
                {
                    Console.WriteLine($"Archivo encontrado en la carpeta temporal: {tempFilePath}");
                }

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
        }

        private static void DesInstalarAplicacion(Models.App app)
        {
            if (string.IsNullOrEmpty(app.name) || string.IsNullOrEmpty(app.version) || string.IsNullOrEmpty(app.path))
            {
                return;
            }

            string tempFolder = Path.Combine(Path.GetTempPath(), "ClientApp");
            string tempFilePath = Path.Combine(tempFolder, Path.GetFileName(app.path));

            try
            {
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                if (!File.Exists(tempFilePath))
                {
                    Console.WriteLine($"Archivo no encontrado en la carpeta temporal. Copiando desde:");
                    File.Copy(app.path, tempFilePath, true);
                }
                else
                {
                    Console.WriteLine($"Archivo encontrado en la carpeta temporal: {tempFilePath}");
                }


                using Process process2 = new();
                process2.StartInfo.FileName = "msiexec";
                process2.StartInfo.Arguments = $"/x \"{tempFilePath}\" /quiet /norestart";
                process2.StartInfo.RedirectStandardOutput = true;
                process2.StartInfo.RedirectStandardError = true;
                process2.StartInfo.UseShellExecute = false; // Para redirigir la salida
                process2.StartInfo.Verb = "runas";         // Ejecutar como administrador

                Console.WriteLine($"Desinstalando la aplicación {app.name}...");
                process2.Start();

                string output = process2.StandardOutput.ReadToEnd();
                string error = process2.StandardError.ReadToEnd();

                process2.WaitForExit();

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
        }
    }
}