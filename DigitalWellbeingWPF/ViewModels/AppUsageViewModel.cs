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
        private readonly string folderPath = ApplicationPath.UsageLogsFolder;

        private DispatcherTimer refreshTimer;
        #endregion

        #region Formatters
        public Func<double, string> HourFormatter { get; set; }
        private Func<ChartPoint, string> PieChartLabelFormatter { get; set; }
        #endregion

        #region String Bindings
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
                return Properties.Settings.Default.MinumumDuration.TotalSeconds <= 0
                    ? ""
                    : $"Apps that run less than {StringParser.TimeSpanToString(Properties.Settings.Default.MinumumDuration)} are hidden.";
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

        // Excluded Processes
        private readonly string[] excludeProcesses = new string[]
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
        private string[] userExcludedProcesses;
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

            try
            {
                LoadWeeklyData();
            }
            catch (Exception)
            {
                // TODO : Find a way to retry loading data
            }

            InitAutoRefreshTimer();
        }

        #region Init Functions
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
            //PieChartLabelFormatter = (chartPoint) => string.Format("{0:F2} min/s", chartPoint.Y);
            PieChartLabelFormatter = (chartPoint) => string.Format("{0}", chartPoint.SeriesView.Title);
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

                    // Calculate Total Hours
                    TimeSpan totalDuration = TimeSpan.Zero;
                    foreach (AppUsage app in appUsageList)
                    {
                        if (IsProcessExcluded(app.ProcessName)) continue;

                        totalDuration = totalDuration.Add(app.Duration);
                    }

                    weekUsage.Add(appUsageList);
                    hours.Add(totalDuration.TotalHours);
                    labels.Add(date.ToString("ddd"));
                    loadedDates.Add(date);
                }

                // Add all values at once
                foreach (List<AppUsage> dayUsage in weekUsage)
                {
                    WeekAppUsage.Add(dayUsage);
                }
                WeeklyChartData.Add(new ColumnSeries
                {
                    Values = hours,
                });
                WeeklyChartLabels = labels.ToArray();
                WeeklyChartLabelDates = loadedDates.ToArray();

                IsWeeklyDataLoaded = true;

                WeeklyChart_SelectionChanged(WeekAppUsage.Count - 1);
            }
            catch (IOException)
            {
                throw;
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
                PieSeries pieChartSeries = (PieSeries)DayPieChartData.Single(pieSeries => pieSeries.Title == processName);
                AppUsageListItem listItem = DayListItems.Single(item => item.ProcessName == processName);

                DayPieChartData.Remove(pieChartSeries);
                DayListItems.Remove(listItem);
            }
            catch { }
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
                    UpdatePieChartAndList(appUsageList);

                    // Refresh Bar Graph
                    WeeklyChartData.ElementAt(0).Values[GetDayIndex(LoadedDate.Date)] = TotalDuration.TotalHours;
                }
                catch
                {
                    AppLogger.WriteLine("Skip Refresh");
                }
            }
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
                foreach (AppUsage app in appUsageList)
                {
                    if (IsProcessExcluded(app.ProcessName)) continue;

                    TotalDuration = TotalDuration.Add(app.Duration);
                }

                // Add List Items and Chart Items
                foreach (AppUsage app in appUsageList)
                {
                    if (IsProcessExcluded(app.ProcessName)) continue;

                    int percentage = (int)Math.Round(app.Duration.TotalSeconds / TotalDuration.TotalSeconds * 100);

                    string durationStr = StringParser.TimeSpanToString(app.Duration);

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
                        tempListItems.Add(new AppUsageListItem(app.ProcessName, app.ProgramName, app.Duration, percentage));
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
            }
            catch { }
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

        private bool IsProcessExcluded(string processName)
        {
            return (excludeProcesses.Contains(processName) || userExcludedProcesses.Contains(processName));
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
                    string programName = data[2] != "" ? data[2] : StringParser.FormatProcessName(processName);

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
            }
            catch (FileNotFoundException)
            {
                AppLogger.WriteLine($"CANNOT FIND: {folderPath}{date:MM-dd-yyyy}.log");
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(folderPath);
            }
            catch (IOException)
            {
                AppLogger.WriteLine("Can't read, file is still being used");
                throw; // triggers catch in LoadWeeklyData()
            }
            catch (Exception ex)
            {
                AppLogger.WriteLine(ex.Message);
            }

            appUsageList.Sort((a, b) => a.Duration.CompareTo(b.Duration) * -1);
            return appUsageList;
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
        }

        private void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
