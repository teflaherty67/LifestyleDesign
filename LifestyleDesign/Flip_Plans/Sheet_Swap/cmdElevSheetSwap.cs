﻿using LifestyleDesign.Classes;
using LifestyleDesign.Common;

namespace LifestyleDesign.Flip_Plans
{
    [Transaction(TransactionMode.Manual)]
    public class cmdElevSheetSwap : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // get all viewports
            FilteredElementCollector vpCollector = new FilteredElementCollector(curDoc);
            vpCollector.OfCategory(BuiltInCategory.OST_Viewports);

            // filter viewports for sheet names that contains "Exterior Elevations"
            List<Viewport> eSheets = new List<Viewport>();
            List<ViewSheet> lrSheets = new List<ViewSheet>();
            List<string> sheetNum = new List<string>();

            foreach (Viewport vPort in vpCollector)
            {
                ViewSheet curSheet = curDoc.GetElement(vPort.SheetId) as ViewSheet;
                View curView = curDoc.GetElement(vPort.ViewId) as View;

                string sName = Utils.GetParameterValueByName(curSheet, "Sheet Name");
                string vName = curView.Name;

                if (sName.Contains("Exterior Elevations"))
                {
                    if (vName.Contains("Left") || vName.Contains("Right"))
                        lrSheets.Add(curSheet);
                }
            }

            frmElevSheetSwap curForm = new frmElevSheetSwap(lrSheets)
            {
                Width = 375,
                Height = 275,
                WindowStartupLocation = Forms.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };

            curForm.ShowDialog();

            if (curForm.DialogResult == true)
            {

                // create variable for current sheet numbers
                string curLeftNum1 = Utils.CleanSheetNumber(curForm.GetComboBoxLeft1Item());
                string curRightNum1 = Utils.CleanSheetNumber(curForm.GetComboBoxRight1Item());

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
                using (Transaction t = new Transaction(curDoc))
                {
                    t.Start("Reorder Elevation Sheets");

                    List<ViewSheet> matchingLeftSheets = Utils.GetSheetsByNumber(curDoc, numLeft1);
                    List<ViewSheet> matchingRightSheets = Utils.GetSheetsByNumber(curDoc, numRight1);

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

                    string msgText = "Elevation sheets have been reordered.";
                    string msgTitle = "Complete";
                    Forms.MessageBoxButton msgButtons = Forms.MessageBoxButton.OK;

                    Forms.MessageBox.Show(msgText, msgTitle, msgButtons, Forms.MessageBoxImage.Information);
                }
            }

            return Result.Succeeded;
        }
    }
}
