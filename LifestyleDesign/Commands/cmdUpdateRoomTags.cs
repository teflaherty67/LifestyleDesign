using Autodesk.Revit.DB.Architecture;
using LifestyleDesign.Classes;
using LifestyleDesign.Common;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdUpdateRoomTags : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // create a variable to hold parameter name
            string paramName = "Ceiling Height";

            // create a transaction group
            using (TransactionGroup tg = new TransactionGroup(curDoc, "Update Room Tags"))
            {
                tg.Start();

                #region Add Ceiling Height Parameter

                // create a transaction
                using (Transaction t1 = new Transaction(curDoc, "Add Clg Height Parameter"))
                {
                    // load prameter into project & assign to Rooms
                    t1.Start();

                    // Access shared parameter file
                    DefinitionFile defFile = curDoc.Application.OpenSharedParameterFile();

                    // null check
                    if (defFile == null)
                    {
                        Utils.TaskDialogError("Error", "Update Room Tags", "No shared parameter file found");
                        t1.RollBack();
                        return Result.Failed;
                    }

                    // Find the parameter definition
                    Definition paramDef = null;
                    foreach (DefinitionGroup group in defFile.Groups)
                    {
                        Definition def = group.Definitions.get_Item(paramName);
                        if (def != null)
                        {
                            paramDef = def;
                            break;
                        }
                    }

                    // null check
                    if (paramDef == null)
                    {
                        // notify user parameter not found
                        Utils.TaskDialogError("Error", "Update Room Tags", $"Parameter '{paramName}' not found");
                        t1.RollBack();
                        return Result.Failed;
                    }

                    // Create category set for Rooms
                    CategorySet catSet = curDoc.Application.Create.NewCategorySet();
                    catSet.Insert(curDoc.Settings.Categories.get_Item(BuiltInCategory.OST_Rooms));

                    // Create instance binding with Identity Data parameter group
                    InstanceBinding binding = curDoc.Application.Create.NewInstanceBinding(catSet);

                    // Insert the binding
                    curDoc.ParameterBindings.Insert(paramDef, binding, GroupTypeId.IdentityData);

                    t1.Commit();
                }

                // notify user
                Utils.TaskDialogInformation("Info", "Update Room Tags", $"Parameter '{paramName}' added to project and assigned to Rooms");

                #endregion

                #region Setup Parameter Values

                // start a transaction
                using (Transaction t2 = new Transaction(curDoc, "Transfer Parameter Values"))
                {
                    // start transaction
                    t2.Start();

                    // collect all rooms in the project
                    List<Room> allRooms = Utils.GetAllRooms(curDoc);

                    // loop through each room & transfer parameter values
                    foreach (Room curRoom in allRooms)
                    {
                        // get Ceiling Height & Ceiling Finish parameters
                        Parameter paramClgHeight = curRoom.LookupParameter("Ceiling Height");
                        Parameter paramClgFinish = curRoom.LookupParameter("Ceiling Finish");

                        // set Ceiling Height value to Ceiling Finish value
                        if (paramClgFinish != null && paramClgHeight != null && paramClgFinish.HasValue)
                        {
                            string finishValue = paramClgFinish.AsString();
                            if (!string.IsNullOrEmpty(finishValue))
                            {
                                paramClgHeight.Set(finishValue);
                            }
                        }
                    }

                    // commit transaction
                    t2.Commit();
                }

                // notify user
                Utils.TaskDialogInformation("Information", "Update Room Tags", $"Parameter '{paramName}' values set for all rooms");

                #endregion

                #region Update Room Tags

                // create a transaction
                using (Transaction t3 = new Transaction(curDoc, "Update Room Tags"))
                {
                    // start transaction
                    t3.Start();

                    // load the new tag type
                    Utils.LoadFamilyFromLibrary(curDoc, $@"S:\Shared Folders\Lifestyle USA Design\Library 2025\Annotation\Tags", "LD_AN_Tag_Room");

                    // verify tag family loaded successfully
                    Family loadedFamily = new FilteredElementCollector(curDoc)
                        .OfClass(typeof(Family))
                        .Cast<Family>()
                        .FirstOrDefault(f => f.Name == "LD_AN_Tag_Room");

                    if (loadedFamily == null)
                    {
                        Utils.TaskDialogError("Error", "Update Room Tags", "Failed to load new room tag family");
                        t3.RollBack();
                        return Result.Failed;
                    }

                    // get all room tags in the project
                    List<RoomTag> allRoomTags = Utils.GetAllRoomTags(curDoc);

                    // loop through each tag & update to new type
                    foreach (RoomTag curRoomTag in allRoomTags)
                    {
                        // determine view type
                        View tagView = curDoc.GetElement(curRoomTag.OwnerViewId) as View;

                        // determine if view is floor plan
                        bool isFloorPlan = tagView != null && tagView.Name.Contains("Annotation");

                        // determine if tag is single or double line
                        string curTagTypeName = curRoomTag.RoomTagType.Name;
                        bool isSingleLine = curRoomTag.RoomTagType.Name.Contains("single");

                        // map old tag type to new tag based on view type
                        string newTagTypeName = "";
                        if (isFloorPlan)
                        {
                            newTagTypeName = isSingleLine ?
                                "Small - Name, Ceiling Ht, Floor Finish - single" :
                                "Small - Name, Ceiling Ht, Floor Finish - double";
                        }
                        else // Presentation or Section views
                        {
                            newTagTypeName = isSingleLine ?
                                "Small - Name, single" :
                                "Small - Name, double";
                        }

                        // find new tag type
                        FamilySymbol newTagType = Utils.FindFamilySymbol(curDoc, "LD_AN_Tag_Room", newTagTypeName);

                        // null check
                        if (newTagType == null)
                        {
                            Utils.TaskDialogError("Error", "Update Room Tags", $"New tag type '{newTagTypeName}' not found");
                            t3.RollBack();
                            return Result.Failed;
                        }

                        // activate new tag type
                        if (!newTagType.IsActive)
                        {
                            newTagType.Activate();
                            curDoc.Regenerate();
                        }

                        // update tag type
                        curRoomTag.ChangeTypeId(newTagType.Id);
                    }

                    // commit transaction
                    t3.Commit();
                }

                // notify user
                Utils.TaskDialogInformation("Information", "Update Room Tags", "All room tags updated to new LD_AN_Tag_Room family");

                #endregion

                tg.Assimilate();
            }

            return Result.Succeeded;
        }

        internal static PushButtonData GetButtonData()
        {
            string buttonInternalName = "btnCmd2_5";
            string buttonTitle = "Update\r Room Tags";
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
                    Properties.Resources.UpdateRoomTags_32,
                    Properties.Resources.UpdateRoomTags_16,
                    "Updates view templates to new standards");

                return myBtnData1.Data;
            }
        }
    }
}
