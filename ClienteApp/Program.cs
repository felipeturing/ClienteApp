using System;
using System.Threading.Tasks;
using ClienteApp.Helpers;

namespace ClienteApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);
            Console.WriteLine("Iniciando...");

            // temporizador para enviar vida
            await Liveness();
            //var timer = new System.Timers.Timer(60000); // 30 segundos en milisegundos
            //timer.Elapsed += async (sender, e) => await LivenessAsync();
            //timer.AutoReset = true; // Para que se repita cada 30 segundos
            //timer.Enabled = true;

            // temporizador para actualizar apps
            await Apps();
            //var timer2 = new System.Timers.Timer(60000); // 40 segundos en milisegundos
            //timer2.Elapsed += async (sender, e) => await Apps();
            //timer2.AutoReset = true; // Para que se repita cada 40 segundos
            //timer2.Enabled = true;


            Console.ReadLine();
        }
        
        private static async Task Apps()
        {
            try
            {
                string worker = PowerShellHelper.GetWorker();
                if (!string.IsNullOrEmpty(worker))
                {
                    var data = await ApiHelper.GetApps(worker);

                    if (data != null)
                    {
                        Console.WriteLine($"Consultando aplicaciones para el host: {data.host}");
                        await PowerShellHelper.ProcessApps(data);
                        await PersistenceHelper.Save(data);
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