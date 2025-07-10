using LifestyleDesign.Classes;
using LifestyleDesign.Common;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdReverseDoorSwings : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // set the active view to the Door Schedule
            View curView;
            curView = Utils.GetScheduleByNameContains(curDoc, "Door Schedule");

            uidoc.ActiveView = curView;

            string msgText = "About to reverse door swings.";
            string msgTitle = "Warning";
            Forms.MessageBoxButton msgButtons = Forms.MessageBoxButton.OKCancel;

            Forms.MessageBoxResult result = Forms.MessageBox.Show(msgText, msgTitle, msgButtons, Forms.MessageBoxImage.Warning);

            if (result == Forms.MessageBoxResult.OK)
            {
                // get all the doors in the project & create lists by swing
                FilteredElementCollector colDoors = new FilteredElementCollector(curDoc);
                colDoors.OfCategory(BuiltInCategory.OST_Doors);
                colDoors.WhereElementIsNotElementType();

                List<FamilyInstance> leftSwing = new List<FamilyInstance>();
                List<FamilyInstance> rightSwing = new List<FamilyInstance>();

                // loop through the doors & add to appropriate list
                foreach (FamilyInstance door in colDoors)
                {
                    string lSwing = Utils.GetParameterValueByName(door, "Swing Left");
                    string rSwing = Utils.GetParameterValueByName(door, "Swing Right");

                    if (lSwing == null || rSwing == null)
                    {
                        continue;
                    }

                    if (lSwing == "Yes")
                    {
                        leftSwing.Add(door);
                    }

                    else if (rSwing == "Yes")
                    {
                        rightSwing.Add(door);
                    }
                }

                // start the transaction
                using (Transaction t = new Transaction(curDoc))
                {
                    t.Start("Reverse Door Swings");

                    foreach (FamilyInstance curDoor in leftSwing)
                    {
                        // set Swing Left value to no
                        Utils.SetParameterByName(curDoor, "Swing Left", 0);

                        // set Swing Right value to yes
                        Utils.SetParameterByName(curDoor, "Swing Right", 1);
                    }

                    foreach (FamilyInstance curDoor in rightSwing)
                    {
                        // set Swing Right value to no
                        Utils.SetParameterByName(curDoor, "Swing Right", 0);

                        // set Swing Left value to yes
                        Utils.SetParameterByName(curDoor, "Swing Left", 1);
                    }

                    t.Commit();

                    string msgText2 = "Door swings reversed.";
                    string msgTitle2 = "Complete";
                    Forms.MessageBoxButton msgButtons2 = Forms.MessageBoxButton.OK;

                    Forms.MessageBox.Show(msgText2, msgTitle2, msgButtons2, Forms.MessageBoxImage.Information);
                }

                return Result.Succeeded;
            }
            else
            {
                // exit the command

                return Result.Failed;
            }
        }       
    }
}
