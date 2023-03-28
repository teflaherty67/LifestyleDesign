#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdRevisionJournal : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            JobNumber curForm1 = new JobNumber();

            curForm1.Width = 375;
            curForm1.Height = 120;

            curForm1.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            curForm1.Topmost = true;

            curForm1.ShowDialog();

            RevisionJournal curForm = new RevisionJournal(doc.PathName);

            curForm.Width = 700;
            curForm.Height = 500;

            curForm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            curForm.Topmost = true;

            curForm.ShowDialog();

            return Result.Succeeded;
        }
    }
}
