using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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

        private DispatcherTimer refreshTimer;

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
        public bool IsLoading { get; set; }

        public AppUsageViewModel()
        {
            folderPath = Environment.ExpandEnvironmentVariables(envFolderPath);

            AppData = new ObservableCollection<ChartDataPoint>();
            AppDataItems = new ObservableCollection<AppUsageListItem>();

            OnNavigate(false);
        }

        public void OnNavigate(bool fromNavView = true)
        {
            // Apply new settings
            bool enableAutoRefresh = Properties.Settings.Default.EnableAutoRefresh;
            int refreshInterval = Properties.Settings.Default.RefreshIntervalSeconds;

            if (!fromNavView)
            {
                LoadData();
                return;
            }

            if (enableAutoRefresh)
            {
                TimeSpan intervalDuration = TimeSpan.FromSeconds(refreshInterval);

                refreshTimer = new DispatcherTimer() { Interval = intervalDuration };
                refreshTimer.Tick += (s, e) =>
                {
                    // Only refresh data when the selected date is today,
                    // Else, no point in auto-refreshing non-changing data.
                    if (DateTime.Now.Date == LoadedDate.Date)
                    {
                        LoadData();
                    }
                };
                refreshTimer.Start();
            }
            else
            {
                LoadData();

                try
                {
                    refreshTimer.Stop();
                }
                catch (NullReferenceException)
                {
                    // No timer to start with
                    Debug.WriteLine("No Timer");
                }

            }
        }

        public async void LoadData()
        {
            SetLoading(true);

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

                    try
                    {
                        ChartDataPoint existingChartPoint = AppData.Single(c => c.Name.Contains(app.Name));
                        existingChartPoint.Name = label;
                        existingChartPoint.Value = percentage;
                        existingChartPoint.Refresh();
                    }
                    catch (InvalidOperationException)
                    {
                        AppData.Add(new ChartDataPoint(label, percentage));
                    }

                    try
                    {
                        AppUsageListItem existingListItem = AppDataItems.Single(c => c.AppName == app.Name);
                        existingListItem.AppName = app.Name;
                        existingListItem.Duration = app.Duration;
                        existingListItem.Percentage = percentage;
                        existingListItem.Refresh();
                    }
                    catch (InvalidOperationException)
                    {
                        AppDataItems.Add(new AppUsageListItem(app.Name, app.Duration, percentage));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"CANNOT FIND: {folderPath}{LoadedDate:MM-dd-yyyy}.log");
            }
            finally
            {
                SetLoading(false);
                NotifyChange();
            }
        }

        private void SetLoading(bool value)
        {
            if (value == false)
            {
                // Apply delay on hiding LoadingProgress
                TimeSpan delay = TimeSpan.FromMilliseconds(1000);

                var delayTimer = new DispatcherTimer() { Interval = delay };
                delayTimer.Tick += (s, e) =>
                {
                    IsLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                    delayTimer.Stop();
                };
                delayTimer.Start();
            }
            else
            {
                IsLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public void OpenLogsFolder()
        {
            Process.Start($"explorer.exe", folderPath);
        }

        public void ManualRefresh()
        {
            OnNavigate(false);
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

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
