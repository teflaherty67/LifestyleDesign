#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;

#endregion

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdReverseDoorSwings : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // get all the doors in the project & create lists by swing

            FilteredElementCollector colDoors = new FilteredElementCollector(doc);
            colDoors.OfCategory(BuiltInCategory.OST_Doors);
            colDoors.WhereElementIsNotElementType();

            List<FamilyInstance> leftSwing = new List<FamilyInstance>();
            List<FamilyInstance> rightSwing = new List<FamilyInstance>();

            // code snippetts; not sure where these go

            foreach (FamilyInstance door in colDoors)
            {
                string lSwing = Utils.GetParameterValueByName(door, "Swing Left");
                string rSwing = Utils.GetParameterValueByName(door, "Swing Right");

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
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Reverse Door Swings");

                foreach (FamilyInstance curDoor in leftSwing)
                {
                    // set Swing Left value to no
                    Utils.SetParameterByName(curDoor, "Swing Left", "No");

                    // set Swing Right value to yes
                    Utils.SetParameterByName(curDoor, "Swing Right", "Yes");
                }

                foreach (FamilyInstance curDoor in rightSwing)
                {
                    // set Swing Right value to no
                    Utils.SetParameterByName(curDoor, "Swing Right", "No");

                    // set Swing Left value to yes
                    Utils.SetParameterByName(curDoor, "Swing Left", "Yes");
                }

                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}