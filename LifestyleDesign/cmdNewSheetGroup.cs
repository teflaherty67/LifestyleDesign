
#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OfficeOpenXml;
using Forms = System.Windows.Forms;

#endregion

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdNewSheetGroup : IExternalCommand
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

            frmNewSheetGroup curForm = new frmNewSheetGroup()
            {
                Width = 320,
                Height = 420,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };

            // hard-code Excel file

            string excelFile = @"S:\Shared Folders\!RBA Addins\Lifestyle Design\Data Source\NewSheetSetup.xlsx";

            using(var package = new ExcelPackage(excelFile))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                ExcelWorkbook wb = package.Workbook;

                if (curForm.GetGroup1 = "Basement" && curForm.GetGroup2 = "One Story")
                    ExcelWorksheet ws = wb.Worksheets[0];
                else if (curForm.GetGroup1 = "Basement" && curForm.GetGroup2 = "Two Story")
                    ExcelWorksheet ws = wb.Worksheets[1];
                else if (curForm.GetGroup1 = "Crawlspace" && curForm.GetGroup2 = "One Story")
                    ExcelWorksheet ws = wb.Worksheets[2];
                else if (curForm.GetGroup1 = "Crawlspace" && curForm.GetGroup2 = "Two Story")
                    ExcelWorksheet ws = wb.Worksheets[3];
                else if (curForm.GetGroup1 = "Slab" && curForm.GetGroup2 = "One Story")
                    ExcelWorksheet ws = wb.Worksheets[4];
                else
                    ExcelWorksheet ws = wb.Worksheets[5];
            }

           


            // create lists for Excel data

            // remove the headers

            // create sheets with specifed titleblock

            // add elevation designation to sheet numbers

            // groups sheets by elevation designation

            return Result.Succeeded;
        }       
    }
}