using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using FilterTreeControlWPF;
using LifestyleDesign.Common;
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

            #region Panel Creation

            // create ribbon panel
            RibbonPanel panel1 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Project Standards");
            RibbonPanel panel2 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Project Management");
            RibbonPanel panel3 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Project Setup");
            RibbonPanel panel4 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Mirror Plans");            
            RibbonPanel panel5 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Selection");
            RibbonPanel panel6 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Modify");            
            RibbonPanel panel7 = Common.Utils.CreateRibbonPanel(app, "Lifestyle Design", "Support Tools");

            #endregion

            #region Button Creation

            // create button data instances for Panel 1
            PushButtonData btnData1_3 = cmdUpdateRoomTags.GetButtonData();
            PushButtonData btnData1_4 = cmdUpdateSchedules.GetButtonData();
            PushButtonData btnData1_5 = cmdUpdateVTs.GetButtonData();

            // create button data instances for Panel 2
            PushButtonData btnData2_1 = cmdDeleteRevisions.GetButtonData();
            PushButtonData btnData2_2 = cmdRevisionJournal.GetButtonData();
            PushButtonData btnData2_3 = cmdStripIt.GetButtonData();

            // create button data instances for Panel 3
            PushButtonData btnData3_1 = cmdElevDesignation.GetButtonData();
            PushButtonData btnData3_2 = cmdCreateSchedules.GetButtonData();
            PushButtonData btnData7_1 = cmdCreateSheetGroup.GetButtonData();
            PulldownButtonData btnPullDn7_2 = new PulldownButtonData("btnPullDn7_2", "Sheet\rTools");
            PushButtonData btnData7_2a = cmdSelectSheets.GetButtonData();
            PushButtonData btnData7_2b = cmdIncrementSheets.GetButtonData();
            PushButtonData btnData7_2c = cmdDecrementSheets.GetButtonData();
            PushButtonData btnData7_2d = cmdDupSheetsToNewElev.GetButtonData();

            // create button data instances for Panel 4
            PushButtonData btnData4_1 = cmdRevitMirror.GetButtonData();            
            PushButtonData btnData4_2 = cmdFlipPlan.GetButtonData();            

            // create button data instances for panel 5
           

            // create button data instances for Panel 7
            PushButtonData btnData8_1 = cmdReportBugs.GetButtonData();

            #endregion

            #region Add Buttons to Panels

            // create buttons for panel 2
            PushButton myBtn2_1 = panel2.AddItem(btnData2_1) as PushButton;
            PushButton myBtn2_2 = panel2.AddItem(btnData2_2) as PushButton;
            PushButton myBtn2_3 = panel2.AddItem(btnData2_3) as PushButton;
            
            // create buttons for panel 4
            PushButton myBtn4_1 = panel4.AddItem(btnData4_1) as PushButton;            
            PushButton myBtn4_2 = panel4.AddItem(btnData4_2) as PushButton;            

            // create buttons for panel 3
            PushButton myBtn3_1 = panel3.AddItem(btnData3_1) as PushButton;
            PushButton myBtn3_2 = panel3.AddItem(btnData3_2) as PushButton;

            // create buttons for panel 7
            PushButton myBtn7_1 = panel7.AddItem(btnData7_1) as PushButton;
            PulldownButton myPulldn7_2 = panel7.AddItem(btnPullDn7_2) as PulldownButton;
            PushButton myBtn7_2a = myPulldn7_2.AddPushButton(btnData7_2a) as PushButton;
            PushButton myBtn7_2b = myPulldn7_2.AddPushButton(btnData7_2b) as PushButton;
            PushButton myBtn7_2c = myPulldn7_2.AddPushButton(btnData7_2c) as PushButton;
            PushButton myBtn7_2d = myPulldn7_2.AddPushButton(btnData7_2d) as PushButton;

            // create buttons for panel 8
            PushButton myBtn8_1 = panel7.AddItem(btnData8_1) as PushButton;

            #endregion

            #region Assign Images to Buttons

            // assign images to pulldown buttons
            myPulldn7_2.LargeImage = Utils.GetEmbeddedImage("LifestyleDesign.Resources.SheetTools_32.png");
            myPulldn7_2.Image = Utils.GetEmbeddedImage("LifestyleDesign.Resources.SheetTools_16.png");

            #endregion

            #region Application Event Handlers

            app.ControlledApplication.DocumentOpened += OnDocumentOpened;
            app.ControlledApplication.DocumentSaving += OnDocumentSaving;
            //app.ControlledApplication.DocumentClosing += OnDocumentClosing;

            #endregion


            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            a.ControlledApplication.DocumentOpened -= OnDocumentOpened;
            return Result.Succeeded;
        }

        private void OnDocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            // null check
            if (e.Document == null) return;

            Document curDoc = e.Document;           

            // check if document should be skipped (Family Document, Project Standards)
            if (ShouldSkipDocument(curDoc)) return;

            // check all standards
            AppUtils.CheckAllStandards(curDoc);
        }

        private void OnDocumentSaving(object sender, DocumentSavingEventArgs e)
        {
            if (e.Document == null) return;

            Document curDoc = e.Document;

            AppUtils.OnDocumentSaving(curDoc);
        }

        private void OnDocumentClosing(object sender, DocumentSavingEventArgs e)
        {
            if (e.Document == null) return;

            Document curDoc = e.Document;

            AppUtils.OnDocumentClosing(curDoc);
        }

        private bool ShouldSkipDocument(Document curDoc)
        {
            return curDoc.IsFamilyDocument || curDoc.PathName.Contains("Project Standards");
        }
    }
}
