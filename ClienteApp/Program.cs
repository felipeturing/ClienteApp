using System;
using System.CodeDom;
using System.Drawing;
using System.Runtime.Versioning;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClienteApp.Helpers;
using ClienteApp.Models;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;

namespace ClienteApp
{
    class Program : Form
    {
        public static System.Threading.Timer? timerLiveness;
        public static System.Threading.Timer? timerRefreshApps;
        private static NotifyIcon? notifyIcon;

        // Se asegura que el código funcione correctamente como una aplicación Windows Forms y que solo se debe ejecutar en Windows 7 o superior**
        [STAThread]
        [SupportedOSPlatform("windows6.1")]
        static void Main(string[] args)
        {
            try
            {
                System.Threading.Thread.Sleep(5000);

                //TaskSchedulerHelper.RegisterTask();
                AddToStartup();

                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

                notifyIcon = new NotifyIcon
                {
                    Visible = true,
                    Text = "AppClient - Controlado por Sistemas"
                };

                string iconPath = Path.Combine(AppContext.BaseDirectory, "logo.ico");
                if (File.Exists(iconPath))
                {
                    notifyIcon.Icon = new Icon(iconPath);
                }
                else
                {
                   throw new Exception($"No se encontró el icono en: {iconPath}");
                }

                var contextMenuStrip = new ContextMenuStrip();
                var openMenuItem = new ToolStripMenuItem("Abrir", null, (s, e) =>
                {
                    var title = "Información de la Aplicación";
                    var message = "⚙ Aplicación en ejecución ⚙\n\n" +
                                  "📌 Versión: v1.0.0-beta\n" +
                                  "👨‍💻 Desarrollado por Sistemas 2025\n\n" +
                                  "✅ Estado: Activo y monitoreando.";
                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                });

                var exitMenuItem = new ToolStripMenuItem("Salir", null, (s, e) => System.Windows.Forms.Application.Exit());

                contextMenuStrip.Items.Add(openMenuItem);
                contextMenuStrip.Items.Add(exitMenuItem);

                notifyIcon.ContextMenuStrip = contextMenuStrip;

                Utils.ConsoleHelper.WriteColoredMessage("ClienteApp.Program.Main: Iniciando...", ConsoleColor.Green);

                timerLiveness = new System.Threading.Timer(async _ => await Liveness(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
                timerRefreshApps = new System.Threading.Timer(async _ => await Apps(), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));

                System.Windows.Forms.Application.Run();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar la aplicación: \n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private static async Task Apps()
        {
            string worker = await PowerShellHelper.GetWorker();
            if (!string.IsNullOrEmpty(worker))
            {
                Models.Data? data = await ApiHelper.GetApps(worker);
                if (data != null)
                {
                    await PersistenceHelper.WriteLog($"ClienteApp.Program.Apps (Error): Esperando 1min.");
                    System.Threading.Thread.Sleep(60000);

                    await PowerShellHelper.ProcessApps(data);
                    await PersistenceHelper.Save(data);
                }
            }
            else
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Program.Apps (Error): No se pudo obtener el nombre del worker");
            }
        }

        private static async Task Liveness()
        {
            string worker = await PowerShellHelper.GetWorker();
            if (!string.IsNullOrEmpty(worker))
            {
                await ApiHelper.Liveness(worker);
            }
            else
            {
                await PersistenceHelper.WriteLog($"ClienteApp.Program.Liveness (Error): No se pudo obtener el nombre del worker");
            }
        }

        private static void AddToStartup()
        {
            // Verificar si la aplicación se está ejecutando como administrador
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show("La aplicación debe ejecutarse como administrador para agregarse al inicio del sistema.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception($"La aplicación debe ejecutarse como administrador para agregarse al inicio del sistema.");
            }

            string exePath = Path.Combine(AppContext.BaseDirectory, "ClienteApp.exe");

            if (!File.Exists(exePath))
                throw new Exception($"El ClienteApp.exe no existe al inicio");

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                key?.SetValue("ClienteApp Beta", $"\"{exePath}\"");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar la aplicación al inicio al registro: {ex.Message}");
            }
        }

        private static bool IsRunningAsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
    }
}
