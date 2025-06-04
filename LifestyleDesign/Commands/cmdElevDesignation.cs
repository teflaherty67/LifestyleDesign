using LifestyleDesign.Classes;
using LifestyleDesign.Common;

namespace LifestyleDesign.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class cmdElevDesignation : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Your code goes here


            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCmd3_1";
            string buttonTitle = "Elevation\rDesignation";
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
                    Properties.Resources.ReplaceDesignation_32,
                    Properties.Resources.ReplaceDesignation_16,
                    "Replaces the existing elevaiton designation");

                return myBtnData1.Data;
            }
        }
    }

}
