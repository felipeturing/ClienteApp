//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ClienteApp.Helpers
//{
//    public class TaskSchedulerHelper
//    {
//        public static void RegisterTask()
//        {
//            try
//            {
//                string exePath = Path.Combine(AppContext.BaseDirectory, "ClienteApp.exe");

//                if (!File.Exists(exePath))
//                    throw new Exception($"El ClienteApp.exe no existe en {exePath}");

//                using (TaskService ts = new TaskService())
//                {
//                    TaskDefinition td = ts.NewTask();
//                    td.RegistrationInfo.Description = "Ejecutar ClienteApp.exe como SYSTEM";

//                    // Configurar para ejecutarse con permisos elevados
//                    td.Principal.UserId = "SYSTEM";
//                    td.Principal.LogonType = TaskLogonType.ServiceAccount;
//                    td.Principal.RunLevel = TaskRunLevel.Highest;

//                    // Configurar la acción: iniciar ClienteApp.exe
//                    td.Actions.Add(new ExecAction(exePath, null, null));

//                    // Configurar el disparador: iniciar al encender el sistema
//                    td.Triggers.Add(new BootTrigger());

//                    // Crear la tarea
//                    ts.RootFolder.RegisterTaskDefinition(@"ClienteApp SYSTEM", td);
//                    Console.WriteLine("Tarea programada creada exitosamente.");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error al registrar la tarea: {ex.Message}");
//            }
//        }
//    }
//}
