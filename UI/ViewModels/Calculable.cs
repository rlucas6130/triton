using System;
using UI.ViewModels.Helpers;

namespace UI.ViewModels
{
    public abstract class Calculable
    {
        public DateTime Created { get; set; }
        public DateTime? Completed { get; set; }

        public TimeSpan TotalCalculationTime
        {
            get
            {
                return Utilities.GetTotalCalculationTime(this.Completed, this.Created);
            }
        }

        public string TotalCalculationTimeString
        {
            get
            {
                return Utilities.GetTotalCalculationTimeString(this.Completed, this.Created);
            }
        }
    }
}