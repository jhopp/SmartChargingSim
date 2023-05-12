
namespace SmartChargingSim
{
    internal class Car
    {
        public float arrivalTime; // time at which the car arrived
        public float connectionTime; // time for which the car will remain at the parking place (if not delayed)
        public float chargeTimeRemaining; // time the car still has to charge
        public float latestFeasibleStartTime; // latest feasible time the car can start charging without delay
        public Car (float arrivalTime, float connectionTime, float chargeTimeRemaining)
        {
            this.arrivalTime = arrivalTime;
            this.connectionTime = connectionTime;
            this.chargeTimeRemaining = chargeTimeRemaining;
            latestFeasibleStartTime = arrivalTime + connectionTime - chargeTimeRemaining;
        }
    }
}
