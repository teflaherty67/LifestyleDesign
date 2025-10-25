using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using LifestyleDesign.Classes;
using LifestyleDesign.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdUpdateSchedules : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            #region Code for Transaction Group

            // create a hashset to hold all renamed schedules
            HashSet<ElementId> modifiedScheduleIds = new HashSet<ElementId>();

            // list to hold schedules to rename
            List<ViewSchedule> schedsToRename = new List<ViewSchedule>();

            // variable for regex pattern to match old naming convention
            string oldPattern = @"[A-Za-z]-\d{2}/[A-Za-z]/[A-Za-z]/([A-Za-z]/)?\w";

            // get all the schedules in the project
            List<ViewSchedule> allSchedules = Utils.GetAllSchedules(curDoc);

            // loop through each schedule
            foreach (ViewSchedule curSched in allSchedules)
            {
                // check for old naming convention
                string schedName = curSched.Name;

                if (Regex.IsMatch(schedName, oldPattern))
                {
                    // add it to a list to rename
                    schedsToRename.Add(curSched);
                }
            }

            #endregion

            // create a transaction group to update the schedules
            using (TransactionGroup tgroup = new TransactionGroup(curDoc, "Update Schedules"))
            {
                // start the transaction group
                tgroup.Start();

                #region replace plan code

                // create a transaction to replace the plan code
                using (Transaction t1 = new Transaction(curDoc, "Replace Plan Code"))
                {
                    // start the transaction
                    t1.Start();

                    // loop through the list and rename
                    foreach (ViewSchedule curSched in schedsToRename)
                    {
                        // add to the hashset
                        modifiedScheduleIds.Add(curSched.Id);

                        // get the exisitng name
                        string curName = curSched.Name;

                        // extract elevation designation from current name using regex
                        Match elevMatch = Regex.Match(curName, oldPattern);
                        if (elevMatch.Success)
                        {
                            // get first character of the old pattern
                            string elevLetter = elevMatch.Value.Substring(0, 1); // Get first char of the matched pattern

                            // create the new pattern
                            string newPattern = "Elevation " + elevLetter;

                            // replace old pattern with new pattern
                            string newName = curName.Replace(elevMatch.Value, newPattern);

                            // rename the schedule
                            curSched.Name = newName;
                        }
                    }

                    // commit the transaction
                    t1.Commit();
                }

                #endregion

                #region Add Missing Hyphens

                // get all the schedules again
                List<ViewSchedule> allCurSchedules = Utils.GetAllSchedules(curDoc);

                // create a list to hold schedule without the hyphen
                List<ViewSchedule> schedNeedsHyphen = new List<ViewSchedule>();

                // loop through each schedule
                foreach (ViewSchedule curSched in allCurSchedules)
                {

                    // check for old naming convention
                    string schedName = curSched.Name;
                    if (schedName.Contains("Elevation") && !schedName.Contains(" - "))
                    {
                        // add it to a list to rename
                        schedNeedsHyphen.Add(curSched);
                    }
                }

                // create a transaction to add missing hyphens
                using (Transaction t2 = new Transaction(curDoc, "Add Missing Hyphens"))
                {
                    // start the transaction
                    t2.Start();

                    // loop through the list and rename
                    foreach (ViewSchedule curSched in schedNeedsHyphen)
                    {
                        // add to the hashset
                        modifiedScheduleIds.Add(curSched.Id);

                        // get the exisitng name
                        string curName = curSched.Name;

                        // insert hyphen before "Elevation"
                        string newName = curName.Replace("Elevation ", "- Elevation ");

                        // rename the schedule
                        curSched.Name = newName;
                    }

                    // commit the transaction
                    t2.Commit();
                }

                // notify the user of completion
                Utils.TaskDialogInformation("Success", "Rename Schedules", $"Renamed {modifiedScheduleIds.Count} schedules.");

                #endregion

                #region Add Elevation Designation 

                // get all the schedules in the project
                List<ViewSchedule> allNewSchedules = Utils.GetAllSchedules(curDoc);

                // create a variable for parameter name            
                string paramName = "Elevation Designation";

                // access shared parameter file
                DefinitionFile defFile = curDoc.Application.OpenSharedParameterFile();

                // null check
                if (defFile == null)
                {
                    Utils.TaskDialogError("Error", "Browser Organization", "No shared parameter file found");
                    return Result.Failed;
                }

                // create a variable to hold parameter definition
                Definition paramDef = null;

                // loop through the groups in the shared parameter file
                foreach (DefinitionGroup group in defFile.Groups)
                {
                    // find the group we want
                    Definition curDef = group.Definitions.get_Item(paramName);
                    if (curDef != null)
                    {
                        paramDef = curDef;
                        break;
                    }
                }

                // null check
                if (paramDef == null)
                {
                    // notify user parameter not found
                    Utils.TaskDialogError("Error", "Browser Organization", $"Parameter '{paramName}' not found");
                    return Result.Failed;
                }

                // check if the parameter exists in the project
                bool paramAdded = Utils.ParamAddedToFile(curDoc, paramName);

                // if not, create a transaction to add it
                if (!paramAdded)
                {
                    using (Transaction t3 = new Transaction(curDoc, "Add Shared Parameter"))
                    {
                        // start the transaction
                        t3.Start();

                        // assign the parameter to the Schedules category
                        // 
                        // create category set for Schedules
                        CategorySet catSet = curDoc.Application.Create.NewCategorySet();
                        catSet.Insert(curDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Schedules));

                        // Create instance binding with Identity Data parameter group
                        InstanceBinding binding = curDoc.Application.Create.NewInstanceBinding(catSet);

                        // Insert the binding
                        curDoc.ParameterBindings.Insert(paramDef, binding, GroupTypeId.IdentityData);

                        // commit the transaction
                        t3.Commit();
                    }

                    // inform the user that the parameter was added
                    Utils.TaskDialogInformation("Success", "Browser Organization", $"Added '{paramName}' to the project & assigned it to Schedules.");
                }
                else
                {
                    // inform the user that the parameter already exists
                    Utils.TaskDialogInformation("Info", "Browser Organization", $"The parameter '{paramName}' already exists in the project.");
                }

                #endregion

                #region Set Parameter Values

                // start a transaction to update the parameter values
                using (Transaction t4 = new Transaction(curDoc, "Update Parameter Value"))
                {
                    // start the transaction
                    t4.Start();

                    // get current parameter binding
                    BindingMap bindingMap = curDoc.ParameterBindings;
                    ElementBinding curBinding = bindingMap.get_Item(paramDef) as ElementBinding;

                    // edit the binding to include schedules
                    if (curBinding != null)
                    {
                        // check if schedules are already included
                        CategorySet catSet = curBinding.Categories;
                        Category catSchedule = curDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Schedules);

                        // if Schedules not included...
                        if (!catSet.Contains(catSchedule))
                        {
                            // add Schedules to the current category set
                            catSet.Insert(catSchedule);

                            // create new binding with updated category set
                            InstanceBinding newBinding = curDoc.Application.Create.NewInstanceBinding(catSet);

                            // replace existing binding with new binding
                            curDoc.ParameterBindings.ReInsert(paramDef, newBinding, GroupTypeId.IdentityData);
                        }
                    }

                    // loop through all the schedules & set the parameter value
                    foreach (ViewSchedule curSchedule in allNewSchedules)
                    {
                        if (curSchedule == null) continue;

                        string paramValue = "Shared"; // fallback value

                        if (curSchedule.Name.Contains("Elevation"))
                        {
                            string[] partsElev = curSchedule.Name.Split('-');

                            if (partsElev.Length > 1)
                            {
                                string partsName = partsElev[1].Trim();

                                // Normalize spacing
                                string cleanName = Regex.Replace(partsName, @"\s+", " ").Trim();

                                // Extract pattern like "Elevation C"
                                Match match = Regex.Match(cleanName, @"^Elevation [A-Z]$", RegexOptions.IgnoreCase);

                                if (match.Success)
                                {
                                    paramValue = match.Value;
                                }
                            }
                        }

                        Parameter param = curSchedule.LookupParameter(paramName);
                        if (param != null && !param.IsReadOnly)
                        {
                            param.Set(paramValue);
                        }
                    }


                    // commit the transaction
                    t4.Commit();
                }

                #endregion

                #region Transfer Project Standards - Browser Organization

                // set the path to the view template file
                string templateDoc = "S:\\Shared Folders\\Lifestyle USA Design\\Library 2025\\Template\\Project Standards.rvt";

                // create a variable for the source document
                Document sourceDoc = uidoc.Application.Application.OpenDocumentFile(templateDoc);

                // create a variable for the target document                  
                Document targetDoc = uidoc.Document;

                // get the Browser Organizations from the source document
                BrowserOrganization schedBrowserOrg = GetScheduleBrowserOrg(sourceDoc, "elevation");

                // null check
                if (schedBrowserOrg == null)
                {
                    Utils.TaskDialogError("Error", "Template Issue", "Browser organization 'elevation' not found in template file.");
                    return Result.Failed;
                }

                // check if it already exists in the target document
                BrowserOrganization targetBrowserOrg = GetScheduleBrowserOrg(targetDoc, "elevation");

                try
                {
                    // start a transaction to transfer the browser organization
                    using (Transaction t5 = new Transaction(curDoc, "Transfer Browser Organization"))
                    {
                        // start the transaction
                        t5.Start();

                        if (targetBrowserOrg == null)
                        {
                            ElementId newOrgId = ImportBrowserOrg(sourceDoc, targetDoc, schedBrowserOrg);
                        }

                        // commit the transaction
                        t5.Commit();
                    }
                }
                catch (Exception)
                {
                    // inform the user of any errors during the transfer
                    Utils.TaskDialogError("Error", "Browser Organization", "Failed to transfer Browser Organization.");
                    return Result.Failed;
                }
                finally
                {
                    // Close the source document when done
                    if (sourceDoc != null && !sourceDoc.IsFamilyDocument)
                    {
                        sourceDoc.Close(false); // false = don't save
                    }
                }

                #endregion

                // assimilate the transaction group
                tgroup.Assimilate();
            }

            // notify the user
            Utils.TaskDialogInformation("Success", "Update Schedules", "The value of the Elevation Designation parameter has been updated for all schedules. " +
                "The 'elevation' Browser Organization for Schedules/Quantities has been added. Please aplly that organization to the Project Browser.");

            return Result.Succeeded;
        }

        private ElementId ImportBrowserOrg(Document sourceDoc, Document targetDoc, BrowserOrganization schedBrowserOrg)
        {
            CopyPasteOptions copyPasteOptions = new CopyPasteOptions();
            ElementId sourceOrgId = schedBrowserOrg.Id;
            List<ElementId> elementIds = new List<ElementId>();
            elementIds.Add(schedBrowserOrg.Id);
            ElementTransformUtils.CopyElements(sourceDoc, elementIds, targetDoc, Autodesk.Revit.DB.Transform.Identity, copyPasteOptions);
            return schedBrowserOrg.Id;
        }

        private BrowserOrganization GetScheduleBrowserOrg(Document sourceDoc, string orgName)
        {
            List<BrowserOrganization> m_browserOrgs = new FilteredElementCollector(sourceDoc)
                .OfClass(typeof(BrowserOrganization))
                .Cast<BrowserOrganization>()
                .ToList();

            foreach (BrowserOrganization curOrg in m_browserOrgs)
            {
                if (curOrg.Name.Equals(orgName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return curOrg;
                }
            }

            return null;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand2";
            string buttonTitle = "Button 2";

            clsButtonData myButtonData = new clsButtonData(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.UpdateSchedules_32,
                Properties.Resources.UpdateSchedules_16,
                "This is a tooltip for Button 2");

            return myButtonData.Data;
        }
    }
}

