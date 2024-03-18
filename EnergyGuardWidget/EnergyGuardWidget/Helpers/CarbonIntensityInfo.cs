using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Web.Http;
using Newtonsoft.Json;
using Windows.Data.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.ComponentModel;
using System.Collections;
using Windows.System;
using System.Runtime.CompilerServices;

namespace EnergyGuardWidget.Helpers
{
    /// <summary>
    /// The container class for carbon emissions data.
    /// </summary>
    public class CarbonIntensityInfo : INotifyPropertyChanged
    {
        private static readonly string ukUrl = "https://api.carbonintensity.org.uk/intensity";

        private HttpClient httpClient = new HttpClient();
        private Uri requestUri = new Uri(ukUrl);

        private double _carbonIntensity;
        private string _intensityIndex;

        public event PropertyChangedEventHandler PropertyChanged;

        private DispatcherQueue _queue;
        private DispatcherQueueController _queueController;
        private DispatcherQueueTimer _repeatingTimer;

        public double CarbonIntensity
        {
            get { return _carbonIntensity;  }
            set
            {
                _carbonIntensity = value;
                RaisePropertyChanged("CarbonIntensity");
            }
        }

        public string IntensityIndex
        {
            get { return _intensityIndex; }
            set
            {
                _intensityIndex = value;
                RaisePropertyChanged("IntensityIndex");
            }
        }

        public string Country = "united kingdom";
        public string Postcode = "WC1";

        public CarbonIntensityInfo()
        {
            CarbonIntensity = 0;
            IntensityIndex = "very low";
            ExecuteAsync();
        }

        protected async Task ExecuteAsync()
        {
            await DoAsync();
            _queueController = DispatcherQueueController.CreateOnDedicatedThread();
            _queue = _queueController.DispatcherQueue;

            _repeatingTimer = _queue.CreateTimer();
            _repeatingTimer.Interval = TimeSpan.FromMinutes(30);

            // The tick handler will be invoked repeatedly after every 30 minutes.
            _repeatingTimer.Tick += async (s, e) =>
            {
                await DoAsync();
            };

            _repeatingTimer.Start();
        }

        public async Task DoAsync()
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
            try
            {
                HttpResponseMessage httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();

                string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();

                JObject response = JObject.Parse(httpResponseBody);
                IList<JToken> data = response["data"].Children().ToList();
                JObject intensity = JObject.Parse(JObject.Parse(data[0].ToString())["intensity"].ToString());
                CarbonIntensity = intensity.GetValue("forecast").Value<int>();
                IntensityIndex = intensity.GetValue("index").ToString();

            }
            catch (Exception e)
            {
                Debug.WriteLine("Cannot fetch data", e);
            }
        }

        /// <summary>
        /// Event used to notify UI that the data has changed.
        /// </summary>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}

