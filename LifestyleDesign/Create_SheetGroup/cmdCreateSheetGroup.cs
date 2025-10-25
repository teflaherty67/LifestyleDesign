using LifestyleDesign.Classes;
using LifestyleDesign.Common;
using System.ComponentModel;
using System.Data;
using ExcelDataReader;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdCreateSheetGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Register encoding provider for Excel Data Reader (required for .NET Core/.NET 5+)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            frmCreateSheetGroup curForm = new frmCreateSheetGroup()
            {
                Width = 320,
                Height = 420,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                Topmost = true,
            };

            curForm.ShowDialog();

            // hard-code Excel file
            string excelFile = "S:\\Shared Folders\\!RBA Addins\\Lifestyle Design\\Data Source\\NewSheetSetup.xlsx";

            // create a list to hold the sheetdata
            List<List<string>> dataSheets = new List<List<string>>();

            // get data from the form
            string newElev = curForm.GetComboboxElevation();

            // set some variables for parameter values
            string newFilter = "";

            if (newElev == "A")
                newFilter = "1";
            else if (newElev == "B")
                newFilter = "2";
            else if (newElev == "C")
                newFilter = "3";
            else if (newElev == "D")
                newFilter = "4";
            else if (newElev == "S")
                newFilter = "5";
            else if (newElev == "T")
                newFilter = "6";

            using (var stream = File.Open(excelFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Convert to DataSet to access worksheets
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true // Set to true if first row contains headers
                        }
                    });

                    // Select worksheet based on foundation and floors
                    DataTable ws;
                    if (curForm.GetComboboxFoundation() == "Basement" && curForm.GetComboboxFloors() == "1")
                        ws = dataSet.Tables[0];
                    else if (curForm.GetComboboxFoundation() == "Basement" && curForm.GetComboboxFloors() == "2")
                        ws = dataSet.Tables[1];
                    else if (curForm.GetComboboxFoundation() == "Crawlspace" && curForm.GetComboboxFloors() == "1")
                        ws = dataSet.Tables[2];
                    else if (curForm.GetComboboxFoundation() == "Crawlspace" && curForm.GetComboboxFloors() == "2")
                        ws = dataSet.Tables[3];
                    else if (curForm.GetComboboxFoundation() == "Slab" && curForm.GetComboboxFloors() == "1")
                        ws = dataSet.Tables[4];
                    else
                        ws = dataSet.Tables[5];

                    // get row & column count
                    int rows = ws.Rows.Count;
                    int columns = ws.Columns.Count;

                    // read Excel data into a list                
                    for (int i = 0; i < rows; i++)
                    {
                        List<string> rowData = new List<string>();
                        for (int j = 0; j < columns; j++)
                        {
                            string cellContent = ws.Rows[i][j]?.ToString() ?? string.Empty;
                            rowData.Add(cellContent);
                        }
                        dataSheets.Add(rowData);
                    }

                    // Remove header row (equivalent to RemoveAt(0))
                    if (dataSheets.Count > 0)
                        dataSheets.RemoveAt(0);
                }
            }

            // create sheets with specified titleblock
            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Create Sheets");

                foreach (List<string> curSheetData in dataSheets)
                {
                    FamilySymbol tblock = Utils.GetTitleBlockByNameContains(curDoc, curSheetData[2]);
                    ElementId tBlockId = tblock.Id;

                    ViewSheet curSheet = ViewSheet.Create(curDoc, tBlockId);

                    // add elevation designation to sheet number
                    curSheet.SheetNumber = curSheetData[0] + curForm.GetComboboxElevation().ToLower();
                    curSheet.Name = curSheetData[1];

                    // set parameter values                    
                    Utils.SetParameterByName(curSheet, "Category", "Active");
                    Utils.SetParameterByName(curSheet, "Group", "Elevation " + curForm.GetComboboxElevation());
                    Utils.SetParameterByName(curSheet, "Elevation Designation", curForm.GetComboboxElevation());
                    Utils.SetParameterByName(curSheet, "Code Filter", newFilter);
                    Utils.SetParameterByName(curSheet, "Index Position", int.Parse(curSheetData[3]));
                }

                t.Commit();
            }

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCmd7_1";
            string buttonTitle = "Sheet\rGroup";
            string methodBase = MethodBase.GetCurrentMethod().DeclaringType?.FullName;

            if (methodBase == null)
            {
                throw new InvalidOperationException("MethodBase.GetCurrentMethod().DeclaringType?.FullName is null");
            }
            else
            {
                clsButtonData myBtnData1 = new clsButtonData(
                    buttonInternalName,
                    buttonTitle,
                    methodBase,
                    Properties.Resources.CreateSheetGroup_32,
                    Properties.Resources.CreateSheetGroup_16,
                    "Create sheet set for specified Elevation");

                return myBtnData1.Data;
            }
        }
    }
}