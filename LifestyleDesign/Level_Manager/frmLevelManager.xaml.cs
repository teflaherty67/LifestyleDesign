using LifestyleDesign.Classes;
using LifestyleDesign.Common;
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

namespace LifestyleDesign
{
    /// <summary>
    /// Interaction logic for frmLevelManager.xaml
    /// </summary>
    public partial class frmLevelManager : Window
    {
        // create list to hold the levels
        private List<Level> Levels;

        // create a dictionary to hold the level-adjustment pairs
        private Dictionary<Level, TextBox> Level_TextBoxes = new Dictionary<Level, TextBox>();

        // create public property for Dictionary
        public Dictionary<Level, double> LevelAdjustments { get; private set; }

        public frmLevelManager(List<Level> levels)
        {
            // store passed data
            Levels = levels;

            // intialize the XAML controls
            InitializeComponent();

            // generate level controls
            GenerateLevelControls();

            // configure window controls based on story count
            ConfigureWindowControlsForStoryCount();
        }

        #region Global Form Controls

        private void cbxGlobal_Checked(object sender, RoutedEventArgs e)
        {
            txtGlobalAdjustment.IsEnabled = true;

            // Disable all individual level textboxes
            foreach (var textBox in Level_TextBoxes.Values)
            {
                textBox.IsEnabled = false;
            }

            // validate and update the UI
            ValidateAndUpdateUI();
        }

        private void cbxGlobal_Unchecked(object sender, RoutedEventArgs e)
        {
            txtGlobalAdjustment.IsEnabled = false;
            txtGlobalAdjustment.Text = ""; // Clear the value when unchecked

            // Re-enable all individual level textboxes
            foreach (var textBox in Level_TextBoxes.Values)
            {
                textBox.IsEnabled = true;
            }

            // validate and update the UI
            ValidateAndUpdateUI();
        }

        private void txtGlobalAdjustment_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateAndUpdateUI();
        }

        #endregion

        #region Dynamic Controls

