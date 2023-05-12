using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartChargingSim
{
    internal class DebugConsole
    {
        bool DebugOn;
        /// <summary>
        /// A class which only writes to console if debug is true
        /// </summary>
        /// <param name="debug">The bool to write to console</param>
        public DebugConsole(bool debug = false)
        {
            DebugOn = debug;
        }

        /// <summary>
        /// Writes a line to console if debug is true
        /// </summary>
        /// <param name="message">The message to write to the console</param>
        public void WriteLine(string message)
        {
            if (DebugOn)
            {
                Console.WriteLine(message);
            }
        }
    }
}
