using LifestyleDesign.Classes;
using LifestyleDesign.Common;
using System.Text;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdLevelManager : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            #region Get Data

            // get filtered & sorted list of levels in the document
            List<Level> filteredLevels = Utils.GetFilteredAndSortedLevels(curDoc);

            // get all the ViewSection views
            List<View> listViews = Utils.GetAllSectionViews(curDoc);

            // get the first view whose Title on Sheet is "Front Elevation"
            View elevFront = listViews
                .FirstOrDefault(v => v.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION)?.AsString() == "Front Elevation");

            // set that view as the active view
            if (elevFront != null)
            {
                uidoc.ActiveView = elevFront;
            }
            else
            {
                Utils.TaskDialogInformation("Information", "Level Manager", "Front Elevation view not found. Proceeding with level management in current view.");
            }

            // create a counter for summary report
            int countLevels = 0;

            #endregion

            #region Form

            // launch the form with level data
            frmLevelManager curForm = new frmLevelManager(filteredLevels);
            curForm.Topmost = true; // ensure the form is on top

            curForm.ShowDialog();

            // check if user clicked Cancel
            if (curForm.DialogResult != true)
            {
                return Result.Cancelled;
            }

            // get data from the form
            Dictionary<Level, double> levelAdjustments = curForm.LevelAdjustments;

            #endregion

            #region Level Adjustments
            // process level adjustments based on user input

            // create & start a transaction
            using (Transaction t = new Transaction(curDoc, "Plate Height Adjustements"))
            {
                // process the data from the form
                t.Start();

                // loop through the dictionary
                foreach (var kvp in levelAdjustments)
                {
                    // get the key value pairs
                    Level level = kvp.Key;
                    double adjustment = kvp.Value;

                    // only adjust if value is valid
                    if (adjustment != 0)
                    {
                        // adjust the level elevation
                        level.Elevation = level.Elevation + adjustment;

                        // increment the counter
                        countLevels++;
                    }
                }

                t.Commit();
            }

            #endregion

            #region Window Head Height Adjustments
            // process window head height adjustments based on user input

            // get user input from form
            bool firstFlrHeadHeights = curForm.IsFirstFloorHeadHeightsChecked();
            bool firstFlrWinHeights = curForm.IsFirstFloorWindowHeightsChecked();

            bool secondFlrHeadHeights = curForm.IsSecondFloorHeadHeightsChecked();
            bool secondFlrWinHeights = curForm.IsSecondFloorWindowHeightsChecked();

            // process windows if any boolean is true
            if (firstFlrHeadHeights || secondFlrHeadHeights)
            {
                // determine head height adjustment based on level adjustments made
                bool adjustFirstFlrHeadHeights = false;
                bool adjustSecondFlrHeadHeights = false;

                if (firstFlrHeadHeights)
                {
                    adjustFirstFlrHeadHeights = DetermineHeadHeightAdjustment("Plate 1", levelAdjustments);
                }

                if (secondFlrHeadHeights)
                {
                    adjustSecondFlrHeadHeights = DetermineHeadHeightAdjustment("Plate 2", levelAdjustments);
                }

                // get windows by floor
                List<FamilyInstance> firstFlrWindows = Utils.GetWindowsByLevel(curDoc, "First Floor");
                List<FamilyInstance> secondFlrWindows = Utils.GetWindowsByLevel(curDoc, "Second Floor");

                // create a list to hold skipped windows
                List<string> skippedWindows = new List<string>();

                // count variables
                int firstFlrWinHeadAdjusted = 0;
                int firstFlrWinHeightAdjusted = 0;
                int secondFlrWinHeadAdjusted = 0;
                int secondFlrWinHeightAdjusted = 0;

                // process first floor windows
                if (firstFlrHeadHeights)
                {
                    // create a varioable to hold the Plate 1 adjustment value
                    double plate1Adjustment = 0;

                    // loop through level adjustments to find Plate 1 value
                    foreach (var kvp in levelAdjustments)
                    {
                        Level level = kvp.Key;
                        double adjustment = kvp.Value;

                        // check if level is Plate 1
                        if (level.Name == "Plate 1")
                        {
                            // store the adjustment value
                            plate1Adjustment = adjustment;
                            break;
                        }
                    }

                    // create a transaction
                    using (Transaction t = new Transaction(curDoc, "Adjust First Floor Windows"))
                    {
                        // start the transaction
                        t.Start();

                        // loop through the windows
                        foreach (FamilyInstance curWin in firstFlrWindows)
                        {
                            if (curWin != null)
                            {
                                // create a clsWindowData object
                                clsWindowData curWinData = new clsWindowData(curWin);

                                // head height adjustment code
                                double newHeadHeight;

                                if (adjustFirstFlrHeadHeights)
                                {
                                    newHeadHeight = curWinData.CurHeadHeight + plate1Adjustment;
                                }
                                else
                                {
                                    newHeadHeight = curWinData.CurHeadHeight - plate1Adjustment;
                                }

                                // set the new head height
                                if (curWinData.HeadHeightParam != null && !curWinData.HeadHeightParam.IsReadOnly)
                                {
                                    curWinData.HeadHeightParam.Set(newHeadHeight);

                                    // increment counter for tracking
                                    firstFlrWinHeadAdjusted++;
                                }

                                // check if adjust window heights is true
                                if (firstFlrWinHeights)
                                {
                                    bool success = AdjustWindowHeights(curDoc, curWinData, plate1Adjustment, adjustFirstFlrHeadHeights, skippedWindows);
                                    if (success)
                                    {
                                        // ensure head height is set correctly after type change
                                        curWinData.HeadHeightParam.Set(newHeadHeight);

                                        // increment counter for tracking
                                        firstFlrWinHeightAdjusted++;
                                    }
                                }
                            }
                        }

                        // commit the transaction
                        t.Commit();
                    }
                }

                // process second floor windows
                if (secondFlrHeadHeights)
                {
                    // create a varioable to hold the Plate 2 adjustment value
                    double plate2Adjustment = 0;

                    // loop through level adjustments to find Plate 2 value
                    foreach (var kvp in levelAdjustments)
                    {
                        Level level = kvp.Key;
                        double adjustment = kvp.Value;

                        // check if level is Plate 2
                        if (level.Name == "Plate 2")
                        {
                            // store the adjustment value
                            plate2Adjustment = adjustment;
                            break;
                        }
                    }

                    // create a transaction
                    using (Transaction t = new Transaction(curDoc, "Adjust Second Floor Windows"))
                    {
                        // start the transaction
                        t.Start();

                        // loop through the Second Floor windows
                        foreach (FamilyInstance curWin in secondFlrWindows)
                        {
                            if (curWin != null)
                            {
                                // create a clsWindowData object
                                clsWindowData curWinData = new clsWindowData(curWin);

                                // head height adjustment code
                                double newHeadHeight;

                                if (adjustSecondFlrHeadHeights)
                                {
                                    newHeadHeight = curWinData.CurHeadHeight + plate2Adjustment;
                                }
                                else
                                {
                                    newHeadHeight = curWinData.CurHeadHeight - plate2Adjustment;
                                }

                                // set the new head height
                                if (curWinData.HeadHeightParam != null && !curWinData.HeadHeightParam.IsReadOnly)
                                {
                                    curWinData.HeadHeightParam.Set(newHeadHeight);

                                    // increment counter for tracking
                                    secondFlrWinHeadAdjusted++;
                                }

                                // check if adjust window heights is true
                                if (secondFlrWinHeights)
                                {
                                    bool success = AdjustWindowHeights(curDoc, curWinData, plate2Adjustment, adjustSecondFlrHeadHeights, skippedWindows);
                                    if (success)
                                    {
                                        // ensure head height is set correctly after type change
                                        curWinData.HeadHeightParam.Set(newHeadHeight);

                                        // increment counter for tracking
                                        secondFlrWinHeightAdjusted++;
                                    }
                                }
                            }
                        }

                        // commit the transaction
                        t.Commit();
                    }
                }

                #endregion

                #region Summary Report
                // determine the summary message based on user selections

                // create a variable to hold the summary message
                StringBuilder summaryMessage = new StringBuilder();

                // add level adjustment summary
                if (countLevels > 0)
                {
                    summaryMessage.AppendLine($"{countLevels} Level{(countLevels == 1 ? "" : "s")}" +
                $" {(countLevels == 1 ? "was" : "were")} adjusted per the specified level adjustment{(countLevels == 1 ? "" : "s")}.");
                    summaryMessage.AppendLine(); // Add blank line for separation
                }

                // add first floor window adjustment summary
                if (firstFlrWinHeadAdjusted > 0 || firstFlrWinHeightAdjusted > 0)
                {
                    List<string> firstFloorAdjustments = new List<string>();

                    if (firstFlrWinHeadAdjusted > 0)
                        firstFloorAdjustments.Add($"head heights adjusted for {firstFlrWinHeadAdjusted} window{(firstFlrWinHeadAdjusted == 1 ? "" : "s")}");

                    if (firstFlrWinHeightAdjusted > 0)
                        firstFloorAdjustments.Add($"heights adjusted for {firstFlrWinHeightAdjusted} window{(firstFlrWinHeightAdjusted == 1 ? "" : "s")}");

                    summaryMessage.AppendLine($"First Floor Windows: {string.Join(" and ", firstFloorAdjustments)}.");
                    summaryMessage.AppendLine(); // Add blank line for separation
                }

                // add second floor window adjustment summary
                if (secondFlrWinHeadAdjusted > 0 || secondFlrWinHeightAdjusted > 0)
                {
                    List<string> secondFloorAdjustments = new List<string>();

                    if (secondFlrWinHeadAdjusted > 0)
                        secondFloorAdjustments.Add($"head heights adjusted for {secondFlrWinHeadAdjusted} window{(secondFlrWinHeadAdjusted == 1 ? "" : "s")}");

                    if (secondFlrWinHeightAdjusted > 0)
                        secondFloorAdjustments.Add($"heights adjusted for {secondFlrWinHeightAdjusted} window{(secondFlrWinHeightAdjusted == 1 ? "" : "s")}");

                    summaryMessage.AppendLine($"Second Floor Windows: {string.Join(" and ", secondFloorAdjustments)}.");
                }

                // add skipped windows summary (if any)
                if (skippedWindows.Count > 0)
                {
                    summaryMessage.AppendLine("\nThe following windows were skipped while adjusting the window heights; the required type doesn't exist:");

                    foreach (string windowInfo in skippedWindows)
                    {
                        summaryMessage.AppendLine($"  • {windowInfo}");
                    }
                }

                // display single comprehensive summary
                string finalMessage = summaryMessage.Length > 0 ?
                    summaryMessage.ToString().Trim() :
                    "No adjustments were made.";

                Utils.TaskDialogInformation("Summary", "Level Manager", finalMessage);

                #endregion                
            }

            return Result.Succeeded;
        }

        private bool AdjustWindowHeights(Document curDoc, clsWindowData curWinData, double plateAdjustment, bool adjustHeadHeights, List<string> skippedWindows)
        {
            // get the current family
            Family curFam = curWinData.WindowInstance.Symbol.Family;

            // get the current window instance type name
            string curTypeName = curWinData.WindowInstance.Symbol.Name;

            // split the Type Name into parts
            string[] stringParts = curTypeName.Split(' ');
            string sizePart = stringParts[0];

            // store the width & mull indicator if present
            string wndwPrefix = sizePart.Substring(0, sizePart.Length - 2);

            // get the current window height
            string wndwHeight = sizePart.Substring(sizePart.Length - 2);

            // change the string to an interger
            int curHeight = int.Parse(wndwHeight);

            // create variable for new height
            string newHeightPart;

            if (adjustHeadHeights)
            {
                // set the new height number
                int newHeight = curHeight + 10;

                // convert to a string
                newHeightPart = newHeight.ToString();
            }
            else
            {
                // set the new height number
                int newHeight = curHeight - 10;

                // convert to a string
                newHeightPart = newHeight.ToString();
            }

            // set the new type name
            string newTypeName = wndwPrefix + newHeightPart + " " + string.Join(" ", stringParts.Skip(1));

            foreach (ElementId curTypeId in curFam.GetFamilySymbolIds())
            {
                FamilySymbol curFamType = curDoc.GetElement(curTypeId) as FamilySymbol;
                string typeName = curFamType.Name;

                if (typeName == newTypeName)
                {
                    curWinData.WindowInstance.ChangeTypeId(curFamType.Id);
                    return true; // Success
                }
            }

            // If we get here, no matching type was found

            // create variables to hold skipped window info
            string curFamName = curWinData.WindowInstance.Symbol.Family.Name;
            string curFamTypeName = curWinData.WindowInstance.Symbol.Name;


            skippedWindows.Add($"{curFamName} - {curFamTypeName}");
            return false; // Failed
        }

        private bool DetermineHeadHeightAdjustment(string plateName, Dictionary<Level, double> levelAdjustments)
        {
            foreach (var kvp in levelAdjustments)
            {
                Level level = kvp.Key;
                double adjustment = kvp.Value;

                if (level.Name == plateName)
                {
                    // return true if positive, false if negative
                    if (adjustment > 0)
                    {
                        return true; // raise head heights
                    }
                    else if (adjustment < 0)
                    {
                        return false; // lower head heights
                    }
                }
            }

            // if plate not found notify user & return false
            Utils.TaskDialogError("Error", "Level Manager", $"Could not find level '{plateName}' in the project. Window adjustments cannot proceed.");
            return false;
        }



        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            clsButtonData myButtonData = new clsButtonData(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Red_32,
                Properties.Resources.Red_16,
                "This is a tooltip for Button 1");

            return myButtonData.Data;
        }
    }
}