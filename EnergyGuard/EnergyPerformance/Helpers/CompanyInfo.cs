using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace EnergyPerformance.Helpers;

public class Company
{
    public string Name
    {
        get; set;
    }
    public Perks Perks
    {
        get; set;
    }
}

public class Perks
{
    public float NightRate
    {
        get; set;
    }
}

public class CompanyData
{
    public List<Company> EnergyDistributors
    {
        get; set;
    }
}


/// <summary>
/// Helper for deserializing company energy rate data.
/// </summary>
public class CompanyInfo
{
    public static CompanyData GetCompanyData()
    {
        try
        {
            string filePath = "../Config/CompanyList.json";
            string companyRates = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<CompanyData>(companyRates);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }
    }
}