using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Services;

namespace EnergyPerformance.Tests.MSTest.Services;

[TestClass()]
public class MediatorServiceTests
{
    [TestMethod()]
    public void TestRateValueChange()
    {
        // Arrange
        var mediatorService = new MediatorService();
        double newValue = 2.5;
        double receivedValue = 0;
        mediatorService.RateValueChanged += (sender, value) =>
        {
            receivedValue = value;
        };

        // Act
        mediatorService.RateValue = newValue;

        // Assert
        Assert.AreEqual(newValue, receivedValue);
    }

    [TestMethod()]
    public void TestSelectedCompanyChange()
    {
        // Arrange
        var mediatorService = new MediatorService();
        string newCompany = "Company 1";
        string receivedCompany = "";
        mediatorService.SelectedCompanyChanged += (sender, company) =>
        {
            receivedCompany = company;
        };

        // Act
        mediatorService.SelectedCompany = newCompany;

        // Assert
        Assert.AreEqual(newCompany, receivedCompany);
    }

    [TestMethod()]
    public void TestRateValueDoesNotChangeWhenSameValueSet()
    {
        // Arrange
        var mediatorService = new MediatorService();
        double initialValue = 3.0;
        double receivedValue = 0;
        mediatorService.RateValueChanged += (sender, value) =>
        {
            receivedValue = value;
        };

        // Act
        mediatorService.RateValue = initialValue;
        mediatorService.RateValue = initialValue; // Setting the same value again

        // Assert
        Assert.AreEqual(initialValue, receivedValue);
    }

    [TestMethod()]
    public void TestSelectedCompanyDoesNotChangeWhenSameValueSet()
    {
        // Arrange
        var mediatorService = new MediatorService();
        string initialCompany = "Company 2";
        string receivedCompany = "";
        mediatorService.SelectedCompanyChanged += (sender, company) =>
        {
            receivedCompany = company;
        };

        // Act
        mediatorService.SelectedCompany = initialCompany;
        mediatorService.SelectedCompany = initialCompany; // Setting the same value again

        // Assert
        Assert.AreEqual(initialCompany, receivedCompany);
    }
}