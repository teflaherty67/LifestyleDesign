using LifestyleDesign.Classes;
using LifestyleDesign.Common;
using System.Linq;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdUpdateVTs : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // get all the view templates in the project
            List<View> curVTs = Utils.GetAllViewTemplates(curDoc);

            // get views by current view template name and store in dictionary
            Dictionary<string, List<View>> viewsByTemplate = new Dictionary<string, List<View>>
            {
                {"01-Enlarged Plans", Utils.GetViewsByViewTemplateName(curDoc, "01-Enlarged Plans")},
                {"01-Floor Annotations", Utils.GetViewsByViewTemplateName(curDoc, "01-Floor Annotations")},
                {"01-Floor Dimensions", Utils.GetViewsByViewTemplateName(curDoc, "01-Floor Dimensions")},
                {"01-Key Plans", Utils.GetViewsByViewTemplateName(curDoc, "01-Key Plans")},
                {"02-Elevations", Utils.GetViewsByViewTemplateName(curDoc, "02-Elevations")},
                {"02-Key Elevations", Utils.GetViewsByViewTemplateName(curDoc, "02-Key Elevations")},
                {"02-Porch Elevations", Utils.GetViewsByViewTemplateName(curDoc, "02-Porch Elevations")},
                {"03-Roof Plan", Utils.GetViewsByViewTemplateName(curDoc, "03-Roof Plan")},
                {"04-Sections", Utils.GetViewsByViewTemplateName(curDoc, "04-Sections")},
                {"04-Sections_3/8\"", Utils.GetViewsByViewTemplateName(curDoc, "04-Sections_3/8\"")},
                {"05-Cabinet Layout Plans", Utils.GetViewsByViewTemplateName(curDoc, "05-Cabinet Layout Plans")},
                {"05-Interior Elevations", Utils.GetViewsByViewTemplateName(curDoc, "05-Interior Elevations")},
                {"06-Electrical Plans", Utils.GetViewsByViewTemplateName(curDoc, "06-Electrical Plans")},
                {"07-Enlarged Form/Foundation Plans", Utils.GetViewsByViewTemplateName(curDoc, "07-Enlarged Form/Foundation Plans")},
                {"07-Form/Foundation Plans", Utils.GetViewsByViewTemplateName(curDoc, "07-Form/Foundation Plans")}
            };

            // create list of all views getting new view templates
            List<View> allViewsToUpdate = new List<View>();
            foreach (var viewList in viewsByTemplate.Values)
            {
                allViewsToUpdate.AddRange(viewList);
            }

            // create counter variables for final report
            int templatesImported = 0;
            int viewsUpdated = 0;
            int templatesDeleted = 0;
            int totalViews = allViewsToUpdate.Count;

            // set the path to the view template file
            string templateDoc = "S:\\Shared Folders\\Lifestyle USA Design\\Library 2025\\Template\\View Templates.rvt";

            // create a variable for the source document
            Document sourceDoc = null;

            try
            {
                sourceDoc = uidoc.Application.Application.OpenDocumentFile(templateDoc);

                // create a transaction group
                using (TransactionGroup tGroup = new TransactionGroup(curDoc, "Update View Templates"))
                {
                    // create the 1st transaction
                    using (Transaction t = new Transaction(curDoc))
                    {
                        // start the transaction group
                        tGroup.Start();

                        #region Delete View Templates

                        // initialize progress bar for deletion phase
                        ProgressBarHelper deleteProgressHelper = new ProgressBarHelper();
                        deleteProgressHelper.ShowProgress(curVTs.Count);

                        try
                        {
                            // start the 1st transaction
                            t.Start("Delete View Templates");

                            // create instance of progress bar
                            for (int i = 0; i < curVTs.Count; i++)
                            {
                                // check for user cancellation
                                if (deleteProgressHelper.IsCancelled())
                                {
                                    // close progress bar
                                    deleteProgressHelper.CloseProgress();

                                    // rollback the transaction
                                    t.RollBack();
                                    tGroup.RollBack();
                                    return Result.Cancelled;
                                }

                                View curVT = curVTs[i];

                                // update the progress bar
                                deleteProgressHelper.UpdateProgress(i + 1, $"Deleting {i + 1} of {curVTs.Count} View Templates");

                                // get the name of the view template
                                string curName = curVT.Name;

                                // check view template name for deletion criteria
                                if (!string.IsNullOrEmpty(curName))
                                {
                                    // check if first character is letter
                                    bool isLetter = Char.IsLetter(curName[0]);

                                    // check if starts with 01, 02, 03, 04, 05, 06, or 07                    
                                    bool isTargetNumber = curName.StartsWith("01") ||
                                                          curName.StartsWith("02") ||
                                                          curName.StartsWith("03") ||
                                                          curName.StartsWith("04") ||
                                                          curName.StartsWith("05") ||
                                                          curName.StartsWith("06") ||
                                                          curName.StartsWith("07");

                                    // if yes, delete it
                                    if (isLetter == true || isTargetNumber == true)
                                    {
                                        try
                                        {
                                            curDoc.Delete(curVT.Id);
                                            templatesDeleted++; // increment the counter
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                }
                            }

                            // commit the 1st transaction
                            t.Commit();
                        }

                        finally
                        {
                            // close the progress bar for deletion phase
                            deleteProgressHelper.CloseProgress();
                        }

                        #endregion

                        // create a variable for the target document                  
                        Document targetDoc = uidoc.Document;

                        // get the view templates from the source document
                        List<View> listViewTemplates = new FilteredElementCollector(sourceDoc)
                            .OfClass(typeof(View))
                            .Cast<View>()
                            .Where(v => v.IsTemplate)
                            .ToList();

                        #region Transfer View Templates

                        // start the 2nd transaction
                        t.Start("Transfer View Teamplates");

                        // create instance of progress bar

                        // transfer the vew templates from the source document
                        foreach (View sourceTemplate in listViewTemplates)
                        {
                            // increment progress bar

                            // check if template with exact same name already exists
                            View existingTemplate = new FilteredElementCollector(curDoc)
                                .OfClass(typeof(View))
                                .Cast<View>()
                                .FirstOrDefault(v => v.IsTemplate && v.Name.Equals(sourceTemplate.Name));

                            if (existingTemplate == null)
                            {
                                ElementId newTemplateID = Utils.ImportViewTemplates(sourceDoc, sourceTemplate, targetDoc);
                                templatesImported++; // increment the counter
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Skipping existing template: {sourceTemplate.Name}");
                            }
                        }

                        // close progress bar

                        t.Commit();

                        #endregion                        

                        #region Assign View Templates                                               

                        // get all the new view templates in the project
                        List<View> newVTs = Utils.GetAllViewTemplates(curDoc);

                        // get the template map
                        var mapVTs = Utils.GetViewTemplateMap();

                        // start the 3rd transaction 
                        t.Start("Assign View Teamplates");

                        // create insatnce of progress bar

                        foreach (var curMap in mapVTs)
                        {
                            // increment progress bar

                            if (viewsByTemplate.ContainsKey(curMap.OldTemplateName))
                            {
                                var allViews = viewsByTemplate[curMap.OldTemplateName];
                                Utils.AssignTemplateToView(allViews, curMap.NewTemplateName, curDoc, ref viewsUpdated);
                            }
                        }

                        // close progress bar

                        // commit the 3rd transaction
                        t.Commit();

                        #endregion

                        tGroup.Assimilate();
                    }
                }
            }

            finally
            {
                // Close the source document when done
                if (sourceDoc != null && !sourceDoc.IsFamilyDocument)
                {
                    sourceDoc.Close(false); // false = don't save
                }
            }

            // Show final report
            TaskDialog tdFinalReport = new TaskDialog("Complete");
            tdFinalReport.MainIcon = Icon.TaskDialogIconInformation;
            tdFinalReport.Title = "Update View Templates";
            tdFinalReport.TitleAutoPrefix = false;
            tdFinalReport.MainContent = $"{templatesDeleted} existing view templates have been deleted, {templatesImported} new view templates were added to the project and assigned to {viewsUpdated} out of {totalViews} views.";
            tdFinalReport.CommonButtons = TaskDialogCommonButtons.Close;

            TaskDialogResult tdSchedSuccessRes = tdFinalReport.Show();

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCmd2_4";
            string buttonTitle = "Update View\rTemplates";
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
                    Properties.Resources.UpdateVTs_32,
                    Properties.Resources.UpdateVTs_16,
                    "Updates view templates to new standards");

                return myBtnData1.Data;
            }
        }
    }
}

