using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Activation;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Services;
using EnergyPerformance.ViewModels;
using EnergyPerformance.Views;
using Microsoft.UI.Xaml;
using Moq;

namespace EnergyPerformance.Tests.MSTest.ViewModels;

[TestClass]
public class PopupViewModelTests
{
    private PopupViewModel GetViewModel()
    {
        var mediatorService = new MediatorService();
        var companyService = new CompanyService();

        return new PopupViewModel(mediatorService, companyService);
    }

    [TestMethod]
    public void TestViewModelInitialization()
    {
        var viewModel = GetViewModel();

        Assert.IsNotNull(viewModel.CompanyList);
        Assert.AreEqual(0, viewModel.RateValue);
        Assert.IsNull(viewModel.SelectedCompany);
        Assert.IsNull(viewModel.ErrorMessage);
    }

    [TestMethod]
    public void TestValidateAndUpdateInput_InvalidRateValue_SetsErrorMessage()
    {
        var viewModel = GetViewModel();
        viewModel.RateValue = -1;
        viewModel.SelectedCompany = "OPUS ENERGY";

        viewModel.ValidateAndUpdateInputCommand.Execute(null);

        Assert.IsNotNull(viewModel.ErrorMessage);
    }

    [TestMethod]
    public void TestValidateAndUpdateInput_NullOrEmptySelectedCompany_SetsErrorMessage()
    {
        var viewModel = GetViewModel();
        viewModel.RateValue = 2.5;
        viewModel.SelectedCompany = null;

        viewModel.ValidateAndUpdateInputCommand.Execute(null);

        Assert.IsNotNull(viewModel.ErrorMessage);

        viewModel.RateValue = 0;
        viewModel.SelectedCompany = "";

        viewModel.ValidateAndUpdateInputCommand.Execute(null) ;

        Assert.IsNotNull(viewModel.ErrorMessage);
    }
}
