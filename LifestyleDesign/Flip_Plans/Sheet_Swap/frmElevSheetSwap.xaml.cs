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
    /// Interaction logic for frmElevSheetSwap.xaml
    /// </summary>
    public partial class frmElevSheetSwap : Window
    {
        public frmElevSheetSwap(List<ViewSheet> elevSheets)
        {
            InitializeComponent();

            tbkLeft2.IsEnabled = false;
            tbkRight2.IsEnabled = false;
            cbxLeft2.IsEnabled = false;
            cbxRight2.IsEnabled = false;

            chbSplit.IsChecked = false;

            foreach (ViewSheet sheet in elevSheets)
            {
                cbxLeft1.Items.Add(sheet.SheetNumber);
                cbxRight1.Items.Add(sheet.SheetNumber);
                cbxLeft2.Items.Add(sheet.SheetNumber);
                cbxRight2.Items.Add(sheet.SheetNumber);
            }
        }

        private void chbSplit_Checked(object sender, RoutedEventArgs e)
        {
            // if checked show bottom combo boxes

            // not then hide bottom combo boxes

            Controls.CheckBox cBox = (Controls.CheckBox)sender;

            if (cBox.IsChecked == true) { tbkLeft2.IsEnabled = true; cbxLeft2.IsEnabled = true; tbkRight2.IsEnabled = true; cbxRight2.IsEnabled = true; }
            else { cbxLeft2.IsEnabled = false; cbxRight2.IsEnabled = false; tbkLeft2.IsEnabled = false; tbkRight2.IsEnabled = false; }
        }

        internal bool GetCheckBox1()
        {
            if (chbSplit.IsChecked == true)
                return true;

            return false;
        }

        internal string GetComboBoxLeft1Item()
        {
            return cbxLeft1.SelectedItem.ToString();
        }

        internal string GetComboBoxRight1Item()
        {
            return cbxRight1.SelectedItem.ToString();
        }

        internal string GetComboBoxLeft2Item()
        {
            return cbxLeft2.SelectedItem.ToString();
        }

        internal string GetComboBoxRight2Item()
        {
            return cbxRight2.SelectedItem.ToString();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
