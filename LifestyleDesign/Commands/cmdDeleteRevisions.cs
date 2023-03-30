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

            // get all the revisions in the project

            IList<ElementId> revisions = Revision.GetAllRevisionIds(doc);

            // remove the first revision from the list

            revisions.RemoveAt(0);       

            // delete all the remaining revisions

            using(Transaction t = new Transaction(doc))
            {
                t.Start("Delete Revisions");

                doc.Delete(revisions);

                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}
