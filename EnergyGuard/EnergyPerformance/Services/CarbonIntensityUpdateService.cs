using System.Diagnostics;
using EnergyPerformance.Helpers;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace EnergyPerformance.Services;
public class CarbonIntensityUpdateService : BackgroundService
{
    private readonly string _ukUrl = "https://api.carbonintensity.org.uk/intensity";
    
    // API updates carbon intensity information every 30 minutes.
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMinutes(30));

    private readonly CarbonIntensityInfo _carbonIntensityInfo;
    private readonly LocationInfo _locationInfo;
    private readonly IHttpClientFactory _httpClientFactory;

    public string Country = "united kingdom";
    public string Postcode = "WC1";

    public double CarbonIntensity
    {
        get => _carbonIntensityInfo.CarbonIntensity;
        set => _carbonIntensityInfo.CarbonIntensity = value;
    }

    public CarbonIntensityUpdateService(CarbonIntensityInfo carbonIntensityInfo, LocationInfo locationInfo, IHttpClientFactory httpClientFactory)
    {
        _carbonIntensityInfo = carbonIntensityInfo;
        _locationInfo = locationInfo;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    ///  Main function which is called every 30 minutes based on PeriodicTimer.
    ///  Fetches and updates Carbon Intensity.
    /// </summary>
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            await DoAsync();
        }
        while (await _periodicTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested);
    }

    private async Task DoAsync()
    {
        Debug.WriteLine($"Retrieving live carbon intensity for {Country}");

        if (Country.ToLower() == "united kingdom")
        {
            await FetchLiveCarbonIntensity();
        }
        else
        {
            Debug.WriteLine("Other countries and regions are currently not supported");
        }
        Debug.WriteLine($"Current carbon intensity: {CarbonIntensity}");
    }

    private async Task FetchLiveCarbonIntensity()
    {
        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            var jsonResponse = await ApiProcessor<dynamic>.Load(httpClient, _ukUrl) ??
                throw new InvalidOperationException("Cannot deserialize object");

            var jsonData = jsonResponse.GetProperty("data");
            foreach (var entry in jsonData.EnumerateArray())
            {
                CarbonIntensity = entry.GetProperty("intensity").GetProperty("forecast").GetDouble();
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Cannot fetch data", e);
        }
    }

}