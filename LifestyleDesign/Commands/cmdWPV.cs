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

            // find the Standard text note type
            TextNoteType standardTextType = new FilteredElementCollector(curDoc)
                .OfClass(typeof(TextNoteType))
                .Cast<TextNoteType>()
                .FirstOrDefault(tnt => tnt.Name == "Standard");

            if (standardTextType == null)
            {
                Utils.TaskDialogError("Error", "Create Visitability Plan", "Could not find a text note type named 'Standard' in the project.");
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

                // delete all existing text notes in the new view
                List<ElementId> existingTextNoteIds = new FilteredElementCollector(curDoc, newView.Id)
                    .OfClass(typeof(TextNote))
                    .ToElementIds()
                    .ToList();

                if (existingTextNoteIds.Count > 0)
                {
                    curDoc.Delete(existingTextNoteIds);
                }

                // create the required visitability text notes using the Standard type, center justified
                List<string> visitabilityNotes = new List<string>
                {
                    "Exterior visitable route to carport",
                    "Visitable entrance: Option 2 (see notes)",
                    "Visitable bath (see notes)",
                    "Visitable entrance: Option 1 (see notes)",
                    "Exterior visitable route to public street"
                };

                double verticalOffset = 1.5; // feet between stacked notes, so they don't land on top of each other

                for (int i = 0; i < visitabilityNotes.Count; i++)
                {
                    TextNoteOptions textOptions = new TextNoteOptions(standardTextType.Id)
                    {
                        HorizontalAlignment = HorizontalTextAlignment.Center
                    };

                    XYZ noteLocation = new XYZ(0, -i * verticalOffset, 0);

                    TextNote.Create(curDoc, newView.Id, noteLocation, visitabilityNotes[i], textOptions);
                }

                // delete all "Door Tag-Type Comments" door tag instances
                List<ElementId> doorTagsToDelete = new FilteredElementCollector(curDoc, newView.Id)
                    .OfCategory(BuiltInCategory.OST_DoorTags)
                    .WhereElementIsNotElementType()
                    .Cast<IndependentTag>()
                    .Where(tag => (curDoc.GetElement(tag.GetTypeId()) as FamilySymbol)?.Family.Name == "Door Tag-Type Comments")
                    .Select(tag => tag.Id)
                    .ToList();

                if (doorTagsToDelete.Count > 0)
                {
                    curDoc.Delete(doorTagsToDelete);
                }

                // delete all detail lines using the "Thermal envelope" line style
                List<ElementId> thermalLinesToDelete = new FilteredElementCollector(curDoc, newView.Id)
                    .OfClass(typeof(CurveElement))
                    .Cast<CurveElement>()
                    .Where(ce => ce.LineStyle != null && ce.LineStyle.Name == "Thermal envelope")
                    .Select(ce => ce.Id)
                    .ToList();

                if (thermalLinesToDelete.Count > 0)
                {
                    curDoc.Delete(thermalLinesToDelete);
                }

                t.Commit();
            }

            Utils.TaskDialogInformation("Success", "Create Visitability Plan", "The Visitability Plan view has been created.");

            return Result.Succeeded;
        }
    }
}
