using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using EnergyPerformance.Models;
using Microsoft.Extensions.Hosting;

namespace EnergyPerformance.Services;


/// <summary>
/// Service related to companies, the list of companies available and operations regarding them.
/// </summary>
public class CompanyService
{
    public List<string> CompanyList
    {
        get; 
    } = new List<string> {
        "BRITISH GAS",
        "EDF ENERGY",
        "EON ENERGY",
        "NPOWER",
        "OCTOPUS ENERGY",
        "OPUS ENERGY",
        "SCOTTISH POWER",
        "SHELL ENERGY",
        "OTHER"
        };

}
