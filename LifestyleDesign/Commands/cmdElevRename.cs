using LifestyleDesign.Common;

namespace LifestyleDesign.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class cmdElevRename : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // get all the elevation views

            List<View> viewList = Utils.GetAllElevationViews(doc);

            List<View> renamedList = new List<View>();

            int counter = 0;

            // start the transaction

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Rename Elevations");

                // loop through the view collector

                foreach (View curView in viewList)
                {
                    Parameter titleParam = null;

                    string curTitle = "";

                    foreach (Parameter curParam in curView.Parameters)
                    {
                        if (curParam.Definition.Name == "Title on Sheet")
                        {
                            titleParam = curParam;

                            curTitle = curParam.AsString();
                        }
                    }

                    // change view name

                    if (curView.Name.Contains("Left") == true)
                    {
                        curView.Name = curView.Name.Replace("Left", "$Right");
                        renamedList.Add(curView);
                    }

                    else if (curView.Name.Contains("Right") == true)
                    {
                        curView.Name = curView.Name.Replace("Right", "$Left");
                        renamedList.Add(curView);
                    }

                    // change the title on sheet

                    if (curTitle.Contains("Left"))
                        titleParam.Set(curTitle.Replace("Left", "Right"));

                    else if (curTitle.Contains("Right"))
                        titleParam.Set(curTitle.Replace("Right", "Left"));
                }

                foreach (View curView in renamedList)
                {
                    // remove $ from view name

                    if (curView.Name.Contains("$Right") == true)
                        curView.Name = curView.Name.Replace("$Right", "Right");

                    else if (curView.Name.Contains("$Left") == true)
                        curView.Name = curView.Name.Replace("$Left", "Left");

                    counter++;
                }

                // alert the user

                string msgText = "Renamed " + counter.ToString() + " views.";
                string msgTitle = "Complete";
                Forms.MessageBoxButton msgButtons = Forms.MessageBoxButton.OK;

                Forms.MessageBox.Show(msgText, msgTitle, msgButtons, Forms.MessageBoxImage.Information);

                // commit the changes

                t.Commit();

                return Result.Succeeded;
            }
        }
        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            Classes.clsButtonData myButtonData1 = new Classes.clsButtonData(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
