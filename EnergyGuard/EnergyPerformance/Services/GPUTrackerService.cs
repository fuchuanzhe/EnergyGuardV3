using System.Diagnostics;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Hosting;

namespace EnergyPerformance.Services;

// User LibreHardwareMonitor to track the total GPU percentage usage of the system.
public class GpuTrackerService : BackgroundService
{
    // Execute every second
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMilliseconds(1000));
    private List<ISensor> gpuSensors = new List<ISensor>();
    private readonly GpuInfo gpuInfo;
    private readonly Computer computer;
    private const int GpuUsageDoublePrecision = 2;
    private double lastUsage = 0;

    public double GpuUsage
    {
        get => gpuInfo.GpuUsage;
        private set => gpuInfo.GpuUsage = value;
    }

    public GpuTrackerService(GpuInfo gpuInfo, DebugModel debugInf)
    {
        this.gpuInfo = gpuInfo;
        computer = new Computer
        {
            IsGpuEnabled = true
        };
    }

    /// <summary>
    /// Method to detect sensors which report power values in the device.
    /// </summary>
    public void DetectRequiredSensors()
    {
        computer.Open();
        computer.Accept(new UpdateVisitor());
        foreach (IHardware hardware in computer.Hardware)
        {
            hardware.Update();
            foreach (ISensor sensor in hardware.Sensors)
            {
                // Read GPU sensors which report percentage usage values
                if ((sensor.Name.Contains("GPU Core") || sensor.Name.Contains("D3D 3D")) && sensor.SensorType.Equals(SensorType.Load))
                {
                    gpuSensors.Add(sensor);
                }
            }
        }
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DetectRequiredSensors();

        while (await _periodicTimer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            await DoAsync();
        }
    }

    /// <summary>
    ///  Main function which is called every 1 second based on PeriodicTimer.
    ///  Performs GPU usage tracking and reports the highest GPU power value.
    /// </summary>
    private async Task DoAsync()
    {
        await Task.Run(() =>
        {
            foreach (IHardware hardware in computer.Hardware)
            {
                hardware.Update();
            }
        });

        // Only reports GPU with highest power usage.
        var gpu = gpuSensors.MaxBy(x => x.Value);

        if (gpu != null)
        {
            GpuUsage = Math.Round(gpu.Value ?? lastUsage, GpuUsageDoublePrecision);
        }
        else
        {
            GpuUsage = lastUsage;
        }
       
        lastUsage = GpuUsage;
        await Task.CompletedTask;
    }
}
