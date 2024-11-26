using LifestyleDesign.Classes;

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
    }
}
