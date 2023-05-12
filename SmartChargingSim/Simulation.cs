using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using N = SmartChargingSim.Network;
using System.Threading.Tasks;
namespace SmartChargingSim
{
    internal class Simulation
    {
        public State state { get; set; }
        public float time;
        public int endTime;
        public PriorityQueue<ModelEvent, ModelEvent> events;
        public RandomInput input;
        public PerformanceMeasures performanceMeasures;
        public N.Network network;
        public float previousChargeUpdateTime;
        public DebugConsole debugConsole;
        public int scenario;
        bool run;
        float previousTime = 0;
        bool writePerf;
        /// <summary>
        /// The base simulation. Other simulations inherit from this class.
        /// </summary>
        /// <param name="input">The class for generating random input</param>
        /// <param name="debug">set to True if debug information is needed</param>
        /// <param name="scenario">The scenario that has to be ran</param>
        public Simulation(RandomInput input, bool debug, bool writePerf = false, int scenario = 0)
        {
            this.input = input;
            events = new PriorityQueue<ModelEvent, ModelEvent> { };
            performanceMeasures = new PerformanceMeasures();
            network = new N.Network();
            debugConsole = new DebugConsole(debug);
            this.scenario = scenario;
            this.writePerf = writePerf;
        }
        /// <summary>
        /// Runs the simulation, picks the right event handler
        /// </summary>
        /// <param name="endTime">The total time a simulation should run after a stable cycle has been reached.</param>
        public virtual void RunSim(int endTime)
        {
            this.endTime = endTime;
            run = true;
            InitBaseCase(scenario);
            debugConsole.WriteLine("Initialised state");
            debugConsole.WriteLine($"Scenario: {scenario}");
            while (run)
            {
                ModelEvent currentEvent = events.Dequeue();
                time = currentEvent.time;

                debugConsole.WriteLine(time + $" {currentEvent.type}");

                switch (currentEvent.type)
                {
                    case EventType.Arrival:
                        ArrivalEventHandler(currentEvent);
                        break;
                    case EventType.Departure:
                        DepartureEventHandler(currentEvent);
                        break;
                    case EventType.StartCharging:
                        StartChargingEventHandler(currentEvent);
                        break;
                    case EventType.StopCharging:
                        StopChargingEventHandler(currentEvent);
                        break;
                    case EventType.EndSimulation:
                        EndSimulationEventHandler(currentEvent);
                        break;
                    case EventType.StableCycleCheck:
                        StableCycleCheckHandler(currentEvent);
                        break;
                    case EventType.SolarUpdate:
                        SolarUpdateHandler(currentEvent);
                        break;
                    default:
                        //
                        break;
                }
                if (!state.ValidateState())
                {
                    Console.WriteLine("INCORRECT STATE.");
                }
            }
        }
        /// <summary>
        /// Initialises the base case
        /// </summary>
        /// <param name="scenario">The scenario (solar panels + season) for the simulation strategy</param>
        public virtual void InitBaseCase(int scenario)
        {
            EnqueueEvent(new ModelEvent(24, EventType.StableCycleCheck));
            EnqueueEvent(new ModelEvent(0, EventType.SolarUpdate));
            GenerateArrivalEvent(0);
            state = new State(scenario);
        }

        /// <summary>
        /// Base case arrival handler
        /// </summary>
        /// <param name="arrivalEvent"></param>
        public virtual void ArrivalEventHandler(ModelEvent arrivalEvent)
        {
            GenerateArrivalEvent(time);
            int p = ChooseParkingPlace(); // chosen parking place number
            if (p == -1)
            {
                performanceMeasures.TotalNonServed++;
                return;
            }
            state.UsedSpaces[p - 1]++;
            state.ChargingSpaces[p - 1]++; //(Start charging event)
            
            UpdateCableCharges();
            ScheduleChargingDeparture(time, p);
        }

        /// <summary>
        /// Handles a start charging event. In this scenario this is empty as cars start charging instantly on arrival, so this is handled in arrivalevents.
        /// </summary>
        /// <param name="startChargingEvent"></param>
        public virtual void StartChargingEventHandler(ModelEvent startChargingEvent)
        {
            
        }

        /// <summary>
        /// Fake eventhandler which checks if the cycle has reached stability. It is not an actual simulation event, but easily implemented this way
        /// </summary>
        /// <param name="stableCycleCheck"></param>
        public void StableCycleCheckHandler(ModelEvent stableCycleCheck)
        {
            //If the time is larger than 3 times the simulation time and the cycle is still not stable, prevent infinite loops.
            if(time> 3 * endTime)
            {
                EnqueueEvent(new ModelEvent(time, EventType.EndSimulation));
                performanceMeasures = new PerformanceMeasures();
            }
            if (state.IsStable(0.05f))
            {
                performanceMeasures = new PerformanceMeasures();
                performanceMeasures.StableCycleDay = (int)time /24;

                EnqueueEvent(new ModelEvent(time + endTime, EventType.EndSimulation));
            }
            else
            {
                EnqueueEvent(new ModelEvent(time + 24, EventType.StableCycleCheck));
            }
        }

        public virtual void SolarUpdateHandler(ModelEvent solarUpdateEvent)
        {
            float solarInput = input.GetCurrentSolarRevenue(time, state.Season);
            for (int i = 0; i < state.SolarRevenue.Length; i++)
            {
                state.SolarRevenue[i] = state.PSpotSolarPower[i] * solarInput;
            }

            EnqueueEvent(new ModelEvent(time + 1, EventType.SolarUpdate));
            UpdateCableCharges();
        }

        public virtual void StopChargingEventHandler(ModelEvent stopChargingEvent)
        {
            state.ChargingSpaces[stopChargingEvent.parkingSpot - 1]--;
            UpdateCableCharges();
        }

