using Snake.Properties;
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

namespace Snake {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window {
        public SettingsWindow() {
            InitializeComponent();

            GameWidthTB.Text = Settings.Default.GameWidth.ToString();
            GameHeightTB.Text = Settings.Default.GameHeight.ToString();
            NoSnakesTB.Text = Settings.Default.NoSnakes.ToString();
            NoGamesAtOnceTB.Text = Settings.Default.NoGamesAtOnce.ToString();
            PenaltyTB.Text = Settings.Default.Penalty.ToString();
            NoRoundsTB.Text = Settings.Default.NoRounds.ToString();
            AppleTimeSpanTB.Text = Settings.Default.AppleEnergy.ToString();
            MutateratioTB.Text = Settings.Default.MutateRatio.ToString();
            TresholdTB.Text = Settings.Default.Treshold.ToString();
            SkipRoundsTB.Text = Settings.Default.skipRoudns.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Settings.Default.GameWidth = int.Parse(GameWidthTB.Text);
            Settings.Default.GameHeight = int.Parse(GameHeightTB.Text);
            Settings.Default.NoSnakes = int.Parse(NoSnakesTB.Text);
            Settings.Default.NoGamesAtOnce = int.Parse(NoGamesAtOnceTB.Text);
            Settings.Default.Penalty = int.Parse(PenaltyTB.Text);
            Settings.Default.NoRounds = int.Parse(NoRoundsTB.Text);
            Settings.Default.AppleEnergy = int.Parse(AppleTimeSpanTB.Text);
            Settings.Default.MutateRatio = double.Parse(MutateratioTB.Text);
            Settings.Default.Treshold = double.Parse(TresholdTB.Text);
            Settings.Default.skipRoudns = int.Parse(SkipRoundsTB.Text);

            Settings.Default.Save();
        }
    }
}
