#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;

#endregion

namespace LifestyleDesign
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            
            // create the ribbon tab

            try
            {
                app.CreateRibbonTab("Lifestyle Design");
            }
            catch (Exception)
            {
                Debug.Print("Tab already exists");
            }

            // create ribbon panel

            RibbonPanel panel1 = Utils.CreateRibbonPanel(app, "Lifestyle Design", "Mirror Plan");
            RibbonPanel panel2 = Utils.CreateRibbonPanel(app, "Lifestyle Design", "Project Management");
            RibbonPanel panel3 = Utils.CreateRibbonPanel(app, "Lifestyle Design", "Project Setup");

            // create button data instances

            ButtonDataClass btn1 = new ButtonDataClass("Tool1", "Mirror\rProject", "LifestyleDesign.cmdRevitMirror",
               LifestyleDesign.Properties.Resources.MirrorProject_32,
               LifestyleDesign.Properties.Resources.MirrorProject_16, "Mirrors project on specified axis");

            ButtonDataClass btn2 = new ButtonDataClass("Tool2", "Reverse\rSwings", "LifestyleDesign.cmdReverseDoorSwings",
               LifestyleDesign.Properties.Resources.ReverseSwings_32,
               LifestyleDesign.Properties.Resources.ReverseSwings_16, "Reverses all door swings");

            ButtonDataClass btn3 = new ButtonDataClass("Tool3", "Rename\rElevations", "LifestyleDesign.cmdElevationRename",
               LifestyleDesign.Properties.Resources.ElevationRename_32,
               LifestyleDesign.Properties.Resources.ElevationRename_16, "Renames Left & Right Elevations for Mirrored Projects");

            ButtonDataClass btn4 = new ButtonDataClass("Tool4", "Swap\rSheets", "LifestyleDesign.cmdElevationSheetSwap",
               LifestyleDesign.Properties.Resources.SheetSwap_32,
               LifestyleDesign.Properties.Resources.SheetSwap_16, "Swaps the Left & Right Elevation sheets for Mirrored Projects");

            ButtonDataClass btn5 = new ButtonDataClass("Tool6", "Delete\rRevisions", "LifestyleDesign.cmdDeleteRevisions",
               LifestyleDesign.Properties.Resources.DeleteRevisions_32,
               LifestyleDesign.Properties.Resources.DeleteRevisions_16, "Deletes revisions from project");

            ButtonDataClass btn6 = new ButtonDataClass("Tool7", "To Do\rManager", "LifestyleDesign.cmdToDoManager",
                LifestyleDesign.Properties.Resources.ToDo_32,
                LifestyleDesign.Properties.Resources.ToDo_16, "Launches To Do Manager");

            // create buttons

            panel1.AddItem(btn1.Data);
            panel1.AddItem(btn2.Data);
            panel1.AddItem(btn3.Data);
            panel1.AddItem(btn4.Data);

            panel2.AddItem(btn5.Data);
            panel2.AddItem(btn6.Data);


            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }    
}