        /// <summary>
        /// Schedules a new stop charging and a departure event charging.
        /// </summary>
        /// <param name="time">Time that the car starts charging</param>
        /// <param name="pSpot">Parking spot where it starts charging</param>
        public virtual void ScheduleChargingDeparture(float time, int pSpot)
        {
            float chargingTime = input.GenerateChargingTime();
            float connectionTime = input.GenerateConnectionTime(chargingTime);
            EnqueueEvent(new ModelEvent(time + chargingTime, EventType.StopCharging, pSpot));
            EnqueueEvent(new ModelEvent(time + connectionTime, EventType.Departure, pSpot));
        }

        public virtual void DepartureEventHandler(ModelEvent departureEvent)
        {
            state.UsedSpaces[departureEvent.parkingSpot - 1]--;
        }

        /// <summary>
        /// Chooses a new parking spot for a newly arrived car.
        /// </summary>
        /// <returns>The chosen parking spot or -1 if no parkingspot was found</returns>
        public virtual int ChooseParkingPlace()
        {
            List<int> ps = input.GenerateParkingPlaceNums();
            for (int i = 0; i < ps.Count; i++)
            {
                if (state.UsedSpaces[ps[i] - 1] < state.numberOfChargingStations[ps[i] - 1])
                {
                    return ps[i];
                }
            }
            return -1;
        }

        /// <summary>
        /// Updates the charge on the cables depending on the currently charging cars in the state.
        /// </summary>
        public virtual void UpdateCableCharges()
        {
            state.CarChargeCable = network.Transformer.CalculateCableCharge(0,state.SolarRevenue, state.ChargingSpaces, new float[9]);
            UpdateChargePerformanceMeasures();
        }

        /// <summary>
        /// Queues a new arrival event based on the current time.
        /// </summary>
        /// <param name="time">The current time</param>
        public virtual void GenerateArrivalEvent(float time)
        {
            float newtime = time + input.GenerateArrivalTime((time % 24));
            UpdateArrivalPerformanceMeasures(newtime);
            EnqueueEvent(new ModelEvent(newtime, EventType.Arrival));
        }

        /// <summary>
        /// Updates all performancemeasures that have to do with arriving cars
        /// </summary>
        /// <param name="newTime">The current time</param>
        public virtual void UpdateArrivalPerformanceMeasures(float newTime)
        {
            performanceMeasures.numberOfCars++;
            if (newTime-time > performanceMeasures.maxTimeBetweenArrivals)
                performanceMeasures.maxTimeBetweenArrivals = newTime-time;
        }

        /// <summary>
        /// Updates performancemeasures that have to do with the load on the cables
        /// </summary>
        public virtual void UpdateChargePerformanceMeasures()
        {
            //update Maxload
            performanceMeasures.LoadOverTime[0].Add(time-24*performanceMeasures.StableCycleDay);
            for (int i = 0; i < state.CarChargeCable.Length; i++)
            {
                var totalLoad = state.CarChargeCable[i];
                if (totalLoad > performanceMeasures.MaxLoad[i])
                    performanceMeasures.MaxLoad[i] = totalLoad;
                performanceMeasures.LoadOverTime[i+1].Add(totalLoad);
            }
            //update time with 10% overload or underload
            for(int i = 0; i < state.PreviousCarChargeCable.Length; i++)
            {
                float oldLoad = state.PreviousCarChargeCable[i];
                if (oldLoad >= 220)
                    performanceMeasures.TotalTimeOverloaded[i] += time - previousChargeUpdateTime;
                if (oldLoad <= 220)
                    performanceMeasures.TotalTimeUnderloaded[i] += time - previousChargeUpdateTime;
            }

            for (int i = 0; i < network.parkingSpots.Count; i++)
            {
                float oldSolar = network.parkingSpots[i].previousSolarUsed;
                float oldSolarGenerated = network.parkingSpots[i].previousSolarGenerated;
                performanceMeasures.solarUsed[i] += (time - previousChargeUpdateTime)*oldSolar;
                performanceMeasures.solarGenerated[i] += (time - previousChargeUpdateTime) * oldSolarGenerated;
                network.parkingSpots[i].previousSolarUsed = network.parkingSpots[i].solarUsed;
                network.parkingSpots[i].previousSolarGenerated = state.SolarRevenue[i];
            }
            //if (time > 2400 && time < 2424 && time % 1 == 0 && previousTime != time)
            //{
            //    Console.WriteLine("time: " + time + " charging cars: " + state.ChargingSpaces.Sum() + "solar: " + state.SolarRevenue.Sum() / 6);
            //    previousTime = time;
            //}
            state.PreviousCarChargeCable = state.CarChargeCable;
            previousChargeUpdateTime = time;
        }

        /// <summary>
        /// Updates performance measures regarding departing cars. In the base case there are no delays, so this is empty.
        /// </summary>
        /// <param name="car">The car that departs</param>
        public virtual void UpdateDeparturePerformanceMeasures(Car car)
        {

        }

        /// <summary>
        /// Ends the simulation, prints the performance measures
        /// </summary>
        /// <param name="e">The endsimulation event</param>
        public virtual void EndSimulationEventHandler(ModelEvent e)
        {
            run = false;
            UpdateChargePerformanceMeasures();
            string endstring = performanceMeasures.PrettyPrint(time);
            if (writePerf)
            {
                Console.WriteLine(endstring);
            }
            performanceMeasures.WritePerformanceFile(endstring);
        }

        /// <summary>
        /// Enqueues an event in the priorityqueue. The priority is based on the type of event as well as the time.
        /// </summary>
        /// <param name="e">The event to be enqueued</param>
        public void EnqueueEvent(ModelEvent e)
        {
            events.Enqueue(e, e);
        }

    }
}
