using System;

namespace WinterRose
{
    /// <summary>
    /// All extra Math methods Snow needs
    /// </summary>
    public static partial class MathS
    {
        private static int ToIntFloor(object num)
        {
            if (decimal.TryParse(num.ToString(), out decimal numResult))
                if (int.TryParse(System.Math.Floor(numResult).ToString(), out int result))
                    return result;
            return -1;
        }

        private static int ToIntCeiling(object num)
        {
            if (decimal.TryParse(num.ToString(), out decimal numResult))
                if (int.TryParse(System.Math.Ceiling(numResult).ToString(), out int result))
                    return result;
            return -1;
        }

        /// <summary>
        /// Gets the hexadecimal value from the given int
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string GetHexadecimal(int num) => Convert.ToString(num, 16).ToUpper();
        /// <summary>
        /// gets the integer value from the given hexadecimal
        /// </summary>
        /// <param name="hexadecimal"></param>
        /// <returns></returns>
        public static int GetNumber(string hexadecimal) => int.Parse(hexadecimal, System.Globalization.NumberStyles.HexNumber);

        /// <summary>
        /// Get the persentage based on the given parameters
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="decimals"></param>
        /// <returns>the persentage calculated from the given parameters</returns>
        public static double GetPercentage(double current, double max, int decimals) => Math.Round(double.Parse($"{new System.Data.DataTable().Compute($"100 / {max} * {current}", "")}"), decimals);
        /// <summary>
        /// Get the persentage based on the given parameters
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="decimals"></param>
        /// <returns>the persentage calculated from the given parameters</returns>
        public static double GetPercentage(int current, double max, int decimals) => Math.Round(double.Parse($"{new System.Data.DataTable().Compute($"100 / {max} * {current}", "")}"), decimals);
        /// <summary>
        /// Get the persentage based on the given parameters
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="decimals"></param>
        /// <returns>the persentage calculated from the given parameters</returns>
        public static double GetPercentage(double current, int max, int decimals) => Math.Round(double.Parse($"{new System.Data.DataTable().Compute($"100 / {max} * {current}", "")}"), decimals);
        /// <summary>
        /// Get the persentage based on the given parameters
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="decimals"></param>
        /// <returns>the persentage calculated from the given parameters</returns>
        public static double GetPercentage(float current, float max, int decimals) => Math.Round(double.Parse($"{new System.Data.DataTable().Compute($"100 / {max} * {current}", "")}"), decimals);
        /// <summary>
        /// Get the persentage based on the given parameters
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="decimals"></param>
        /// <returns>the persentage calculated from the given parameters</returns>
        public static double GetPercentage(float current, double max, int decimals) => Math.Round(double.Parse($"{new System.Data.DataTable().Compute($"100 / {max} * {current}", "")}"), decimals);
        /// <summary>
        /// Get the persentage based on the given parameters
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="decimals"></param>
        /// <returns>the persentage calculated from the given parameters</returns>
        public static double GetPercentage(double current, float max, int decimals) => Math.Round(double.Parse($"{new System.Data.DataTable().Compute($"100 / {max} * {current}", "")}"), decimals);
        /// <summary>
        /// Get the persentage based on the given parameters
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="decimals"></param>
        /// <returns>the persentage calculated from the given parameters</returns>
        public static double GetPercentage(int current, float max, int decimals) => Math.Round(double.Parse($"{new System.Data.DataTable().Compute($"100 / {max} * {current}", "")}"), decimals);
        /// <summary>
        /// Get the persentage based on the given parameters
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="decimals"></param>
        /// <returns>the persentage calculated from the given parameters</returns>
        public static double GetPercentage(float current, int max, int decimals) => Math.Round(double.Parse($"{new System.Data.DataTable().Compute($"100 / {max} * {current}", "")}"), decimals);
        /// <summary>
        /// Get the persentage based on the given parameters
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        /// <param name="decimals"></param>
        /// <returns>the persentage calculated from the given parameters</returns>
        public static double GetPercentage(int current, int max, int decimals) => Math.Round(double.Parse($"{new System.Data.DataTable().Compute($"100 / {max} * {current}", "")}"), decimals);
    }
}
