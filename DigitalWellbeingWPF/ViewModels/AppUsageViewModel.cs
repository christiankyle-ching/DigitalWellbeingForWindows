using DigitalWellbeingWPF.Helpers;
using DigitalWellbeingWPF.Models;
using DigitalWellbeingWPF.Models.UserControls;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DigitalWellbeingWPF.ViewModels
{
    public class AppUsageViewModel : INotifyPropertyChanged
    {
        private static readonly TextInfo txtInfo = new CultureInfo("en-US", false).TextInfo;

        private const string envFolderPath = @"%USERPROFILE%\.digitalwellbeing\dailylogs\";
        private string folderPath;

        private readonly string[] excludeProcesses = new string[]
        {
            "DigitalWellbeingWPF",

            "explorer",
            "SearchHost",
            "Idle",
            "StartMenuExperienceHost",
            
            // Custom Indicators (from Service)
            "*LAST",
        };

        private DispatcherTimer refreshTimer;

        // Formatters
        public Func<double, string> HourFormatter { get; set; }
        private Func<ChartPoint, string> PieChartTooltipFormatter { get; set; }

        // Loaded Date
        public DateTime LoadedDate = DateTime.Now.Date;
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

        // MinimumDuration
        public string StrMinumumDuration
        {
            get
            {
                if (Properties.Settings.Default.MinumumDuration.TotalSeconds <= 0) return "";

                return $"Apps that run less than {StringParser.TimeSpanToString(Properties.Settings.Default.MinumumDuration)} are hidden.";
            }
        }

        // Collections
        public ObservableCollection<List<AppUsage>> WeekAppUsage { get; set; } // Week's App Usage
        public SeriesCollection WeeklyChartData { get; set; } // Week's App Usage
        public DateTime[] WeeklyChartLabelDates { get; set; }
        public string[] WeeklyChartLabels { get; set; }
        public SeriesCollection DayPieChartData { get; set; } // Pie Chart Data
        public ObservableCollection<AppUsageListItem> DayListItems { get; set; } // List Items

        // Getters
        private readonly int prevDaysToLoad = 7;
        public event PropertyChangedEventHandler PropertyChanged;
        public bool HasData { get => DayPieChartData.Count > 0; }
        public bool CanGoNext { get => LoadedDate.Date < DateTime.Now.Date; }
        public bool CanGoPrev { get => LoadedDate.Date > DateTime.Now.AddDays(-prevDaysToLoad + 1).Date; }
        public bool IsLoading { get; set; }

        public AppUsageViewModel()
        {
            folderPath = Environment.ExpandEnvironmentVariables(envFolderPath);

            InitCollections();
            InitFormatters();

            LoadWeeklyData();

            InitAutoRefreshTimer();
        }

        private void InitCollections()
        {
            WeekAppUsage = new ObservableCollection<List<AppUsage>>();
            WeeklyChartData = new SeriesCollection();
            WeeklyChartLabels = new string[0];
            WeeklyChartLabelDates = new DateTime[0];

            DayPieChartData = new SeriesCollection();
            DayListItems = new ObservableCollection<AppUsageListItem>();
        }

        private void InitFormatters()
        {
            HourFormatter = (hours) => hours.ToString("F1") + " h";
            PieChartTooltipFormatter = (chartPoint) => string.Format("{0:F2} min/s ({1:P})", chartPoint.Y, chartPoint.Participation);
        }

        private void InitAutoRefreshTimer()
        {
            int refreshInterval = Properties.Settings.Default.RefreshIntervalSeconds;
            TimeSpan intervalDuration = TimeSpan.FromSeconds(refreshInterval);
            refreshTimer = new DispatcherTimer() { Interval = intervalDuration };
            refreshTimer.Tick += (s, e) => TryRefreshData();
        }

        private async void TryRefreshData()
        {
            // Refresh Data only if loaded date is set today
            // Only refresh data when the selected date is today,
            // Else, no point in auto-refreshing non-changing data.
            if (DateTime.Now.Date == LoadedDate.Date)
            {
                List<AppUsage> appUsageList = await GetData(LoadedDate.Date);
                UpdatePieChartAndList(appUsageList);
            }
        }

        public void OnNavigate()
        {
            // Apply new settings
            bool enableAutoRefresh = Properties.Settings.Default.EnableAutoRefresh;
            if (enableAutoRefresh)
            {
                if (!refreshTimer.IsEnabled)
                {
                    refreshTimer.Start();
                }
            }
            else
            {
                TryRefreshData();

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

        private async void LoadWeeklyData()
        {
            SetLoading(true);

            try
            {
                DateTime minDate = DateTime.Now.AddDays(-prevDaysToLoad);

                ChartValues<double> hours = new ChartValues<double>();
                List<string> labels = new List<string>();
                List<DateTime> loadedDates = new List<DateTime>();

                for (int i = 1; i <= prevDaysToLoad; i++)
                {
                    DateTime date = minDate.AddDays(i).Date;

                    // Store App Usage List
                    List<AppUsage> appUsageList = await GetData(date);
                    WeekAppUsage.Add(appUsageList);

                    // Calculate Total Hours
                    TimeSpan totalDuration = TimeSpan.Zero;
                    foreach (AppUsage app in appUsageList)
                    {
                        totalDuration = totalDuration.Add(app.Duration);
                    }

                    hours.Add(totalDuration.TotalHours);
                    labels.Add(date.ToString("ddd"));
                    loadedDates.Add(date);
                }

                WeeklyChartData.Add(new ColumnSeries
                {
                    Values = hours,
                });
                WeeklyChartLabels = labels.ToArray();
                WeeklyChartLabelDates = loadedDates.ToArray();
            }
            catch { }
            finally
            {
                SetLoading(false);
                WeeklyChart_SelectionChanged(WeekAppUsage.Count - 1);
            }
        }

        public async Task<List<AppUsage>> GetData(DateTime date)
        {
            List<AppUsage> appUsageList = new List<AppUsage>();

            try
            {
                string text = await Task.Run(() => File.ReadAllText($"{folderPath}{date:MM-dd-yyyy}.log")); ;
                string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                // Parse .log data
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i + 1 >= lines.Length) break;

                    string line = lines[i];

                    if (line == "") continue;

                    string[] data = line.Split('\t');



                    string processName = data[1];
                    string programName = data[2] != "" ? data[2] : txtInfo.ToTitleCase(processName);

                    if (excludeProcesses.Contains(processName)) continue;

                    DateTime startTime = DateTime.Parse(data[0]);
                    DateTime endTime = DateTime.Parse(lines[i + 1].Split('\t')[0]);

                    if (endTime < startTime) continue; // Prevents negative values

                    TimeSpan duration = endTime - startTime;

                    AppUsage existingRecord = appUsageList.Find(a => a.ProcessName == processName);
                    if (existingRecord == null)
                    {
                        appUsageList.Add(new AppUsage(processName, programName, duration));
                    }
                    else
                    {
                        existingRecord.Duration = existingRecord.Duration.Add(duration);
                    }
                }

                appUsageList.Sort((a, b) => a.Duration.CompareTo(b.Duration) * -1);
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine($"CANNOT FIND: {folderPath}{date:MM-dd-yyyy}.log");
            }

            return appUsageList;
        }

        private void UpdatePieChartAndList(List<AppUsage> appUsageList)
        {
            SetLoading(true);

            try
            {
                TotalDuration = TimeSpan.Zero;

                // Calculate Total Duration
                foreach (AppUsage app in appUsageList)
                {
                    TotalDuration = TotalDuration.Add(app.Duration);
                }

                // Add List Items and Chart Items
                foreach (AppUsage app in appUsageList)
                {
                    int percentage = (int)Math.Round(app.Duration.TotalSeconds / TotalDuration.TotalSeconds * 100);

                    string durationStr = StringParser.TimeSpanToString(app.Duration);

                    string label = app.ProcessName;
                    if (durationStr != "") { label += $" ({durationStr})"; }

                    // Add Chart Points
                    try
                    {
                        PieSeries existingData = (PieSeries)DayPieChartData.Single(pieSeries => pieSeries.Title == app.ProcessName);
                        existingData.Values[0] = app.Duration.TotalMinutes;

                        // Remove if not within MinimumDuration
                        if (app.Duration <= Properties.Settings.Default.MinumumDuration)
                        {
                            DayPieChartData.Remove(existingData);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Add record only if higher than MinimumDuration and not currently on the list
                        if (app.Duration > Properties.Settings.Default.MinumumDuration)
                        {
                            DayPieChartData.Add(new PieSeries()
                            {
                                Title = app.ProcessName,
                                Values = new ChartValues<double> { app.Duration.TotalMinutes },
                                LabelPoint = PieChartTooltipFormatter,
                                StrokeThickness = 0,
                            });
                        }
                    }

                    // Add List Items
                    try
                    {
                        AppUsageListItem existingListItem = DayListItems.Single(c => c.ProcessName == app.ProcessName);
                        existingListItem.Duration = app.Duration;
                        existingListItem.Percentage = percentage;
                        existingListItem.Refresh();

                        // Remove if not within MinimumDuration
                        if (app.Duration <= Properties.Settings.Default.MinumumDuration)
                        {
                            DayListItems.Remove(existingListItem);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        // Add record only if higher than MinimumDuration and not currently on the list
                        if (app.Duration > Properties.Settings.Default.MinumumDuration)
                        {
                            DayListItems.Add(new AppUsageListItem(app.ProcessName, app.ProgramName, app.Duration, percentage));
                        }
                    }
                }
            }
            catch { }
            finally
            {
                NotifyChange();
                SetLoading(false);
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
            UpdatePieChartAndList(WeekAppUsage.ElementAt(GetDayIndex(LoadedDate)));
        }

        public void LoadPreviousDay()
        {
            WeeklyChart_SelectionChanged(GetDayIndex(LoadedDate.AddDays(-1)));
        }

        public void LoadNextDay()
        {
            WeeklyChart_SelectionChanged(GetDayIndex(LoadedDate.AddDays(1)));
        }

        private int GetDayIndex(DateTime date)
        {
            return Array.FindIndex(WeeklyChartLabelDates, labelDates => labelDates.Date == date.Date);
        }

        public void NotifyChange()
        {
            OnPropertyChanged(nameof(StrLoadedDate));
            OnPropertyChanged(nameof(StrTotalDuration));
            OnPropertyChanged(nameof(StrMinumumDuration));
            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrev));
        }

        public AppUsageListItem OnAppUsageChart_SelectionChanged(ChartPoint chartPoint)
        {
            try
            {
                return DayListItems.Single(listItem =>
                {
                    Debug.WriteLine(listItem);
                    Debug.WriteLine(chartPoint);
                    return listItem.ProcessName == chartPoint.SeriesView.Title;
                });
            }
            catch
            {
                return null;
            }
        }

        public void WeeklyChart_SelectionChanged(int index)
        {
            try
            {
                DateTime selectedDate = WeeklyChartLabelDates.ElementAt(index);

                // If selected date is already shown (loaded) and it is not the date today
                // Avoid Refresh, but Refresh if date is today
                if (selectedDate == LoadedDate && selectedDate != DateTime.Now.Date)
                {
                    return;
                }
                else
                {
                    LoadedDate = selectedDate;

                    DayPieChartData.Clear();
                    DayListItems.Clear();

                    UpdatePieChartAndList(WeekAppUsage.ElementAt(index));
                }
            }
            catch (IndexOutOfRangeException)
            {
                Debug.WriteLine("Element index exceeded in WeeklyChart");
            } catch { }
        }

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
