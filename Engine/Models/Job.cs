﻿using Engine.Helpers;
using System;

namespace Engine
{
    public partial class Job
    {
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
