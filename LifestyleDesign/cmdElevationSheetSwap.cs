#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdElevationSheetSwap : IExternalCommand
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

            // get all viewports
            FilteredElementCollector vpCollector = new FilteredElementCollector(doc);
            vpCollector.OfCategory(BuiltInCategory.OST_Viewports);

            // filter viewports for sheet names that contains "Exterior Elevations"

            List<Viewport> eSheets = new List<Viewport>();
            List<ViewSheet> lrSheets = new List<ViewSheet>();
            List<string> sheetNum = new List<string>();

            foreach (Viewport vPort in vpCollector)
            {
                ViewSheet curSheet = doc.GetElement(vPort.SheetId) as ViewSheet;
                View curView = doc.GetElement(vPort.ViewId) as View;

                string sName = Utils.GetParameterValueByName(curSheet, "Sheet Name");
                string vName = curView.Name;

                if (sName.Contains("Exterior Elevations"))
                {
                    if (vName.Contains("Left") || vName.Contains("Right"))
                        lrSheets.Add(curSheet);
                }
            }

            frmElevationSheetSwap curForm = new frmElevationSheetSwap(lrSheets)
            {
                Width = 350,
                Height = 250,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };

            curForm.ShowDialog();

            if (curForm.DialogResult == true)
            {

                // create variable for current sheet numbers

                string curLeftNum1 = curForm.GetComboBoxLeft1Item();
                string curRightNum1 = curForm.GetComboBoxRight1Item();

                string numLeft1 = Utils.GetStringBetweenCharacters(curLeftNum1, curLeftNum1[0].ToString(),
                    curLeftNum1[curLeftNum1.Length - 1].ToString());

                string numRight1 = Utils.GetStringBetweenCharacters(curRightNum1, curRightNum1[0].ToString(),
                    curRightNum1[curRightNum1.Length - 1].ToString());

                if (curForm.GetCheckBox1() == true)
                {
                    string curLeftNum2 = curForm.GetComboBoxLeft2Item();
                    string curRightNum2 = curForm.GetComboBoxRight2Item();

                    string numLeft2 = Utils.GetStringBetweenCharacters(curLeftNum2, curLeftNum2[0].ToString(),
                   curLeftNum2[curLeftNum2.Length - 1].ToString());

                    string numRight2 = Utils.GetStringBetweenCharacters(curRightNum2, curRightNum2[0].ToString(),
                        curRightNum2[curRightNum2.Length - 1].ToString());
                }

                // hide project browser

                DockablePaneId dpId = DockablePanes.BuiltInDockablePanes.ProjectBrowser;
                DockablePane pB = new DockablePane(dpId);
                pB.Hide();

                // start the transaction

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Reorder Elevation Sheets");

                    List<ViewSheet> matchingLeftSheets = Utils.GetSheetsByNumber(doc, numLeft1);
                    List<ViewSheet> matchingRightSheets = Utils.GetSheetsByNumber(doc, numRight1);

                    foreach (ViewSheet sheet in matchingLeftSheets)
                    {
                        sheet.SheetNumber = sheet.SheetNumber.Replace(numLeft1, numRight1 + "zz");
                    }

                    foreach (ViewSheet sheet in matchingRightSheets)
                    {
                        sheet.SheetNumber = sheet.SheetNumber.Replace(numRight1, numLeft1);
                    }

                    foreach (ViewSheet sheet in matchingLeftSheets)
                    {
                        sheet.SheetNumber = sheet.SheetNumber.Replace(numRight1 + "zz", numRight1);
                    }

                    t.Commit();

                    pB.Show();
                }
            }

            return Result.Succeeded;
        }
    }
}