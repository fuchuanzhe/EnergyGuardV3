using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using EnergyPerformance.Helpers;
using EnergyPerformance.Services;
using Microsoft.UI.Xaml.Input;

namespace EnergyPerformance.Models;


/// <summary> 
/// This class is responsible for storing the data that is used by the EnergyUsageViewModel 
/// </summary>
public class EnergyUsageModel
{
    // initialize default values for fallback in case there is no file or the file is corrupted
    private readonly double DefaultWeeklyBudget = 2.0;

    private double _costPerKwh;
    private string _company = "";

    private double _liveCostPerKwh;
    private string _liveCompany;

    private const int DoublePrecision = 2;

    private readonly CarbonIntensityInfo _carbonIntensityInfo;
    private readonly EnergyRateInfo _energyRateInfo;

    private EnergyUsageData _energyUsage;
    private readonly IDatabaseService _databaseService;
    private readonly IMediatorService _mediatorService;

    private double _accumulatedEnergy;

    public event PropertyChangedEventHandler? EnergyUsageChanged;

    public DateTime SelectedDate
    {
        get; set;
    }

    public String SelectedModel
    {
        get; set;
    }

    /// <summary>
    /// Encodes whether the energy rate is fetched live
    /// </summary>
    public bool IsLiveCost
    {
        get; set;
    }

    private bool IsCostRetrieved
    {
        get; set;
    }

    public DateTimeOffset CurrentDay
    {
        get; set;
    }
    public DateTimeOffset CurrentHour
    {
        get; set;
    }

    public double CarbonIntensity
    {
        get => _carbonIntensityInfo.CarbonIntensity;
        private set => _carbonIntensityInfo.CarbonIntensity = value;
    }

    /// <summary>
    /// The energy supplier company used. Returns data loaded from file or default value if file is not found or corrupted.
    /// </summary>
    public string Company
    {
        get
        {
            if(_company == "" && IsLiveCost)
            {
                _company = _liveCompany;
            }
            return _company;
        }

        set
        {
            if(_company != value)
            {
                _company = value;
            }
        }
    }

    /// <summary>
    /// The emission per kWh for the user. Returns data loaded from file or default value if file is not found or corrupted.
    /// </summary>
    public double CostPerKwh
    {
        get
        {
            // additional check incase the file is corrupted, modified, or is initialized for the first time
            if (IsLiveCost && !IsCostRetrieved)
            {
                _costPerKwh = _liveCostPerKwh;
                IsCostRetrieved = true;        // Flag added to enable any changes made in costperkwh in settings page.
            }

            return _costPerKwh;
        }
        set
        {
            if (!double.IsNaN(value) && value > 0)
            {
                _costPerKwh = value;
            }
        }
    }

    /// <summary>
    /// The weekly budget for the user in kWh. Returns data loaded from file or default value if file is not found or corrupted.
    /// </summary>
    public double WeeklyBudget
    {
        get
        {
            var budget = DefaultWeeklyBudget;
            // additional check incase the file is corrupted or modified
            if (!double.IsNaN(_energyUsage.WeeklyBudget) && _energyUsage.WeeklyBudget > 0)
            {
                budget = _energyUsage.WeeklyBudget;
            }
            else
            {
                _energyUsage.WeeklyBudget = budget;
            }
            return budget;
        }
        set
        {
            if (!double.IsNaN(value) && value > 0)
            {
                _energyUsage.WeeklyBudget = value;
            }
        }
    }

