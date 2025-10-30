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

namespace LifestyleDesign.On_Startup
{
    /// <summary>
    /// Interaction logic for frmUncatagorizedViewsWarning.xaml
    /// </summary>
    public partial class frmUncategorizedViewsWarning : Window
    {
        public frmUncategorizedViewsWarning(List<string> viewNames)
        {
            InitializeComponent();

            // Assuming you have a ListBox or ListView in XAML named 'ViewList'
            ViewList.ItemsSource = viewNames;
        }
    }
}
