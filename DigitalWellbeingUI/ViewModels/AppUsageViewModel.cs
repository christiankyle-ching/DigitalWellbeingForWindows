using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DigitalWellbeingUI.Helpers;
using DigitalWellbeingUI.Models;
using DigitalWellbeingUI.Models.UserControls;

namespace DigitalWellbeingUI.ViewModels
{
    public class AppUsageViewModel : INotifyPropertyChanged
    {
        private const string envFolderPath = @"%USERPROFILE%\.digitalwellbeing\dailylogs\";
        private string folderPath;

        private readonly string[] excludeProcesses = new string[]
        {
            "explorer",
            "SearchHost",

            // Custom Indicators (from Service)
            "*LAST",
            "*SHUTDOWN",
        };

        // Loaded Date
        public DateTime LoadedDate = DateTime.Now;
        public string StrLoadedDate
        {
            get => (LoadedDate.Date == DateTime.Now.Date) ? "Today" : this.LoadedDate.ToString("dddd, MMM dd yyyy");
        }

        // Total Duration
        public TimeSpan TotalDuration = new TimeSpan();
        public string StrTotalDuration
        {
            get
            {
                string output = "";
                if (TotalDuration.Hours > 0) { output += $"{TotalDuration.Hours} hr, "; }
                output += $"{TotalDuration.Minutes} min";
                return output;
            }
        }

        // Collections
        public ObservableCollection<ChartDataPoint> AppData { get; set; }
        public ObservableCollection<AppUsageListItem> AppDataItems { get; set; }

        // Getters
        public event PropertyChangedEventHandler PropertyChanged;
        public bool HasData { get => AppData.Count > 0; }
        public bool CanGoNext { get => LoadedDate.Date < DateTime.Now.Date; }

        public AppUsageViewModel()
        {
            folderPath = Environment.ExpandEnvironmentVariables(envFolderPath);

            AppData = new ObservableCollection<ChartDataPoint>();
            AppDataItems = new ObservableCollection<AppUsageListItem>();

            LoadData();
        }

        public async void LoadData()
        {
            AppData.Clear();
            AppDataItems.Clear();
            TotalDuration = TimeSpan.Zero;

            try
            {
                string[] lines = await File.ReadAllLinesAsync($"{folderPath}{LoadedDate:MM-dd-yyyy}.log");

                List<AppUsage> appUsageList = new List<AppUsage>();

                // Parse .log data
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i + 1 >= lines.Length) break;

                    string line = lines[i];

                    if (line == "") continue;

                    string[] data = line.Split('\t');

                    string name = data[1];

                    if (excludeProcesses.Contains(name)) continue;

                    DateTime startTime = DateTime.Parse(data[0]);
                    DateTime endTime = DateTime.Parse(lines[i + 1].Split('\t')[0]);

                    if (endTime < startTime) continue; // Prevents negative values

                    TimeSpan duration = endTime - startTime;

                    // Filter Minimum Duration
                    if (duration <= Properties.Settings.Default.MinumumDuration) continue; 

                    AppUsage existingRecord = appUsageList.Find(a => a.Name == name);
                    if (existingRecord == null)
                    {
                        appUsageList.Add(new AppUsage(name, duration));
                    }
                    else
                    {
                        existingRecord.Duration = existingRecord.Duration.Add(duration);
                    }
                }

                appUsageList.Sort((a, b) => a.Duration.CompareTo(b.Duration) * -1);

                // Calculate Total Duration
                foreach (AppUsage app in appUsageList)
                {
                    TotalDuration = TotalDuration.Add(app.Duration);
                }

                // Add ListItems
                foreach (AppUsage app in appUsageList)
                {
                    int percentage = (int)Math.Round(app.Duration.TotalSeconds / TotalDuration.TotalSeconds * 100);

                    string durationStr = StringParser.TimeSpanToString(app.Duration);

                    string label = app.Name;
                    if (durationStr != "") { label += $" ({durationStr})"; }

                    AppData.Add(new ChartDataPoint(label, percentage));
                    AppDataItems.Add(new AppUsageListItem(app.Name, app.Duration, percentage));
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"CANNOT FIND: {folderPath}{LoadedDate:MM-dd-yyyy}.log");

                // TODO: Handle No Data
            }

            NotifyChange();
        }

        public void OpenLogsFolder()
        {
            Process.Start($"explorer.exe", folderPath);
        }

        public void LoadPreviousDay()
        {
            LoadedDate = LoadedDate.AddDays(-1);
            LoadData();
        }

        public void LoadNextDay()
        {
            LoadedDate = LoadedDate.AddDays(1);
            LoadData();
        }

        public void NotifyChange()
        {
            OnPropertyChanged(nameof(StrLoadedDate));
            OnPropertyChanged(nameof(StrTotalDuration));
            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(CanGoNext));
        }

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ChartDataPoint OnAppUsageListView_SelectionChanged(AppUsageListItem item)
        {
            try
            {
                return AppData.Single(appData => appData.Name.Contains(item.AppName));
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public AppUsageListItem OnAppUsagePieChart_SelectionChanged(ChartDataPoint chartPoint)
        {
            try
            {
                return AppDataItems.Single(listItem => chartPoint.Name.Contains(listItem.AppName));
            }
            catch (InvalidOperationException ex)
            {
                return null;
            }
        }
    }
}
