using LifestyleDesign.Common;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdWPV : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // find the First Floor level
            Level firstFloorLevel = Utils.GetLevelByName(curDoc, "First Floor");

            if (firstFloorLevel == null)
            {
                Utils.TaskDialogError("Error", "Create Visitability Plan", "Could not find a level named 'First Floor' in the project.");
                return Result.Failed;
            }

            // find the floor plan view on the First Floor level with "Annotation" in the name
            ViewPlan sourceView = new FilteredElementCollector(curDoc)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .FirstOrDefault(v => !v.IsTemplate
                    && v.GenLevel != null
                    && v.GenLevel.Id == firstFloorLevel.Id
                    && v.Name.Contains("Annotation"));

            if (sourceView == null)
            {
                Utils.TaskDialogError("Error", "Create Visitability Plan", "Could not find a view on the 'First Floor' level with 'Annotation' in the name.");
                return Result.Failed;
            }

            // view template settings
            string templateName = "01-Floor Visitability";
            string templateSourcePath = @"S:\Shared Folders\Lifestyle USA Design\LGI Homes\Central Texas\Terrata Homes\Whisper Valley (WPV)\Dylan\Dylan(R)-CTX(0-8-27'4)WPV.rvt";

            // find the view template to assign
            View viewTemplate = Utils.GetViewTemplateByName(curDoc, templateName);

            // if it's not already in the project, load it from the source file
            if (viewTemplate == null)
            {
                Document sourceDoc = null;

                try
                {
                    sourceDoc = uidoc.Application.Application.OpenDocumentFile(templateSourcePath);

                    View sourceTemplate = Utils.GetViewTemplateByName(sourceDoc, templateName);

                    if (sourceTemplate == null)
                    {
                        Utils.TaskDialogError("Error", "Create Visitability Plan", $"Could not find a view template named '{templateName}' in the project or in the source file:\n{templateSourcePath}");
                        return Result.Failed;
                    }

                    using (Transaction tImport = new Transaction(curDoc, "Import Project Standards"))
                    {
                        tImport.Start();

                        // bring in any line styles from the source file that don't already exist (new only, never overwrite)
                        Utils.ImportNewLineStyles(sourceDoc, curDoc);

                        // bring in the view template (new only, never overwrite existing dependent types)
                        Utils.ImportViewTemplates(sourceDoc, sourceTemplate, curDoc);

                        tImport.Commit();
                    }

                    viewTemplate = Utils.GetViewTemplateByName(curDoc, templateName);

                    if (viewTemplate == null)
                    {
                        Utils.TaskDialogError("Error", "Create Visitability Plan", $"The view template '{templateName}' failed to import from the source file.");
                        return Result.Failed;
                    }
                }
                catch (Exception ex)
                {
                    Utils.TaskDialogError("Error", "Create Visitability Plan", $"Could not load the view template from the source file:\n{templateSourcePath}\n\n{ex.Message}");
                    return Result.Failed;
                }
                finally
                {
                    if (sourceDoc != null)
                    {
                        sourceDoc.Close(false);
                    }
                }
            }

            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Create Visitability Plan");

                // duplicate the source view
                ElementId newViewId = sourceView.Duplicate(ViewDuplicateOption.WithDetailing);
                ViewPlan newView = curDoc.GetElement(newViewId) as ViewPlan;

                // rename the new view
                try
                {
                    newView.Name = "Visitability Plan";
                }
                catch (Exception ex)
                {
                    t.RollBack();
                    Utils.TaskDialogError("Error", "Create Visitability Plan", $"Could not rename the new view to 'Visitability Plan': {ex.Message}");
                    return Result.Failed;
                }

                // assign the view template
                newView.ViewTemplateId = viewTemplate.Id;

                t.Commit();
            }

            Utils.TaskDialogInformation("Success", "Create Visitability Plan", "The Visitability Plan view has been created.");

            return Result.Succeeded;
        }
    }
}
