# Digital Wellbeing For Windows 10
[![Github All Releases](https://img.shields.io/github/downloads/christiankyle-ching/DigitalWellbeingForWindows/total.svg)](https://github.com/christiankyle-ching/DigitalWellbeingForWindows/releases)<br>
An App Usage tracker for Windows 10 inspired by Digital Wellbeing in Android.

## Screenshots
![dw_main_light](https://user-images.githubusercontent.com/57316283/149713916-8ee2220e-5fc6-4fee-8107-5d5f7359d8d7.png)
![dw_main](https://user-images.githubusercontent.com/57316283/149713883-ed1b0762-44bf-4059-815c-87623925304c.png)
![dw_settings](https://user-images.githubusercontent.com/57316283/149713922-9a073aff-8a2d-48b6-9d58-0ab805d8ac8a.png)

## Features
- View past week's total usage time (last 7 days)
- View daily app usage time (Pie Chart and List)
- Set a filter to hide apps that are run less than the set time limit
- Set a filter of apps to exclude
- Auto-Refresh data on intervals
- Single installation, but different users have their own usage data

## Solution Projects (Folders)
- `DigitalWellbeingService.NET4.6` - A console application that monitors current active process.
- `DigitalWellbeingWPF` - Front-end User Interface built with [ModernWpf](https://github.com/Kinnara/ModernWpf) and [Live Charts](https://lvcharts.net/).
- `Setup` - An [Advanced Installer](https://www.advancedinstaller.com/) setup project for building an .MSI installer.
