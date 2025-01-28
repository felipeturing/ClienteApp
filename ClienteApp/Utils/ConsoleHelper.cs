using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteApp.Utils
{
    public static class ConsoleHelper
    {
        /// <summary>
        /// Muestra un mensaje en la consola con el color especificado.
        /// </summary>
        /// <param name="message">El mensaje a mostrar.</param>
        /// <param name="color">El color del texto.</param> 
        public static void WriteColoredMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }


}
