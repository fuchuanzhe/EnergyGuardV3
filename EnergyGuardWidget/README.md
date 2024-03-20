# **System Manual for EnergyGuard Widget**

## Minimum Requirements

The following requirements must be met in order to fully develop all aspects of the application.

### Operating System

Windows 10, version 20H1 or above. 

### IDE

For developement, [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) is the recommeneded editor. 
Note that there is a current known issue with Visual Studio 2017 project packaging.

### Game Bar

Go to the Microsoft Store app to check for downloads and updates. 
Game Bar should have been downloaded and ensure that the Game Bar is up to date. 
The Game Bar can be activated by pressing **Win + G**.

## NuGet Packages

A reference to `Microsoft.Gaming.XboxGameBar` NuGet package must be added.

1. From Visual Studio toolbar, select Tools &rarr; Nuget Package Manager &rarr; Package Sources. 
2. Check if `nuget.org` is listed as a package source.
   - If the package is removed, add it by clicking the "+" button in the top right corner.
   - Name the new entry "nuget.org"
   - Set the source to [https://api.nuget.org/v3/index.json](https://api.nuget.org/v3/index.json).
   - Click "Ok".
2. Right click on the project in Visual Studio solution explorer.
2. Select "Manage NuGet packages".
3. Check if `Microsoft.Gaming.XboxGameBar` is installed in the "Installed" tab.
   - If it is not installed, select the "Browse" tab.
   - Search for `Microsoft.Gaming.XboxGameBar`.
   - Select it and click install.

## Running the App

1. Clone the project.
2. Open `EnergyGuardWidget.sln`.
3. For the Game Bar to discover the new widget, deploy the app.
   - In the toolbar, select "Build".
   - Click "Deploy Solution".
   - Note: For subsequent changes or tests, just close the Game Bar and load the widget in Game Bar, the Game Bar will load the latest version of the widget.
4. Press **Win + G** and go to widget menu.
5. Click on "EnergyGuard Widget" in the dropdown menu.

## Publishing the Widget

Once ready, the widget can be published to Microsoft Store. 
For a more detailed guidance, please follow [Microsoft Store developer guidance](https://developer.microsoft.com/en-us/microsoft-store/).

1. Go to [aka.ms/newstore](https://developer.microsoft.com/en-us/microsoft-store/?utm_campaign=launch&utm_medium=video&utm_source=online).
2. Click on "Publish".
3. Once signed in, select Windows & Xbox &rarr; Overview &rarr; MSIX or PWA app.
4. Reserve the app name.
5. Click "Start your submission".
6. Fill in the details for the app.
7. Open the solution in Visual Studio, select Package &rarr; Publish &rarr; Create App Package.
8. To distribute the app to the Microsoft Store, select Microsoft Store under a new app name and follow the instructions displayed.
9. Upload the packages to the store.


Project group code: **ENERGY1-2023**
