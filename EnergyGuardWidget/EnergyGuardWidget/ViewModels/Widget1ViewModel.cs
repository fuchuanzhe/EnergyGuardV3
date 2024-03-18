using EnergyGuardWidget.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyGuardWidget.ViewModels
{
    /// <summary>
    /// The ViewModel for the main widget.
    /// </summary>
    public class Widget1ViewModel
    {
        public double CostThisWeek
        {
            get; set;
        }

        public double CostSaved
        {
            get; set;
        }

        // TODO: Look into inter-process communication between EnergyGuard and widget.
        public Widget1ViewModel() 
        {
            CostThisWeek = 0;
            CostSaved = 0;

        }
    }
}
