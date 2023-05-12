using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartChargingSim.Network
{
    internal class Network
    {
        public ParkingSpot p1;
        public ParkingSpot p2;
        public ParkingSpot p3;
        public ParkingSpot p4;
        public ParkingSpot p5;
        public ParkingSpot p6;
        public ParkingSpot p7;
        public ParkingSpot Transformer;
        public Cable c0;
        public Cable c1;
        public Cable c2;
        public Cable c3;
        public Cable c4;
        public Cable c5;
        public Cable c6;
        public Cable c7;
        public Cable c8;
        public List<ParkingSpot> dummySpots;    //Dummynodes in between cables 1,2,3 and 6,7,8 that are used in the flow calculation
        public List<ParkingSpot> parkingSpots;
        /// <summary>
        /// Sets up the parking spaces like a tree network that can be used for flow calculation
        /// </summary>
        public Network()
        {
            c0 = new Cable(0);
            c1 = new Cable(1);
            c2 = new Cable(2);
            c3 = new Cable(3);
            c4 = new Cable(4);
            c5 = new Cable(5);
            c6 = new Cable(6);
            c7 = new Cable(7);
            c8 = new Cable(8);

            p1 = new ParkingSpot(c1, new List<ParkingSpot>(), 1);
            p2 = new ParkingSpot(c2, new List<ParkingSpot>(), 2);
            p3 = new ParkingSpot(c3, new List<ParkingSpot>(), 3);
            p5 = new ParkingSpot(c7, new List<ParkingSpot>(), 5);
            p6 = new ParkingSpot(c8, new List<ParkingSpot>(), 6);
            p7 = new ParkingSpot(c5, new List<ParkingSpot>(), 7);
            ParkingSpot dummy1 = new ParkingSpot(c0, new List<ParkingSpot>() { p1,p2,p3},-2);
            ParkingSpot dummy2 = new ParkingSpot(c6, new List<ParkingSpot>() { p5, p6 }, -3);
            p4 = new ParkingSpot(c4, new List<ParkingSpot>() {dummy2, p7  }, 4);
            Transformer = new ParkingSpot(new Cable(0), new List<ParkingSpot>() { dummy1,p4},0);

            parkingSpots = new List<ParkingSpot>() { p1,p2,p3,p4,p5,p6,p7};

            dummySpots = new List<ParkingSpot> { dummy1 , dummy2};
            
        }
    }
}
