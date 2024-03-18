using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnergyPerformance.Views;
using EnergyPerformance.Models;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Services;
using EnergyPerformance.Helpers;

namespace EnergyPerformance.ViewModels;

/// <summary>
/// ViewModel for the popup page.
/// </summary>
public partial class PopupViewModel : ObservableRecipient
{
    private double _rateValue;
    private string? _selectedCompany;
    private string? _errorMessage;
    private readonly IMediatorService _mediatorService;
    private readonly CompanyService _companyService;
    public List<string> CompanyList { get; set; }

    private RelayCommand? _validateAndUpdateInput;

    public PopupViewModel(IMediatorService mediatorService, CompanyService companyService)
    {
        _mediatorService = mediatorService;
        _companyService = companyService;

        CompanyList = _companyService.CompanyList;
    }

    public double RateValue
    {
        get => _rateValue;

        set
        {
            if (value != _rateValue)
            {
                _rateValue = value;
                OnPropertyChanged(nameof(RateValue));
            }
        }
    }

    public string SelectedCompany
    {
        get => _selectedCompany;

        set
        {
            if (value != _selectedCompany)
            {
                _selectedCompany = value;
                OnPropertyChanged(nameof(SelectedCompany));
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;

        set
        {
            if(value !=  _errorMessage)
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }
    }

    public RelayCommand ValidateAndUpdateInputCommand
    {
        get
        {
            if (_validateAndUpdateInput == null)
            {
                _validateAndUpdateInput = new RelayCommand(ValidateAndUpdateInput);
            }
            return _validateAndUpdateInput;
        }
    }

    /// <summary>
    /// Function to check if the columns are filled with proper information. Sends the user input to other services and pages with mediator service.
    /// </summary>
    private void ValidateAndUpdateInput()
    {
        if(RateValue < 0 || RateValue > 3)
        {
            ErrorMessage = "please enter the rate between 0 and 3 correctly";
        }
        else if (string.IsNullOrEmpty(SelectedCompany) || RateValue == 0)
        {
            ErrorMessage = "Please select an option and enter a number.";
        }
        else
        {
            _mediatorService.RateValue = RateValue;
            _mediatorService.SelectedCompany = SelectedCompany;
            App.MainWindow.Content = App.GetService<ShellPage>();
        }
    }
}