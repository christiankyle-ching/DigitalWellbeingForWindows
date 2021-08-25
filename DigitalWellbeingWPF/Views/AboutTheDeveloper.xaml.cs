using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for AboutTheDeveloper.xaml
    /// </summary>
    public partial class AboutTheDeveloper : Window
    {
        private readonly string emailLink = "mailto:ckching.dev@gmail.com";
        private readonly string githubLink = "https://github.com/christiankyle-ching";
        private readonly string websiteLink = "https://christiankyleching.vercel.app/";

        public AboutTheDeveloper()
        {
            InitializeComponent();
        }

        private void BtnEmail_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(emailLink);
        }

        private void BtnGithub_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(githubLink);
        }

        private void BtnWebsite_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(websiteLink);
        }
    }
}
