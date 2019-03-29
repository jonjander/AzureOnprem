using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Tools
{
    public static class Utils
    {
        public static bool ToBool(double input)
        {
            bool output;
            if (input > 0.5)
            {
                output = true;
            }
            else
            {
                output = false;
            }
            return output;
        }
        public static double flattern(double input)
        {
            double output = input;
            if (output < 0) { output = 0; }
            if (output > 1) { output = 1; }
            return output;
        }
        public static double flattern(float input)
        {
            double output = (double)input;
            if (output < 0) { output = 0; }
            if (output > 1) { output = 1; }
            return output;
        }
        public static double ToDouble(bool input)
        {
            if (input)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static double Remap(this double value, double from1, double to1, double from2, double to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}
