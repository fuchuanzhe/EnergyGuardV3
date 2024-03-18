using CommunityToolkit.Mvvm.Input;

namespace EnergyPerformance.Contracts.Services;

public interface IMediatorService
{
    double RateValue
    {
        get; set;
    }

    string SelectedCompany
    {
        get; set;
    }
    event EventHandler<double> RateValueChanged;
    event EventHandler<string> SelectedCompanyChanged;
}
