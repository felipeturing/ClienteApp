using System;
using System.Threading.Tasks;
using ClienteApp.Helpers;

namespace ClienteApp
{
    class Program
    {
        public static Timer? timerLiveness;
        public static Timer? timerRefreshApps;
        static void Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);
            Console.WriteLine("Iniciando...");

            timerLiveness = new Timer(async _ => await Liveness(), null, TimeSpan.Zero, TimeSpan.FromSeconds(120));
            timerRefreshApps = new Timer(async _ => await Apps(), null, TimeSpan.Zero, TimeSpan.FromSeconds(8));


            Console.ReadLine();
        }
        
        private static async Task Apps()
        {
            try
            {
                string worker = PowerShellHelper.GetWorker();
                if (!string.IsNullOrEmpty(worker))
                {
                    Models.Data? data = await ApiHelper.GetApps(worker);
                    if (data != null)
                    {
                        Console.WriteLine($"Consultando aplicaciones para el host: {data.worker?.name}");
                        await PowerShellHelper.ProcessApps(data);
                        //await PersistenceHelper.Save(data);
                    }
                }
                else
                {
                    Console.WriteLine("No se pudo obtener el nombre del equipo.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }


        }

        private static async Task Liveness()
        {
            try
            {
                string worker = PowerShellHelper.GetWorker();
                if (!string.IsNullOrEmpty(worker))
                {
                    await ApiHelper.Liveness(worker);
                }
                else
                {
                    Console.WriteLine("No se pudo obtener el nombre del equipo.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}