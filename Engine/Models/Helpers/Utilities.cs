using System;
using System.Text;

namespace Engine.Helpers
{
    public static class Utilities
    {
        public static TimeSpan GetTotalCalculationTime(DateTime? completed, DateTime created)
        { 
            return completed.GetValueOrDefault(DateTime.Now).Subtract(created);
        }

        public static string GetTotalCalculationTimeString(DateTime? completed, DateTime created)
        {
            var totalCalculationTime = GetTotalCalculationTime(completed, created);

            var calcTimeString = new StringBuilder();

            if (totalCalculationTime.Days > 0)
            {
                calcTimeString.AppendFormat("{0:%d} days ", totalCalculationTime);
            }

            if (totalCalculationTime.Hours > 0)
            {
                calcTimeString.AppendFormat("{0:%h} hours ", totalCalculationTime);
            }

            if (totalCalculationTime.Minutes > 0)
            {
                calcTimeString.AppendFormat("{0:%m} minutes ", totalCalculationTime);
            }

            if (totalCalculationTime.Seconds > 0)
            {
                calcTimeString.AppendFormat("{0:%s} seconds", totalCalculationTime);
            }

            return calcTimeString.ToString();   
        }
    }
}
