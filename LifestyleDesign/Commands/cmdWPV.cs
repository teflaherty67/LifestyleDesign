using Autodesk.Revit.DB.Architecture;
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

            // find the STANDARD text note type
            TextNoteType standardTextType = new FilteredElementCollector(curDoc)
                .OfClass(typeof(TextNoteType))
                .Cast<TextNoteType>()
                .FirstOrDefault(tnt => tnt.Name == "STANDARD");

            if (standardTextType == null)
            {
                Utils.TaskDialogError("Error", "Create Visitability Plan", "Could not find a text note type named 'STANDARD' in the project.");
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

                // normalize room tag types in the new view
                List<RoomTag> newViewRoomTags = new FilteredElementCollector(curDoc, newView.Id)
                    .OfCategory(BuiltInCategory.OST_RoomTags)
                    .WhereElementIsNotElementType()
                    .Cast<RoomTag>()
                    .ToList();

                foreach (RoomTag curRoomTag in newViewRoomTags)
                {
                    string curTypeName = curRoomTag.RoomTagType.Name;

                    string newTypeName = null;
                    if (curTypeName.StartsWith("Small") && curTypeName.EndsWith("single"))
                    {
                        newTypeName = "Small - Name,single";
                    }
                    else if (curTypeName.StartsWith("Small") && curTypeName.EndsWith("double"))
                    {
                        newTypeName = "Small - Name,double";
                    }

                    if (newTypeName == null || curTypeName == newTypeName)
                    {
                        continue;
                    }

                    string familyName = curRoomTag.RoomTagType.Family.Name;
                    FamilySymbol newTagType = Utils.FindFamilySymbol(curDoc, familyName, newTypeName);

                    if (newTagType == null)
                    {
                        t.RollBack();
                        Utils.TaskDialogError("Error", "Create Visitability Plan", $"Could not find room tag type '{newTypeName}' in family '{familyName}'.");
                        return Result.Failed;
                    }

                    if (!newTagType.IsActive)
                    {
                        newTagType.Activate();
                        curDoc.Regenerate();
                    }

                    curRoomTag.ChangeTypeId(newTagType.Id);
                }

                // change the Powder room's door to a 32"x80" Privacy door
                List<string> powderRoomNames = new List<string> { "Powder", "Pwdr" };

                FamilyInstance powderDoor = Utils.GetAllDoors(curDoc)
                    .FirstOrDefault(curDoor =>
                        (curDoor.ToRoom != null && powderRoomNames.Any(n => curDoor.ToRoom.Name.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0))
                        || (curDoor.FromRoom != null && powderRoomNames.Any(n => curDoor.FromRoom.Name.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0)));

                if (powderDoor == null)
                {
                    t.RollBack();
                    Utils.TaskDialogError("Error", "Create Visitability Plan", "Could not find a door serving the 'Powder' room.");
                    return Result.Failed;
                }

                string powderDoorFamilyName = "Flush Single w_Hardware";
                string powderDoorTypeName = "32\"x80\" Privacy";

                FamilySymbol powderDoorType = Utils.FindFamilySymbol(curDoc, powderDoorFamilyName, powderDoorTypeName);

                if (powderDoorType == null)
                {
                    t.RollBack();
                    Utils.TaskDialogError("Error", "Create Visitability Plan", $"Could not find door type '{powderDoorTypeName}' in family '{powderDoorFamilyName}'.");
                    return Result.Failed;
                }

                if (!powderDoorType.IsActive)
                {
                    powderDoorType.Activate();
                    curDoc.Regenerate();
                }

                powderDoor.ChangeTypeId(powderDoorType.Id);

                // use the "First Floor Plan" sheet as the reference for elevation letter, title block, and
                // browser grouping - it's guaranteed to be a normal content sheet, unlike the Cover sheet, which
                // an arbitrary "any A-series sheet" pick could land on and would carry the wrong title block
                List<ViewSheet> allSheets = Utils.GetAllSheets(curDoc);

                ViewSheet referenceSheet = allSheets
                    .FirstOrDefault(s => s.Name == "First Floor Plan" && Utils.GetParameterValueByName(s, "Category") == "Active")
                    ?? allSheets.FirstOrDefault(s => s.Name == "First Floor Plan");

                string elevLetter = null;

                if (referenceSheet != null)
                {
                    TryParseASeriesSheetNumber(referenceSheet.SheetNumber, out _, out elevLetter);
                }

                if (referenceSheet == null || elevLetter == null)
                {
                    // fall back to any "A" series sheet, preferring one marked Category = "Active"
                    foreach (ViewSheet curSheet in allSheets)
                    {
                        if (TryParseASeriesSheetNumber(curSheet.SheetNumber, out _, out string letter)
                            && Utils.GetParameterValueByName(curSheet, "Category") == "Active")
                        {
                            referenceSheet = curSheet;
                            elevLetter = letter;
                            break;
                        }
                    }

                    if (referenceSheet == null)
                    {
                        foreach (ViewSheet curSheet in allSheets)
                        {
                            if (TryParseASeriesSheetNumber(curSheet.SheetNumber, out _, out string letter))
                            {
                                referenceSheet = curSheet;
                                elevLetter = letter;
                                break;
                            }
                        }
                    }
                }

                if (referenceSheet == null)
                {
                    t.RollBack();
                    Utils.TaskDialogError("Error", "Create Visitability Plan", "Could not find an existing 'A' series sheet to determine the current elevation designation.");
                    return Result.Failed;
                }

                // collect every "A" series sheet numbered 1 and up, across every elevation group present (leave each
                // group's Cover sheet, A0x, alone). Sheet numbers stay in lockstep across elevation letters
                // (A2a/A2b/A2c all mean the same content), so if more than one elevation group exists, they all
                // need to shift together, not just the active one.
                List<(ViewSheet Sheet, int Number, string Letter)> aSeriesSheets = new List<(ViewSheet, int, string)>();

                foreach (ViewSheet curSheet in allSheets)
                {
                    if (TryParseASeriesSheetNumber(curSheet.SheetNumber, out int num, out string letter) && num >= 1)
                    {
                        aSeriesSheets.Add((curSheet, num, letter));
                    }
                }

                // shift them up by 1, highest number first, to free up "A1" + elevation letter without collisions
                foreach (var entry in aSeriesSheets.OrderByDescending(e => e.Number))
                {
                    Parameter sheetNumberParam = entry.Sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER);

                    if (sheetNumberParam == null || sheetNumberParam.IsReadOnly)
                    {
                        t.RollBack();
                        Utils.TaskDialogError("Error", "Create Visitability Plan", $"Sheet '{entry.Sheet.SheetNumber}' has a read-only sheet number (it may be a placeholder/shared sheet) and can't be renumbered.");
                        return Result.Failed;
                    }

                    entry.Sheet.SheetNumber = "A" + (entry.Number + 1) + entry.Letter;
                }

                // match the title block already used on the reference sheet
                FamilyInstance existingTitleBlock = new FilteredElementCollector(curDoc, referenceSheet.Id)
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .FirstOrDefault();

                if (existingTitleBlock == null)
                {
                    t.RollBack();
                    Utils.TaskDialogError("Error", "Create Visitability Plan", "Could not find a title block on the reference sheet to match for the new sheet.");
                    return Result.Failed;
                }

                // create the new sheet in the now-vacant "A1" + elevation letter slot
                ViewSheet visitabilitySheet = ViewSheet.Create(curDoc, existingTitleBlock.Symbol.Id);
                visitabilitySheet.SheetNumber = "A1" + elevLetter;
                visitabilitySheet.Name = "Visitability Plan";

                // copy these bookkeeping fields verbatim from the reference sheet - they may follow a structured
                // project-specific format (e.g. "S-0/E/D/8") rather than a simple "Elevation X" string, so match
                // whatever convention this project actually uses instead of reconstructing it. Non-critical to the
                // sheet itself, so skip any that are locked rather than failing the whole command over them
                TrySetParameterByName(visitabilitySheet, "Category", Utils.GetParameterValueByName(referenceSheet, "Category"));
                TrySetParameterByName(visitabilitySheet, "Group", Utils.GetParameterValueByName(referenceSheet, "Group"));
                TrySetParameterByName(visitabilitySheet, "Elevation Designation", Utils.GetParameterValueByName(referenceSheet, "Elevation Designation"));
                TrySetParameterByName(visitabilitySheet, "Code Filter", Utils.GetParameterValueByName(referenceSheet, "Code Filter"));

                // place the Visitability Plan view on the new sheet, centered on its title block
                FamilyInstance newTitleBlock = new FilteredElementCollector(curDoc, visitabilitySheet.Id)
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .WhereElementIsNotElementType()
                    .Cast<FamilyInstance>()
                    .FirstOrDefault();

                XYZ viewportLocation = new XYZ(2.0, 1.5, 0);

                BoundingBoxXYZ titleBlockBox = newTitleBlock?.get_BoundingBox(visitabilitySheet);

                if (titleBlockBox != null)
                {
                    viewportLocation = (titleBlockBox.Min + titleBlockBox.Max) / 2.0;
                }

                if (!Viewport.CanAddViewToSheet(curDoc, visitabilitySheet.Id, newView.Id))
                {
                    t.RollBack();
                    Utils.TaskDialogError("Error", "Create Visitability Plan", "Could not add the Visitability Plan view to the new sheet.");
                    return Result.Failed;
                }

                Viewport.Create(curDoc, visitabilitySheet.Id, newView.Id, viewportLocation);

                t.Commit();
            }

            Utils.TaskDialogInformation("Success", "Create Visitability Plan", "The Visitability Plan view has been created.");

            return Result.Succeeded;
        }

        private static void TrySetParameterByName(Element element, string paramName, string value)
        {
            if (value == null)
            {
                return;
            }

            Parameter param = element.LookupParameter(paramName);

            if (param != null && !param.IsReadOnly)
            {
                param.Set(value);
            }
        }

        private static bool TryParseASeriesSheetNumber(string sheetNumber, out int number, out string letterSuffix)
        {
            number = 0;
            letterSuffix = null;

            if (string.IsNullOrEmpty(sheetNumber) || sheetNumber.Length < 3 || sheetNumber[0] != 'A')
            {
                return false;
            }

            char lastChar = sheetNumber[sheetNumber.Length - 1];

            if (!char.IsLetter(lastChar))
            {
                return false;
            }

            string middle = sheetNumber.Substring(1, sheetNumber.Length - 2);

            if (!int.TryParse(middle, out number))
            {
                return false;
            }

            letterSuffix = lastChar.ToString();
            return true;
        }
    }
}
