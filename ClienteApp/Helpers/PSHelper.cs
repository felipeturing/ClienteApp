using System;
using System.Diagnostics;
using ClienteApp.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClienteApp.Helpers
{
    public static class PowerShellHelper
    {
        public static async Task<string?> GetWorker()
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
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.GetWorker (Error): process.StandardError.ReadToEnd - {error}");
                    return null;
                }

                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.GetWorker: worker identificado {nombreEquipo}");
                return nombreEquipo;
            }
            catch (Exception ex)
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.GetWorker (Error): Catch - {ex.Message}");
                return null;
            }
        }

        public static async Task ProcessApps(Models.Data data)
        {
            if (data?.grupo?.apps == null || data.grupo.apps.Length == 0)
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.ProcessApps: No hay aplicaciones para procesar.");
                return;
            }

            // Unistall Apps
            // Update Worker Status
            Models.Data? existingData = await PersistenceHelper.Load<Models.Data>();
            await ApiHelper.UpdateWorkerStatus(data.worker?.name, Models.WorkerStatusApp.PENDING);
            bool uninstallOk = true;
            foreach (Models.App app in existingData?.grupo?.apps ?? [])
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.ProcessApps: Procesando aplicación para desinstalación: {app.name} (Versión: {app.version})", data.worker?.name);
                
                bool isDifferentGroup = existingData != null && existingData.grupo != null && existingData.grupo.name != data.grupo.name;
                bool isAppNotInNewData = data.grupo.apps.All(newApp =>
                    newApp.name != app.name ||
                    (newApp.name == app.name && newApp.version != app.version));

                if (isDifferentGroup || isAppNotInNewData)
                {
                    bool uninstallOkActual = await DesInstalarAplicacion(app, data.worker?.name);
                    uninstallOk = uninstallOk && uninstallOkActual;

                }
            }
            if (!uninstallOk)
            {
                await ApiHelper.UpdateWorkerStatus(data.worker?.name, Models.WorkerStatusApp.FAIL);
            }
            else
            {
                await ApiHelper.UpdateWorkerStatus(data.worker?.name, Models.WorkerStatusApp.OK);
            }
            /////////////

            
            // Install Apps
            // Update Worker Status
            await ApiHelper.UpdateWorkerStatus(data.worker?.name, Models.WorkerStatusApp.PENDING);
            bool installOk = true;
            foreach (Models.App app in data.grupo.apps)
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.ProcessApps: Procesando aplicación para instalación: {app.name} (Versión: {app.version})", data.worker?.name);
                
                bool install = true;
                bool isAppAlreadyInstalled = existingData != null && existingData.grupo != null && existingData.grupo.name == data.grupo.name &&
                                             existingData.grupo.apps != null && existingData.grupo.apps.Any(existingApp => existingApp.name == app.name && existingApp.version == app.version) == true;
                if (isAppAlreadyInstalled)
                {
                    install = false;
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.ProcessApps: La aplicación {app.name} ya se encuentra en la data.json (en teoría instalada).", data.worker?.name);
                }

                if (install)
                {
                    bool installOkActual = await InstalarAplicacion(app, data.worker?.name);
                    installOk = installOk && installOkActual;
                }
            }
            if (!installOk)
            {
                await ApiHelper.UpdateWorkerStatus(data.worker?.name, Models.WorkerStatusApp.FAIL);
            }
            else
            {
                await ApiHelper.UpdateWorkerStatus(data.worker?.name, Models.WorkerStatusApp.OK);
            }
            /////////////////////
        }

        private static async Task<bool> InstalarAplicacion(Models.App app, string? workerName)
        {
            if(workerName == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(app.name) || string.IsNullOrEmpty(app.version) || string.IsNullOrEmpty(app.path))
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.InstalarAplicacion: Uno o más parámetros (name, version, path) de la app son nulos", app.name);
                await ApiHelper.ReportAppError(workerName, "Uno o más parámetros (name, version, path) de la app son nulos", Models.ErrorType.INSTALACION, app.name);
                return false;
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
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.InstalarAplicacion: Archivo no encontrado en la carpeta temporal. Copiando hacia {tempFilePath}", app.name);
                    File.Copy(app.path, tempFilePath, true);
                }
                else
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.InstalarAplicacion: Archivo encontrado en la carpeta temporal: {tempFilePath}", app.name);
                }

                using Process process = new();
                process.StartInfo.FileName = "msiexec";
                process.StartInfo.Arguments = $"/i \"{tempFilePath}\" /quiet /norestart";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false; // Para redirigir la salida
                process.StartInfo.CreateNoWindow = true;   // No mostrar ventanas
                process.StartInfo.Verb = "runas";         // Ejecutar como administrador

                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.InstalarAplicacion: Instalando la aplicación", app.name);
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.InstalarAplicacion (Error): process.StandardError.ReadToEnd - {error}", app.name);
                    await ApiHelper.ReportAppError(workerName, $"ClienteApp.Helpers.PowerShellHelper.InstalarAplicacion (Error): process.StandardError.ReadToEnd - {error}", Models.ErrorType.INSTALACION, app.name);
                    return false;
                }
                else
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.InstalarAplicacion: Aplicación {app.name} instalada correctamente.", app.name);
                    return true;
                }
            }
            catch (Exception ex)
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.InstalarAplicacion (Error): Catch - {ex.Message}", app.name);
                await ApiHelper.ReportAppError(workerName, $"ClienteApp.Helpers.PowerShellHelper.InstalarAplicacion (Error): Catch - {ex.Message}", Models.ErrorType.INSTALACION, app.name);
                return false;
            }
        }

        private static async Task<bool> DesInstalarAplicacion(Models.App app, string? workerName)
        {
            if (workerName == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(app.name) || string.IsNullOrEmpty(app.version) || string.IsNullOrEmpty(app.path))
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.DesInstalarAplicacion: Uno o más parámetros (name, version, path) de la app son nulos", app.name);
                await ApiHelper.ReportAppError(workerName, "Uno o más parámetros (name, version, path) de la app son nulos", Models.ErrorType.DESINSTALACION, app.name);
                return false;
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
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.DesInstalarAplicacion: Archivo no encontrado en la carpeta temporal. Copiando hacia {tempFilePath}", app.name);
                    File.Copy(app.path, tempFilePath, true);
                }
                else
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.DesInstalarAplicacion: Archivo encontrado en la carpeta temporal: {tempFilePath}", app.name);
                }


                using Process process2 = new();
                process2.StartInfo.FileName = "msiexec";
                process2.StartInfo.Arguments = $"/x \"{tempFilePath}\" /quiet /norestart";
                process2.StartInfo.RedirectStandardOutput = true;
                process2.StartInfo.RedirectStandardError = true;
                process2.StartInfo.UseShellExecute = false; // Para redirigir la salida
                process2.StartInfo.Verb = "runas";         // Ejecutar como administrador

                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.DesInstalarAplicacion: Desinstalando la aplicación", app.name);
                process2.Start();

                string output = process2.StandardOutput.ReadToEnd();
                string error = process2.StandardError.ReadToEnd();

                process2.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.DesInstalarAplicacion (Error): process.StandardError.ReadToEnd - {error}", app.name);
                    await ApiHelper.ReportAppError(workerName, $"ClienteApp.Helpers.PowerShellHelper.DesInstalarAplicacion (Error): process.StandardError.ReadToEnd - {error}", Models.ErrorType.DESINSTALACION, app.name);
                    return false;
                }
                else
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.DesInstalarAplicacion: Aplicación {app.name} desinstalada correctamente.", app.name);
                    return true;
                }
            }
            catch (Exception ex)
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Helpers.PowerShellHelper.DesInstalarAplicacion (Error): Catch - {ex.Message}", app.name);
                await ApiHelper.ReportAppError(workerName, $"ClienteApp.Helpers.PowerShellHelper.DesInstalarAplicacion (Error): Catch - {ex.Message}", Models.ErrorType.DESINSTALACION, app.name);
                return false;
            }
        }
    }
}