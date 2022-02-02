# Digital Wellbeing For Windows 10
[![Github All Releases](https://img.shields.io/github/downloads/christiankyle-ching/DigitalWellbeingForWindows/total.svg)](https://github.com/christiankyle-ching/DigitalWellbeingForWindows/releases)<br>
An App Usage tracker for Windows 10 inspired by Digital Wellbeing in Android. Built with WPF (.NET 4.6), [ModernWpf](https://github.com/Kinnara/ModernWpf) and [Live Charts](https://lvcharts.net/).

## Screenshots
![dw_main_light](https://user-images.githubusercontent.com/57316283/149713916-8ee2220e-5fc6-4fee-8107-5d5f7359d8d7.png)
<br/><br/>
![dw_main](https://user-images.githubusercontent.com/57316283/149713883-ed1b0762-44bf-4059-815c-87623925304c.png)
<br/><br/>
![dw_settings](https://user-images.githubusercontent.com/57316283/151938045-1eea220c-50d1-4371-9b19-f611a932ecbc.png)

## Features
- View past week's total usage time (last 7 days)
- View daily app usage time (Pie Chart and List)
- Set a filter to hide apps that are run less than the set time limit
- Set a filter of apps to exclude
- Alert Notifications. Set a time limit per app of when to notify you when limit is exceeded.
- Auto-Refresh data on intervals
- Single installation, but different users have their own usage data
- Run on Startup option, minimized to tray.

## Solution Projects (Folders)
- `DigitalWellbeing.Core` - A class library that has static shared classes among the projects.
- `DigitalWellbeingService.NET4.6` - A console application that monitors current active process.
- `DigitalWellbeingWPF` - Front-end UI application.
- `Setup` - An [Advanced Installer](https://www.advancedinstaller.com/) setup project for building an .MSI installer.

## How to Build with .MSI Installer
- Select `Release - AdvancedInstaller` in Solution Configurations, then Build Solution.
