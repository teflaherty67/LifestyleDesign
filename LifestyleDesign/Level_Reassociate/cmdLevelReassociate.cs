using LifestyleDesign.Classes;
using LifestyleDesign.Common;
using System.Text;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdLevelReassociate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            #region Get Data

            // get every level in the project, sorted low to high
            List<Level> allLevels = Utils.GetAllLevels(curDoc)
                .OrderBy(lvl => lvl.Elevation)
                .ToList();

            if (allLevels.Count < 2)
            {
                Utils.TaskDialogWarning("Reassociate Level", "Reassociate Level", "This project needs at least two levels to reassociate elements between.");
                return Result.Cancelled;
            }

            #endregion

            #region Form

            // launch the form so the user can pick the level being vacated and the level to receive its elements
            frmLevelReassociate curForm = new frmLevelReassociate(curDoc, allLevels)
            {
                Topmost = true
            };

            curForm.ShowDialog();

            if (curForm.DialogResult != true)
                return Result.Cancelled;

            Level sourceLevel = curForm.SourceLevel;
            Level targetLevel = curForm.TargetLevel;

            #endregion

            #region Confirm

            TaskDialog confirm = new TaskDialog("Reassociate Level")
            {
                MainInstruction = $"Reassociate all elements from '{sourceLevel.Name}' to '{targetLevel.Name}'?",
                MainContent = "Any element constrained to the target level will take on the target level's elevation " +
                    "(for example, walls whose top constraint currently references the level being vacated will rise " +
                    "or fall to match the target level).",
                CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.Yes
            };

            if (confirm.Show() != TaskDialogResult.Yes)
                return Result.Cancelled;

            #endregion

            #region Reassociate Elements

            ElementId sourceId = sourceLevel.Id;
            ElementId targetId = targetLevel.Id;

            // elements Revit already considers "associated" with the source level
            List<Element> candidates = new FilteredElementCollector(curDoc)
                .WherePasses(new ElementLevelFilter(sourceId))
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            HashSet<ElementId> candidateIds = new HashSet<ElementId>(candidates.Select(e => e.Id));

            // walls can reference the source level through their Top Constraint even when their
            // Base Constraint is a different level, so they need to be checked independently
            List<Element> allWalls = new FilteredElementCollector(curDoc)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            foreach (Element wall in allWalls)
            {
                if (candidateIds.Contains(wall.Id))
                    continue;

                if (HasLevelReference(wall, sourceId))
                {
                    candidateIds.Add(wall.Id);
                    candidates.Add(wall);
                }
            }

            // track which elements got fixed automatically, and which still need manual attention
            int reassociatedElements = 0;
            int reassociatedParams = 0;
            Dictionary<ElementId, string> skippedElements = new Dictionary<ElementId, string>();
            bool rolledBackEntirely = false;

            using (Transaction t = new Transaction(curDoc, "Reassociate Level"))
            {
                t.Start();

                // some elements (e.g. compound walls that can't extend their layers at the new
                // elevation) will fail Revit's geometry validation. Isolate each element's change in
                // its own sub-transaction so a single failure only rolls back that one element, and
                // resolve failures silently instead of letting Revit pop up a blocking dialog.
                FailureHandlingOptions failureOptions = t.GetFailureHandlingOptions();
                failureOptions.SetFailuresPreprocessor(new SilentFailuresPreprocessor());
                failureOptions.SetClearAfterRollback(true);
                t.SetFailureHandlingOptions(failureOptions);

                foreach (Element curElem in candidates)
                {
                    int changed;
                    bool hadUnresolvedReference;

                    using (SubTransaction st = new SubTransaction(curDoc))
                    {
                        st.Start();

                        (changed, hadUnresolvedReference) = ReassignLevelReferences(curElem, sourceId, targetId);

                        if (changed == 0)
                        {
                            st.RollBack();
                        }
                        else if (st.Commit() == TransactionStatus.Committed)
                        {
                            reassociatedElements++;
                            reassociatedParams += changed;
                        }
                        else
                        {
                            // Revit couldn't resolve a geometry error caused by this change
                            // (e.g. a compound wall that can't extend its layers) - rolled back
                            hadUnresolvedReference = true;
                        }
                    }

                    if (hadUnresolvedReference)
                    {
                        skippedElements[curElem.Id] = DescribeElement(curElem);
                    }
                }

                rolledBackEntirely = t.Commit() != TransactionStatus.Committed;
            }

            if (rolledBackEntirely)
            {
                Utils.TaskDialogError("Error", "Reassociate Level",
                    "Revit could not complete the reassociation and rolled back all changes. No elements were modified; nothing was deleted.");
                return Result.Failed;
            }

            // verify: anything Revit still considers associated with the source level that wasn't
            // already flagged above didn't have a settable parameter our scan could find
            List<Element> remaining = new FilteredElementCollector(curDoc)
                .WherePasses(new ElementLevelFilter(sourceId))
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            foreach (Element curElem in remaining)
            {
                if (!skippedElements.ContainsKey(curElem.Id))
                    skippedElements[curElem.Id] = DescribeElement(curElem);
            }

            #endregion

            #region Summary Report & Level Deletion

            StringBuilder summaryMessage = new StringBuilder();

            summaryMessage.AppendLine(reassociatedElements > 0
                ? $"{reassociatedElements} element{(reassociatedElements == 1 ? "" : "s")} reassociated from '{sourceLevel.Name}' to '{targetLevel.Name}' " +
                  $"({reassociatedParams} level reference{(reassociatedParams == 1 ? "" : "s")} updated)."
                : $"No elements needed to be reassociated from '{sourceLevel.Name}'.");

            if (skippedElements.Count > 0)
            {
                summaryMessage.AppendLine();
                summaryMessage.AppendLine($"The following {skippedElements.Count} element(s) still reference '{sourceLevel.Name}' and could not be updated " +
                    "automatically - either their level parameter is read-only in this project, or Revit could not resolve a geometry " +
                    $"error caused by the change (e.g. a compound wall that can't extend its layers). Resolve these manually before " +
                    $"'{sourceLevel.Name}' can be deleted:");

                foreach (string skipped in skippedElements.Values)
                {
                    summaryMessage.AppendLine($"  • {skipped}");
                }

                Utils.TaskDialogInformation("Summary", "Reassociate Level", summaryMessage.ToString().Trim());
            }
            else
            {
                summaryMessage.AppendLine();
                summaryMessage.AppendLine($"'{sourceLevel.Name}' no longer has any dependent elements.");

                Utils.TaskDialogInformation("Summary", "Reassociate Level", summaryMessage.ToString().Trim());

                TaskDialog deleteConfirm = new TaskDialog("Reassociate Level")
                {
                    MainInstruction = $"Delete level '{sourceLevel.Name}' now?",
                    CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                    DefaultButton = TaskDialogResult.No
                };

                if (deleteConfirm.Show() == TaskDialogResult.Yes)
                {
                    try
                    {
                        TransactionStatus deleteStatus;

                        using (Transaction t = new Transaction(curDoc, "Delete Level"))
                        {
                            t.Start();

                            FailureHandlingOptions failureOptions = t.GetFailureHandlingOptions();
                            failureOptions.SetFailuresPreprocessor(new SilentFailuresPreprocessor());
                            failureOptions.SetClearAfterRollback(true);
                            t.SetFailureHandlingOptions(failureOptions);

                            curDoc.Delete(sourceId);
                            deleteStatus = t.Commit();
                        }

                        if (deleteStatus == TransactionStatus.Committed)
                        {
                            Utils.TaskDialogInformation("Summary", "Reassociate Level", $"'{sourceLevel.Name}' has been deleted.");
                        }
                        else
                        {
                            Utils.TaskDialogError("Error", "Reassociate Level",
                                $"'{sourceLevel.Name}' could not be deleted; Revit could not resolve an error and rolled back the deletion.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.TaskDialogError("Error", "Reassociate Level", $"'{sourceLevel.Name}' could not be deleted: {ex.Message}");
                    }
                }
            }

            #endregion

            return Result.Succeeded;
        }

        /// <summary>
        /// Sets every settable ElementId-type parameter on the element that currently points at
        /// sourceLevelId to targetLevelId instead. Working generically off the parameter's stored
        /// value - rather than a hard-coded list of BuiltInParameters - means it transparently
        /// covers Base/Top Constraint, Level, Base/Top Level, Schedule Level, etc. for whichever
        /// category the element happens to be.
        /// </summary>
        private static (int changed, bool hadUnresolvedReference) ReassignLevelReferences(Element elem, ElementId sourceLevelId, ElementId targetLevelId)
        {
            int changed = 0;
            bool hadUnresolvedReference = false;

            foreach (Parameter param in elem.Parameters)
            {
                if (param.StorageType != StorageType.ElementId)
                    continue;

                if (param.AsElementId() != sourceLevelId)
                    continue;

                if (param.IsReadOnly)
                {
                    hadUnresolvedReference = true;
                    continue;
                }

                try
                {
                    param.Set(targetLevelId);
                    changed++;
                }
                catch (Exception)
                {
                    hadUnresolvedReference = true;
                }
            }

            return (changed, hadUnresolvedReference);
        }

        private static bool HasLevelReference(Element elem, ElementId levelId)
        {
            foreach (Parameter param in elem.Parameters)
            {
                if (param.StorageType == StorageType.ElementId && param.AsElementId() == levelId)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Dismisses warnings and rolls back errors without ever showing Revit's interactive
        /// failure dialog, so a single element that can't resolve (e.g. a compound wall that
        /// can't extend its layers at the new elevation) doesn't block the whole command on a
        /// modal prompt the user has no good way to answer.
        /// </summary>
        private class SilentFailuresPreprocessor : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                bool hasUnresolvedError = false;

                foreach (FailureMessageAccessor failure in failuresAccessor.GetFailureMessages())
                {
                    if (failure.GetSeverity() == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(failure);
                    }
                    else
                    {
                        hasUnresolvedError = true;
                    }
                }

                return hasUnresolvedError
                    ? FailureProcessingResult.ProceedWithRollBack
                    : FailureProcessingResult.Continue;
            }
        }

        private static string DescribeElement(Element elem)
        {
            return $"{elem.Category?.Name ?? "Unknown Category"} - {elem.Name} (Id {elem.Id})";
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCmd3_3";
            string buttonTitle = "Reassociate\rLevel";

            clsButtonData myBtnData = new clsButtonData(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.LevelManager_32,
                Properties.Resources.LevelManager_16,
                "Reassociates all elements on one level to another level so the original level can be safely deleted.");

            return myBtnData.Data;
        }
    }
}
