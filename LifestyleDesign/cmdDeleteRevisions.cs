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

            TaskDialog.Show("Results", "There are " + revisions.Count() + " revisions in the project.");

            // set the first revision as Issued

            revisions(0) (BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED).Set(0);

            // delete all the remaining revisions

            // uncheck the Issued box on the first revision

            return Result.Succeeded;
        }
    }
}
