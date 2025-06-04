using LifestyleDesign.Classes;
using LifestyleDesign.Common;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdRevisionJournal : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            frmJobNumber curForm1 = new frmJobNumber();

            curForm1.Width = 375;
            curForm1.Height = 150;

            curForm1.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            curForm1.Topmost = true;

            curForm1.ShowDialog();

            frmRevisionJournal curForm = new frmRevisionJournal(curDoc.PathName);

            curForm.Width = 700;
            curForm.Height = 500;

            curForm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            curForm.Topmost = true;

            curForm.ShowDialog();

            return Result.Succeeded;
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCmd2_2";
            string buttonTitle = "Revision\rJournal";
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
                    Properties.Resources.RevisionJournal_32,
                    Properties.Resources.RevisionJournal_16,
                    "Creates revision journal and saves it to job folder");

                return myBtnData1.Data;
            }
        }
    }
}
