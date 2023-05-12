using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SmartChargingSim
{
    internal class RandomInput
    {
        float[] chargingCDF;
        float[] connectionCDF;
        float[] arrivalTimes;
        float[,] solarRevenues;
        float[] firstChoiceParking;
        float[] tempFirstChoiceParking;
        float[] firstChoiceParkingCDF;
        float[] tempFirstChoiceParkingCDF;
        /// <summary>
        /// Returns a generator to pull charging volumes, connection times, interarrival times and solar revenue from distributions
        /// </summary>
        /// <param name="input_ct">The connection time distribution</param>
        /// <param name="input_cv">The charging volume distribution</param>
        /// <param name="input_at">The arrival time distribution</param>
        /// <param name="input_sol">The solar distribution</param>
        public RandomInput(float[] input_ct, float[] input_cv, float[] input_at, float[,] input_sol)
        {
            RandomNumberGenerator.Create();
            chargingCDF = ToCDF(input_cv);
            connectionCDF = ToCDF(input_ct);
            arrivalTimes = input_at;
            solarRevenues = input_sol;
            firstChoiceParking = new float[] { 0.15f, 0.15f, 0.15f, 0.20f, 0.15f, 0.10f, 0.10f };
            firstChoiceParkingCDF = ToCDF(firstChoiceParking);
            arrivalTimes = arrivalTimes.Select(x => x * 750).ToArray();
        }

        // Returns cumulative distribution converted from input
        public float[] ToCDF(float[] input)
        {
            float[] result = new float[input.Length];
            result[0] = input[0];
            for (int i = 1; i < input.Length; i++)
            {
                result[i] = input[i] + result[i - 1];
            }
            return result;
        }
        
        // Returns generated connection time based on charging time
        public float GenerateConnectionTime(float chargingTime)
        {
            float connectionTime = RandomNumberFromCDF(connectionCDF);
            
            if (connectionTime * 0.7 < chargingTime)
                return (float)(chargingTime / 0.7);
            return connectionTime;
        }

        // Returns generated charging time from CDF
        public float GenerateChargingTime()
        {
            float chargingVolume = RandomNumberFromCDF(chargingCDF);
            return (float)(chargingVolume / 6.0);
        }

        // Returns an index from CDF 
        public float RandomNumberFromCDF(float[] CDF)
        {
            float x = getFloat();
            float add = getFloat();
            for (int i = 0; i < CDF.Length; i++)
            {
                if (CDF[i] > x)
                    return i + add;
            }
            
            return CDF.Length - 1 + add;
        }

        // Returns generated arrival time based on current simulation time
        public float GenerateArrivalTime(float currentTime )
        {
            float time = currentTime;
            currentTime = currentTime - 0.5f;
            int currentHour = (int)currentTime;
            float expectedCars = ( arrivalTimes[(currentHour + 1) % 24] - arrivalTimes[currentHour % 24]) * ((currentTime)%1) + arrivalTimes[currentHour%24];

            var x = getFloat();
            var generatedtime =  (-1/expectedCars)*(float)Math.Log(1 - x);
            if (generatedtime > 2)
                return 2 + GenerateArrivalTime(time+2);
            return generatedtime;

        }

        // returns initial parking place num based on probabilities given in problem descriptions
        public List<int> GenerateParkingPlaceNums()
        {
            List<int> res = new List<int>();
            tempFirstChoiceParking = (float[])firstChoiceParking.Clone();
            tempFirstChoiceParkingCDF = (float[])firstChoiceParkingCDF.Clone(); // ROUNDING ERRORS?

            double num;
            while (res.Count <= 2)
            {
                num = getFloat();
                for (int i = 0; i < tempFirstChoiceParking.Length; i++)
                {
                    // if this is the selected num, add it to the result
                    if (tempFirstChoiceParkingCDF[i] >= num && tempFirstChoiceParking[i] != 0)
                    {
                        res.Add(i + 1);
                        tempFirstChoiceParking[i] = 0.00f; // set prob of picking this place to 0
                        float sum = tempFirstChoiceParking.Sum();

                        // recompute probabilities
                        for (int j = 0; j < tempFirstChoiceParking.Length; j++)
                            tempFirstChoiceParking[j] = tempFirstChoiceParking[j] / sum;

                        //Console.WriteLine(firstChoiceParkingTemp.Sum()); // should be 1
                        tempFirstChoiceParkingCDF = ToCDF(tempFirstChoiceParking);
                        break;
                    }
                }
            }
            // returns real parking place number (starting at 1)
            return res;
        }

        // Returns current solar revenue, based on simulation time and season
        public float GetCurrentSolarRevenue(float time, bool summer)
        {

            int solarTime = (int)Math.Floor(time % 24);
            int season;
            if (summer) season = 1; else season = 0;

            float solarRevenue = solarRevenues[solarTime, season] * 200;

            double a = 1 - getFloat();
            double b = 1 - getFloat();
            double c = (Math.Sqrt(-2.0 * Math.Log(a)) * Math.Cos(2.0 * b * Math.PI));

            // return gaus distr float with solarRevenue as mean and stddv of 0.15 * solarRevenue 
            return (float) (0.15 * solarRevenue * c + solarRevenue);
        }

        /// <summary>
        /// Returns a float between 0 and 1 using the cryptographic randomnumbergenerators
        /// </summary>
        /// <returns>A random float between 0 and 1</returns>
        private float getFloat()
        {
            int a= RandomNumberGenerator.GetInt32(0, int.MaxValue);

            return (float)a / (float)int.MaxValue;
        }
    }
}
