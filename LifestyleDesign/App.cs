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
            string assemblyName = Utils.GetAssemblyName();

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

            // create button data instances

            ButtonClass data1 = new ButtonClass("Tool1", "Delete Revisions", "LifestyleDesign.cmdDeleteRevisions",
               LifestyleDesign.Properties.Resources.DeleteRevisions_32,
               LifestyleDesign.Properties.Resources.DeleteRevisions_16, "Deletes revisions in project");

            ButtonClass data2 = new ButtonClass("Tool2", "Mirror Project", "LifestyleDesign.cmdRevitMirror",
               LifestyleDesign.Properties.Resources.MirrorProject_32,
               LifestyleDesign.Properties.Resources.MirrorProject_16, "Mirrors project on specified axis");

            // create buttons

            panel1.AddItem(data1.Data);

            panel2.AddItem(data2.Data);


            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }

    public class ButtonClass
    {
        public PushButtonData Data { get; set; }

        public ButtonClass(string name, string text, string className, System.Drawing.Bitmap largeImage,
            System.Drawing.Bitmap smallImage, string toolTip)
        {
            Data = new PushButtonData(name, text, Utils.GetAssemblyName(), className);
            Data.ToolTip = toolTip;
            Data.LargeImage = Utils.BitmapToImageSource(largeImage);
            Data.Image = Utils.BitmapToImageSource(smallImage);
        }
    }
}
