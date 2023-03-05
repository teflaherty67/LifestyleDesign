using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LifestyleDesign
{
    /// <summary>
    /// Interaction logic for frmNewSheetGroup.xaml
    /// </summary>
    public partial class frmNewSheetGroup : Window
    {
        public frmNewSheetGroup()
        {
            InitializeComponent();
        }

        public string GetTextBoxValue()
        {
            return tbxElevation.Text;
        }

        public string GetGroup1()
        {
            if (rbBasement.IsChecked == true)
                return rbBasement.Content.ToString();
            else if (rbCrawlspace.IsChecked == true)
                return rbCrawlspace.Content.ToString();
            else
                return rbSlab.Content.ToString();
        }

        public string GetGroup2()
        {
            if (rbOne.IsChecked == true)
                return rbOne.Content.ToString();
            else
                return rbTwo.Content.ToString();
        }

        public string Worksheet()
        {
            return GetGroup1().ToString() + '-' + GetGroup2().ToString();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
