using LifestyleDesign.Classes;
using System.Reflection;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdRevitMirror : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // get Revit Command Id for Mirror Project
            RevitCommandId commandId = RevitCommandId.LookupPostableCommandId(PostableCommand.MirrorProject);

            // run the command using PostCommand
            uiapp.PostCommand(commandId);

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCmd1_1";
            string buttonTitle = "Mirror\rProject";
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
                    Properties.Resources.MirrorProject_32,
                    Properties.Resources.MirrorProject_16,
                    "Mirrors project on specified axis");

                return myBtnData1.Data;
            }
        }
    }
}