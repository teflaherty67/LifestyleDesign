using LifestyleDesign.Classes;
using LifestyleDesign.Common;
using LifestyleDesign.Flip_Plans;

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

            // verify the exterior elevation sheets are named correctly
            List<ViewSheet> extrSheets = Utils.GetAllSheetsByNameContains(curDoc, "Exterior Elevations");

            if(extrSheets.Count == 0)
            {
                // if the sheets are named wrong, alert the user & exit
                TaskDialog tdNewSchedError = new TaskDialog("Error");
                tdNewSchedError.MainIcon = Icon.TaskDialogIconWarning;
                tdNewSchedError.Title = "Flip Plans";
                tdNewSchedError.TitleAutoPrefix = false;
                tdNewSchedError.MainContent = "The exterior elevation sheets do not exist, or do not follow the proper naming convention; " +
                    "please create, or correct the sheet names, and try again.";
                tdNewSchedError.CommonButtons = TaskDialogCommonButtons.Close;

                TaskDialogResult tdNewErrorRes = tdNewSchedError.Show();

                return Result.Cancelled;
            }               

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
            string buttonInternalName = "btnCmd1_2";
            string buttonTitle = "Flip\rPlan";
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
                    Properties.Resources.FlipPlan_32,
                    Properties.Resources.FlipPlan_16,
                    "Completes the flipping process after the project is mirrored.");

                return myBtnData1.Data;
            }
        }
    }

}
