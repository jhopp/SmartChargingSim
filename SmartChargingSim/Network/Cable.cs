using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartChargingSim.Network
{
    internal class Cable
    {
        public int flow;
        public int id;
        /// <summary>
        /// Represents a cable in the network
        /// </summary>
        /// <param name="id">the id of the cable (between 0 and 9)</param>
        public Cable(int id)
        {
            flow = 0;
            this.id = id;
        }
    }
}
