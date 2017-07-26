using System;
using System.Text;

namespace Engine
{
    public partial class Job
    {
        public TimeSpan TotalCalculationTime { get { return this.Completed.GetValueOrDefault().Subtract(this.Created); } }
        public string TotalCalculationTimeString {
            get
            {
                var calcTimeString = new StringBuilder();

                if(this.TotalCalculationTime.Days > 0)
                {
                    calcTimeString.AppendFormat("{0:%d} days ", this.TotalCalculationTime);
                }

                if (this.TotalCalculationTime.Hours > 0)
                {
                    calcTimeString.AppendFormat("{0:%h} hours ", this.TotalCalculationTime);
                }

                if (this.TotalCalculationTime.Minutes > 0)
                {
                    calcTimeString.AppendFormat("{0:%m} minutes ", this.TotalCalculationTime);
                }

                if (this.TotalCalculationTime.Seconds > 0)
                {
                    calcTimeString.AppendFormat("{0:%s} seconds", this.TotalCalculationTime);
                }

                return calcTimeString.ToString();
            }
        }
    }
}
