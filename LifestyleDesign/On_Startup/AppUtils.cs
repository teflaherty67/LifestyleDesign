using Autodesk.Revit.DB;
using LifestyleDesign.On_Startup;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LifestyleDesign.Common
{
    public static class AppUtils
    {
       

        public static void CheckAllStandards(Document curDoc)
        {
            // Create a dictionary to organize violations by category
            Dictionary<string, List<string>> violationsByCategory = new Dictionary<string, List<string>>();

            // Check view templates
            var missingViewTemplates = CheckViewTemplates(curDoc);
            if (missingViewTemplates.Count > 0)
            {
                violationsByCategory["NON-COMPLIANT VIEW TEMPLATES"] = missingViewTemplates;
            }

            // Check family compliance
            var cabinetViolations = CheckFamilyPrefix(curDoc, BuiltInCategory.OST_Casework, "Casework");
            if (cabinetViolations.Count > 0)
            {
                violationsByCategory["NON-COMPLIANT CABINETS"] = cabinetViolations;
            }

            var doorViolations = CheckFamilyPrefix(curDoc, BuiltInCategory.OST_Doors, "Door");
            if (doorViolations.Count > 0)
            {
                violationsByCategory["NON-COMPLIANT DOORS"] = doorViolations;
            }

            var windowViolations = CheckFamilyPrefix(curDoc, BuiltInCategory.OST_Windows, "Window");
            if (windowViolations.Count > 0)
            {
                violationsByCategory["NON-COMPLIANT WINDOWS"] = windowViolations;
            }

            // TODO: Add other compliance checks here
            // var projectInfoViolations = CheckProjectInformation(document);
            // etc.

            // If any violations found, create report and show warning
            if (violationsByCategory.Count > 0)
            {
                CreateComplianceReport(curDoc, violationsByCategory);
                ShowWarningDialog(curDoc.Title);
            }
        }

        private static List<string> CheckViewTemplates(Document document)
        {
            // Define required view templates
            List<string> requiredViewTemplates = new List<string>
            {
                "-Frame Schedule-",
                "-Schedule-",
                "01-Enlarged Form Plans",
                "01-Form Plans",
                "02-Enlarged Plans",
                "02-Floor Annotations",
                "02-Floor Dimensions",
                "02-Key Plans",
                "03-Exterior Elevations",
                "03-Key Elevations",
                "03-Porch Elevations",
                "04-Roof Plans",
                "05-Sections",
                "05-Sections_3/8\"",
                "06-Cabinet Layout Plans",
                "06-Interior Elevations",
                "07-Electrical Plans",
                "08-Frame_Ceiling/Floor",
                "09-Frame_Roof",
                "10-Floor Area",
                "11-Frame Area",
                "12-Roof Ventilation",
                "13-Elevation Presentation",
                "13-Floor Presentation",
                "14-Ceiling",
                "14-Soffit",
                "15-Roof",
                "16-3D",
                "16-3D Frame",
                "17-Details",
                "18-Framing Elevation"
            };

            return FindMissingViewTemplates(document, requiredViewTemplates);
        }

        private static List<string> FindMissingViewTemplates(Document document, List<string> requiredTemplates)
        {
            List<string> missingTemplates = new List<string>();

            // Get all view templates in the document
            FilteredElementCollector collector = new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType();

            HashSet<string> existingTemplates = new HashSet<string>();

            foreach (View view in collector)
            {
                if (view.IsTemplate)
                {
                    existingTemplates.Add(view.Name);
                }
            }

            // Check which required templates are missing
            foreach (string requiredTemplate in requiredTemplates)
            {
                if (!existingTemplates.Contains(requiredTemplate))
                {
                    missingTemplates.Add(requiredTemplate);
                }
            }

            return missingTemplates;
        }

        private static void CreateComplianceReport(Document document, Dictionary<string, List<string>> violationsByCategory)
        {
            try
            {
                // Get the project file path
                string projectPath = document.PathName;

                if (string.IsNullOrEmpty(projectPath))
                {
                    // If document hasn't been saved yet, save to desktop
                    projectPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }
                else
                {
                    projectPath = Path.GetDirectoryName(projectPath);
                }

                // Create report file name
                string projectName = string.IsNullOrEmpty(document.Title) ? "Untitled" : document.Title;
                string reportFileName = $"{projectName} Compliance Report.txt";
                string reportPath = Path.Combine(projectPath, reportFileName);

                // Calculate total violations
                int totalViolations = 0;
                foreach (var category in violationsByCategory.Values)
                {
                    totalViolations += category.Count;
                }

                // Create report content
                List<string> reportLines = new List<string>();
                reportLines.Add("LIFESTYLE DESIGN STANDARDS COMPLIANCE REPORT");
                reportLines.Add("==========================================");
                reportLines.Add($"Project: {projectName}");
                reportLines.Add($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                reportLines.Add($"Total Compliance Issues: {totalViolations}");
                reportLines.Add("");

                // Add each category section
                foreach (var kvp in violationsByCategory)
                {
                    string categoryName = kvp.Key;
                    List<string> violations = kvp.Value;

                    reportLines.Add($"{categoryName} ({violations.Count}):");
                    reportLines.Add(new string('-', categoryName.Length + $" ({violations.Count}):".Length));

                    for (int i = 0; i < violations.Count; i++)
                    {
                        reportLines.Add($"  {i + 1}. {violations[i]}");
                    }
                    reportLines.Add(""); // Add blank line between sections
                }

                // Write to file
                File.WriteAllLines(reportPath, reportLines);
            }
            catch (Exception ex)
            {
                // Could use TaskDialog here if needed for error reporting
                System.Windows.Forms.MessageBox.Show($"Could not create compliance report: {ex.Message}", "Error");
            }
        }

        private static void ShowWarningDialog(string projectName)
        {
            var warningWindow = new frmComplianceWarning(projectName);
            warningWindow.ShowDialog();
        }

        // TODO: Add more compliance check methods here
        // public static List<string> CheckRequiredFamilies(Document document) { }
        // public static List<string> CheckProjectInformation(Document document) { }
        // public static List<string> CheckNamingConventions(Document document) { }
        // public static List<string> CheckUnits(Document document) { }

        private static List<string> CheckFamilyCompliance(Document document)
        {
            List<string> violations = new List<string>();

            // check casework
            var caseworkViolations = CheckFamilyPrefix(document, BuiltInCategory.OST_Casework, "Casework");
            violations.AddRange(caseworkViolations);

            // Check doors
            var doorViolations = CheckFamilyPrefix(document, BuiltInCategory.OST_Doors, "Door");
            violations.AddRange(doorViolations);

            // Check windows
            var windowViolations = CheckFamilyPrefix(document, BuiltInCategory.OST_Windows, "Window");
            violations.AddRange(windowViolations);

            return violations;
        }

        private static List<string> CheckFamilyPrefix(Document document, BuiltInCategory category, string categoryName)
        {
            List<string> violations = new List<string>();

            // Get all elements of the specified category
            FilteredElementCollector collector = new FilteredElementCollector(document)
                .OfCategory(category)
                .WhereElementIsNotElementType();

            HashSet<string> nonCompliantFamilies = new HashSet<string>();

            foreach (Element element in collector)
            {
                // Get the family name
                string familyName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM)?.AsValueString();

                if (!string.IsNullOrEmpty(familyName) && !familyName.StartsWith("LD_"))
                {
                    nonCompliantFamilies.Add(familyName);
                }
            }

            // Add violations for each non-compliant family
            foreach (string familyName in nonCompliantFamilies)
            {
                violations.Add(familyName);
            }

            return violations;
        }

        public static void OnDocumentSaving(Document curDoc)
        {
            // launch the form
            UncatagorizedViewsWarning(curDoc);
        }        

        public static void OnDocumentClosing(Document curDoc)
        {
            // launch the form
            UncatagorizedViewsWarning(curDoc);
        }

        internal static void UncatagorizedViewsWarning(Document curDoc)
        {
            var uncategorizedViews = new FilteredElementCollector(curDoc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate)
                .Where(v => v.ViewType != ViewType.ProjectBrowser && 
                    v.ViewType != ViewType.DrawingSheet && 
                    v.ViewType != ViewType.Legend)
                .Where(IsUncategorized)
                .ToList();

            if (uncategorizedViews.Any())
            {
                var form = new frmUncategorizedViewsWarning(uncategorizedViews);
                form.ShowDialog();
            }
        }

        private static bool IsUncategorized(View view)
        {
            foreach (Parameter param in view.Parameters)
            {
                if (param.Definition.Name == "Category" &&
                    param.StorageType == StorageType.String &&
                    string.IsNullOrWhiteSpace(param.AsValueString()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}