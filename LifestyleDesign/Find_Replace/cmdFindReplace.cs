using LifestyleDesign.Classes;
using LifestyleDesign.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdFindReplace : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            try
            {
                var curForm = new frmFindReplace();
                if (curForm.ShowDialog() != true) return Result.Cancelled;

                string findText = curForm.FindText;
                string replaceText = curForm.ReplaceText;

                var regexOptions = RegexOptions.None;
                if (!curForm.MatchCase) regexOptions |= RegexOptions.IgnoreCase;

                string pattern = curForm.MatchWholeWord
                    ? $@"\b{Regex.Escape(findText)}\b"
                    : Regex.Escape(findText);

                var hits = new List<(Element element, BuiltInParameter param, string newValue)>();

                // Collect views for View Name and/or Title on Sheet
                if (curForm.ReplaceViewName || curForm.ReplaceTitleOnSheet)
                {
                    var viewIds = GetViewIds(curDoc, uidoc, curForm.Scope);
                    foreach (ElementId id in viewIds)
                    {
                        if (!(curDoc.GetElement(id) is View v)) continue;

                        if (curForm.ReplaceViewName)
                            AddHit(v, BuiltInParameter.VIEW_NAME, pattern, replaceText, regexOptions, hits);

                        if (curForm.ReplaceTitleOnSheet)
                            AddHit(v, BuiltInParameter.VIEW_DESCRIPTION, pattern, replaceText, regexOptions, hits);
                    }
                }

                // Collect sheets for Sheet Name
                if (curForm.ReplaceSheetName)
                {
                    var sheets = GetSheets(curDoc, uidoc, curForm.Scope);
                    foreach (ViewSheet sheet in sheets)
                        AddHit(sheet, BuiltInParameter.SHEET_NAME, pattern, replaceText, regexOptions, hits);
                }

                if (hits.Count == 0)
                {
                    Utils.TaskDialogInformation("Find and Replace", "Find and Replace", $"No matches found for \"{findText}\".");
                    return Result.Succeeded;
                }

                // Confirm
                var confirm = new TaskDialog("Find and Replace: Confirm")
                {
                    MainInstruction = $"Update {hits.Count} parameter value(s)?",
                    MainContent = $"Find: \"{findText}\"\nReplace with: \"{replaceText}\"\n\nMatch is case {(curForm.MatchCase ? "sensitive" : "insensitive")}.",
                    CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                    DefaultButton = TaskDialogResult.Yes
                };
                if (confirm.Show() != TaskDialogResult.Yes) return Result.Cancelled;

                // Apply
                int updatedCount = 0;
                using (Transaction t = new Transaction(curDoc, "Find/Replace Parameters"))
                {
                    t.Start();
                    foreach (var (element, param, newValue) in hits)
                    {
                        Parameter p = element.get_Parameter(param);
                        if (p != null && !p.IsReadOnly)
                        {
                            p.Set(newValue);
                            updatedCount++;
                        }
                    }
                    t.Commit();
                }

                Utils.TaskDialogInformation("Find and Replace", "Find and Replace", $"Updated {updatedCount} parameter value(s).");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private static HashSet<ElementId> GetViewIds(Document curDoc, UIDocument uidoc, SearchScope scope)
        {
            var viewIds = new HashSet<ElementId>();
            var sheets = GetSheets(curDoc, uidoc, scope);

            foreach (ViewSheet sheet in sheets)
            {
                foreach (ElementId vpId in sheet.GetAllViewports())
                {
                    if (curDoc.GetElement(vpId) is Viewport vp)
                        viewIds.Add(vp.ViewId);
                }

                var schedules = new FilteredElementCollector(curDoc, sheet.Id)
                    .OfClass(typeof(ScheduleSheetInstance))
                    .Cast<ScheduleSheetInstance>();

                foreach (var si in schedules)
                {
                    if (!si.IsTitleblockRevisionSchedule)
                        viewIds.Add(si.ScheduleId);
                }
            }

            return viewIds;
        }

        private static IEnumerable<ViewSheet> GetSheets(Document curDoc, UIDocument uidoc, SearchScope scope)
        {
            switch (scope)
            {
                case SearchScope.CurrentView:
                    if (uidoc.ActiveView is ViewSheet activeSheet)
                        return new[] { activeSheet };
                    return Enumerable.Empty<ViewSheet>();

                case SearchScope.CurrentSelection:
                    return uidoc.Selection.GetElementIds()
                        .Select(id => curDoc.GetElement(id))
                        .OfType<ViewSheet>()
                        .Where(s => !s.IsPlaceholder);

                default: // EntireProject
                    return Utils.GetAllSheets(curDoc).Where(s => !s.IsPlaceholder);
            }
        }

        private static void AddHit(
            Element element,
            BuiltInParameter param,
            string pattern,
            string replaceText,
            RegexOptions regexOptions,
            List<(Element, BuiltInParameter, string)> hits)
        {
            Parameter p = element.get_Parameter(param);
            if (p == null || p.IsReadOnly || p.StorageType != StorageType.String) return;

            string current = p.AsString() ?? string.Empty;
            string updated = Regex.Replace(current, pattern, replaceText, regexOptions);
            if (!string.Equals(updated, current, StringComparison.Ordinal))
                hits.Add((element, param, updated));
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Find & Replace";

            Classes.clsButtonData myButtonData = new Classes.clsButtonData(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "Find and replace text in View Name, Title on Sheet, or Sheet Name");

            return myButtonData.Data;
        }
    }
}
