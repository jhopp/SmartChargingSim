using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartChargingSim
{
    internal class StatisticalPerformanceMeasures : PerformanceMeasures
    {
        public new List<List<float>> numberOfCars;
        public new List<List<float>> numberOfStartCharging;
        public new List<List<float>> numberOfStopCharging;

        // 1: Cable Load
        public List<List<float>> AverageMaxLoad;
        public List<List<float>> AverageTotalTimeOverloaded;
        public List<List<float>> AverageTotalTimeUnderloaded;

        // 2: Departure delays
        public new List<List<float>> TotalDelayedVehicles;
        public new List<List<float>> TotalDelayedTime;
        public new List<List<float>> MaxDelay;

        // 3: Non-served
        public new List<List<float>> TotalNonServed;
        public new List<List<float>> FractionNonServed;
        public new List<List<float>> NonServedPerDay;
        public int runsDone = 0;
        public int runsPerScen;
        private StatisticalAnalysis stat;

        public List<List<float>> FractionSolarUsed;

        /// <summary>
        /// Gathers the performance measures from each run such that statistical tests can be done on them.
        /// </summary>
        /// <param name="runsPerScen">The number of runs done per scenario</param>
        public StatisticalPerformanceMeasures(int runsPerScen)
        {
            this.runsPerScen = runsPerScen;
            stat = new StatisticalAnalysis();
            numberOfCars = new List<List<float>>();
            numberOfStartCharging = new List<List<float>>();
            numberOfStopCharging = new List<List<float>>();
            TotalDelayedVehicles = new List<List<float>>();
            TotalDelayedTime = new List<List<float>>();
            MaxDelay = new List<List<float>>();
            TotalNonServed = new List<List<float>>();
            AverageMaxLoad = new List<List<float>>();
            AverageTotalTimeOverloaded = new List<List<float>>();
            AverageTotalTimeUnderloaded = new List<List<float>>();
            FractionNonServed = new List<List<float>>();
            NonServedPerDay = new List<List<float>>();
            FractionSolarUsed = new List<List<float>>();
        }

        /// <summary>
        /// Add the performance measures for a single run to the lists.
        /// </summary>
        /// <param name="performanceMeasures">The performance measures from the last run</param>
        public void AddPerformanceMeasures(PerformanceMeasures performanceMeasures)
        {
            if(runsDone%runsPerScen == 0)
            {
                numberOfCars.Add(new List<float>());
                numberOfStartCharging.Add(new List<float>());
                numberOfStopCharging.Add(new List<float>());

                TotalDelayedVehicles.Add(new List<float>());
                TotalDelayedTime.Add(new List<float>());
                MaxDelay.Add(new List<float>());
                TotalNonServed.Add(new List<float>());
                AverageTotalTimeUnderloaded.Add(new List<float>());
                AverageTotalTimeOverloaded.Add(new List<float>());
                AverageMaxLoad.Add(new List<float>());
                FractionNonServed.Add(new List<float>());
                NonServedPerDay.Add(new List<float>());
                FractionSolarUsed.Add(new List<float>());
            }
            numberOfCars[runsDone / runsPerScen].Add(performanceMeasures.numberOfCars);
            numberOfStartCharging[runsDone / runsPerScen].Add(performanceMeasures.numberOfStartCharging);
            numberOfStopCharging[runsDone / runsPerScen].Add(performanceMeasures.numberOfStopCharging);
            TotalDelayedVehicles[runsDone / runsPerScen].Add(performanceMeasures.TotalDelayedVehicles);
            if (performanceMeasures.TotalDelayedVehicles == 0)
                TotalDelayedTime[runsDone / runsPerScen].Add(0);
            else
                TotalDelayedTime[runsDone / runsPerScen].Add(performanceMeasures.TotalDelayedTime / (performanceMeasures.numberOfCars));
            MaxDelay[runsDone / runsPerScen].Add(performanceMeasures.MaxDelay);
            TotalNonServed[runsDone / runsPerScen].Add(performanceMeasures.TotalNonServed);
            FractionNonServed[runsDone / runsPerScen].Add(performanceMeasures.FractionNonServed);
            NonServedPerDay[runsDone / runsPerScen].Add(performanceMeasures.NonServedPerDay);
            FractionSolarUsed[runsDone / runsPerScen].Add(performanceMeasures.solarUsed.Sum() / performanceMeasures.solarGenerated.Sum());
            float mload=0;
            float tOload=0;
            float tUload=0;
            int n = performanceMeasures.MaxLoad.Length;
            for (int i = 0; i< n; i++)
            {
                mload += performanceMeasures.MaxLoad[i];
                tOload += performanceMeasures.averageTimeOverloaded[i];
                tUload += performanceMeasures.averageTimeUnderloaded[i];
            }
            AverageMaxLoad[runsDone / runsPerScen].Add(mload / n);
            AverageTotalTimeOverloaded[runsDone /runsPerScen ].Add(tOload / n);
            AverageTotalTimeUnderloaded[runsDone / runsPerScen].Add(tUload / n);
            runsDone++;
        }

        /// <summary>
        /// Pretty print all of the performance measures
        /// </summary>
        /// <returns>A string of performance measures formatted nicely</returns>
        public string prettyPrintConfidence()
        {
            StringBuilder sb = new StringBuilder();
            if (runsDone / runsPerScen >= 2)
            {
                sb.AppendLine("############ Paired T-test for performance measures #########");
                sb.AppendLine($" ");
                sb.AppendLine($"Total number of cars arrived: {stat.PairedTConfidence(numberOfCars[0], numberOfCars[1])}");
                sb.AppendLine($"Number of cars not served: {stat.PairedTConfidence(TotalNonServed[0], TotalNonServed[1])}");
                sb.AppendLine($"Fraction of cars not served: {stat.PairedTConfidence(FractionNonServed[0], FractionNonServed[1])}");
                sb.AppendLine($"Number of cars not served per day: {stat.PairedTConfidence(NonServedPerDay[0], NonServedPerDay[1])}");
                sb.AppendLine($" ");
                sb.AppendLine($"Total number of delayed cars: {stat.PairedTConfidence(TotalDelayedVehicles[0], TotalDelayedVehicles[1])}");
                sb.AppendLine($"Max. delay time: {stat.PairedTConfidence(MaxDelay[0], MaxDelay[1])}");
                sb.AppendLine($"Average time delay: {stat.PairedTConfidence(TotalDelayedTime[0], TotalDelayedTime[1])}");
                sb.AppendLine($" ");
                sb.AppendLine($"Max. load on each cable: {stat.PairedTConfidence(AverageMaxLoad[0], AverageMaxLoad[1])}");
                sb.AppendLine($"Fraction of time cables overloaded  (110%>=): {stat.PairedTConfidence(AverageTotalTimeOverloaded[0], AverageTotalTimeOverloaded[1])}");
                sb.AppendLine($"Fraction of time cables underloaded (110%<=): {stat.PairedTConfidence(AverageTotalTimeUnderloaded[0], AverageTotalTimeUnderloaded[1])}");
                sb.AppendLine($" ");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Prints the confidence intervals from the t-test for every performance measure
        /// </summary>
        /// <returns></returns>
        public string PrintConfidence()
        {
            StringBuilder sb = new StringBuilder();
            if (runsDone / runsPerScen >= 2)
            {
                sb.AppendLine("############ Paired T-test for performance measures #########");
                sb.AppendLine($" ");
                sb.AppendLine($"{stat.PairedTConfidence(numberOfCars[0], numberOfCars[1])}");
                sb.AppendLine($"{stat.PairedTConfidence(TotalNonServed[0], TotalNonServed[1])}");
                sb.AppendLine($"{stat.PairedTConfidence(FractionNonServed[0], FractionNonServed[1])}");
                sb.AppendLine($"{stat.PairedTConfidence(NonServedPerDay[0], NonServedPerDay[1])}");
                sb.AppendLine($" ");
                sb.AppendLine($"{stat.PairedTConfidence(TotalDelayedVehicles[0], TotalDelayedVehicles[1])}");
                sb.AppendLine($"{stat.PairedTConfidence(MaxDelay[0], MaxDelay[1])}");
                sb.AppendLine($"{stat.PairedTConfidence(TotalDelayedTime[0], TotalDelayedTime[1])}");
                sb.AppendLine($" ");
                sb.AppendLine($"{stat.PairedTConfidence(AverageMaxLoad[0], AverageMaxLoad[1])}");
                sb.AppendLine($"{stat.PairedTConfidence(AverageTotalTimeOverloaded[0], AverageTotalTimeOverloaded[1])}");
                sb.AppendLine($"{stat.PairedTConfidence(AverageTotalTimeUnderloaded[0], AverageTotalTimeUnderloaded[1])}");
                sb.AppendLine($" ");
            }
            return sb.ToString();
        }
        /// <summary>
        /// Pretty prints all pair confidence intervals
        /// </summary>
        /// <returns>A string of all paired confidence intervals</returns>
        public string prettyPrintAllPairs()
        {
            StringBuilder sb = new StringBuilder();
            if (runsDone / runsPerScen > 2)
            {
                var confidenceIntervals = stat.AllPairWiseConfidence(FractionSolarUsed);
                for (int i = 0; i < confidenceIntervals.GetLength(0); i++) {
                    for (int j = 0; j < confidenceIntervals.GetLength(1); j++)
                    {
                        sb.Append(confidenceIntervals[i, j]);
                    }
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// writes the performance measures to file
        /// </summary>
        /// <param name="s">A string of performance measures</param>
        public override void WritePerformanceFile(string s)
        {
            string filename = "../../../StatisticalPerformanceMeasures.txt";
            using StreamWriter file = new StreamWriter(filename, append: true);
            file.Write(s);
        }
    }
}
