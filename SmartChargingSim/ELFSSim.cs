
namespace SmartChargingSim
{
    internal class ELFSSim : FCFSSim
    {
        public ELFSSim(RandomInput input, bool debug, bool writePerf, int scenario = 0) : base(input, debug,writePerf, scenario)
        {

        }
        // Enqueues a car based on its priority value (latest feasible start time)
        public override void EnqueueCar(Car c, int parkingPlace)
        {
            state.carQueues[parkingPlace - 1].Enqueue(c, c.latestFeasibleStartTime);
        }
        // Gets the priority value relevant to this simulation (latest feasible start time)
        public override float GetCarPriorityValue(int i)
        {
            return state.carQueues[i].Peek().latestFeasibleStartTime;
        }
    }
}