        private void GenerateLevelControls()
        {
            // loop through the levels and create a control for each level
            foreach (Level curLevel in Levels)
            {
                // create a grid with 3 columns
                Grid levelGrid = new Grid() { Margin = new Thickness(0, 2, 0, 2) };
                levelGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) }); // level name
                levelGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // current elevation display
                levelGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // adjustment input

                // create & position the level name label
                Label lblLevel = new Label() { Content = curLevel.Name };
                Grid.SetColumn(lblLevel, 0);

                // create & position the current elevation display
                TextBox txtCurrentElevation = new TextBox()
                {
                    Text = FormatElevation(curLevel.Elevation),
                    IsReadOnly = true,
                    Background = System.Windows.Media.Brushes.LightGray,
                    Width = 80,
                    Margin = new Thickness(5, 0, 5, 0)
                };
                Grid.SetColumn(txtCurrentElevation, 1);

                // create the adjustment input textbox
                TextBox txbLevel = new TextBox() { Width = 80 };

                // add the TextChanged event handler
                txbLevel.TextChanged += (s, e) => ValidateAndUpdateUI();

                // position the adjustment input textbox
                Grid.SetColumn(txbLevel, 2);

                // store the TextBox reference
                Level_TextBoxes[curLevel] = txbLevel;

                // add all controls to the grid
                levelGrid.Children.Add(lblLevel);
                levelGrid.Children.Add(txtCurrentElevation);
                levelGrid.Children.Add(txbLevel);

                // add grid to the stack panel
                sp.Children.Add(levelGrid);
            }
        }

        #endregion

        #region Window Adjustement Controls

        private void chkFirstFloorHeadHeights_Checked(object sender, RoutedEventArgs e)
        {
            chkFirstFloorWindowHeights.IsEnabled = true;
        }

        private void chkFirstFloorHeadHeights_Unchecked(object sender, RoutedEventArgs e)
        {
            chkFirstFloorWindowHeights.IsEnabled = false;
            chkFirstFloorWindowHeights.IsChecked = false;
        }

        private void chkSecondFloorHeadHeights_Checked(object sender, RoutedEventArgs e)
        {
            chkSecondFloorWindowHeights.IsEnabled = true;
        }

        private void chkSecondFloorHeadHeights_Unchecked(object sender, RoutedEventArgs e)
        {
            chkSecondFloorWindowHeights.IsEnabled = false;
            chkSecondFloorWindowHeights.IsChecked = false;
        }

        public bool IsFirstFloorHeadHeightsChecked() => chkFirstFloorHeadHeights.IsChecked == true;
        public bool IsFirstFloorWindowHeightsChecked() => chkFirstFloorWindowHeights.IsChecked == true;
        public bool IsSecondFloorHeadHeightsChecked() => chkSecondFloorHeadHeights.IsChecked == true;
        public bool IsSecondFloorWindowHeightsChecked() => chkSecondFloorWindowHeights.IsChecked == true;



        #endregion

        #region Buttons Section

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            LevelAdjustments = LevelAdjustmentData();
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // launch the help site with user's default browser
                string helpUrl = "https://lifestyle-usa-design.atlassian.net/wiki/spaces/MFS/pages/472711169/Spec+Level+Conversion?atlOrigin=eyJpIjoiMmU4MzM3NzFmY2NlNDdiNjk1MjY2M2MyYzZkMjY2YWQiLCJwIjoiYyJ9";
                Process.Start(new ProcessStartInfo
                {
                    FileName = helpUrl,
                    UseShellExecute = true
                });

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("An error occurred while trying to display help: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Helper Methods

        private Dictionary<Level, double> LevelAdjustmentData()
        {
            var levelAdjustements = new Dictionary<Level, double>();

            // Check if global adjustment is being used
            if (cbxGlobal.IsChecked == true)
            {
                // Use global value for all levels
                if (double.TryParse(txtGlobalAdjustment.Text.Replace("\"", ""), out double globalAdjustment))
                {
                    foreach (var kvp in Level_TextBoxes)
                    {
                        levelAdjustements[kvp.Key] = Utils.ConvertINToFT(globalAdjustment);
                    }
                }
                else
                {
                    // Invalid global input - set all to 0
                    foreach (var kvp in Level_TextBoxes)
                    {
                        levelAdjustements[kvp.Key] = 0;
                    }
                }
            }
            else
            {
                // Use individual textbox values (existing logic)
                foreach (var kvp in Level_TextBoxes)
                {
                    Level level = kvp.Key;
                    TextBox textBox = kvp.Value;

                    if (double.TryParse(textBox.Text.Replace("\"", ""), out double adjustment))
                    {
                        levelAdjustements[level] = Utils.ConvertINToFT(adjustment);
                    }
                    else
                    {
                        levelAdjustements[level] = 0;
                    }
                }
            }

            return levelAdjustements;
        }

        private bool CheckForOrderingViolations()
        {
            // Get the original ordering of levels by elevation
            var originalOrder = Levels.OrderBy(l => l.Elevation).ToList();

            // Use your existing method to get current adjustments
            var adjustments = LevelAdjustmentData();
            var projectedElevations = new List<(Level level, double newElevation)>();

            foreach (var level in originalOrder)
            {
                double adjustment = adjustments.ContainsKey(level) ? adjustments[level] : 0;
                double newElevation = level.Elevation + adjustment; // Note: already converted to feet
                projectedElevations.Add((level, newElevation));
            }

            // Check if the new elevations maintain the same ordering
            var newOrder = projectedElevations.OrderBy(x => x.newElevation).Select(x => x.level).ToList();

            // Compare the orderings
            for (int i = 0; i < originalOrder.Count; i++)
            {
                if (originalOrder[i].Id != newOrder[i].Id)
                    return true; // Violation found
            }

            return false; // No violations
        }

        private void ValidateAndUpdateUI()
        {
            bool hasViolations = CheckForOrderingViolations();

            // Update OK button
            btnOK.IsEnabled = !hasViolations;

            if (hasViolations)
            {
                btnOK.Background = System.Windows.Media.Brushes.Red;
                btnOK.Foreground = System.Windows.Media.Brushes.White;
            }
            else
            {
                // Make OK button match the other buttons exactly
                btnOK.Background = btnCancel.Background;
                btnOK.Foreground = btnCancel.Foreground;
                btnOK.BorderBrush = btnCancel.BorderBrush;
            }

            // Update warning label
            lblWarning.Visibility = hasViolations ?
                System.Windows.Visibility.Visible :
                System.Windows.Visibility.Collapsed;
        }

        private void ConfigureWindowControlsForStoryCount()
        {
            bool isMultiStory = Levels.Any(level =>
                ProjectConstants.MultiStoryIndicators.Any(indicator =>
                    level.Name.Contains(indicator)));

            // Hide second floor section if single story
            spSecondFloorSection.Visibility = isMultiStory ?
                System.Windows.Visibility.Visible :
                System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Formats a decimal elevation value in feet to a string in feet, inches, and eighths (e.g. 9'-1 1/8").
        /// </summary>
        /// <param name="elevationInFeet">The elevation value in feet.</param>
        /// <returns>A formatted string representing the elevation.</returns>
        private string FormatElevation(double elevationInFeet)
        {
            // extract whole feet
            int feet = (int)elevationInFeet;

            // convert the remaining decimal to inches
            double inches = (elevationInFeet - feet) * 12;
            int wholeInches = (int)inches;
            double fractionalInches = inches - wholeInches;

            // convert fractional inches to nearest 1/8"
            int eighths = (int)Math.Round(fractionalInches * 8);

            // handle rounding that bumps up to the next whole inch
            if (eighths == 8)
            {
                wholeInches++;
                eighths = 0;
            }

            // return formatted string based on whether there is a fraction
            if (eighths == 0)
            {
                return wholeInches == 0
                    ? $"{feet}'-0\""
                    : $"{feet}'-{wholeInches}\"";
            }
            else
            {
                // Reduce the fraction to its simplest form
                string fractionPart = GetReducedFraction(eighths, 8);
                return $"{feet}'-{wholeInches} {fractionPart}\"";
            }
        }

        private string GetReducedFraction(int numerator, int denominator)
        {
            // Find the greatest common divisor
            int gcd = GetGCD(numerator, denominator);

            // Reduce the fraction
            int reducedNum = numerator / gcd;
            int reducedDen = denominator / gcd;

            return $"{reducedNum}/{reducedDen}";
        }

        private int GetGCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        #endregion
    }
}
