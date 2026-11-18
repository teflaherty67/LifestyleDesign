using LifestyleDesign.Classes;
using LifestyleDesign.Common;

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
            Document curDoc = uidoc.Document;

            // ask the user to confirm before deleting anything
            bool confirmed = Utils.TaskDialogConfirm("Delete Revisions", "Delete Revisions", "About to delete all Revisions.");

            if (confirmed)
            {
                // start the transaction
                using (Transaction t = new Transaction(curDoc))
                {
                    t.Start("Delete Revisions");

                    // add a blank revision
                    Revision newRevision = Revision.Create(curDoc);

                    // get all the revisions in the project
                    IList<ElementId> revisions = Revision.GetAllRevisionIds(curDoc);

                    // remove the last revision from the list
                    revisions.RemoveAt(revisions.Count - 1);

                    // delete the remaining revisions
                    curDoc.Delete(revisions);

                    t.Commit();
                }

                // notify the user
                Utils.TaskDialogInformation("Delete Revisions", "Delete Revisions", "All Revisions have been deleted.");

                return Result.Succeeded;
            }

            else
            {
                // exit the command
                return Result.Failed;
            }
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
