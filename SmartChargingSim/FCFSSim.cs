using System;

namespace SmartChargingSim
{
    internal class FCFSSim : Simulation
    {
        public FCFSSim(RandomInput input, bool debug,bool writePerfm, int scenario = 0) : base(input, debug,writePerfm, scenario)
        {
            
        }
        
        // Handle arrival events
        public override void ArrivalEventHandler(ModelEvent arrivalEvent)
        {
            // generate next arrival event
            GenerateArrivalEvent(time);

            // choose parking place
            int parkingPlace = ChooseParkingPlace(); // parking place number
            if (parkingPlace == -1)
            {
                // couldn't find parking place -> car leaves
                performanceMeasures.TotalNonServed++;
                return;
            }

            // generate time car needs to charge and time car stays at parking place
            float chargingTime = input.GenerateChargingTime();
            float connectionTime = input.GenerateConnectionTime(chargingTime);
            Car car = new Car(time, connectionTime, chargingTime);

            // car is now using a space at the parking place
            state.UsedSpaces[parkingPlace - 1]++;

            // if there is capacity to start charging
            if (CanChargeWithoutOverload(parkingPlace, state.ChargingSpaces))
            {
                // schedule a start charging event (now)
                EnqueueEvent(new ModelEvent(time, EventType.StartCharging, parkingPlace, car));
            }
            else
            {
                // no capacity -> place in queue
                EnqueueCar(car, parkingPlace);
            }
        }

        // Handle start charging events
        public override void StartChargingEventHandler(ModelEvent startChargingEvent)
        {
            int pp = startChargingEvent.parkingSpot;
            Car car = startChargingEvent.car;
            float ct = car.chargeTimeRemaining;

            // update performance measures
            performanceMeasures.numberOfStartCharging++;

            // enqueue event
            EnqueueEvent(new ModelEvent(time + ct, EventType.StopCharging, 
                startChargingEvent.parkingSpot, car));

            // update charge
            state.ChargingSpaces[pp - 1]++;
            UpdateCableCharges();
        }

        // Handle stop charging events
        public override void StopChargingEventHandler(ModelEvent stopChargingEvent)
        {
            int pp = stopChargingEvent.parkingSpot;
            Car car = stopChargingEvent.car;

            // update performance measures
            performanceMeasures.numberOfStopCharging++;

            // update charge
            state.ChargingSpaces[pp - 1]--;
            UpdateCableCharges();

            // schedule departure event
            float dtime = Math.Max(car.arrivalTime + car.connectionTime, time);
            EnqueueEvent(new ModelEvent(dtime, EventType.Departure, pp, car));
            
            // start charging new car(s)
            StartChargingFromQueues();
        }

        // Handles departure events
        public override void DepartureEventHandler(ModelEvent departureEvent)
        {
            // update spaces used
            state.UsedSpaces[departureEvent.parkingSpot - 1]--;
            
            // update performance measures
            UpdateDeparturePerformanceMeasures(departureEvent.car);
        }

        // Handles solar revenue update events
        public override void SolarUpdateHandler(ModelEvent solarUpdateEvent)
        {
            // update solar revenues for each parking place
            float solarInput = input.GetCurrentSolarRevenue(time, state.Season);
            for (int i = 0; i < state.SolarRevenue.Length; i++)
            {
                state.SolarRevenue[i] = state.PSpotSolarPower[i] * solarInput;
            }

            // schedule new solar revenue update event
            EnqueueEvent(new ModelEvent(time + 1, EventType.SolarUpdate));
            UpdateCableCharges();

            // potentially allow new cars to start charging
            StartChargingFromQueues();
        }

        // Start charging cars from queues without it causing overload
        public void StartChargingFromQueues()
        {
            bool[] exclude = new bool[state.carQueues.Length];
            int parkingPlace = GetMostUrgentQueue(exclude);
            int [] newChargeSpaces = (int[])state.ChargingSpaces.Clone();

            // while there is a potential parking place to start charging
            while (parkingPlace > 0)
            {
                // if it is possible to start charging here without causing overload
                if(CanChargeWithoutOverload(parkingPlace, newChargeSpaces))
                {
                    // schedule start charging event 
                    if (state.carQueues[parkingPlace - 1].Count > 0)
                    {
                        Car c = state.carQueues[parkingPlace - 1].Dequeue();
                        EnqueueEvent(new ModelEvent(time, EventType.StartCharging, parkingPlace, c));
                        newChargeSpaces[parkingPlace - 1]++;
                    }
                }
                else
                {
                    // would cause overload -> exclude this parking place from being picked
                    exclude[parkingPlace - 1] = true;
                }

                // pick next most urgent car queue
                parkingPlace = GetMostUrgentQueue(exclude);
            }
        }

        // Return the parking spot with highest priority queue head, excluding some
        public int GetMostUrgentQueue(bool[] exclude)
        {
            float p_val = float.MaxValue; // value of priority
            int p_num = 0; // highest priority parking spot num
            
            for (int i = 0; i < state.carQueues.Length; i++)
            {
                // if queue is not empty
                if (state.carQueues[i].Count > 0)
                    // if not excluded and head has priority over current
                    if (!exclude[i] && state.carQueues[i].Peek().arrivalTime < p_val)
                    {
                        // this parking spot becomes the new priority
                        p_val = GetCarPriorityValue(i);
                        p_num = i + 1;
                    }
            }
            return p_num; 
        }

        // Update departure related performance measures
        public override void UpdateDeparturePerformanceMeasures(Car c)
        {
            float delay = time - (c.arrivalTime + c.connectionTime);
            // if there was a delay, update performance measures
            if (delay > 0)
            {
                performanceMeasures.TotalDelayedTime += delay;
                performanceMeasures.TotalDelayedVehicles++;
                if (delay > performanceMeasures.MaxDelay)
                    performanceMeasures.MaxDelay = delay; 
            }
        }

        // Returns whether a car can start charging at some parking place without causing (more) overload
        public bool CanChargeWithoutOverload(int parkingPlace, int[] chargingSpaces)
        {
            // determine current total overload
            float currentOverload = 0;
            foreach (float c in state.CarChargeCable)
                if (c > 200)
                    currentOverload += c - 200;

            // determine overload if a car starts charing at parkingPlace
            int[] newChargingSpaces = (int[])chargingSpaces.Clone();
            newChargingSpaces[parkingPlace - 1]++;
            float[] newCharges = network.Transformer.CalculateCableCharge(0, state.SolarRevenue, newChargingSpaces, new float[9]);
            float newOverload = 0;
            foreach (float c in newCharges)
                if (c > 200)
                    newOverload += c - 200;
            
            if (newOverload > 0 && newOverload <= currentOverload)
                debugConsole.WriteLine("larger"); // ?

            // return whether there is at most as much overload as before
            return (newOverload <= currentOverload);
        }

        // Enqueues a car based on its priority value (arrival time)
        public virtual void EnqueueCar(Car c, int parkingPlace)
        {
            state.carQueues[parkingPlace - 1].Enqueue(c, c.arrivalTime);
        }
        // Gets the priority value relevant to this simulation (arrival time)
        public virtual float GetCarPriorityValue(int i)
        {
            return state.carQueues[i].Peek().arrivalTime;
        }
    }
}
