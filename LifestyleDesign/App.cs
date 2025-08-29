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
            RibbonPanel panel4 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Selection");
            RibbonPanel panel5 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Modify");
            RibbonPanel panel6 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Views");
            RibbonPanel panel7 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Sheets");
            RibbonPanel panel8 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Support Tools");

            // create button data instances for Panel 1
            PushButtonData btnData1_1 = cmdRevitMirror.GetButtonData();            
            PushButtonData btnData1_2 = cmdFlipPlan.GetButtonData();

            // create button data instances for Panel 2
            PushButtonData btnData2_1 = cmdDeleteRevisions.GetButtonData();
            PushButtonData btnData2_2 = cmdRevisionJournal.GetButtonData();
            PushButtonData btnData2_3 = cmdStripIt.GetButtonData();
            PushButtonData btnData2_4 = cmdUpdateVTs.GetButtonData();
            PushButtonData btnData2_5 = cmdUpdateRoomTags.GetButtonData();

            // create button data instances for Panel 3
            PushButtonData btnData3_1 = cmdElevDesignation.GetButtonData();
            PushButtonData btnData3_2 = cmdCreateSchedules.GetButtonData();

            // create button data instances for panel 7
            PushButtonData btnData7_1 = cmdCreateSheetGroup.GetButtonData();

            // create button data instances for Panel 8
            PushButtonData btnData8_1 = cmdReportBugs.GetButtonData();

            // create buttons for panel 1
            PushButton myBtn1_1 = panel1.AddItem(btnData1_1) as PushButton;            
            PushButton myBtn1_2 = panel1.AddItem(btnData1_2) as PushButton;

            // create buttons for panel 2
            PushButton myBtn2_1 = panel2.AddItem(btnData2_1) as PushButton;
            PushButton myBtn2_2 = panel2.AddItem(btnData2_2) as PushButton;
            PushButton myBtn2_3 = panel2.AddItem(btnData2_3) as PushButton;
            PushButton myBtn2_4 = panel2.AddItem(btnData2_4) as PushButton;
            PushButton myBtn2_5 = panel2.AddItem(btnData2_5) as PushButton;

            // create buttons for panel 3
            PushButton myBtn3_1 = panel3.AddItem(btnData3_1) as PushButton;
            PushButton myBtn3_2 = panel3.AddItem(btnData3_2) as PushButton;

            // create buttons for panel 7
            PushButton myBtn7_1 = panel7.AddItem(btnData7_1) as PushButton;

            // create buttons for panel 8
            PushButton myBtn8_1 = panel8.AddItem(btnData8_1) as PushButton;
           
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}
