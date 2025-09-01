using LifestyleDesign.Classes;
using LifestyleDesign.Common;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdStripIt : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;
            // Step 1: check for the default 3D view
            FilteredElementCollector collector = new FilteredElementCollector(curDoc);
            View3D default3DView = collector
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(v => !v.IsTemplate && v.Name == "{3D}");

            // Step 2: if found, make it the active view
            if (default3DView != null)
            {
                uidoc.ActiveView = default3DView;
                return Result.Succeeded;
            }

            // Step 3: if not found, get the 3D view family type
            ViewFamilyType viewFamilyType = new FilteredElementCollector(curDoc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.ThreeDimensional);

            if (viewFamilyType == null)
            {
                message = "No ViewFamilyType for 3D views found.";
                return Result.Failed;
            }

            // Step 4: create new 3D view
            using (Transaction tx = new Transaction(curDoc))
            {
                tx.Start("Create Default 3D View");

                default3DView = View3D.CreateIsometric(curDoc, viewFamilyType.Id);
                default3DView.Name = "3D";

                tx.Commit();
            }

            // Step 5: set active view
            uidoc.ActiveView = default3DView;

            // 01. create list of all sheets
            List<ViewSheet> allSheets = Utils.GetAllSheets(curDoc);

            // 02. create view lists

            // create list of all views
            List<View> allViews = Utils.GetAllViews(curDoc);

            // create list of views to keep
            List<View> viewsToKeep = Utils.GetAllViewsByCategoryContains(curDoc, "Presentation Views");

            // create list of ElementIds from viewsToKeep for fast lookup
            List<ElementId> viewsToKeepIds = new List<ElementId>(viewsToKeep.Select(view => view.Id));

            // create the filtered list using the ElementId for comparison
            List<View> viewsToDelete = allViews.Where(view => !viewsToKeepIds.Contains(view.Id)).ToList();

            // 03. create list of all schedules
            List<ViewSchedule> allSchedules = Utils.GetAllSchedules(curDoc);

            // 04. get Revit Command Id for Purge
            RevitCommandId commandId = RevitCommandId.LookupPostableCommandId(PostableCommand.PurgeUnused);

            // 05. create & start transaction
            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Strip the file");

                // 01a. delete all sheets
                foreach (ViewSheet curSheet in allSheets)
                {
                    curDoc.Delete(curSheet.Id);
                }

                // 01b. delete all sheet collections
                Utils.DeleteAllSheetCollections(curDoc);

                // 02a. loop through the views & delete them
                foreach (View deleteView in viewsToDelete)
                {
                    try
                    {
                        // delete the view
                        curDoc.Delete(deleteView.Id);
                    }
                    catch (Exception)
                    {
                    }
                }

                // 03a. loop through the schedules & delete them

                foreach (ViewSchedule deleteSched in allSchedules)
                {
                    curDoc.Delete(deleteSched.Id);
                }

                // 06. purged unused elements programatically
                Utils.PurgeUnusedFamilySymbols(curDoc);
                Utils.PurgeUnusedViewTemplates(curDoc);
                Utils.PurgeUnusedFilters(curDoc);
                Utils.PurgeUnusedMaterials(curDoc);
                Utils.PurgeUnusedLinePatterns(curDoc);
                Utils.PurgeUnusedFillPatterns(curDoc);
                Utils.PurgeUnusedGroups(curDoc);
                Utils.PurgeUnusedTextStyles(curDoc);
                Utils.PurgeUnusedDimensionStyles(curDoc);
                Utils.PurgeUnusedLineStyles(curDoc);
                Utils.PurgeUnusedAnnotationSymbols(curDoc);
                Utils.PurgeUnusedAppearanceAssets(curDoc);
                Utils.PurgeUnusedRenderingMaterials(curDoc);

                t.Commit();
            }

            // 07. save file to Luis' folder
            SaveToLuisFolder(curDoc);

            // 08. switch to Manage ribbon tab before running Purge command
            try
            {
                // Get the ribbon control
                Autodesk.Windows.RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;

                // Find and activate the Manage tab
                foreach (Autodesk.Windows.RibbonTab tab in ribbon.Tabs)
                {
                    if (tab.Id == "Manage" || tab.Title == "Manage")
                    {
                        tab.IsActive = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the command
                System.Diagnostics.Debug.WriteLine($"Failed to switch to Manage ribbon: {ex.Message}");
            }

            // 06a. run the Purge Unused command using PostCommand
            uiapp.PostCommand(commandId);

            return Result.Succeeded;
        }

        internal static bool SaveToLuisFolder(Document doc, string fileName = null)
        {
            try
            {
                // Target directory
                string targetDirectory = @"S:\Shared Folders\Lifestyle USA Design\LGI Homes\!Luis";

                // Generate filename if not provided
                if (string.IsNullOrEmpty(fileName))
                {
                    string originalName = Path.GetFileNameWithoutExtension(doc.Title);

                    // Extract plan name and region code (remove lot dimensions and anything after)
                    // Example: "Torres(R)-CTX(50-5-29'11)" should become "Torres(R)-CTX"

                    // Find the pattern: look for the first hyphen followed by letters, then a parenthesis
                    // This should identify the region code section
                    // int regionCodeStart = -1;
                    for (int i = originalName.Length - 1; i >= 0; i--)
                    {
                        if (originalName[i] == '-')
                        {
                            // Check if this is followed by letters (region code)
                            int letterStart = i + 1;
                            int letterEnd = letterStart;
                            while (letterEnd < originalName.Length && char.IsLetter(originalName[letterEnd]))
                            {
                                letterEnd++;
                            }

                            if (letterEnd > letterStart && letterEnd < originalName.Length && originalName[letterEnd] == '(')
                            {
                                // Found region code pattern, keep everything up to the opening parenthesis after region
                                originalName = originalName.Substring(0, letterEnd);
                                break;
                            }
                        }
                    }

                    fileName = $"{originalName}.rvt";
                }

                // Ensure filename has .rvt extension
                if (!fileName.EndsWith(".rvt", StringComparison.OrdinalIgnoreCase))
                {
                    fileName += ".rvt";
                }

                string fullPath = Path.Combine(targetDirectory, fileName);

                // Ensure the directory exists
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                // Configure save options
                SaveAsOptions saveAsOptions = new SaveAsOptions();
                saveAsOptions.OverwriteExistingFile = true;

                // Perform the Save As operation
                doc.SaveAs(fullPath, saveAsOptions);

                // Optional: Show success message
                Utils.TaskDialogInformation("Save Complete", "Strip It", $"File successfully saved to:\n{fullPath}");               

                return true;
            }
            catch (Exception ex)
            {
                // Show error message
                TaskDialog errorDialog = new TaskDialog("Save Error");
                errorDialog.MainContent = $"Failed to save file:\n{ex.Message}";
                errorDialog.Show();

                return false;
            }
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCmd2_3";
            string buttonTitle = "Strip It";
            string methodBase = MethodBase.GetCurrentMethod().DeclaringType?.FullName;

            if (methodBase == null)
            {
                throw new InvalidOperationException("MethodBase.GetCurrentMethod().DeclaringType?.FullName is null");
            }
            else
            {
                clsButtonData myBtnData1 = new Classes.clsButtonData(
                    buttonInternalName,
                    buttonTitle,
                    methodBase,
                    Properties.Resources.StripIt_32,
                    Properties.Resources.StripIt_16,
                    "Prepares file for transfer to LGI Homes");

                return myBtnData1.Data;
            }
        }
    }
}
