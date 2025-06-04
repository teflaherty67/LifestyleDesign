using LifestyleDesign.Classes;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdDeleteRevisions : IExternalCommand
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
            string buttonInternalName = "btnCmd2_1";
            string buttonTitle = "Delete\rRevisions";
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
                    Properties.Resources.DeleteRevisions_32,
                    Properties.Resources.DeleteRevisions_16,
                    "Deletes all revisions from project");

                return myBtnData1.Data;
            }
        }
    }

}
