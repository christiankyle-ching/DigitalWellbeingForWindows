using DigitalWellbeing.Core;
using DigitalWellbeingWPF.Helpers;
using DigitalWellbeingWPF.Models;
using DigitalWellbeingWPF.Models.UserControls;
using DigitalWellbeingWPF.Views;
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
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace DigitalWellbeingWPF.ViewModels
{
    public class AppUsageViewModel : INotifyPropertyChanged
    {
        #region Configurations
        public static readonly int PrevDaysToLoad = 7;
        public static readonly int MinimumPieChartPercentage = 10;
        #endregion

        #region Temporary 
        private readonly static string folderPath = ApplicationPath.UsageLogsFolder;

        private DispatcherTimer refreshTimer;
        #endregion

        #region Formatters
        public Func<double, string> HourFormatter { get; set; }
        private Func<ChartPoint, string> PieChartLabelFormatter { get; set; }
        private Func<ChartPoint, string> TagChartFormatter { get; set; }
        #endregion

        #region String Bindings
        // Loaded Date
        public DateTime LoadedDate = DateTime.Now.Date;
        public string StrLoadedDate
        {
            get => (LoadedDate.Date == DateTime.Now.Date) ?
                "Today, " + this.LoadedDate.ToString("dddd") :
                this.LoadedDate.ToString("dddd, MMM dd yyyy");
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
                return Properties.Settings.Default.MinumumDuration.TotalSeconds <= 0
                    ? ""
                    : $"Apps that run less than {StringHelper.TimeSpanToString(Properties.Settings.Default.MinumumDuration)} are hidden.";
            }
        }
        #endregion

        #region Collections
        public ObservableCollection<List<AppUsage>> WeekAppUsage { get; set; } // Week's App Usage
        public SeriesCollection WeeklyChartData { get; set; } // Week's App Usage
        public DateTime[] WeeklyChartLabelDates { get; set; }
        public string[] WeeklyChartLabels { get; set; }
        public SeriesCollection DayPieChartData { get; set; } // Pie Chart Data
        public ObservableCollection<AppUsageListItem> DayListItems { get; set; } // List Items
        public SeriesCollection TagsChartData { get; set; } // Tags's Combined Usage

        // Excluded Processes
        private static readonly string[] excludeProcesses = new string[]
        {
            // Exclude Self
            "DigitalWellbeingWPF",

            // Windows-specific Processes
            "explorer",
            "SearchHost",
            "Idle",
            "StartMenuExperienceHost",
            "ShellExperienceHost",
            "dwm",
            "LockApp",
            "msiexec",
            "ApplicationFrameHost",
            
            // Custom Indicators (from Service)
            "*LAST",
        };
        private static string[] userExcludedProcesses;
        #endregion

        #region Getters with Bindings
        public event PropertyChangedEventHandler PropertyChanged;
        public bool CanGoNext { get => LoadedDate.Date < DateTime.Now.Date; }
        public bool CanGoPrev { get => LoadedDate.Date > DateTime.Now.AddDays(-PrevDaysToLoad + 1).Date; }
        public bool IsLoading { get; set; }
        public double PieChartInnerRadius
        {
            get; set;
        }

        public void OnPageResize(double width, double height)
        {
            double area = width * height;
            PieChartInnerRadius = Math.Sqrt(area / 10);
            OnPropertyChanged(nameof(PieChartInnerRadius));
        }

        public bool IsWeeklyDataLoaded = false;
        #endregion

        public AppUsageViewModel()
        {
            InitCollections();
            InitFormatters();

            LoadUserExcludedProcesses();

            LoadWeeklyData();

            InitAutoRefreshTimer();
        }

        #region Init Functions
        private void InitCollections()
        {
            WeekAppUsage = new ObservableCollection<List<AppUsage>>();
            WeeklyChartData = new SeriesCollection();
            WeeklyChartLabels = new string[0];
            WeeklyChartLabelDates = new DateTime[0];
            TagsChartData = new SeriesCollection();

            DayPieChartData = new SeriesCollection();
            DayListItems = new ObservableCollection<AppUsageListItem>();
        }

        private void InitFormatters()
        {
            HourFormatter = (hours) => hours.ToString("F1") + " h";
            //PieChartLabelFormatter = (chartPoint) => string.Format("{0:F2} min/s", chartPoint.Y);
            PieChartLabelFormatter = (chartPoint) => string.Format("{0}", chartPoint.SeriesView.Title);

            TagChartFormatter = (chartPoint) => string.Format("{0}", chartPoint.SeriesView.Title);
        }

        private void InitAutoRefreshTimer()
        {
            int refreshInterval = Properties.Settings.Default.RefreshIntervalSeconds;
            TimeSpan intervalDuration = TimeSpan.FromSeconds(refreshInterval);
            refreshTimer = new DispatcherTimer() { Interval = intervalDuration };
            refreshTimer.Tick += (s, e) => TryRefreshData();
        }

        private void LoadUserExcludedProcesses()
        {
            userExcludedProcesses = Properties.Settings.Default.UserExcludedProcesses.Cast<string>().ToArray();
        }

        private async void LoadWeeklyData()
        {
            SetLoading(true);

            try
            {
                DateTime minDate = DateTime.Now.AddDays(-PrevDaysToLoad);

                List<List<AppUsage>> weekUsage = new List<List<AppUsage>>();
                ChartValues<double> hours = new ChartValues<double>();
                List<string> labels = new List<string>();
                List<DateTime> loadedDates = new List<DateTime>();

                // Load last week's data
                for (int i = 1; i <= PrevDaysToLoad; i++)
                {
                    DateTime date = minDate.AddDays(i).Date;

                    // Store App Usage List
                    List<AppUsage> appUsageList = await GetData(date);
                    List<AppUsage> filteredUsageList = appUsageList.Where(appUsageFilter).ToList();
                    filteredUsageList.Sort(appUsageSorter);

                    // Calculate Total Hours
                    TimeSpan totalDuration = TimeSpan.Zero;
                    foreach (AppUsage app in filteredUsageList)
                    {
                        totalDuration = totalDuration.Add(app.Duration);
                    }

                    weekUsage.Add(filteredUsageList);
                    hours.Add(totalDuration.TotalHours);
                    labels.Add(date.ToString("ddd"));
                    loadedDates.Add(date);
                }

                // Add all values at once
                foreach (List<AppUsage> dayUsage in weekUsage) { WeekAppUsage.Add(dayUsage); } // AppUsage
                // Bar Chart : Data
                WeeklyChartData.Add(new ColumnSeries { Values = hours });
                // Bar Chart : Labels
                WeeklyChartLabels = labels.ToArray();
                // Bar Chart : Labels as DateTime for SelectionChanged
                WeeklyChartLabelDates = loadedDates.ToArray();

                IsWeeklyDataLoaded = true;

                WeeklyChart_SelectionChanged(WeekAppUsage.Count - 1);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLine($"Load Weekly Data Exception {ex}");
            }
            finally
            {
                SetLoading(false);
            }
        }
        #endregion

        #region Events
        public void OnNavigate()
        {
            ReloadSettings();
            TryRefreshData();
        }

        public void OnExcludeApp(string processName)
        {
            try
            {
                LoadUserExcludedProcesses();

                // Remove List Item
                AppUsageListItem listItem = DayListItems.Single(item => item.ProcessName == processName);
                DayListItems.Remove(listItem);

                // Remove in Pie: Might throw due to aggregated data in Pie > Other Apps
                PieSeries pieChartSeries = (PieSeries)DayPieChartData.Single(pieSeries => pieSeries.Title == processName);
                DayPieChartData.Remove(pieChartSeries);
            }
            catch (Exception ex)
            {
                // Cannot remove from pie
                Console.WriteLine(ex);
            }
            finally
            {
                RefreshTagChart();
            }
        }

        public void OnSetTimeLimit(string processName)
        {
            SetTimeLimitWindow window = new SetTimeLimitWindow(processName);
            window.ShowDialog();

            Notifier.ResetNotificationForApp(processName);
        }

        public void OnSetAppTag(string processName)
        {
            SetAppTagWindow window = new SetAppTagWindow(processName);
            window.ShowDialog();

            try
            {
                AppUsageListItem item = DayListItems.Single(i => i.ProcessName == processName);
                item._AppTag = AppTagHelper.GetAppTag(processName);
            }
            catch { }

            RefreshListItems();
            RefreshTagChart();
        }

        public AppUsageListItem OnAppUsageChart_SelectionChanged(ChartPoint chartPoint)
        {
            try
            {
                return DayListItems.Single(listItem =>
                {
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

                    TryRefreshData();
                    UpdatePieChartAndList(WeekAppUsage.ElementAt(index));
                }
            }
            catch (IndexOutOfRangeException)
            {
                AppLogger.WriteLine("Element index exceeded in WeeklyChart");
            }
            catch { }
        }
        #endregion

        // * -1 for desc, default is asc
        Comparison<AppUsage> appUsageSorter = (a, b) => a.Duration.CompareTo(b.Duration) * -1;
        Func<AppUsage, bool> appUsageFilter = (a) => !IsProcessExcluded(a.ProcessName);

        private async void TryRefreshData()
        {
            // If weekly data not loaded yet, do not refresh
            if (!IsWeeklyDataLoaded) return;

            // Refresh Data only if loaded date is set today
            // Only refresh data when the selected date is today,
            // Else, no point in auto-refreshing non-changing data.
            if (DateTime.Now.Date == LoadedDate.Date)
            {
                try
                {
                    List<AppUsage> appUsageList = await GetData(LoadedDate.Date);

                    // Also update source, which is the last in list
                    WeekAppUsage[WeekAppUsage.Count - 1] = appUsageList;

                    List<AppUsage> filteredUsageList = appUsageList.Where(appUsageFilter).ToList();
                    filteredUsageList.Sort(appUsageSorter);
                    UpdatePieChartAndList(filteredUsageList);

                    // Refresh Bar Graph
                    WeeklyChartData.ElementAt(0).Values[GetDayIndex(LoadedDate.Date)] = TotalDuration.TotalHours;
                }
                catch
                {
                    AppLogger.WriteLine("Skip Refresh");
                }
            }
        }

        private void RefreshListItems()
        {
            foreach (AppUsageListItem item in DayListItems)
            {
                item.Refresh();
            }
        }

        private void RefreshTagChart()
        {
            List<AppUsage> usageList = GetDayUsage(LoadedDate);

            // Load Tags
            SeriesCollection tempTagChartData = new SeriesCollection();
            Dictionary<string, double> tagHours = new Dictionary<string, double>();
            double totalMinutes = 0;

            // Load empty first
            foreach (string tagName in Enum.GetNames(typeof(AppTag)))
            {
                tagHours.Add(tagName, 0);
            }

            // Combine hours of different tags
            foreach (AppUsage app in usageList)
            {
                string appTag = EnumUtils.GetEnumName(AppTagHelper.GetAppTag(app.ProcessName));
                tagHours[appTag] += app.Duration.TotalMinutes;

                totalMinutes += app.Duration.TotalMinutes;
            }

            // Sort from largest to smallest
            List<KeyValuePair<string, double>> sortedTagHours = tagHours.ToList();
            sortedTagHours.Sort((a, b) => a.Value.CompareTo(b.Value) * -1);
            // Then place AppTag.Untagged at last
            int untaggedIndex = sortedTagHours.IndexOf(sortedTagHours.Single(t => t.Key == EnumUtils.GetEnumName(AppTag.Untagged)));
            sortedTagHours.Add(sortedTagHours[untaggedIndex]);
            sortedTagHours.RemoveAt(untaggedIndex);

            // Prevents dividing by zero
            if (totalMinutes > 0)
            {
                foreach (KeyValuePair<string, double> tagHour in sortedTagHours)
                {
                    double percentage = tagHour.Value / totalMinutes;

                    tempTagChartData.Add(new StackedRowSeries()
                    {
                        Values = new ChartValues<double> { percentage },
                        DataLabels = percentage >= 0.1,
                        Title = tagHour.Key,
                        LabelPoint = TagChartFormatter,
                        Fill = AppTagHelper.GetTagColor(tagHour.Key),
                    });
                }
            }
            else
            {
                tempTagChartData.Add(new StackedRowSeries()
                {
                    Values = new ChartValues<double> { 1.0 },
                    DataLabels = true,
                    Title = "No Data",
                    LabelPoint = TagChartFormatter,
                    Fill = AppTagHelper.GetTagColor(AppTag.Untagged),
                });
            }

            TagsChartData.Clear();
            TagsChartData.AddRange(tempTagChartData);
        }

        private void ReloadSettings()
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
                try
                {
                    refreshTimer.Stop();
                }
                catch (NullReferenceException)
                {
                    // No timer to start with
                    AppLogger.WriteLine("No Timer");
                }
            }

            LoadUserExcludedProcesses();
        }

        #region Functions
        public void LoadPreviousDay()
        {
            WeeklyChart_SelectionChanged(GetDayIndex(LoadedDate.AddDays(-1)));
        }

        public void LoadNextDay()
        {
            WeeklyChart_SelectionChanged(GetDayIndex(LoadedDate.AddDays(1)));
        }

        private void UpdatePieChartAndList(List<AppUsage> appUsageList)
        {
            SetLoading(true);

            List<AppUsage> filteredUsageList = appUsageList.Where(appUsageFilter).ToList();

            try
            {
                TotalDuration = TimeSpan.Zero;

                PieSeries noDataSeries = new PieSeries()
                {
                    Title = "No Data",
                    Fill = Brushes.LightGray,
                    Values = new ChartValues<double> { 1 },
                    LabelPoint = PieChartLabelFormatter,
                };
                PieSeries otherProcessesSeries = new PieSeries()
                {
                    Title = "Other Apps",
                    LabelPoint = PieChartLabelFormatter,
                };
                double otherProcessesTotalMinutes = 0;

                SeriesCollection tempPieChartData = new SeriesCollection();
                ObservableCollection<AppUsageListItem> tempListItems = new ObservableCollection<AppUsageListItem>();

                // Calculate Total Duration
                foreach (AppUsage app in filteredUsageList)
                {
                    TotalDuration = TotalDuration.Add(app.Duration);
                }

                // Add List Items and Chart Items
                foreach (AppUsage app in filteredUsageList)
                {
                    int percentage = (int)Math.Round(app.Duration.TotalSeconds / TotalDuration.TotalSeconds * 100);

                    string durationStr = StringHelper.TimeSpanToString(app.Duration);

                    string label = app.ProcessName;
                    if (durationStr != "") { label += $" ({durationStr})"; }

                    // Add Chart Points
                    if (app.Duration > Properties.Settings.Default.MinumumDuration)
                    {
                        if (percentage <= MinimumPieChartPercentage)
                        {
                            otherProcessesTotalMinutes += app.Duration.TotalMinutes;
                        }
                        else
                        {
                            tempPieChartData.Add(new PieSeries()
                            {
                                Title = app.ProcessName,
                                Values = new ChartValues<double> { app.Duration.TotalMinutes },
                                LabelPoint = PieChartLabelFormatter,
                            });
                        }
                    }

                    // Add List Item
                    if (app.Duration > Properties.Settings.Default.MinumumDuration)
                    {
                        tempListItems.Add(new AppUsageListItem(app.ProcessName, app.ProgramName, app.Duration, percentage, AppTagHelper.GetAppTag(app.ProcessName)));
                    }
                }

                // Add Chart Point (Other Processes)
                if (otherProcessesTotalMinutes > 0)
                {
                    otherProcessesSeries.Values = new ChartValues<double> { otherProcessesTotalMinutes };
                    tempPieChartData.Add(otherProcessesSeries);
                }

                // Update UI Data
                DayPieChartData.Clear();
                if (tempPieChartData.Count > 0)
                {
                    DayPieChartData.AddRange(tempPieChartData);
                }
                else
                {
                    DayPieChartData.Add(noDataSeries);
                }

                DayListItems.Clear();
                foreach (AppUsageListItem item in tempListItems)
                {
                    DayListItems.Add(item);
                }

                RefreshTagChart();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLine(ex);
            }
            finally
            {
                NotifyChange();
                SetLoading(false);
            }
        }
        #endregion

        #region Getters
        private int GetDayIndex(DateTime date)
        {
            return Array.FindIndex(WeeklyChartLabelDates, labelDates => labelDates.Date == date.Date);
        }

        private List<AppUsage> GetDayUsage(DateTime date)
        {
            List<AppUsage> appUsageList = WeekAppUsage[GetDayIndex(date)];
            List<AppUsage> filteredUsageList = appUsageList.Where(appUsageFilter).ToList();
            filteredUsageList.Sort(appUsageSorter);

            return filteredUsageList;
        }

        public static bool IsProcessExcluded(string processName)
        {
            return excludeProcesses.Contains(processName) || userExcludedProcesses.Contains(processName);
        }

        public static async Task<List<AppUsage>> GetData(DateTime date)
        {
            int retryCount = 0;
            const int maxRetries = 3;

            do
            {
                // Always retry with an empty list
                List<AppUsage> appUsageList = new List<AppUsage>();

                try
                {
                    string text = await Task.Run(() => File.ReadAllText($"{folderPath}{date:MM-dd-yyyy}.log")); ;
                    string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                    // Parse .log data ROWS
                    for (int i = 0; i < lines.Length; i++)
                    {
                        try
                        {
                            // Skip empty rows
                            if (lines[i].Trim() == string.Empty) continue;

                            string[] cells = lines[i].Split('\t');

                            string processName = cells[0];
                            int seconds = 0;
                            string programName = cells.Length > 2 ? cells[2] : "";

                            // Try get seconds
                            int.TryParse(cells[1], out seconds);

                            appUsageList.Add(
                                new AppUsage(processName, programName, TimeSpan.FromSeconds(seconds))
                            );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("SKIP READING ROW: " + ex);
                        }
                    }

                    return appUsageList;
                }
                catch (FileNotFoundException)
                {
                    // If not found, just return []
                    AppLogger.WriteLine($"CANNOT FIND: {folderPath}{date:MM-dd-yyyy}.log");
                    return appUsageList;
                }
                catch (DirectoryNotFoundException)
                {
                    // If not found, just return []
                    Directory.CreateDirectory(folderPath);
                    return appUsageList;
                }
                catch (IOException)
                {
                    // TODO: RETRY HERE
                    retryCount++;
                }
                catch (Exception ex)
                {
                    // Global Exception: throw to report
                    AppLogger.WriteLine(ex.Message);
                    throw;
                }
            } while (retryCount < maxRetries);

            // If after 3 retries and still cannot access, just return empty
            return new List<AppUsage>();
        }
        #endregion

        #region Setters
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
        #endregion

        public void NotifyChange()
        {
            OnPropertyChanged(nameof(StrLoadedDate));
            OnPropertyChanged(nameof(StrTotalDuration));
            OnPropertyChanged(nameof(StrMinumumDuration));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrev));

            RefreshListItems();
        }

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
