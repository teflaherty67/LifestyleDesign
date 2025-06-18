using FilterTreeControlWPF;
using LifestyleDesign.Elevation_Designation;

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
            RibbonPanel panel4 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Support Tools");

            // create button data instances for Panel 1
            PushButtonData btnData1_1 = cmdRevitMirror.GetButtonData();            
            PushButtonData btnData1_2 = cmdFlipPlan.GetButtonData();

            // create button data instances for Panel 2
            PushButtonData btnData2_1 = cmdDeleteRevisions.GetButtonData();
            PushButtonData btnData2_2 = cmdRevisionJournal.GetButtonData();
            PushButtonData btnData2_3 = cmdStripIt.GetButtonData();

            // create button data instances for Panel 3
            PushButtonData btnData3_1 = cmdElevDesignation.GetButtonData();
            PushButtonData btnData3_2 = cmdCreateSchedules.GetButtonData();
            PushButtonData btnData3_3 = cmdCreateSheetGroup.GetButtonData();

            // create button data instances for Panel 4
            PushButtonData btnData4_1 = cmdReportBugs.GetButtonData();

            // create buttons for panel 1
            PushButton myBtn1_1 = panel1.AddItem(btnData1_1) as PushButton;            
            PushButton myBtn1_2 = panel1.AddItem(btnData1_2) as PushButton;

            // create buttons for panel 2
            PushButton myBtn2_1 = panel2.AddItem(btnData2_1) as PushButton;
            PushButton myBtn2_2 = panel2.AddItem(btnData2_2) as PushButton;
            PushButton myBtn2_3 = panel2.AddItem(btnData2_3) as PushButton;

            // create buttons for panel 3
            PushButton myBtn3_1 = panel3.AddItem(btnData3_1) as PushButton;
            PushButton myBtn3_2 = panel3.AddItem(btnData3_2) as PushButton;
            PushButton myBtn3_3 = panel3.AddItem(btnData3_3) as PushButton;

            // create buttons for panel 4
            PushButton myBtn4_1 = panel4.AddItem(btnData4_1) as PushButton;

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
