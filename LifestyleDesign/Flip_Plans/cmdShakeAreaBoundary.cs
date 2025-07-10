using LifestyleDesign.Classes;
using LifestyleDesign.Common;

namespace LifestyleDesign
{
    [Transaction(TransactionMode.Manual)]
    public class cmdShakeAreaBoundary : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Revit application and document variables
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document curDoc = uidoc.Document;

            // get all the area plans in the project
            List<View> areaViews = new List<View>();
            List<View> areaFloor = Utils.GetAllViewsByCategory(curDoc, "10:Floor Areas");
            List<View> areaFrame = Utils.GetAllViewsByCategory(curDoc, "11:Frame Areas");
            List<View> areaAttic = Utils.GetAllViewsByCategory(curDoc, "12:Attic Areas");

            areaViews.AddRange(areaFloor);
            areaViews.AddRange(areaFrame);
            areaViews.AddRange(areaAttic);

            // start the transaction
            using (Transaction t = new Transaction(curDoc))
            {
                t.Start("Shake Area Boundary Lines");
                {
                    // loop through each view in the list
                    foreach (View curView in areaViews)
                    {
                        // Skip if view is null, is a template, or cannot display elements
                        if (curView == null || curView.IsTemplate || !IsViewValidForCollection(curView))
                            continue;

                        try
                        {
                            // get all area boundary lines in the active view
                            IEnumerable<Element> colABLines = new FilteredElementCollector(curDoc, curView.Id)
                                .OfCategory(BuiltInCategory.OST_AreaSchemeLines)
                                .WhereElementIsNotElementType();

                            // get all the area tags
                            FilteredElementCollector colAreaTags = new FilteredElementCollector(curDoc, curView.Id)
                                .OfClass(typeof(SpatialElementTag));

                            foreach (SpatialElementTag curTag in colAreaTags)
                            {
                                curTag.HasLeader = false;
                            }

                            if (colABLines.Count() > 0)
                            {
                                // get the first line in the list
                                Element lineToMove = colABLines.FirstOrDefault();

                                // get the location of the line
                                LocationCurve curLocation = lineToMove.Location as LocationCurve;

                                if (curLocation != null)
                                {
                                    // create a vector to move the line
                                    XYZ curPoint = curLocation.Curve.GetEndPoint(0) as XYZ;
                                    XYZ newVector = new XYZ(.25, 0, 0);
                                    XYZ oldVector = new XYZ(-.25, 0, 0);

                                    // move the line to the left
                                    lineToMove.Location.Move(newVector);
                                    // move the line back to the right                
                                    lineToMove.Location.Move(oldVector);
                                }
                            }
                        }
                        catch (Autodesk.Revit.Exceptions.ArgumentException)
                        {
                            // Skip views that can't be used for element collection
                            continue;
                        }
                    }
                }
                t.Commit();
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Check if a view is valid for element collection
        /// </summary>
        /// <param name="view">The view to check</param>
        /// <returns>True if view can be used for element collection</returns>
        private bool IsViewValidForCollection(View view)
        {
            // Check if view can display elements (not a schedule, legend, etc.)
            if (view.ViewType == ViewType.Schedule ||
                view.ViewType == ViewType.Legend ||
                view.ViewType == ViewType.DrawingSheet ||
                view.ViewType == ViewType.Report)
                return false;

            // Check if view is not a template
            if (view.IsTemplate)
                return false;

            return true;
        }
    }
}