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

            // find the view template to assign
            View viewTemplate = Utils.GetViewTemplateByName(curDoc, "01-Floor Visitability");

            if (viewTemplate == null)
            {
                Utils.TaskDialogError("Error", "Create Visitability Plan", "Could not find a view template named '01-Floor Visitability' in the project.");
                return Result.Failed;
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
