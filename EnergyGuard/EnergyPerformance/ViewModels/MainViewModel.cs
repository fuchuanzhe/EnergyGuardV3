﻿using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using EnergyPerformance.Services;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Hosting;

namespace EnergyPerformance.ViewModels;

/// <summary>
/// View model responsible for the main application view.
/// </summary>
public partial class MainViewModel : ObservableRecipient
{

    [ObservableProperty]
    private double budgetUsedPercent;

    private readonly EnergyUsageModel _model;
    private readonly ILocalSettingsService _settingsService;
    private readonly CpuInfo _cpuInfo;
    private readonly GpuInfo _gpuInfo;
    private readonly PowerInfo _powerInfo;
    private readonly IAppNotificationService _notificationService;

    public bool AutoControl => _settingsService.AutoControlSetting && _cpuInfo.IsSupported;

    // Admin privileges are needed to report CpuUsage, GpuUsage and Power.
    public double CpuUsage => _cpuInfo.CpuUsage;
    public double GpuUsage => _gpuInfo.GpuUsage;
    public double Power => _powerInfo.Power;

    public float CostThisWeek => _model.GetCostForCurrentWeek();

    public float CostPreviousWeek => _model.GetCostForPreviousWeek();

    /// <summary>
    /// Gets the total daily energy usage from the model. Unit is in Joules.
    /// </summary>
    public double AccumulatedWatts => _model.GetEnergyUsed();

    public double DailyCarbonEmission => _model.GetDailyCarbonEmission();

    /// <summary>
    /// Gets the time needed for a single tree to absorb the daily carbon emission. Unit is in hours.
    /// </summary>
    public double TimeToAbsorbEmission => _model.GetTimeToAbsorbEmission();

    /// <summary>
    /// Gets the cost saved after applying the personas. Unit is in GBP.
    /// Method only updates when the persona is applied.
    /// </summary>
    public double CostSaved => _model.GetCostSaved();

    /// <summary>
    /// Gets the selected mode from the settings service.
    /// </summary>
    public string SelectedMode
    {
        get => _settingsService.SelectedMode;
        set => _settingsService.SelectedMode = value;
    }

    /// <summary>
    /// Constructor for the MainViewModel.
    /// Sets the hardware info containers, services and model.
    /// Attaches event handlers for the hardware info containers and services.
    /// </summary>
    public MainViewModel(PowerInfo powerInfo, CpuInfo cpu, GpuInfo gpu
    , ILocalSettingsService settingsService, IAppNotificationService notificationService, EnergyUsageModel model)
    {
        // set hardware info containers
        _powerInfo = powerInfo;
        _cpuInfo = cpu;
        _gpuInfo = gpu;

        // set services
        _settingsService = settingsService;
        _notificationService = notificationService;

        // set model
        _model = model;

        // attach event handlers
        _powerInfo.PowerUsageChanged += Power_PropertyChanged;
        _cpuInfo.CpuUsageChanged += CpuUsage_PropertyChanged;
        _gpuInfo.GpuUsageChanged += GpuUsage_PropertyChanged;
        _settingsService.AutoControlEventHandler += AutoControl_PropertyChanged;
        _model.EnergyUsageChanged += AccumulatedWatts_PropertyChanged;

        // set percentage value for circular progress bar monitoring the current budget
        BudgetUsedPercent = (model.GetCostForCurrentWeek() / model.WeeklyBudget) * 100;
    }


    [RelayCommand]
    public async Task SelectAutoControl()
    {
        SelectedMode = "Auto";
        await _notificationService.ShowAsync("AutoControlEnabledNotification");
    }

    /// <summary>
    /// Relay Commands for selecting the different modes
    /// </summary>
    /// <returns></returns>

    [RelayCommand]
    public async Task SelectCasualMode()
    {
        SelectedMode = "Casual";
        await _notificationService.ShowAsync("CasualModeNotification");
    }

    [RelayCommand]
    public async Task SelectWorkMode()
    {
        SelectedMode = "Work";
        await _notificationService.ShowAsync("WorkModeNotification");
    }


    [RelayCommand]
    public async Task SelectPerformanceMode()
    {
        SelectedMode = "Performance";
        await _notificationService.ShowAsync("PerformanceModeNotification");
    }


    /// <summary>
    /// Updates the power property when the power usage changes.
    /// </summary>
    private void Power_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_powerInfo.Power))
        {
            OnPropertyChanged(nameof(Power));
        }
        return;
    }

    /// <summary>
    /// Updates the auto control property when the auto control setting changes.
    /// </summary>
    private void AutoControl_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.AutoControlSetting))
        {
            OnPropertyChanged(nameof(AutoControl));
        }
        return;
    }

    /// <summary>
    /// Updates the CPU usage property when the CPU usage changes.
    /// </summary>
    private void CpuUsage_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_cpuInfo.CpuUsage))
        {
            OnPropertyChanged(nameof(CpuUsage));
        }
        return;
    }


    /// <summary>
    /// Updates the GPU usage property when the GPU usage changes.
    /// </summary>
    private void GpuUsage_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_gpuInfo.GpuUsage))
        {
            OnPropertyChanged(nameof(GpuUsage));
        }
        return;
    }

    /// <summary>
    /// Updates the AccumulatedWatts property when the accumulated watts changes.
    /// </summary>
    private void AccumulatedWatts_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_model.AccumulatedEnergy))
        {
            OnPropertyChanged(nameof(AccumulatedWatts));
        }
        return;
    }

}