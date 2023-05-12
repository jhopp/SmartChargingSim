using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartChargingSim
{
    internal class PerformanceMeasures
    {
        // general stats
        public float numberOfCars = 0;
        public float numberOfStartCharging = 0;
        public float numberOfStopCharging = 0;
        public float maxTimeBetweenArrivals = 0;


        // 1: Cable Load
        public float [] MaxLoad;
        public float [] TotalTimeOverloaded;    //The total time that a cable has more than 220kW load for each cables
        public float [] TotalTimeUnderloaded;   //The total time that a cable has less than 220kW load for each cables
        public float[] averageTimeOverloaded;   //Fraction of time that a cable has more than 220kW load for each cables
        public float[] averageTimeUnderloaded;  //Fraction of time that a cable has less than 220kW load for each cables
        public List<float>[] LoadOverTime;

        // 2: Departure delays
        public float TotalDelayedVehicles;
        public float TotalDelayedTime;
        public float MaxDelay;

        public float averageTimeDelay = 0;

        // 3: Non-served
        public float TotalNonServed;
        public float NonServedPerDay;
        public float FractionNonServed;

        //4: Steady Cycle
        public float StableCycleDay;

        public float[] solarUsed;
        public float[] solarGenerated;
        public PerformanceMeasures()
        {
            TotalNonServed = 0;
            StableCycleDay = 0;
            MaxLoad = new float[9];
            TotalTimeOverloaded = new float[9];
            TotalTimeUnderloaded = new float[9];
            LoadOverTime = new List<float>[10];
            solarUsed = new float[7];
            solarGenerated = new float[7];
            for (int i = 0; i < LoadOverTime.Length; i++)
            {
                LoadOverTime[i] = new List<float>();
            }
        }

        /// <summary>
        /// Print the performance measures nicely formatted
        /// </summary>
        /// <param name="time">the current time</param>
        /// <returns>A string of formatted performance measures</returns>
        public virtual string PrettyPrint(float time)
        {
            StringBuilder sb = new StringBuilder();
            if (TotalDelayedVehicles != 0)
                averageTimeDelay = (float)Math.Round(TotalDelayedTime / (numberOfCars), 3);
            sb.AppendLine($" ");
            sb.AppendLine($"Total number of cars arrived: {numberOfCars}");
            sb.AppendLine($"Steady cycle started at day: {StableCycleDay}");
            sb.AppendLine($"Number of cars not served: {TotalNonServed}");
            FractionNonServed = TotalNonServed / numberOfCars;
            sb.AppendLine($"Fraction of cars not served: {FractionNonServed}");
            NonServedPerDay = TotalNonServed / (time / 24);
            sb.AppendLine($"Number of cars not served per day: {NonServedPerDay}");
            sb.AppendLine($"Max. time between arrivals: {maxTimeBetweenArrivals}");
            sb.AppendLine($" ");
            sb.AppendLine($"Total number of delayed cars: {TotalDelayedVehicles}");
            sb.AppendLine($"Max. delay time: {MaxDelay}");
            sb.AppendLine($"Average time delay: {averageTimeDelay}");
            sb.AppendLine($" ");
            sb.AppendLine($"Max. load on each cable: {ATS(MaxLoad)}");
            averageTimeOverloaded = DAR(TotalTimeOverloaded, time);
            averageTimeUnderloaded = DAR(TotalTimeUnderloaded, time);
            sb.AppendLine($"Fraction of time cables overloaded  (110%>=): {ATS(averageTimeOverloaded)}");
            sb.AppendLine($"Fraction of time cables underloaded (110%<=): {ATS(averageTimeUnderloaded)}");
            sb.AppendLine($" ");
            sb.AppendLine($"Fraction of generated solar used: {solarUsed.Sum()/ solarGenerated.Sum()}");
            return sb.ToString();
        }

        //Write plottable performance measures to file
        public void WriteToFile()
        {
            string filename = "../../../LoadOverTime.txt";
            StringBuilder sb = new StringBuilder();
            foreach (List<float> item in LoadOverTime)
            {
                sb.AppendLine(string.Join(";",item));
            }
            using StreamWriter file = new(filename, append: true);
            file.Write(sb.ToString());

        }

        /// <summary>
        /// Converts an array of type T to string
        /// </summary>
        /// <typeparam name="T">type of array</typeparam>
        /// <param name="a">the array</param>
        /// <returns>A string of concatenated array items</returns>
        public string ATS<T>(T[] a)
        {
            return "[" + string.Join(", ", a) + "]";
        }
        //ATS but for Lists
        public string LTS<T>(List<T> a)
        {
            return "[" + string.Join(", ", a) + "]";
        }

        // divide every value in an array (by time) and round values
        public float[] DAR(float[] a, float time)
        {
            return a.Select(x => (float)Math.Round(x / (time- 24*StableCycleDay), 3)).ToArray();
        }
        //divide array a by array b if b not zero
        public float [] DAA(float []a, float[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (b[i] > 0)
                    a[i] = a[i] / b[i];
                else
                    a[i] = 0;
            }
            return a;
        }
        public virtual void WritePerformanceFile(string s)
        {
            string filename = "../../../PerformanceMeasures.txt";
            using StreamWriter file = new StreamWriter(filename, append: true);
            file.Write(s);
        }
        /// <summary>
        /// Prints the performance measures without any formatting
        /// </summary>
        /// <param name="time"></param>
        /// <returns>A string of performancemeasures</returns>
        public virtual string Print(float time)
        {
            StringBuilder sb = new StringBuilder();
            if (TotalDelayedVehicles != 0)
                averageTimeDelay = (float)Math.Round(TotalDelayedTime / (TotalDelayedVehicles), 3);
            sb.AppendLine($" ");
            FractionNonServed = TotalNonServed / numberOfCars;
            sb.AppendLine($"{FractionNonServed}");
            NonServedPerDay = TotalNonServed / (time / 24);
            sb.AppendLine($"{NonServedPerDay}");
            sb.AppendLine($"{maxTimeBetweenArrivals}");
            sb.AppendLine($"{TotalDelayedVehicles}");
            sb.AppendLine($"{MaxDelay}");
            sb.AppendLine($"{averageTimeDelay}");
            for(int i = 0; i <MaxLoad.Length; i++)
                sb.AppendLine($"{MaxLoad[i]}");
            averageTimeOverloaded = DAR(TotalTimeOverloaded, time);
            averageTimeUnderloaded = DAR(TotalTimeUnderloaded, time);
            for (int i = 0; i < averageTimeOverloaded.Length; i++)
                sb.AppendLine($"{averageTimeOverloaded[i]}");
            sb.AppendLine($" ");
            return sb.ToString();
        }
    }
}
