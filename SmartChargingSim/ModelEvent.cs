using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartChargingSim
{
    internal class ModelEvent : IComparable<ModelEvent>
    {
        public float time;
        public EventType type;
        public int parkingSpot; // parking place !! !! !
        public Car car;
        public ModelEvent(float time, EventType et, int parkingSpot= 0)
        {
            this.time = time;
            type = et;
            this.parkingSpot = parkingSpot;
        }

        public ModelEvent(float time, EventType et, int parkingSpot, Car car)
        {
            this.time = time;
            type = et;
            this.parkingSpot = parkingSpot;
            this.car = car;
        }

        public int CompareTo(ModelEvent other)
        {
            if (this.time < other.time)
                return -1;
            else if (this.time > other.time)
                return 1;
            else if (this.type < other.type)
                return -1;
            else if (this.type > other.type)
                return 1;
            else return 0;
        }
    }
    public enum EventType {
        StartCharging, StopCharging, Departure, ExpectedDeparture, 
        ExpectedStopCharging, EndSimulation, StableCycleCheck, SolarUpdate, Arrival}
}
