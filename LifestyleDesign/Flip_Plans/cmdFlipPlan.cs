﻿using LifestyleDesign.Classes;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdFlipPlan : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // run door swing reversal
            cmdReverseDoorSwings com_1 = new cmdReverseDoorSwings();
            com_1.Execute(commandData, ref message, elements);

            // run elevation rename
            cmdElevRename com_2 = new cmdElevRename();
            com_2.Execute(commandData, ref message, elements);

            // run sheet swap
            cmdElevSheetSwap com_3 = new cmdElevSheetSwap();
            com_3.Execute(commandData, ref message, elements);

            // run boundary shake
            cmdShakeAreaBoundary com_4 = new cmdShakeAreaBoundary();
            com_4.Execute(commandData, ref message, elements);

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            clsButtonData myButtonData = new clsButtonData(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData.Data;
        }
    }

}