using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartChargingSim
{
    internal class PriceDrivenSim : Simulation
    {
        /// <summary>
        /// The simulation for the price driven strategy.
        /// </summary>
        /// <param name="input">The random input generator</param>
        /// <param name="debug">A bool to specify if debug output should be printed</param>
        /// <param name="scenario"></param>
        public PriceDrivenSim(RandomInput input, bool debug, bool writePerf, int scenario = 0) : base(input, debug,writePerf, scenario)
        {
        }
        public override void ArrivalEventHandler(ModelEvent arrivalEvent)
        {
            GenerateArrivalEvent(time);
            int p = ChooseParkingPlace();
            if (p == -1)
            {
                performanceMeasures.TotalNonServed++;
                return;
            }
            state.UsedSpaces[p - 1]++;
            ScheduleChargingDeparture(time, p);
        }
        public override void ScheduleChargingDeparture(float time, int pSpot)
        {
            float chargingTime = input.GenerateChargingTime();
            float connectionTime = input.GenerateConnectionTime(chargingTime);
            var startTime = GenerateStartTime(time, chargingTime, connectionTime);
            EnqueueEvent(new ModelEvent(startTime, EventType.StartCharging, pSpot));
            EnqueueEvent(new ModelEvent(startTime + chargingTime, EventType.StopCharging, pSpot));
            EnqueueEvent(new ModelEvent(time + connectionTime, EventType.Departure, pSpot));
        }

        /// <summary>
        /// Generates the preferred start time for a car, based on its departure time, charging time and the cost of electricity.
        /// </summary>
        /// <param name="time">The current time</param>
        /// <param name="chargingTime">The charging time for a car</param>
        /// <param name="connectionTime">The connection time for a car</param>
        /// <returns>The time at which a car has to start charging</returns>
        public float GenerateStartTime(float time, float chargingTime,float connectionTime)
        {
            float[] startTimes = { time % 24, 0, 8, 16, 20, (time + connectionTime - chargingTime)%24 };
            float startTime = 0;
            float minPrice = 10000;
            for(int i = 0; i< startTimes.Length; i++)
            {
                float timeToStart = (startTimes[i]-time)%24;
                timeToStart = (timeToStart < 0) ? 24 + timeToStart : timeToStart;
                if ((time + timeToStart + chargingTime) <= (time + connectionTime))
                {
                    float price = CalculatePrice(startTimes[i], chargingTime);
                    if (price < minPrice)
                    {
                        minPrice = price;
                        startTime = time + timeToStart;
                    }
                }
            }
            return startTime;
        }

        /// <summary>
        /// Calculates the price to start charging at a specific time
        /// </summary>
        /// <param name="startTime">The time to start charging</param>
        /// <param name="chargingTime">The time spent charging</param>
        /// <returns></returns>
        public float CalculatePrice(float startTime, float chargingTime)
        {

            float price = chargingTime / 24;
            chargingTime = chargingTime % 24;
            int StartInterval = ((int)startTime) / 6;
            int[] intervalPrices = { 16, 18, 22, 20 };
            float leftOverTime = StartInterval * 6 + 6 - startTime;
            price += intervalPrices[StartInterval]*leftOverTime;
            float remainingTime = chargingTime -leftOverTime;
            int i = StartInterval;
            while (remainingTime > 6)
            {
                i++;
                price += 6 * intervalPrices[(i-1)%4];
                remainingTime -= 6;
            }
            price += remainingTime * intervalPrices[i%4];
            return price;
        }
        public override void StartChargingEventHandler(ModelEvent startChargingEvent)
        {
            state.ChargingSpaces[startChargingEvent.parkingSpot - 1]++;
            UpdateCableCharges();
        }
    }
}
