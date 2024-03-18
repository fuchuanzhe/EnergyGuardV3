using EnergyGuardWidget.Helpers;
using EnergyGuardWidget.ViewModels;
using Microsoft.Gaming.XboxGameBar;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EnergyGuardWidget
{
    /// <summary>
    /// The main widget that will be seen once activated.
    /// </summary>
    public sealed partial class Widget1 : Page
    {
        private XboxGameBarWidget widget = null;
        private SolidColorBrush widgetDarkThemeBrush =  null;
        private SolidColorBrush widgetLightThemeBrush = null;
        private double? opacityOverride = null;

        private CarbonIntensityInfo _carbonInfo = new CarbonIntensityInfo();

        private Widget1ViewModel _viewModel = new Widget1ViewModel();

        public CarbonIntensityInfo CarbonInfo
        {
            get;
        }

        public Widget1ViewModel ViewModel { get; }

        public Widget1()
        {
            this.InitializeComponent();
            CarbonInfo = _carbonInfo;
            ViewModel = _viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            widget = e.Parameter as XboxGameBarWidget;

            widgetDarkThemeBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 38, 38, 38));
            widgetLightThemeBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 219, 219, 219));

            // Hook up events for when the ui is updated.
            widget.HorizontalResizeSupported = true;
            widget.VerticalResizeSupported = true;
            widget.PinningSupported = true;
            // widget.SettingsSupported should change to true once settings page has been done.
            widget.SettingsSupported = false;

            widget.MinWindowSize = new Size(470, 250);
            widget.MaxWindowSize = new Size(705, 375);

            widget.SettingsClicked += WidgetSettingsClicked;
            widget.RequestedOpacityChanged += WidgetRequestedOpacityChanged;

            SetRequestedOpacityState();
            SetBackgroundColor();
            SetBackgroundOpacity();
        }

        private async void WidgetSettingsClicked(XboxGameBarWidget sender, object args)
        {
            await sender.ActivateSettingsAsync();
        }

        private void SetRequestedOpacityState()
        {
            if (opacityOverride.HasValue)
            {
                OpacitySlider.Value = opacityOverride.Value;
            }
            else
            {
                OpacitySlider.Value = widget.RequestedOpacity;
            }
        }

        /// <summary>
        /// Changes the background opacity to value of opacityOverride.
        /// </summary>
        private void SetBackgroundOpacity()
        {
            if (opacityOverride.HasValue)
            {
                BackgroundGrid.Opacity = opacityOverride.Value;
            }
            else
            {
                BackgroundGrid.Opacity = widget.RequestedOpacity;
            }
        }

        private void SetBackgroundColor()
        {
            this.RequestedTheme = widget.RequestedTheme;
            BackgroundGrid.Background = (widget.RequestedTheme == ElementTheme.Dark) ? widgetDarkThemeBrush : widgetLightThemeBrush;
        }


        /// <summary>
        /// Event when the requested opacity is changed.
        /// </summary>
        private async void WidgetRequestedOpacityChanged(XboxGameBarWidget sender, object args)
        {
            if (!opacityOverride.HasValue)
            {
                await OpacitySlider.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    SetRequestedOpacityState();
                    SetBackgroundOpacity();
                });
            }
        }

        /// <summary>
        /// Invoked when the value of slider in UI changes.
        /// </summary>
        private void OpacitySliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            if (slider != null)
            {
                opacityOverride = slider.Value/50;
                SetBackgroundOpacity();
            }
        }

    }
}
