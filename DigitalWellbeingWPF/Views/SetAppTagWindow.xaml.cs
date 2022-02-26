using DigitalWellbeingWPF.Helpers;
using DigitalWellbeingWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DigitalWellbeingWPF.Views
{
    /// <summary>
    /// Interaction logic for SetAppTag.xaml
    /// </summary>
    public partial class SetAppTagWindow : Window
    {
        string _processName = "";
        AppTag _tag = AppTag.Untagged;
        Dictionary<string, int> appTags;

        public SetAppTagWindow(string processName)
        {
            InitializeComponent();

            _processName = processName;
            this.Title += $" for {_processName}";
            txtLabel.Text += $" for {_processName}";

            LoadCBTagChoices();

            // Reload current tag
            AppTag tag = AppTagHelper.GetAppTag(_processName);
            CBTag.SelectedItem = AppTagHelper.GetTagDisplayName(tag);
        }

        private void LoadCBTagChoices()
        {
            CBTag.Items.Clear();

            appTags = AppTagHelper.GetComboBoxChoices();

            foreach (KeyValuePair<string, int> appTag in appTags)
            {
                CBTag.Items.Add(appTag.Key);
            }
        }

        private void CBTag_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            _tag = (AppTag)appTags[cb.SelectedItem.ToString()];

            Console.WriteLine(_processName + ":" + _tag.ToString());
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.UpdateAppTag(_processName, _tag);

            this.Close();
        }
    }
}
