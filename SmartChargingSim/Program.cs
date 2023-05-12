using System;
using System.Collections.Generic;
using N = SmartChargingSim.Network;

namespace SmartChargingSim
{
    internal class Program
    {
        static void Main(string[] args)
        {

            float[] connection_time = Input.ReadInput("connection_time.csv");
            float[] arrival_hours = Input.ReadInput("arrival_hours.csv");
            float[] charging_volume = Input.ReadInput("charging_volume.csv");
            float[,] solar = Input.ReadInput2D("solar.csv", 3);

            System.IO.File.WriteAllText("../../../LoadOverTime.txt", string.Empty);
            System.IO.File.WriteAllText("../../../PerformanceMeasures.txt", string.Empty);
            System.IO.File.WriteAllText("../../../StatisticalPerformanceMeasures.txt", string.Empty);

            Simulation sim;
            RandomInput input;
            //Here the simulations and scenarios can be changed
            List<int> sims = new List<int>() { 3,4 };//4 { sim, price, fcfs, elfs};
            List<int> scens = new List<int>() { 6};//6 { true, false, false, false, false, false };
            bool debug = false;
            int runtime = 2400;
            int numberOfRuns = 5;
            StatisticalPerformanceMeasures statMes = new StatisticalPerformanceMeasures(numberOfRuns);
            bool writePerfMeasures = YesNo("Do you want to write the performance measures?(y/n)");
            bool writeAveragePerfMeasures = YesNo("Do you want to write the average performance measures?(y/n)");
            bool writeTTests = YesNo("Do you want to write statistical performance measures?(y/n)");
            foreach (int n in sims)
            {
                foreach (int m in scens)
                {
                    AveragePerformanceMeasures averagePerformanceMeasures = new AveragePerformanceMeasures();
                    for (int i = 0; i < numberOfRuns; i++)
                    {
                        input = new RandomInput(connection_time, charging_volume, arrival_hours, solar);
                        Console.WriteLine($"starting sim {n}");
                        switch (n)
                        {
                            case 1:
                                sim = new Simulation(input, debug, writePerfMeasures, m);
                                break;
                            case 2:
                                sim = new PriceDrivenSim(input, debug, writePerfMeasures,m);
                                break;
                            case 3:
                                sim = new FCFSSim(input, debug, writePerfMeasures, m);
                                break;
                            case 4:
                                sim = new ELFSSim(input, debug, writePerfMeasures, m);
                                break;
                            default:
                                sim = new Simulation(input, debug, writePerfMeasures, m);
                                break;
                        }
                        sim.RunSim(runtime); // in hours
                        sim.performanceMeasures.WriteToFile();
                        averagePerformanceMeasures.AddPerformanceMeasures(sim.performanceMeasures);
                        statMes.AddPerformanceMeasures(sim.performanceMeasures);

                    }
                    string perfMes = averagePerformanceMeasures.Print(runtime);
                    averagePerformanceMeasures.WritePerformanceFile(perfMes);
                    if (writeAveragePerfMeasures)
                    {
                        Console.WriteLine(perfMes);
                    }
                }
            }
            if (writeTTests)
            {
                //Console.WriteLine(averagePerformanceMeasures.PrettyPrint(runtime));
                string statMesString = statMes.prettyPrintConfidence();
                statMes.WritePerformanceFile(statMesString);
                Console.WriteLine(statMes.prettyPrintAllPairs());
                Console.WriteLine(statMesString);
                Console.ReadLine();
            }
        }
        static bool YesNo(string s)
        {
            Console.WriteLine(s);
            var answer = Console.ReadLine();
            if (answer == "y" || answer == "")
                return true;
            return false;
        }
    }

}
