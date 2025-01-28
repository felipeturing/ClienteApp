using System;
using System.Threading.Tasks;
using ClienteApp.Helpers;
using ClienteApp.Models;

namespace ClienteApp
{
    class Program
    {
        public static Timer? timerLiveness;
        public static Timer? timerRefreshApps;
        static async Task Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);
            
            Utils.ConsoleHelper.WriteColoredMessage("ClienteApp.Program.Main: Iniciando...", ConsoleColor.Green);
            await PersistenceHelper.WriteLog($"ClienteApp.Program.Main: Iniciando...");
            
            timerLiveness = new Timer(async _ => await Liveness(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            timerRefreshApps = new Timer(async _ => await Apps(), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
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