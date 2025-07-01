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

            string folderPath = "";            
            string searchPath1 = @"S:\Shared Folders\-Job Log-\01-Current Jobs";
            string searchPath2 = @"S:\Shared Folders\-Job log-\02-Completed Jobs";

            frmJobNumber curForm1 = new frmJobNumber();

            curForm1.Width = 375;
            curForm1.Height = 150;

            curForm1.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            curForm1.Topmost = true;

            curForm1.ShowDialog();

            string[] directory1 = Directory.GetDirectories(searchPath1);
            string[] directory2 = Directory.GetDirectories(searchPath2);

            // search path 1

            foreach (string dir in directory1)
            {
                if (dir.Contains(GlobalVars.JobNumber))
                    folderPath = dir;
            }

            // search path 2

            if (folderPath == "")
            {
                foreach (string dir in directory2)
                {
                    if (dir.Contains(GlobalVars.JobNumber))
                        folderPath = dir;
                }
            }

            if (folderPath == "")
            {
                // add task dialog to warn user job doesn't exist
                // Show final report
                TaskDialog tdJobNumber = new TaskDialog("Job Number");
                tdJobNumber.MainIcon = Icon.TaskDialogIconInformation;
                tdJobNumber.Title = "Folder Not Found";
                tdJobNumber.TitleAutoPrefix = false;
                tdJobNumber.MainContent = $"No job folder found for job number {GlobalVars.JobNumber} in the Job Log. Contact a project manager to verify the job number and try again.";
                tdJobNumber.CommonButtons = TaskDialogCommonButtons.Close;

                TaskDialogResult tdSchedSuccessRes = tdJobNumber.Show();

                return Result.Failed;
            }

            frmRevisionJournal curForm = new frmRevisionJournal(curDoc.PathName, folderPath);

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
