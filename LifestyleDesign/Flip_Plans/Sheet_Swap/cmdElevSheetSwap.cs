using LifestyleDesign.Classes;
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

                string numLeft2 = "";
                string numRight2 = "";

                if (curForm.GetCheckBox1() == true)
                {
                    string curLeftNum2 = curForm.GetComboBoxLeft2Item();
                    string curRightNum2 = curForm.GetComboBoxRight2Item();

                    numLeft2 = Utils.GetStringBetweenCharacters(curLeftNum2, curLeftNum2[0].ToString(),
                   curLeftNum2[curLeftNum2.Length - 1].ToString());

                    numRight2 = Utils.GetStringBetweenCharacters(curRightNum2, curRightNum2[0].ToString(),
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

                    // swap logic for single elevation sheets & first pair of split elevation sheets
                    List<ViewSheet> matchingLeft1Sheets = Utils.GetSheetsByNumber(curDoc, numLeft1);
                    List<ViewSheet> matchingRight1Sheets = Utils.GetSheetsByNumber(curDoc, numRight1);              

                    foreach (ViewSheet sheet in matchingLeft1Sheets)
                    {
                        sheet.SheetNumber = sheet.SheetNumber.Replace(numLeft1, numRight1 + "zz");
                    }

                    foreach (ViewSheet sheet in matchingRight1Sheets)
                    {
                        sheet.SheetNumber = sheet.SheetNumber.Replace(numRight1, numLeft1);
                    }

                    foreach (ViewSheet sheet in matchingLeft1Sheets)
                    {
                        sheet.SheetNumber = sheet.SheetNumber.Replace(numRight1 + "zz", numRight1);
                    }

                    if (curForm.GetCheckBox1() == true && numLeft2 != "" && numRight2 != "")
                    {
                        // swap logic for second pair of split elevation sheets
                        List<ViewSheet> matchingLeft2Sheets = Utils.GetSheetsByNumber(curDoc, numLeft2);
                        List<ViewSheet> matchingRight2Sheets = Utils.GetSheetsByNumber(curDoc, numRight2);
                        foreach (ViewSheet sheet in matchingLeft2Sheets)
                        {
                            sheet.SheetNumber = sheet.SheetNumber.Replace(numLeft2, numRight2 + "zz");
                        }
                        foreach (ViewSheet sheet in matchingRight2Sheets)
                        {
                            sheet.SheetNumber = sheet.SheetNumber.Replace(numRight2, numLeft2);
                        }
                        foreach (ViewSheet sheet in matchingLeft2Sheets)
                        {
                            sheet.SheetNumber = sheet.SheetNumber.Replace(numRight2 + "zz", numRight2);
                        }
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