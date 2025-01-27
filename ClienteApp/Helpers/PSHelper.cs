using System;
using System.Diagnostics;

namespace ClienteApp.Helpers
{
    public static class PowerShellHelper
    {
        public static string? getWorker()
        {
            using Process process = new();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = "-Command \"hostname\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
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

        public static async Task ProcessApps(ApiHelper.Data data)
        {
            if (data?.apps == null || data.apps.Length == 0)
            {
                Console.WriteLine("No hay aplicaciones para procesar.");
                return;
            }

            ApiHelper.Data? existingData = await PersistenceHelper.Load<ApiHelper.Data>();

            // Install
            foreach (ApiHelper.App app in data.apps)
            {
                Console.WriteLine($"Procesando aplicación: {app.name} (Versión: {app.version})");
                bool install = true;
                if (existingData != null && 
                    existingData.group == data.group && 
                    existingData.apps != null && existingData.apps.Any(existingApp => existingApp.name == app.name && existingApp.version == app.version ))
                {
                    install = false;
                }

                if (install)
                {
                    Console.WriteLine($"La aplicación {app.name} no está instalada.");
                    InstalarAplicacion(app.name);
                    //bool estaInstalada = VerificarAplicacionInstalada(app.name);
                    //if (!estaInstalada)
                    //{
                    //}
                }
            }
            // Unistall
        }

        private static bool VerificarAplicacionInstalada(string nombreApp)
        {
            using Process process = new Process();
            process.StartInfo.FileName = "powershell.exe";
            process.StartInfo.Arguments = $"-Command \"Get-WmiObject -Class Win32_Product | Where-Object {{ $_.Name -eq '{nombreApp}' }}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine($"Error al verificar la aplicación {nombreApp}: {error}");
                    return false;
                }

                // Si hay salida, la aplicación está instalada
                return !string.IsNullOrWhiteSpace(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al verificar la aplicación {nombreApp}: {ex.Message}");
                return false;
            }
        }

        private static void InstalarAplicacion(string? nombreApp)
        {
            if (string.IsNullOrEmpty(nombreApp))
            {
                return;
            }

            string rutaCompletaMsi = Path.Combine(@"\\server-nube\sistemas\Aplicaciones MSI", $"{nombreApp}");
            try
            {
                using Process process = new Process();
                process.StartInfo.FileName = "msiexec"; // Usar msiexec directamente
                process.StartInfo.Arguments = $"/i \"{rutaCompletaMsi}\" /quiet"; ;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false; // Para redirigir la salida
                process.StartInfo.CreateNoWindow = true;   // No mostrar ventanas
                process.StartInfo.Verb = "runas";         // Ejecutar como administrador

                Console.WriteLine($"Instalando la aplicación {nombreApp} desde {rutaCompletaMsi}...");
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine($"Error al instalar la aplicación {nombreApp}: {error}");
                }
                else
                {
                    Console.WriteLine($"Aplicación {nombreApp} instalada correctamente.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al instalar la aplicación {nombreApp}: {ex.Message}");
            }
        }
    }
}