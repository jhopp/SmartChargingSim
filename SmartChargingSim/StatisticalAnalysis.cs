using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartChargingSim
{
    internal class StatisticalAnalysis
    {
        /// <summary>
        /// Given two lists, returns the confidence interval from a paired t-test
        /// </summary>
        /// <param name="list1">The first list of observations</param>
        /// <param name="list2">the second list of observations</param>
        /// <returns>The confidence interval for the two lists from a paired t-test</returns>
        public (float, float) PairedTConfidence(List<float> list1, List<float> list2, float alpha = 0.05f)
        {
            int n = list1.Count;
            float Zn = 0;
            float[] Zi = new float[n];
            for (int i = 0; i < n; i++)
            {
                float z = list1[i] - list2[i];
                Zi[i] = z;
                Zn += z;
            }
            Zn /= n;
            float S2 = 0;
            for (int i = 0; i < n; i++)
            {
                S2 += (Zi[i] - Zn) * (Zi[i] - Zn);
            }
            S2 /= n - 1;
            float[] TalphaN;
            switch (alpha)
            {
                case 0.05f:
                    TalphaN = new float[] { 12.706f, 4.303f, 3.182f, 2.776f, 2.571f, 2.447f, 2.365f, 2.306f };
                    break;
                case 0.01f:
                    TalphaN = new float[] { 63.657f, 9.925f, 5.841f, 4.604f, 4.032f, 3.707f, 3.499f, 3.355f };
                    break;
                default:
                    TalphaN = new float[] { 12.706f, 4.303f, 3.182f, 2.776f, 2.571f, 2.447f, 2.365f, 2.306f };
                    break;

            }
            float delta = (float)(TalphaN[n - 1] * (Math.Sqrt(S2 / n)));
            return (Zn-delta, Zn+delta);
        }
        /// <summary>
        /// Generate all pairwise confidence intervals
        /// </summary>
        /// <param name="lists1"></param>
        /// <returns></returns>
        public (float, float) [,]AllPairWiseConfidence(List<List<float>> lists1)
        {
            (float, float)[,] confidenceIntervals = new (float, float)[lists1.Count, lists1.Count];
            for (int i = 0; i < lists1.Count; i++)
            {
                for(int j = 0; j < lists1.Count; j++)
                {
                    if(j < i)
                        confidenceIntervals[i,j] = PairedTConfidence(lists1[i], lists1[j], 0.01f);
                }
            }
            return confidenceIntervals;
        }
    }
}
