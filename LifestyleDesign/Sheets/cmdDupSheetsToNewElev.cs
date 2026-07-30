using LifestyleDesign.Classes;
using LifestyleDesign.Common;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdDupSheetsToNewElev : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // get all the sheets in the document
            List<ViewSheet> allSheets = Utils.GetAndSortAllSheets(curDoc);

            // if no sheets are found
            if (!allSheets.Any())
            {
                // notify the user
                Utils.TaskDialogWarning("Lifestyle Design", "Duplicate Sheets to New Elevation", "No sheets were found in the current document.");

                // exit the command
                return Result.Failed;
            }

            // get the sheet sets from the document
            List<ViewSheetSet> allPrintSets = Utils.GetAndSortAllPrintSets(curDoc);

            // create sheet set list with "All Sheets" as the first item
            List<string> listSheetSetNames = new List<string> { "All Sheets" };

            // add the print sets to the list
            listSheetSetNames.AddRange(allPrintSets.Select(vss => vss.Name).OrderBy(name => name));

            // configure the form
            var frmConfig = new SelectFromListConfig
            {
                Title = "Duplicate Sheets to New Elevation",
                ButtonText = "Duplicate",
                ShowSheetSets = true,
                SheetSetOptions = listSheetSetNames,
                DefaultSheetSet = "All Sheets",
                ViewSheetSets = allPrintSets,
                ShowIncrementInput = true,
                DefaultIncrementValue = "",
                IncrementLabel = "New Elevation Designation:"
            };

            // launch the form
            var frmResult = frmSheetsFromList.ShowWithResult
                (
                    items: allSheets,
                    displayNameSelector: sheet => $"{sheet.SheetNumber} - {sheet.Name}",
                    config: frmConfig
                );

            // process the results if user made a choice
            if (frmResult != null && frmResult.DialogResult && frmResult.SelectedItems.Any())
            {
                var selectedSheets = frmResult.SelectedItems.Cast<ViewSheet>().ToList();

                string newElevation = frmResult.IncrementValue?.Trim();

                if (string.IsNullOrEmpty(newElevation))
                {
                    TaskDialog.Show("Error", "Please enter a new elevation designation.");
                    return Result.Failed;
                }

                string newElevationUpper = newElevation.ToUpper();

                // Code Filter matches the elevation letter's position in the alphabet (A=1, B=2, ...)
                string newCodeFilter = (char.ToUpper(newElevationUpper[0]) - 'A' + 1).ToString();

                // track sheet numbers already in use to avoid collisions
                HashSet<string> existingNumbers = new HashSet<string>(allSheets.Select(s => s.SheetNumber));

                int duplicatedCount = 0;

                using (Transaction trans = new Transaction(curDoc, "Duplicate Sheets to New Elevation"))
                {
                    trans.Start();

                    foreach (var sheet in selectedSheets)
                    {
                        string newSheetNumber = ReplaceElevationLetter(sheet.SheetNumber, newElevation);

                        if (existingNumbers.Contains(newSheetNumber))
                        {
                            TaskDialog.Show("Warning",
                                $"Sheet {newSheetNumber} already exists. Skipping {sheet.SheetNumber}.");
                            continue;
                        }

                        if (!sheet.CanBeDuplicated(SheetDuplicateOption.DuplicateEmptySheet))
                        {
                            TaskDialog.Show("Warning",
                                $"Sheet {sheet.SheetNumber} cannot be duplicated. Skipping.");
                            continue;
                        }

                        try
                        {
                            ElementId newSheetId = sheet.Duplicate(SheetDuplicateOption.DuplicateEmptySheet);
                            ViewSheet newSheet = curDoc.GetElement(newSheetId) as ViewSheet;

                            newSheet.SheetNumber = newSheetNumber;
                            newSheet.Name = sheet.Name;

                            // update parameters so the sheet reflects the new elevation instead of the original
                            Utils.SetParameterByName(newSheet, "Group", "Elevation " + newElevationUpper);
                            Utils.SetParameterByName(newSheet, "Elevation Designation", newElevationUpper);
                            Utils.SetParameterByName(newSheet, "Code Filter", newCodeFilter);

                            existingNumbers.Add(newSheetNumber);
                            duplicatedCount++;
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show("Warning",
                                $"Could not duplicate sheet {sheet.SheetNumber}: {ex.Message}");
                        }
                    }

                    trans.Commit();
                }

                // show success message
                TaskDialog.Show("Success",
                    $"Duplicated {duplicatedCount} sheet(s) to elevation {newElevationUpper}.");
            }

            return Result.Succeeded;
        }

        // replaces the trailing elevation letter on a sheet number with the new designation
        // (e.g. "A3a" with new designation "B" becomes "A3b"); appends it if no letter suffix exists
        private string ReplaceElevationLetter(string sheetNumber, string newElevation)
        {
            if (string.IsNullOrEmpty(sheetNumber)) return sheetNumber;

            string newSuffix = newElevation.Trim().ToLower();

            if (char.IsLetter(sheetNumber[sheetNumber.Length - 1]))
                return sheetNumber.Substring(0, sheetNumber.Length - 1) + newSuffix;

            return sheetNumber + newSuffix;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCmd7_2d";
            string buttonTitle = "Duplicate Sheets\rto New Elevation";

            clsButtonData myButtonData = new clsButtonData(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.DupSheetGrp_32,
                Properties.Resources.DupSheetGrp_16,
                "Duplicates selected sheets to a new elevation designation");

            return myButtonData.Data;
        }
    }
}