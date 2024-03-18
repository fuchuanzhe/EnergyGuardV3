using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using EnergyPerformance.Contracts.Services;
using HidSharp.Reports;

namespace EnergyPerformance.Services;

/// <summary>
/// Mediator Service for loosely coupling data transfer between pages/viewmodels.
/// Facilitates communication with event handlers.
/// </summary>
public class MediatorService : IMediatorService
{
    private double _rateValue;
    private string _selectedCompany;

    /// <summary>
    /// Gets or sets the current rate value from popup page to settings page.
    /// </summary>
    public double RateValue
    {
        get => _rateValue;
        set
        {
            if (_rateValue != value)
            {
                _rateValue = value;
                RateValueChanged?.Invoke(this, _rateValue);
            }
        }
    }

    /// <summary>
    /// Gets or sets the current company value from popup page to settings page.
    /// </summary>
    public string SelectedCompany
    {
        get => _selectedCompany;
        set
        {
            if (_selectedCompany != value)
            {
                _selectedCompany = value;
                SelectedCompanyChanged?.Invoke(this, _selectedCompany);
            }
        }
    }

    public event EventHandler<double>? RateValueChanged;
    public event EventHandler<string>? SelectedCompanyChanged;
}
