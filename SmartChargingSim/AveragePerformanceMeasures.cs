using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartChargingSim
{
    internal class AveragePerformanceMeasures : PerformanceMeasures
    {
        int runs;
        float fractionSolarUsed;
        public AveragePerformanceMeasures() : base()
        {
            runs = 0;
        }

        /// <summary>
        /// Add the performance measures of the current run to the average performance measures.
        /// </summary>
        /// <param name="performanceMeasures"></param>
        public void AddPerformanceMeasures(PerformanceMeasures performanceMeasures)
        {
            if (performanceMeasures.StableCycleDay > 0)
            {
                runs++;
                numberOfCars += performanceMeasures.numberOfCars;
                numberOfStartCharging += performanceMeasures.numberOfStartCharging;
                numberOfStopCharging += performanceMeasures.numberOfStartCharging;
                maxTimeBetweenArrivals += performanceMeasures.maxTimeBetweenArrivals;

                // 1: Cable Load
                for (int i = 0; i < MaxLoad.Length; i++)
                {
                    MaxLoad[i] += performanceMeasures.MaxLoad[i];
                    TotalTimeOverloaded[i] += performanceMeasures.TotalTimeOverloaded[i];
                    TotalTimeUnderloaded[i] += performanceMeasures.TotalTimeUnderloaded[i];
                }
                // 2: Departure delays
                TotalDelayedVehicles += performanceMeasures.TotalDelayedVehicles;
                TotalDelayedTime += performanceMeasures.TotalDelayedTime;
                MaxDelay += performanceMeasures.MaxDelay;

                // 3: Non-served
                TotalNonServed += performanceMeasures.TotalNonServed;

                //4: Steady Cycle
                StableCycleDay += StableCycleDay;

                fractionSolarUsed += performanceMeasures.solarUsed.Sum() / performanceMeasures.solarGenerated.Sum();
                
            }
        }

        /// <summary>
        /// After running all scenarios, calculates the average of the runs.
        /// </summary>
        public void CalculateAverage()
        {
            if (runs > 0)
            {
                numberOfCars /= runs;
                numberOfStartCharging /= runs;
                numberOfStopCharging /= runs;
                maxTimeBetweenArrivals /= runs;

                // 1: Cable Load
                MaxLoad = MaxLoad.Select(x => x / runs).ToArray();
                TotalTimeOverloaded = TotalTimeOverloaded.Select(x => x / runs).ToArray(); ;
                TotalTimeUnderloaded = TotalTimeUnderloaded.Select(x => x / runs).ToArray();

                // 2: Departure delays
                TotalDelayedVehicles /= runs;
                TotalDelayedTime /= runs;
                MaxDelay /= runs;

                // 3: Non-served
                TotalNonServed /= runs;

                //4: Steady Cycle
                StableCycleDay /=  runs;

                fractionSolarUsed /= runs;
            }
        }

        public override string PrettyPrint(float time)
        {
            CalculateAverage();
            string average = "############ Average Performance measures ######### \n";
            string numberofRuns = "number of runs =" + runs.ToString() + "\n";
            return (average +numberofRuns + base.PrettyPrint(time));
        }

        /// <summary>
        /// Print the performance measures without formatting
        /// </summary>
        /// <param name="time">Current time</param>
        /// <returns></returns>
        public override string Print(float time)
        {
            CalculateAverage();
            string average = "############ Average Performance measures ######### \n";
            string numberofRuns = "number of runs =" + runs.ToString() + "\n";
            string fractionSolar = fractionSolarUsed.ToString() + "\n";
            return (average + numberofRuns + base.Print(time) + fractionSolar);
        }
    }
}
