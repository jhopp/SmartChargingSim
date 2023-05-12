using System;
using System.Collections.Generic;
using System.IO;

namespace SmartChargingSim
{
    internal class Input
    {
        // Returns a list of floats read from a CSV file
        public static List<float> ReadFromCSV(string filename)
        {
            List<float> result = new List<float>();

            try
            {
                StreamReader reader = new StreamReader("../../../../" + filename);
                if (!reader.EndOfStream) reader.ReadLine(); // header

                // read lines as long as there are lines to read
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    foreach (string i in line.Split(";"))
                    {
                        result.Add(float.Parse(i));
                    }
                }
            }
            // file not found
            catch (Exception e) { Console.WriteLine(e); Console.WriteLine(filename); }

            return result;
        }

        // Reads input using ReadFromCSV and store into array
        public static float[] ReadInput(string filename)
        {
            List<float> input = ReadFromCSV(filename);
            int rows = input.Count / 2;
            float[] result = new float[rows];

            for (int i = 0; i < rows; i++)
            {
                result[i] = input[i * 2 + 1];
            }
            return result;
        }

        // Reads input using ReadFromCSV and store into 2D-array
        public static float[,] ReadInput2D(string filename, int columns)
        {
            List<float> input = ReadFromCSV(filename);
            int rows = input.Count / columns;
            float[,] result = new float[rows, columns - 1];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 1; j < columns; j++)
                {
                    result[i, j - 1] = input[i * columns + j];
                }
            }
            return result;
        }
    }
}
