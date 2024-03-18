using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyPerformance.Helpers;

public class CarbonIntensityInfo
{
    private double _carbonIntensity;

    public event PropertyChangedEventHandler? CarbonIntensityChanged;
    public double CarbonIntensity
    {
        get => _carbonIntensity;
        set
        {
            _carbonIntensity = value;
            CarbonIntensityChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CarbonIntensity)));
        }
    }
    public CarbonIntensityInfo()
    {
        // Default carbon intensity is 100
        CarbonIntensity = 100;
    }
}