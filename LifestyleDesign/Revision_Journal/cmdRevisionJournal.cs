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

            // clear any job number left over from an earlier run
            GlobalVars.JobNumber = "";

            // exit if the user cancelled or closed the form
            if (curForm1.ShowDialog() != true)
                return Result.Cancelled;

            // a blank job number would match every folder
            if (string.IsNullOrWhiteSpace(GlobalVars.JobNumber))
            {
                Utils.TaskDialogWarning("Job Number", "Job Number Required", "Please enter a job number and try again.");
                return Result.Cancelled;
            }

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
                // warn the user the job folder doesn't exist
                Utils.TaskDialogInformation("Job Number", "Folder Not Found",
                    $"No job folder found for job number {GlobalVars.JobNumber} in the Job Log. Contact a project manager to verify the job number and try again.");

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
