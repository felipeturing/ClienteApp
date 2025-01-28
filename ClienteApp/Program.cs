using System;
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

        // Se asegura que el código funcione correctamente como una aplicación Windows Forms
        [STAThread]
        [SupportedOSPlatform("windows6.1")]
        static void Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);

            //AddToStartup();

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            notifyIcon = new NotifyIcon
            {
                Icon = new Icon("logo.ico"), // Asegúrate de tener un archivo logo.ico en tu proyecto
                Visible = true,
                Text = "AppClient - Controlado por Sistemas"
            };

            var contextMenuStrip = new ContextMenuStrip();
            var openMenuItem = new ToolStripMenuItem("Abrir", null, (s, e) => MessageBox.Show("Aplicación en ejecución..\n\nv1.0.0-beta\nDesarrollado por Sistemas 2025"));
            var exitMenuItem = new ToolStripMenuItem("Salir", null, (s, e) => System.Windows.Forms.Application.Exit());

            contextMenuStrip.Items.AddRange(new ToolStripMenuItem[] { openMenuItem, exitMenuItem });

            notifyIcon.ContextMenuStrip = contextMenuStrip;

            Utils.ConsoleHelper.WriteColoredMessage("ClienteApp.Program.Main: Iniciando...", ConsoleColor.Green);

            timerLiveness = new System.Threading.Timer(async _ => await Liveness(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            timerRefreshApps = new System.Threading.Timer(async _ => await Apps(), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));

            System.Windows.Forms.Application.Run();
        }


        private static async Task Apps()
        {
            string worker = await PowerShellHelper.GetWorker();
            if (!string.IsNullOrEmpty(worker))
            {
                Models.Data? data = await ApiHelper.GetApps(worker);
                if (data != null)
                {
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

        //private static void AddToStartup()
        //{
        //    string appName = "AppClient";
        //    string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

        //    try
        //    {
        //        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
        //        {
        //            if (key == null)
        //            {
        //                throw new Exception("No se pudo abrir la clave de registro.");
        //            }

        //            // Verifica si ya está registrado
        //            object? existingValue = key.GetValue(appName);
        //            if (existingValue == null || !existingValue.ToString().Equals($"\"{appPath}\"", StringComparison.OrdinalIgnoreCase))
        //            {
        //                key.SetValue(appName, $"\"{appPath}\"");
        //                Utils.ConsoleHelper.WriteColoredMessage($"Aplicación registrada para inicio automático: {appPath}", ConsoleColor.Yellow);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error al registrar la aplicación para el inicio automático: {ex.Message}",
        //                        "Error",
        //                        MessageBoxButtons.OK,
        //                        MessageBoxIcon.Error);
        //    }
        //}
    }
}
