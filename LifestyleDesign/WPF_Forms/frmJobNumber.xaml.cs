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
    /// Interaction logic for frmJobNumber.xaml
    /// </summary>
    public partial class frmJobNumber : Window
    {
        public frmJobNumber()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            GlobalVars.JobNumber = this.tbxJobNum.Text.Trim();
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