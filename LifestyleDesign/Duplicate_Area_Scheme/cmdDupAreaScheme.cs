using LifestyleDesign.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdDupAreaScheme : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // edit these to match an existing Area Scheme in the test model before running
            string sourceSchemeName = "A Floor";
            string newSchemeName = "A Floor - Copy";

            // find the source Area Scheme
            AreaScheme sourceScheme = Utils.GetAreaSchemeByName(curDoc, sourceSchemeName);

            if (sourceScheme == null)
            {
                TaskDialog.Show("Duplicate Area Scheme", "Could not find an Area Scheme named \"" + sourceSchemeName + "\".");
                return Result.Cancelled;
            }

            // find the Color Fill Schemes tied to the source Area Scheme
            List<ColorFillScheme> sourceColorSchemes = new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_ColorFillSchema)
                .Cast<ColorFillScheme>()
                .Where(x => x.AreaSchemeId.Equals(sourceScheme.Id))
                .ToList();

            StringBuilder report = new StringBuilder();

            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Duplicate Area Scheme");

                try
                {
                    // copy the Area Scheme
                    ICollection<ElementId> copiedIds = ElementTransformUtils.CopyElement(curDoc, sourceScheme.Id, XYZ.Zero);
                    AreaScheme newScheme = curDoc.GetElement(copiedIds.FirstOrDefault()) as AreaScheme;

                    if (newScheme == null)
                    {
                        t.RollBack();
                        TaskDialog.Show("Duplicate Area Scheme", "CopyElement did not return a new Area Scheme.");
                        return Result.Failed;
                    }

                    newScheme.Name = newSchemeName;

                    report.AppendLine("New Area Scheme: " + newScheme.Name + " (Id " + newScheme.Id + ")");
                    report.AppendLine();

                    // copy each Color Fill Scheme tied to the source Area Scheme
                    foreach (ColorFillScheme sourceColor in sourceColorSchemes)
                    {
                        ElementId newColorId = sourceColor.Duplicate(sourceColor.Name + " - Copy");
                        ColorFillScheme newColor = curDoc.GetElement(newColorId) as ColorFillScheme;

                        report.AppendLine("Color Fill Scheme: " + newColor.Name);
                        report.AppendLine("  Source AreaSchemeId: " + sourceColor.AreaSchemeId);
                        report.AppendLine("  Copy AreaSchemeId:   " + newColor.AreaSchemeId);
                        report.AppendLine("  New Area Scheme Id:  " + newScheme.Id);
                        report.AppendLine();
                    }

                    t.Commit();
                }
                catch (Exception ex)
                {
                    t.RollBack();
                    TaskDialog.Show("Duplicate Area Scheme", "Failed: " + ex.Message);
                    return Result.Failed;
                }
            }

            if (sourceColorSchemes.Count == 0)
                report.AppendLine("No Color Fill Schemes were found tied to \"" + sourceSchemeName + "\".");

            // report the results so the copy's AreaSchemeId can be checked against the new Area Scheme's Id
            TaskDialog.Show("Duplicate Area Scheme", report.ToString());

            return Result.Succeeded;
        }
    }
}
