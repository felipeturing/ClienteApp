using System;
using System.Drawing;
using System.Runtime.Versioning;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClienteApp.Helpers;
using ClienteApp.Models;
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

            // Inicializar la aplicación Windows Forms
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // Crear y configurar el ícono de la bandeja del sistema
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon("logo.ico"), // Asegúrate de tener un archivo logo.ico en tu proyecto
                Visible = true,
                Text = "AppClient - Controlado por Sistemas"
            };

            // Menú contextual utilizando ContextMenuStrip
            var contextMenuStrip = new ContextMenuStrip();

            // Agregar elementos al menú
            var openMenuItem = new ToolStripMenuItem("Abrir", null, (s, e) => MessageBox.Show("Aplicación en ejecución."));
            var exitMenuItem = new ToolStripMenuItem("Salir", null, (s, e) => System.Windows.Forms.Application.Exit());

            // Agregar los items al ContextMenuStrip
            contextMenuStrip.Items.AddRange(new ToolStripMenuItem[] { openMenuItem, exitMenuItem });

            // Asignar el ContextMenuStrip al NotifyIcon
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            // Mostrar mensaje en consola
            Utils.ConsoleHelper.WriteColoredMessage("ClienteApp.Program.Main: Iniciando...", ConsoleColor.Green);

            // Iniciar temporizadores
            timerLiveness = new System.Threading.Timer(async _ => await Liveness(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            timerRefreshApps = new System.Threading.Timer(async _ => await Apps(), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));

            // Mantener la aplicación ejecutándose
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
    }
}
