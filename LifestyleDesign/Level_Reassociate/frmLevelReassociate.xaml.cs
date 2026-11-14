using LifestyleDesign.Common;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LifestyleDesign
{
    /// <summary>
    /// Interaction logic for frmLevelReassociate.xaml
    /// </summary>
    public partial class frmLevelReassociate : Window
    {
        private class LevelItem
        {
            public string Display { get; set; }
            public Level Level { get; set; }
        }

        public Level SourceLevel { get; private set; }
        public Level TargetLevel { get; private set; }

        public frmLevelReassociate(Document curDoc, List<Level> levels)
        {
            InitializeComponent();

            List<LevelItem> items = levels
                .OrderBy(lvl => lvl.Elevation)
                .Select(lvl => new LevelItem { Display = FormatLevelDisplay(curDoc, lvl), Level = lvl })
                .ToList();

            cmbSourceLevel.ItemsSource = items;
            cmbTargetLevel.ItemsSource = items;

            if (items.Count > 0)
                cmbSourceLevel.SelectedIndex = 0;

            if (items.Count > 1)
                cmbTargetLevel.SelectedIndex = 1;
        }

        private static string FormatLevelDisplay(Document curDoc, Level lvl)
        {
            string elevText = UnitFormatUtils.Format(curDoc.GetUnits(), SpecTypeId.Length, lvl.Elevation, false);

            return $"{lvl.Name} ({elevText})";
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            LevelItem sourceItem = cmbSourceLevel.SelectedItem as LevelItem;
            LevelItem targetItem = cmbTargetLevel.SelectedItem as LevelItem;

            if (sourceItem == null || targetItem == null)
            {
                Utils.TaskDialogWarning("Reassociate Level", "Reassociate Level", "Please select both a level to reassociate and a target level.");
                return;
            }

            if (sourceItem.Level.Id == targetItem.Level.Id)
            {
                Utils.TaskDialogWarning("Reassociate Level", "Reassociate Level", "The level to reassociate and the target level must be different.");
                return;
            }

            SourceLevel = sourceItem.Level;
            TargetLevel = targetItem.Level;

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