    /// <summary>
    /// The accumulated energy usage used for the current day. Unit is in Joules.
    /// </summary>
    public double AccumulatedEnergy
    {
        get => _accumulatedEnergy;
        set
        {
            _accumulatedEnergy = value;
            EnergyUsageChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccumulatedEnergy)));
        }
    }

    /// <summary>
    /// Theaccumulated energy usage used for the current hour. Unit is in Joules.
    /// </summary>
    public virtual double AccumulatedEnergyHourly
    {
        get; set;
    }

    /// <summary>
    /// The Accumulated Watts used per app for the current day.
    /// </summary>
    public Dictionary<string, double> AccumulatedEnergyPerApp
    {
        get; set;
    }

    public bool PersonaEnabled
    {
        get; set;
    }

    /// <summary>
    /// The reference energy usage to be compared in GetCostSaved().
    /// </summary>
    public IEnumerable<EnergyUsageLog> CostSavedRef
    {
        get; set; 
    }
    
    public DateTime PersonaStartTime
    {
        get; set; 
    }

    /// <summary>
    /// Constructor for the EnergyUsageModel, basic initialization is performed here.
    /// Full initialization is performed in the InitializeAsync method.
    /// </summary>
    /// <param name="fileService"></param>
    public EnergyUsageModel(CarbonIntensityInfo carbonIntensityInfo, EnergyRateInfo energyRateInfo, IDatabaseService databaseService, IMediatorService mediatorService)
    {
        CurrentDay = DateTimeOffset.Now;
        CurrentHour = DateTimeOffset.Now;
        SelectedModel = "Energy Usage";
        AccumulatedEnergy = 0;
        AccumulatedEnergyHourly = 0;
        AccumulatedEnergyPerApp = new Dictionary<string, double>();
        _energyUsage = new EnergyUsageData();
        _carbonIntensityInfo = carbonIntensityInfo;
        _energyRateInfo = energyRateInfo;
        _databaseService = databaseService;
        _mediatorService = mediatorService;
        _mediatorService.RateValueChanged += OnRateValueChanged;
        CostSavedRef = new List<EnergyUsageLog>();
        PersonaEnabled = false;
    }

    /// <summary>
    /// Loads the costperkwh information from popup to the model via mediator service, activates with an event.
    /// </summary>
    private void OnRateValueChanged(object sender, double newRateValue)
    {
        _costPerKwh = newRateValue;
    }

    /// <summary>
    /// Performs asynchronous tasks required at initialization, including loading data from files. 
    /// Called by the ActivationService.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Initialize energyFileService
        await _databaseService.InitializeDB();
        _energyUsage = await _databaseService.LoadUsageData();
        var current = DateTime.Now;
        if (_energyUsage.Diaries.Count > 0 && _energyUsage.Diaries.Last().Date.Date == current.Date)
        {
            var lastDiary = _energyUsage.Diaries.Last();
            AccumulatedEnergy = ConvertKwhToWs(lastDiary.DailyUsage.PowerUsed);
            if (lastDiary.HourlyUsage.Count > 0 && lastDiary.HourlyUsage.Last().Date.Hour == current.Hour)
                AccumulatedEnergyHourly = ConvertKwhToWs(lastDiary.HourlyUsage.Last().PowerUsed);
            else
                AccumulatedEnergyHourly = 0;
        }
        else
        {
            AccumulatedEnergy = 0;
            AccumulatedEnergyHourly = 0;
        }

        (_liveCostPerKwh, _, _liveCompany) = _databaseService.RetrieveLatestCostPerKwhBudgetCompany();
        IsLiveCost = _liveCostPerKwh != 0 && !string.IsNullOrEmpty(_liveCompany);
        IsCostRetrieved = false;
    }

    /// <summary>
    /// Saves the current state of the model to the data file in local app data storage through the EnergyUsageFileService.
    /// </summary>
    public async Task Save()
    {
        Update(); // update the model before saving.
        Debug.WriteLine("Saving model.");
        await _databaseService.SaveEnergyData(_energyUsage);
    }

    /// <summary>
    /// Calculates the total emission of energy used for all the days in the current week.
    /// </summary>
    /// <returns>Cost of energy for the previous week</returns>
    public float GetCostForCurrentWeek()
    {
        var current = DateTime.Now;
        float cost = 0;
        var start = _energyUsage.Diaries.Count - 1;
        for (var i = start; i >= 0 && i > start - 7; i--)
        {
            var log = _energyUsage.Diaries[i];
            var date = log.Date.Date;
            if (!CheckTwoDatesAreInTheSameWeek(current, date))
                break;
            cost += log.DailyUsage.Cost;
        }
        return cost;
    }

    /// <summary>
    /// Calculates the total emission of energy used for all the days in the previous week.
    /// </summary>
    /// <returns>Cost of energy for the previous week</returns>
    public float GetCostForPreviousWeek()
    {
        // iterate through _energyUsage.DailyLogs backwards and calculate the emission for all the days in the previous week
        var current = DateTime.Now;
        var previousWeek = current.AddDays(-7);

        float cost = 0;
        var start = _energyUsage.Diaries.Count - 1;
        for (var i = start; i >= 0 && i > start - 14; i--)
        {
            var log = _energyUsage.Diaries[i];
            var date = log.Date.Date;
            if (CheckTwoDatesAreInTheSameWeek(previousWeek, date))
                cost += log.DailyUsage.Cost;
        }

        return cost;
    }

    public float GetEmissionForCurrentWeek()
    {
        var current = DateTime.Now;
        float emission = 0;
        var start = _energyUsage.Diaries.Count - 1;
        for (var i = start; i >= 0 && i > start - 7; i--)
        {
            var log = _energyUsage.Diaries[i];
            var date = log.Date.Date;
            if (!CheckTwoDatesAreInTheSameWeek(current, date))
                break;
            emission += log.DailyUsage.CarbonEmission;
        }
        return emission;
    }

    /// <summary>
    /// Checks if two dates are in the same week.
    /// </summary>
    /// <param name="date1">The first date.</param>
    /// <param name="date2">The second date.</param>
    /// <returns>True if the two dates are in the same week, false otherwise.</returns>
    private bool CheckTwoDatesAreInTheSameWeek(DateTime date1, DateTime date2)
    {
        // Calculates the starting day of the week for both given dates and checks if they are the same.
        // If true, both dates are in the same week.

        // Note: DayOfWeek's built-in enum starts from Sunday so subtract 1 at the end
        var cal = DateTimeFormatInfo.CurrentInfo.Calendar;
        var startDayOfWeekDate1 = date1.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date1) - 1);
        var startDayOfWeekDate2 = date2.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date2) - 1);
        return startDayOfWeekDate1 == startDayOfWeekDate2;
    }

    /// <summary>
    /// Returns the daily energy usage logs from the model.
    /// </summary>
    public List<EnergyUsageLog> GetDailyEnergyUsageLogs()
    {
        var dailyLogs = new List<EnergyUsageLog>();

        foreach (var diary in _energyUsage.Diaries)
        {
            dailyLogs.Add(diary.DailyUsage);
        }

        return dailyLogs;
    }

    /// <summary>
    /// Returns the hourly energy usage logs from the model.
    /// </summary>
    public List<EnergyUsageLog> GetHourlyEnergyUsageLogs(DateTime date)
    {

        foreach (var diary in _energyUsage.Diaries)
        {
            if (diary.Date.Date == date.Date)
                return diary.HourlyUsage;
        }

        return new List<EnergyUsageLog>();
    }

    /// <summary>
    /// Returns all applications' energy log on a given day
    /// </summary>
    /// <param name="date"></param>
    public List<(string, EnergyUsageLog)> GetPerAppUsageLogs(DateTime date)
    {
        foreach (var diary in _energyUsage.Diaries)
            if (diary.Date.Date == date.Date)
                return diary.PerProcUsage.Select(x => (x.Key, x.Value)).ToList();

        return new List<(string, EnergyUsageLog)>();
    }

    /// <summary>
    /// Calculates the energy used in the last day.
    /// </summary>
    public double GetEnergyUsed()
    {
        /// 1s is the sampling rate, divide by 3.6Mil to convert Ws -> kWh
        var energyUsed = ConvertWsToKwh(AccumulatedEnergy);
        return energyUsed;

    }

    public double GetEnergyUsed(string proc)
    {
        var energyUsed = ConvertWsToKwh(AccumulatedEnergyPerApp.GetValueOrDefault(proc, 0));
        return energyUsed;
    }

    /// <summary>
    /// Calculates the energy used in the last hour.
    /// </summary>
    private double GetEnergyUsedHourly()
    {
        // 1s is the sampling rate, divide by 3.6Mil to convert Ws -> kWh
        var energyUsed = ConvertWsToKwh(AccumulatedEnergyHourly);
        return energyUsed;

    }

    /// <summary>
    /// Converts energy usage value from Ws to kWh.
    /// </summary>
    /// <param name="value">Energy usage value in Ws.</param>
    private double ConvertWsToKwh(double value)
    {
        return (value * 1) / 3600000;
    }


    /// <summary>
    /// Converts energy usage value from kWh to Ws.
    /// </summary>
    /// <param name="value">Energy usage value in kWh.</param>
    private double ConvertKwhToWs(double value)
    {
        return value * 3600000;
    }

    /// <summary>
    /// Calculates the emission of energy used in the last day.
    /// </summary>
    private double GetDailyCost()
    {
        return GetEnergyUsed() * CostPerKwh;
    }

    /// <summary>
    /// Calculates the daily emission of a process
    /// </summary>
    /// <param name="proc">Name of the process</param>
    private double GetDailyCost(string proc)
    {
        return GetEnergyUsed(proc) * CostPerKwh;
    }

    /// <summary>
    /// Calculates the emission of energy used in the last hour.
    /// </summary>
    private double GetHourlyCost()
    {
        return GetEnergyUsedHourly() * CostPerKwh;
    }

    /// <summary>
    /// Calculates the daily carbon emission of the machine
    /// </summary>
    public double GetDailyCarbonEmission()
    {
        return Math.Round(GetEnergyUsed() * CarbonIntensity, DoublePrecision);
    }

    /// <summary>
    /// Calculates the daily carbon emission of a process
    /// </summary>
    /// <param name="proc">Name of the process</param>
    private double GetDailyCarbonEmission(string proc)
    {
        return GetEnergyUsed(proc) * CarbonIntensity;
    }

    /// <summary>
    /// Calculates the hourly carbon emission of the machine
    /// </summary>
    private double GetHourlyCarbonEmission()
    {
        return GetEnergyUsedHourly() * CarbonIntensity;
    }

    /// <summary>
    /// Calculates a daily emission budget based on the weekly budget.
    /// Used for displaying budget line on EnergyUsage page.
    /// </summary>
    public double GetDailyCostBudget()
    {
        return Math.Round(WeeklyBudget / 7.0, 2);
    }


    /// <summary>
    /// Calculates a daily energy budget based on the weekly budget and emission per unit.
    /// </summary>
    public double GetDailyEnergyBudget()
    {
        // Calculate weekly energy budget based on emission per unit and weekly budget and divide by 7 to determine daily budget
        var res = WeeklyBudget / CostPerKwh / 7.0;
        return Math.Round(res, 2);
    }

    /// <summary>
    /// Calculates the amount of hours a single tree needs to absorb the CO2 emitted.
    /// A tree absorbs approximately 25kg of CO2 per year.
    /// <see cref="https://ecotree.green/en/how-much-co2-does-a-tree-absorb"/>
    /// </summary>
    public double GetTimeToAbsorbEmission()
    {
       return (GetDailyCarbonEmission() / 26000.0) * (365.0 * 24.0);
    }

    /// <summary>
    /// Gets the last specified energy logs from the database.
    /// </summary>
    /// <param name="num">The number of energy logs to be retrieved.</param>
    /// <returns>List of EnergyUsageLog.</returns>
    private IEnumerable<EnergyUsageLog> GetLastEnergyLogs(int num)
    {
        var current = DateTime.Now;
        List<EnergyUsageLog> energyUsageLogs = GetHourlyEnergyUsageLogs(current);
        if (energyUsageLogs.Count >= num)
        {
            return energyUsageLogs.TakeLast(num);
        }
        // returns the whole list if num is greater than available energy logs.
        return energyUsageLogs;
    }

    private List<EnergyUsageDiary> GetEnergyDiariesAfter(DateTime date)
    {
        var result = new List<EnergyUsageDiary>();
        foreach (var diary in _energyUsage.Diaries)
        {
            if (DateTime.Compare(diary.Date, date) >= 0)
            {
                result.Add(diary);
            }
        }
        return result;
    }

    private List<EnergyUsageLog> GetEnergyLogsAfter(DateTime date)
    {
        var result = new List<EnergyUsageLog>();
        var diaries = GetEnergyDiariesAfter(date);
        foreach (var diary in diaries)
        {
            foreach (var log in diary.HourlyUsage)
            {
                if (DateTime.Compare(log.Date, date) >= 0)
                {
                    result.Add(log);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Gets the accumulated energy usage after a specified date.
    /// </summary>
    /// <param name="date">The specified date.</param>
    /// <returns>Accumulated energy in Joules.</returns>
    private double GetEnergyAfter(DateTime date)
    {
        var logs = GetEnergyLogsAfter(date);
        double energy = 0;
        foreach (var log in logs)
        {
            energy += log.PowerUsed;
        }
        return energy;
    }

    private float GetTotalEnergyInLogs(IEnumerable<EnergyUsageLog> logs)
    {
        float energy = 0;
        foreach (var log in logs)
        {
            energy += log.PowerUsed;
        }
        return energy;
    }

    /// <summary>
    /// Gets the cost saved after applying the persona.
    /// </summary>
    /// <returns>Cost saved in GBP.</returns>
    public double GetCostSaved()
    {
        if (CostSavedRef.Count() != 0)
        {
            var logs = CostSavedRef;
            var refTime = CostSavedRef.Last().Date - CostSavedRef.First().Date;
            var energy = GetTotalEnergyInLogs(logs);
            // Calculate the reference power usage in kW.
            var averageRefPower = energy / (refTime.TotalSeconds * 1000.0);

            var personaTime = DateTime.Now - PersonaStartTime;
            // Calculate the actual power usage in kW.
            var powerUsed = ConvertWsToKwh(GetEnergyAfter(PersonaStartTime) - energy) / personaTime.TotalHours;
            var powerDiff = averageRefPower - powerUsed;
            if (powerDiff > 0.0 && PersonaEnabled)
            {
                return powerDiff * personaTime.TotalHours * CostPerKwh;
            }
        }
        return 0;
    }

    public void RecordCostSavedRef(DateTime start)
    {
        // Energy logs are saved to database every 5 minutes by default.
        CostSavedRef = GetLastEnergyLogs(2);
        PersonaEnabled = true;
        PersonaStartTime = start;
    }

    public void ClearCostSavedRef()
    {
        CostSavedRef = new List<EnergyUsageLog>();
        PersonaEnabled = false;
    }

    /// <summary>
    /// Update the model with the latest energy usage data.
    /// Creates a new EnergyUsageLog for the total daily measurement, the hourly measurement, as well as per process measurement
    /// then adds this to EnergyUsageData which stores all records.
    /// </summary>
    public void Update()
    {
        var current = DateTime.Now;
        var lastMeasurement = new EnergyUsageLog(current, (float)GetEnergyUsed(), (float)GetDailyCost(), (float)GetDailyCarbonEmission());
        var lastMeasurementHourly = new EnergyUsageLog(current, (float)GetEnergyUsedHourly(), (float)GetHourlyCost(), (float)GetHourlyCarbonEmission());
        _energyUsage.CostPerKwh = CostPerKwh;
        _energyUsage.Company = Company;  

        // Update daily log
        if (!(_energyUsage.Diaries.Count > 0) || _energyUsage.Diaries.Last().Date.Date < current.Date)
            _energyUsage.Diaries.Add(new EnergyUsageDiary());

        var lastDiary = _energyUsage.Diaries.Last();
        lastDiary.DailyUsage = lastMeasurement;

        // Update hourly log
        if (!(lastDiary.HourlyUsage.Count > 0) || lastDiary.HourlyUsage.Last().Date.Hour < current.Hour)
            lastDiary.HourlyUsage.Add(new EnergyUsageLog());
        lastDiary.HourlyUsage[^1] = lastMeasurementHourly;

        // Update per process usage
        foreach (var proc in AccumulatedEnergyPerApp.Keys)
            lastDiary.PerProcUsage[proc] = new EnergyUsageLog(current, (float)GetEnergyUsed(proc), (float)GetDailyCost(proc), (float)GetDailyCarbonEmission(proc));

        Debug.WriteLine("Model updated.");
    }
}
