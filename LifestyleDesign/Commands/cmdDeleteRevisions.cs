#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Forms = System.Windows;
#endregion

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdDeleteRevisions : IExternalCommand
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

            string msgText = "About to delete all Revisions.";
            string msgTitle = "Warning";
            Forms.MessageBoxButton msgButtons = Forms.MessageBoxButton.OKCancel;

            Forms.MessageBoxResult result = Forms.MessageBox.Show(msgText, msgTitle, msgButtons, Forms.MessageBoxImage.Warning);

            if (result == Forms.MessageBoxResult.OK)
            {
                // start the transaction

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Delete Revisions");

                    // add a blank revision

                    Revision newRevision = Revision.Create(doc);

                    // get all the revisions in the project

                    IList<ElementId> revisions = Revision.GetAllRevisionIds(doc);

                    // remove the last revision from the list

                    revisions.RemoveAt(revisions.Count - 1);

                    // delete the remaining revisions

                    doc.Delete(revisions);

                    t.Commit();

                    string msgText2 = "All Revisions have been deleted.";
                    string msgTitle2 = "Complete";
                    Forms.MessageBoxButton msgButtons2 = Forms.MessageBoxButton.OK;

                    Forms.MessageBox.Show(msgText2, msgTitle2, msgButtons2, Forms.MessageBoxImage.Information);
                }

                return Result.Succeeded;
            }

            else
            {
                // exit the command

                return Result.Failed;
            }
        }
    }
}
