using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LifestyleDesign
{
    public partial class frmUncategorizedViewsWarning : Window
    {
        public frmUncategorizedViewsWarning(List<View> views)
        {
            InitializeComponent();

            // Bind the view names to the list
            ViewList.ItemsSource = views
                .Select(v => v.Name)
                .OrderBy(name => name)
                .ToList();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
