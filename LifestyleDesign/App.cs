using FilterTreeControlWPF;

namespace LifestyleDesign
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            // create ribbon tab
            try
            {
                app.CreateRibbonTab("Lifestyle Design");
            }
            catch (Exception)
            {
                Debug.Print("Tab already exists");
            }

            // create ribbon panel 
            RibbonPanel panel1 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Mirror Plans");
            RibbonPanel panel2 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Project Management");
            RibbonPanel panel3 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Project Setup");

            // create button data instances for Panel 1
            PushButtonData btnData1_1 = cmdRevitMirror.GetButtonData();            
            PushButtonData btnData1_2 = cmdFlipPlan.GetButtonData();

            // create button data instances for Panel 2
            PushButtonData btnData2_1 = cmdDeleteRevisions.GetButtonData();

            // create buttons for panel 1
            PushButton myButton1_1 = panel1.AddItem(btnData1_1) as PushButton;            
            PushButton myButton2_1 = panel1.AddItem(btnData1_2) as PushButton;

            // create buttons for panel 2
            //PushButton myButton1_2 = panel2.AddItem(btnData1_2) as PushButton;


            // NOTE:
            // To create a new tool, copy lines 35 and 39 and rename the variables to "btnData3" and "myButton3". 
            // Change the name of the tool in the arguments of line 

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }

}
