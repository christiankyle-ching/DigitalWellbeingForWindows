# Digital Wellbeing For Windows 10
[![Github All Releases](https://img.shields.io/github/downloads/christiankyle-ching/DigitalWellbeingForWindows/total.svg)](https://github.com/christiankyle-ching/DigitalWellbeingForWindows/releases)<br>
An App Usage tracker for Windows 10 inspired by Digital Wellbeing in Android. Built with WPF (.NET 4.6), [ModernWpf](https://github.com/Kinnara/ModernWpf) and [Live Charts](https://lvcharts.net/).

**This is only a hobby project. You will experience bugs.** See the [troubleshooting guide](#troubleshooting).
You can help me fix them by reporting it in the [Issues tab](https://github.com/christiankyle-ching/DigitalWellbeingForWindows/issues/new).

There are no warranties associated in using this app.

## Main Features
- **Weekly Usage**. View past week's total usage time (last 7 days).
- **Day App Usage**. View daily app usage time (Pie Chart and List).
- **Alert Notifications**. Set a time limit per app of when to notify you when limit is exceeded, and has the option to close the app directly.
- **Auto-Start**. Run on Startup option, minimized to tray.
- **App Tagging**. Tag apps based on their category. See their percentage of usage through your daily PC usage.
- **Multi-user compatible**. Single installation, but different users have their own usage data.
- **Exclude Apps**. Set a filter of apps to exclude.
- **Filter out short time entries**. Set a filter to hide apps that are run less than the set time limit.
- **Auto-Refresh**. Auto-Refresh charts on intervals.

## Installation
**Download the .msi** installer of the [latest version / release](https://github.com/christiankyle-ching/DigitalWellbeingForWindows/releases/latest).

_Windows Defender SmartScreen will block the installation of this file. Read the source code if you have any doubts, or run a VirusTotal scan._

NOTE: You may have multiple versions of this app installed. Just uninstall older versions.
If you experience any problems, see the troubleshooting guide.

## Screenshots
![image](https://user-images.githubusercontent.com/57316283/155863828-f970b952-d4c4-4a78-9f30-52a2bd0e5a7b.png)
<br/><br/>
![image](https://user-images.githubusercontent.com/57316283/155863834-f1f41525-2232-4db9-a2ce-3d02e2f1b4d6.png)
<br/><br/>
![image](https://user-images.githubusercontent.com/57316283/155863844-2b066189-cac8-4e07-acfd-7f91ea8a2969.png)

## Troubleshooting

### App crashing when opened
If the app crashes upon opening, try:
1. Uninstall.
2. Delete the contents of `dailylogs` folder
  - `WIN + R` to open the Run window.
  - Paste this then hit enter: `%LOCALAPPDATA%/digital-wellbeing/dailylogs`
  - Delete all `.log` files
3. Re-install the latest version.

This will remove app usage history, but will mostly fix the issues. If the app is still crashing, go to: `%LOCALAPPDATA%/digital-wellbeing/internal-logs`, then send me the `.log` file for the current day when the crash happens. This will help me identify the issue.

### App Icons not showing
Fetching icons from running apps is a hit or miss. For better chances, try running the app in administrator mode. (`Right Click > Run as Administrator`).

## Solution Projects (Folders)
- `DigitalWellbeing.Core` - A class library that has static shared classes among the projects.
- `DigitalWellbeingService.NET4.6` - A console application that monitors current active process.
- `DigitalWellbeingWPF` - Front-end UI application.
- `Setup` - An [Advanced Installer](https://www.advancedinstaller.com/) setup project for building an .MSI installer.

## How to Build with .MSI Installer
- Select `Release - AdvancedInstaller` in Solution Configurations, then Build Solution.
