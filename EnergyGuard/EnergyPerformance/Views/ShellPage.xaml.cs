﻿// TemplateStudio

using System.Diagnostics;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Windows.System;

namespace EnergyPerformance.Views;

/// <summary>
/// Code behind for <see cref="ShellPage"/> which contains controls for navigating between pages and shows content in a "main" frame within the page.
/// </summary>
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel
    {
        get;
    }

    public ShellPage(ShellViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
        ViewModel.NavigationViewService.Initialize(NavigationViewControl);

        // Custom title bar required for full window theme and Mica support.
        // https://docs.microsoft.com/windows/apps/develop/title-bar?tabs=winui3#full-customization
        App.MainWindow.ExtendsContentIntoTitleBar = true;
        App.MainWindow.SetTitleBar(AppTitleBar);
        App.MainWindow.Activated += MainWindow_Activated;
        AppTitleBarText.Text = "AppDisplayName".GetLocalized();
        TrayIconView.Unloaded += (sender, e) => Debug.WriteLine("Tray closed");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Selected = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
        TitleBarHelper.UpdateTitleBar(RequestedTheme);
        MonitorDetailNavItem.Visibility = Visibility.Collapsed;
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
        KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

        AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
    }

    private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
    {
        AppTitleBar.Margin = new Thickness()
        {
            Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
            Top = AppTitleBar.Margin.Top,
            Right = AppTitleBar.Margin.Right,
            Bottom = AppTitleBar.Margin.Bottom
        };
    }

    // Event for initializing the main page after finishing popup page
    private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem selectedItem)
        {
            var navigateTo = NavigationHelper.GetNavigateTo(selectedItem);
            ViewModel.NavigationService.NavigateTo(navigateTo);
        }
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };

        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;

        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var navigationService = App.GetService<INavigationService>();

        var result = navigationService.GoBack();

        args.Handled = result;
    }
}
