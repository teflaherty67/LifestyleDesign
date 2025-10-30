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
    using Autodesk.Revit.DB;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    namespace LifestyleDesign
    {
        public partial class frmUncategorizedViewsWarning : Window
        {
            public bool Proceed { get; private set; } = false;

            public frmUncategorizedViewsWarning(List<View> views)
            {
                InitializeComponent();

                // Bind ordered list of view names
                var viewNames = views.Select(v => v.Name).OrderBy(n => n).ToList();
                ViewList.ItemsSource = viewNames;
            }

            private void OK_Click(object sender, RoutedEventArgs e)
            {
                Proceed = true;
                this.DialogResult = true;
                this.Close();
            }

            private void Cancel_Click(object sender, RoutedEventArgs e)
            {
                Proceed = false;
                this.DialogResult = false;
                this.Close();
            }
        }
    }
}
