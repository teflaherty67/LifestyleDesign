using LifestyleDesign.Classes;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace LifestyleDesign.Common
{
    internal static class Utils
    {
        #region Area Plans

        internal static void CreateFloorAreaWithTag(Document curDoc, ViewPlan areaPlan, ref UV insPoint, ref XYZ tagInsert, clsAreaData areaInfo)
        {
            Area curArea = curDoc.Create.NewArea(areaPlan, insPoint);
            curArea.Number = areaInfo.Number;
            curArea.Name = areaInfo.Name;
            curArea.LookupParameter("Area Category").Set(areaInfo.Category);
            curArea.LookupParameter("Comments").Set(areaInfo.Comments);

            AreaTag tag = curDoc.Create.NewAreaTag(areaPlan, curArea, insPoint);
            tag.TagHeadPosition = tagInsert;
            tag.HasLeader = false;

            UV offset = new UV(0, 8);
            insPoint = insPoint.Subtract(offset);

            XYZ tagOffset = new XYZ(0, 8, 0);
            tagInsert = tagInsert.Subtract(tagOffset);

            if (areaInfo.Ratio != 99)
            {
                curArea.LookupParameter("150 Ratio").Set(areaInfo.Ratio);
            }
        }

        internal static ViewPlan GetAreaPlanByViewFamilyName(Document curDoc, string vftName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(curDoc);
            collector.OfClass(typeof(ViewPlan));

            foreach (ViewPlan curViewPlan in collector)
            {
                if (curViewPlan.ViewType == ViewType.AreaPlan)
                {
                    ViewFamilyType curVFT = curDoc.GetElement(curViewPlan.GetTypeId()) as ViewFamilyType;

                    if (curVFT.Name == vftName)
                        return curViewPlan;
                }
            }

            return null;
        }

        #endregion        

        #region Area Scheme

        internal static AreaScheme GetAreaSchemeByName(Document curDoc, string schemeName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(curDoc);
            collector.OfClass(typeof(AreaScheme));

            foreach (AreaScheme areaScheme in collector)
            {
                if (areaScheme.Name == schemeName)
                {
                    return areaScheme;
                }
            }

            return null;
        }

        #endregion

        #region Color Fill Scheme

        internal static ColorFillScheme GetColorFillSchemeByName(Document curDoc, string schemeName, AreaScheme areaScheme)
        {
            try
            {
                ColorFillScheme colorfill = new FilteredElementCollector(curDoc)
               .OfCategory(BuiltInCategory.OST_ColorFillSchema)
               .Cast<ColorFillScheme>()
               .Where(x => x.Name.Equals(schemeName) && x.AreaSchemeId.Equals(areaScheme.Id))
               .First();

                return colorfill;
            }
            catch
            {
                return null;
            }
        }

        internal static void AddColorLegend(View view, ColorFillScheme scheme)
        {
            ElementId areaCatId = new ElementId(BuiltInCategory.OST_Areas);
            ElementId curLegendId = view.GetColorFillSchemeId(areaCatId);

            if (curLegendId == ElementId.InvalidElementId)
                view.SetColorFillSchemeId(areaCatId, scheme.Id);

            ColorFillLegend.Create(view.Document, view.Id, areaCatId, XYZ.Zero);
        }

        #endregion

        #region Families
        public static List<Family> GetAllFamilies(Document curDoc)
        {
            List<Family> m_returnList = new List<Family>();

            FilteredElementCollector collector = new FilteredElementCollector(curDoc);
            collector.OfClass(typeof(Family));

            foreach (Family family in collector)
            {
                m_returnList.Add(family);
            }

            return m_returnList;
        }

        public static List<Family> GetFamilyByNameContains(Document curDoc, string familyName)
        {
            List<Family> m_famList = GetAllFamilies(curDoc);

            List<Family> m_returnList = new List<Family>();

            //loop through family symbols in current project and look for a match
            foreach (Family curFam in m_famList)
            {
                if (curFam.Name.Contains(familyName))
                {
                    m_returnList.Add(curFam);
                }
            }

            return m_returnList;
        }

        public static Family GetFamilyByName(Document curDoc, string familyName)
        {
            List<Family> famList = GetAllFamilies(curDoc);

            foreach (Family curFam in famList)
            {
                if (curFam.Name == familyName)
                    return curFam;
            }

            return null;
        }

        #endregion

        #region Levels

        public static List<Level> GetAllLevels(Document curDoc)
        {
            FilteredElementCollector colLevels = new FilteredElementCollector(curDoc);
            colLevels.OfCategory(BuiltInCategory.OST_Levels);

            List<Level> levels = new List<Level>();
            foreach (Element x in colLevels.ToElements())
            {
                if (x.GetType() == typeof(Level))
                {
                    levels.Add((Level)x);
                }
            }

            return levels;
        }

        internal static List<Level> GetLevelByNameContains(Document curDoc, string levelWord)
        {
            List<Level> levels = GetAllLevels(curDoc);

            List<Level> returnList = new List<Level>();

            foreach (Level curLevel in levels)
            {
                if (curLevel.Name.Contains(levelWord))
                    returnList.Add(curLevel);
            }

            return returnList;
        }

        internal static Level GetLevelByName(Document curDoc, string levelName)
        {
            List<Level> levels = GetAllLevels(curDoc);

            foreach (Level curLevel in levels)
            {
                Debug.Print(curLevel.Name);

                if (curLevel.Name.Equals(levelName))
                    return curLevel;
            }

            return null;
        }

        #endregion

        #region Lines

        internal static LinePatternElement GetLinePatternByName(Document curDoc, string typeName)
        {
            if (typeName != null)
                return LinePatternElement.GetLinePatternElementByName(curDoc, typeName);
            else
                return null;
        }

        internal static GraphicsStyle GetLinestyleByName(Document curDoc, string styleName)
        {
            GraphicsStyle retlinestyle = null;

            FilteredElementCollector gstylescollector = new FilteredElementCollector(curDoc);
            gstylescollector.OfClass(typeof(GraphicsStyle));

            foreach (Element element in gstylescollector)
            {
                GraphicsStyle curLS = element as GraphicsStyle;

                if (curLS.Name == styleName)
                    retlinestyle = curLS;
            }

            return retlinestyle;
        }

        #endregion

        #region Parameters

        public struct ParameterData
        {
            public Definition def;
            public ElementBinding binding;
            public string name;
            public bool IsSharedStatusKnown;
            public bool IsShared;
            public string GUID;
            public ElementId id;
        }

        public static List<ParameterData> GetAllProjectParameters(Document curDoc)
        {
            if (curDoc.IsFamilyDocument)
            {
                TaskDialog.Show("Error", "Cannot be a family curDocument.");
                return null;
            }

            List<ParameterData> paraList = new List<ParameterData>();

            BindingMap map = curDoc.ParameterBindings;
            DefinitionBindingMapIterator iter = map.ForwardIterator();
            iter.Reset();
            while (iter.MoveNext())
            {
                ParameterData pd = new ParameterData();
                pd.def = iter.Key;
                pd.name = iter.Key.Name;
                pd.binding = iter.Current as ElementBinding;
                paraList.Add(pd);
            }

            return paraList;
        }

        public static bool DoesProjectParamExist(Document curDoc, string pName)
        {
            List<ParameterData> pdList = GetAllProjectParameters(curDoc);
            foreach (ParameterData pd in pdList)
            {
                if (pd.name == pName)
                {
                    return true;
                }
            }
            return false;
        }
        public static void CreateSharedParam(Document curDoc, string groupName, string paramName, BuiltInCategory cat)
        {
            Definition curDef = null;

            //check if current file has shared param file - if not then exit
            DefinitionFile defFile = curDoc.Application.OpenSharedParameterFile();

            //check if file has shared parameter file
            if (defFile == null)
            {
                TaskDialog.Show("Error", "No shared parameter file.");
                //Throw New Exception("No Shared Parameter File!")
            }

            //check if shared parameter exists in shared param file - if not then create
            if (ParamExists(defFile.Groups, groupName, paramName) == false)
            {
                //create param
                curDef = AddParamToFile(defFile, groupName, paramName);
            }
            else
            {
                curDef = GetParameterDefinitionFromFile(defFile, groupName, paramName);
            }

            //check if param is added to views - if not then add
            if (ParamAddedToFile(curDoc, paramName) == false)
            {
                //add parameter to current Revitfile
                AddParamToDocument(curDoc, curDef, cat);
            }
        }

        internal static string GetParameterValueByName(Element element, string paramName)
        {
            IList<Parameter> paramList = element.GetParameters(paramName);

            if (paramList != null)
                try
                {
                    Parameter param = paramList[0];
                    string paramValue = param.AsValueString();
                    return paramValue;
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    return null;
                }

            return "";
        }

        internal static Parameter GetParameterByName(Element curElem, string paramName)
        {
            foreach (Parameter curParam in curElem.Parameters)
            {
                if (curParam.Definition.Name.ToString() == paramName)
                    return curParam;
            }

            return null;
        }

        internal static Parameter GetParameterByNameAndWritable(Element curElem, string paramName)
        {
            foreach (Parameter curParam in curElem.Parameters)
            {
                if (curParam.Definition.Name.ToString() == paramName && curParam.IsReadOnly == false)
                    return curParam;
            }

            return null;
        }

        internal static ElementId GetProjectParameterId(Document curDoc, string name)
        {
            ParameterElement pElem = new FilteredElementCollector(curDoc)
                .OfClass(typeof(ParameterElement))
                .Cast<ParameterElement>()
                .Where(e => e.Name.Equals(name))
                .FirstOrDefault();

            return pElem?.Id;
        }

        internal static ElementId GetBuiltInParameterId(Document curDoc, BuiltInCategory cat, BuiltInParameter bip)
        {
            FilteredElementCollector collector = new FilteredElementCollector(curDoc);
            collector.OfCategory(cat);

            Parameter curParam = collector.FirstElement().get_Parameter(bip);

            return curParam?.Id;
        }

        internal static string SetParameterByNameAndWritable(Element curElem, string paramName, string value)
        {
            Parameter curParam = GetParameterByNameAndWritable(curElem, paramName);

            curParam.Set(value);
            return curParam.ToString();
        }

        internal static void SetParameterByName(Element element, string paramName, string value)
        {
            IList<Parameter> paramList = element.GetParameters(paramName);

            if (paramList != null)
            {
                Parameter param = paramList[0];

                param.Set(value);
            }
        }

        internal static void SetParameterByName(Element element, string paramName, int value)
        {
            IList<Parameter> paramList = element.GetParameters(paramName);

            if (paramList != null)
            {
                Parameter param = paramList[0];

                param.Set(value);
            }
        }

        internal static bool SetParameterValue(Element curElem, string paramName, string value)
        {
            Parameter curParam = GetParameterByName(curElem, paramName);

            if (curParam != null)
            {
                curParam.Set(value);
                return true;
            }

            return false;
        }

        private static Definition GetParameterDefinitionFromFile(DefinitionFile defFile, string groupName, string paramName)
        {
            // iterate the Definition groups of this file
            foreach (DefinitionGroup group in defFile.Groups)
            {
                if (group.Name == groupName)
                {
                    // iterate the difinitions
                    foreach (Definition definition in group.Definitions)
                    {
                        if (definition.Name == paramName)
                            return definition;
                    }
                }
            }
            return null;
        }

        //check if specified parameter is already added to Revit file
        public static bool ParamAddedToFile(Document curDoc, string paramName)
        {
            foreach (Parameter curParam in curDoc.ProjectInformation.Parameters)
            {
                if (curParam.Definition.Name.Equals(paramName))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool AddParamToDocument(Document curDoc, Definition curDef, BuiltInCategory cat)
        {
            bool paramAdded = false;

            //define category for shared param
            Category myCat = curDoc.Settings.Categories.get_Item(cat);
            CategorySet myCatSet = curDoc.Application.Create.NewCategorySet();
            myCatSet.Insert(myCat);

            //create binding
            ElementBinding curBinding = curDoc.Application.Create.NewInstanceBinding(myCatSet);

            //do something
            //paramAdded = curDoc.ParameterBindings.Insert(curDef, curBinding, BuiltInParameterGroup.PG_IDENTITY_DATA);

            return paramAdded;
        }


        //check if specified parameter exists in shared parameter file
        public static bool ParamExists(DefinitionGroups groupList, string groupName, string paramName)
        {
            //loop through groups and look for match
            foreach (DefinitionGroup curGroup in groupList)
            {
                if (curGroup.Name.Equals(groupName) == true)
                {
                    //check if param exists
                    foreach (Definition curDef in curGroup.Definitions)
                    {
                        if (curDef.Name.Equals(paramName))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        //add parameter to specified shared parameter file
        public static Definition AddParamToFile(DefinitionFile defFile, string groupName, string paramName)
        {
            //create new shared parameter in specified file
            DefinitionGroup defGroup = GetDefinitionGroup(defFile, groupName);

            //check if group exists - if not then create
            if (defGroup == null)
            {
                //create group
                defGroup = defFile.Groups.Create(groupName);
            }

            //create parameter in group
            ExternalDefinitionCreationOptions curOptions = new ExternalDefinitionCreationOptions(paramName, SpecTypeId.String.Text);
            curOptions.Visible = true;

            Definition newParam = defGroup.Definitions.Create(curOptions);

            return newParam;
        }

        public static DefinitionGroup GetDefinitionGroup(DefinitionFile defFile, string groupName)
        {
            //loop through groups and look for match
            foreach (DefinitionGroup curGroup in defFile.Groups)
            {
                if (curGroup.Name.Equals(groupName))
                {
                    return curGroup;
                }
            }

            return null;
        }

        #endregion

        #region Ribbon

        internal static RibbonPanel CreateRibbonPanel(UIControlledApplication app, string tabName, string panelName)
        {
            RibbonPanel currentPanel = GetRibbonPanelByName(app, tabName, panelName);

            if (currentPanel == null)
                currentPanel = app.CreateRibbonPanel(tabName, panelName);

            return currentPanel;
        }

        internal static RibbonPanel GetRibbonPanelByName(UIControlledApplication app, string tabName, string panelName)
        {
            foreach (RibbonPanel tmpPanel in app.GetRibbonPanels(tabName))
            {
                if (tmpPanel.Name == panelName)
                    return tmpPanel;
            }

            return null;
        }

        internal static BitmapImage BitmapToImageSource(Bitmap bm)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                bm.Save(mem, System.Drawing.Imaging.ImageFormat.Png);
                mem.Position = 0;
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.StreamSource = mem;
                bmi.CacheOption = BitmapCacheOption.OnLoad;
                bmi.EndInit();

                return bmi;
            }
        }

        #endregion

        #region Schedules

        internal static ViewSchedule CreateAreaSchedule(Document curDoc, string schedName, AreaScheme curAreaScheme)
        {
            ElementId catId = new ElementId(BuiltInCategory.OST_Areas);
            ViewSchedule newSchedule = ViewSchedule.CreateSchedule(curDoc, catId, curAreaScheme.Id);
            newSchedule.Name = schedName;

            return newSchedule;
        }

        internal static List<ViewSchedule> GetAllSchedulesByElevation(Document curDoc, string newElev)
        {
            List<ViewSchedule> m_scheduleList = GetAllSchedules(curDoc);

            List<ViewSchedule> m_returnList = new List<ViewSchedule>();

            foreach (ViewSchedule curVS in m_scheduleList)
            {
                if (curVS.Name.EndsWith(newElev))
                {
                    m_returnList.Add(curVS);
                }
            }

            return m_returnList;
        }

        internal static List<ViewSchedule> GetAllSchedules(Document curDoc)
        {
            List<ViewSchedule> schedList = new List<ViewSchedule>();

            FilteredElementCollector curCollector = new FilteredElementCollector(curDoc);
            curCollector.OfClass(typeof(ViewSchedule));

            //loop through views and check if schedule - if so then put into schedule list
            foreach (ViewSchedule curView in curCollector)
            {
                if (curView.ViewType == ViewType.Schedule)
                {
                    schedList.Add((ViewSchedule)curView);
                }
            }

            return schedList;
        }

        internal static ViewSchedule GetScheduleByName(Document curDoc, string v)
        {
            FilteredElementCollector collector = new FilteredElementCollector(curDoc);
            collector.OfClass(typeof(ViewSchedule));

            foreach (ViewSchedule curSchedule in collector)
            {
                if (curSchedule.Name == v)
                    return curSchedule;
            }

            return null;
        }

        internal static List<ViewSchedule> GetAllScheduleByNameContains(Document curDoc, string schedName)
        {
            List<ViewSchedule> m_scheduleList = GetAllSchedules(curDoc);

            List<ViewSchedule> m_returnList = new List<ViewSchedule>();

            foreach (ViewSchedule curSchedule in m_scheduleList)
            {
                if (curSchedule.Name.Contains(schedName))
                    m_returnList.Add(curSchedule);
            }

            return m_returnList;
        }

        private static List<ViewSchedule> GetAllViewScheduleTemplates(Document curDoc)
        {
            List<ViewSchedule> returnList = new List<ViewSchedule>();
            List<ViewSchedule> viewList = GetAllSchedules(curDoc);

            //loop through views and check if is view template
            foreach (ViewSchedule v in viewList)
            {
                if (v.IsTemplate == true)
                {
                    //add view template to list
                    returnList.Add(v);
                }
            }

            return returnList;
        }

        public static ViewSchedule GetViewScheduleTemplateByName(Document curDoc, string viewSchedTemplateName)
        {
            List<ViewSchedule> viewSchedTemplateList = GetAllViewScheduleTemplates(curDoc);

            foreach (ViewSchedule v in viewSchedTemplateList)
            {
                if (v.Name == viewSchedTemplateName)
                {
                    return v;
                }
            }

            return null;
        }

        internal static List<ScheduleSheetInstance> GetAllScheduleSheetInstancesByNameAndView(Document curDoc, string elevName, View activeView)
        {
            List<ScheduleSheetInstance> ssiList = GetAllScheduleSheetInstancesByView(curDoc, activeView);

            List<ScheduleSheetInstance> returnList = new List<ScheduleSheetInstance>();

            foreach (ScheduleSheetInstance curInstance in ssiList)
            {
                if (curInstance.Name.Contains(elevName))
                    returnList.Add(curInstance);
            }

            return returnList;
        }

        internal static List<ScheduleSheetInstance> GetAllScheduleSheetInstancesByView(Document curDoc, View activeView)
        {
            FilteredElementCollector colSSI = new FilteredElementCollector(curDoc, activeView.Id);
            colSSI.OfClass(typeof(ScheduleSheetInstance));

            List<ScheduleSheetInstance> returnList = new List<ScheduleSheetInstance>();

            foreach (ScheduleSheetInstance curInstance in colSSI)
            {
                returnList.Add(curInstance);
            }

            return returnList;
        }

        internal static List<ScheduleSheetInstance> GetAllScheduleSheetInstancesByName(Document curDoc, string elevName)
        {
            List<ScheduleSheetInstance> ssiList = GetAllScheduleSheetInstances(curDoc);

            List<ScheduleSheetInstance> returnList = new List<ScheduleSheetInstance>();

            foreach (ScheduleSheetInstance curInstance in ssiList)
            {
                if (curInstance.Name.Contains(elevName))
                    returnList.Add(curInstance);
            }

            return returnList;
        }

        internal static List<ScheduleSheetInstance> GetAllScheduleSheetInstances(Document curDoc)
        {
            FilteredElementCollector colSSI = new FilteredElementCollector(curDoc);
            colSSI.OfClass(typeof(ScheduleSheetInstance));

            List<ScheduleSheetInstance> returnList = new List<ScheduleSheetInstance>();

            foreach (ScheduleSheetInstance curInstance in colSSI)
            {
                returnList.Add(curInstance);
            }

            return returnList;
        }

        internal static ViewSchedule GetScheduleByNameContains(Document curDoc, string scheduleString)
        {
            List<ViewSchedule> m_scheduleList = GetAllSchedules(curDoc);

            foreach (ViewSchedule curSchedule in m_scheduleList)
            {
                if (curSchedule.Name.Contains(scheduleString))
                    return curSchedule;
            }

            return null;
        }

        internal static List<string> GetAllScheduleNames(Document curDoc)
        {
            List<ViewSchedule> m_schedList = GetAllSchedules(curDoc);

            List<string> m_Names = new List<string>();

            foreach (ViewSchedule curSched in m_schedList)
            {
                m_Names.Add(curSched.Name);
            }

            return m_Names;
        }

        internal static List<string> GetSchedulesNotUsed(List<string> schedNames, List<string> schedInstances)
        {
            IEnumerable<string> m_returnList;

            m_returnList = schedNames.Except(schedInstances);

            return m_returnList.ToList();
        }

        internal static List<ViewSchedule> GetSchedulesToDelete(Document curDoc, List<string> schedNotUsed)
        {
            List<ViewSchedule> m_returnList = new List<ViewSchedule>();

            foreach (string schedName in schedNotUsed)
            {
                string curName = schedName;

                ViewSchedule curSched = GetViewScheduleByName(curDoc, curName);

                if (curSched != null)
                {
                    m_returnList.Add(curSched);
                }
            }

            return m_returnList;
        }

        internal static ViewSchedule GetViewScheduleByName(Document curDoc, string viewScheduleName)
        {
            List<ViewSchedule> m_SchedList = GetAllSchedules(curDoc);

            ViewSchedule m_viewSchedNotFound = null;

            foreach (ViewSchedule curViewSched in m_SchedList)
            {
                if (curViewSched.Name == viewScheduleName)
                {
                    return curViewSched;
                }
            }

            return m_viewSchedNotFound;
        }

        internal static List<string> GetAllSSINames(Document curDoc)
        {
            FilteredElementCollector m_colSSI = new FilteredElementCollector(curDoc);
            m_colSSI.OfClass(typeof(ScheduleSheetInstance));

            List<string> m_returnList = new List<string>();

            foreach (ScheduleSheetInstance curInstance in m_colSSI)
            {
                string schedName = curInstance.Name as string;
                m_returnList.Add(schedName);
            }

            return m_returnList;
        }

        internal static void DuplicateAndRenameSheetIndex(Document curDoc, string newFilter)
        {
            // duplicate the first schedule found with "Sheet Index" in the name
            List<ViewSchedule> listSched = Utils.GetAllScheduleByNameContains(curDoc, "Sheet Index");
            ViewSchedule dupSched = listSched.FirstOrDefault();

            if (dupSched == null)
            {
                // call another method to create one

                return; // no schedule to duplicate
            }

            ViewSchedule indexSched = curDoc.GetElement(dupSched.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;

            // rename the duplicated schedule to the new elevation
            string originalName = indexSched.Name;
            string[] schedTitle = originalName.Split('-');

            string curTitle = schedTitle[0];

            indexSched.Name = schedTitle[0] + "- Elevation " + GlobalVars.ElevDesignation;

            Utils.SetParameterValue(indexSched, "Elevation Designation", "Elevation " + GlobalVars.ElevDesignation);

            // update the filter value to the new elevation code filter
            ScheduleFilter codeFilter = indexSched.Definition.GetFilter(0);

            if (codeFilter.IsStringValue)
            {
                codeFilter.SetValue(newFilter);
                indexSched.Definition.SetFilter(0, codeFilter);
            }
        }

        internal static void DuplicateAndConfigureVeneerSchedule(Document curDoc)
        {
            // duplicate the first schedule with "Exterior Venner Calculations" in the name
            List<ViewSchedule> listSched = Utils.GetAllScheduleByNameContains(curDoc, "Exterior Veneer Calculations");
            ViewSchedule dupSched = listSched.FirstOrDefault();

            if (dupSched == null)
            {
                // call another method to create one

                return; // no schedule to duplicate
            }

            // duplicate the schedule
            ViewSchedule veneerSched = curDoc.GetElement(dupSched.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;

            // rename the duplicated schedule to the new elevation
            string originalName = veneerSched.Name;
            string[] schedTitle = originalName.Split('-');

            veneerSched.Name = schedTitle[0] + "- Elevation " + GlobalVars.ElevDesignation;

            Utils.SetParameterValue(veneerSched, "Elevation Designation", "Elevation " + GlobalVars.ElevDesignation);

            //// set the design option to the specified elevation designation
            //DesignOption curOption = Utils.getDesignOptionByName(curDoc, "Elevation : " + Globals.ElevDesignation);

            //Parameter doParam = veneerSched.get_Parameter(BuiltInParameter.VIEWER_OPTION_VISIBILITY);

            //doParam.Set(curOption.Id); //??? the code is getting the right option, but it's not changing anything in the model
        }

        internal static void DuplicateAndConfigureEquipmentSchedule(Document curDoc)
        {
            // duplicate the first schedule with "Roof Ventilation Equipment" in the name
            List<ViewSchedule> listSched = Utils.GetAllScheduleByNameContains(curDoc, "Roof Ventilation Equipment");
            ViewSchedule dupSched = listSched.FirstOrDefault();

            if (dupSched == null)
            {
                // call another method to create one

                return; // no schedule to duplicate
            }

            // duplicate the schedule
            ViewSchedule equipmentSched = curDoc.GetElement(dupSched.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;

            // rename the duplicated schedule to the new elevation
            string originalName = equipmentSched.Name;
            string[] schedTitle = originalName.Split('-');

            equipmentSched.Name = schedTitle[0] + "- Elevation " + GlobalVars.ElevDesignation;

            Utils.SetParameterValue(equipmentSched, "Elevation Designation", "Elevation " + GlobalVars.ElevDesignation);

            //// set the design option to the specified elevation designation
            //DesignOption curOption = Utils.getDesignOptionByName(curDoc, "Elevation : " + Globals.ElevDesignation);

            //Parameter doParam = veneerSched.get_Parameter(BuiltInParameter.VIEWER_OPTION_VISIBILITY);

            //doParam.Set(curOption.Id); //??? the code is getting the right option, but it's not changing anything in the model
        }

        #endregion

        #region Sheets

        internal static List<ViewSheet> GetAllSheets(Document curDoc)
        {
            //get all sheets
            FilteredElementCollector m_colSheets = new FilteredElementCollector(curDoc);
            m_colSheets.OfCategory(BuiltInCategory.OST_Sheets);

            List<ViewSheet> m_returnSheets = new List<ViewSheet>();
            foreach (ViewSheet curSheet in m_colSheets.ToElements())
            {
                m_returnSheets.Add(curSheet);
            }

            return m_returnSheets;
        }

        internal static ViewSheet GetSheetByElevationAndNameContains(Document curDoc, string newElev, string sheetName)
        {
            List<ViewSheet> sheetList = GetAllSheets(curDoc);

            foreach (ViewSheet curVS in sheetList)
            {
                if (curVS.SheetNumber.Contains(newElev.ToLower()) && curVS.Name.Contains(sheetName))
                {
                    return curVS;
                }
            }

            return null;
        }

        internal static List<ViewSheet> GetAllSheetsByElevation(Document curDoc, string elevDesignation)
        {
            //get all sheets
            List<ViewSheet> m_sheetList = GetAllSheets(curDoc);

            List<ViewSheet> m_returnSheets = new List<ViewSheet>();

            foreach (ViewSheet curSheet in m_sheetList)
            {
                if (curSheet.SheetNumber.Contains(elevDesignation))
                    m_returnSheets.Add(curSheet);
            }

            return m_returnSheets;
        }

        public static List<ViewSheet> GetSheetsByNumber(Document curDoc, string sheetNumber)
        {
            List<ViewSheet> returnSheets = new List<ViewSheet>();

            //get all sheets
            List<ViewSheet> curSheets = GetAllSheets(curDoc);

            //loop through sheets and check sheet name
            foreach (ViewSheet curSheet in curSheets)
            {
                if (curSheet.SheetNumber.Contains(sheetNumber))
                {
                    returnSheets.Add(curSheet);
                }
            }

            return returnSheets;
        }

        internal static List<string> GetAllSheetGroupsByCategory(Document curDoc, string categoryValue)
        {
            List<string> m_groups = new List<string>();

            // Get all sheet views in the project that have the specified category value
            List<ViewSheet> m_sheets = GetAllSheetsByCategory(curDoc, categoryValue);

            // Iterate through each sheet view and get the value of the "Group" parameter
            foreach (ViewSheet sheet in m_sheets)
            {
                // Get the "Group" parameter of the sheet view
                Parameter groupParameter = sheet.LookupParameter("Group");

                // Check if the "Group" parameter is valid and get its value
                if (groupParameter != null && groupParameter.Definition.Name == "Group")
                {
                    string groupValue = groupParameter.AsString();

                    // Check if the group value is not null or empty, and if it hasn't already been added to the list
                    if (!string.IsNullOrEmpty(groupValue) && !m_groups.Contains(groupValue))
                    {
                        m_groups.Add(groupValue);
                    }
                }
            }

            return m_groups;
        }

        public static List<ViewSheet> GetAllSheetsByCategory(Document curDoc, string categoryValue)
        {
            List<ViewSheet> m_sheets = new List<ViewSheet>();

            // Get all sheets in the project
            FilteredElementCollector sheetCollector = new FilteredElementCollector(curDoc);
            ICollection<Element> sheetElements = sheetCollector.OfClass(typeof(ViewSheet)).ToElements();

            // Iterate through each sheet and check if it has the specified category parameter with the value of "Inactive"
            foreach (Element sheetElement in sheetElements)
            {
                ViewSheet sheet = sheetElement as ViewSheet;
                if (sheet != null)
                {
                    // Get the category parameter of the sheet
                    Parameter categoryParameter = sheet.LookupParameter("Category");

                    // Check if the category parameter is valid and has the expected value
                    if (categoryParameter != null && categoryParameter.Definition.Name == "Category" &&
                        categoryParameter.AsValueString() == categoryValue)
                    {
                        m_sheets.Add(sheet);
                    }
                }
            }

            return m_sheets;
        }

        internal static List<ViewSheet> GetSheetsByGroupName(Document curDoc, string stringValue)
        {
            List<ViewSheet> m_viewSheets = GetAllSheets(curDoc);

            List<ViewSheet> m_returnGroups = new List<ViewSheet>();

            foreach (ViewSheet curSheet in m_viewSheets)
            {
                // Get the "Group" parameter of the sheet view
                Parameter groupParameter = curSheet.LookupParameter("Group");

                // Check for the "Group" parameter and add sheet tolist
                if (groupParameter != null && groupParameter.AsValueString() == stringValue)
                    m_returnGroups.Add(curSheet);
            }

            return m_returnGroups;
        }

        #endregion

        #region Strings

        internal static string GetLastCharacterInString(string grpName, string curElev, string newElev)
        {
            char lastChar = grpName[grpName.Length - 1];

            char firstChar = grpName[0];

            string grpLastChar = lastChar.ToString();

            string grpFirstChar = firstChar.ToString();

            if (grpLastChar == curElev)
            {
                return "Elevation " + newElev;
            }
            else if (grpFirstChar == curElev)
            {
                return newElev + grpName.Substring(1);
            }
            else
            {
                return grpName;
            }
        }

        internal static string GetStringBetweenCharacters(string input, string charFrom, string charTo)
        {
            //string cleanInput = CleanSheetNumber(input);

            int posFrom = input.IndexOf(charFrom);
            if (posFrom != -1) //if found char
            {
                int posTo = input.IndexOf(charTo, posFrom + 1);
                if (posTo != -1) //if found char
                {
                    return input.Substring(posFrom + 1, posTo - posFrom - 1);
                }
            }

            return string.Empty;
        }

        internal static string CleanSheetNumber(string sheetNumber)
        {
            Regex regex = new Regex(@"[^a-zA-Z0-9\s]", (RegexOptions)0);
            string replaceText = regex.Replace(sheetNumber, "");

            return replaceText;
        }

        internal static int GetIndexOfFirstLetter(string schedTitle)
        {
            var index = 0;
            foreach (var c in schedTitle)
                if (char.IsLetter(c))
                    return index;
                else
                    index++;

            return schedTitle.Length;
        }

        #endregion

        #region Titleblocks

        internal static FamilySymbol GetTitleBlockByNameContains(Document curDoc, string tBlockName)
        {
            FilteredElementCollector m_tBlocks = new FilteredElementCollector(curDoc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsElementType();

            foreach (FamilySymbol curTBlock in m_tBlocks)
            {
                if (curTBlock.Name.Contains(tBlockName))
                    return curTBlock;
            }

            return null;
        }

        #endregion

        #region Views

        public static List<View> GetAllNonTemplateViews(Document curDoc)
        {
            FilteredElementCollector m_colviews = new FilteredElementCollector(curDoc);
            m_colviews.OfCategory(BuiltInCategory.OST_Views);

            List<View> m_returnViews = new List<View>();
            foreach (View curView in m_colviews.ToElements())
            {
                m_returnViews.Add(curView);
            }

            return m_returnViews;
        }

        public static List<View> GetAllElevationViews(Document curDoc)
        {
            List<View> returnList = new List<View>();

            FilteredElementCollector colViews = new FilteredElementCollector(curDoc);
            colViews.OfClass(typeof(View));

            // loop through views and check for elevation views
            foreach (View x in colViews)
            {
                if (x.GetType() == typeof(ViewSection))
                {
                    if (x.IsTemplate == false)
                    {
                        if (x.ViewType == ViewType.Elevation)
                        {
                            // add view to list
                            returnList.Add(x);
                        }
                    }
                }
            }

            return returnList;
        }

        internal static List<View> GetAllViewsByCategory(Document curDoc, string catName)
        {
            List<View> m_colViews = GetAllNonTemplateViews(curDoc);

            List<View> m_returnList = new List<View>();

            foreach (View curView in m_colViews)
            {
                string viewCat = GetParameterValueByName(curView, "Category");

                if (viewCat == catName)
                {
                    if (curView.IsTemplate == false)
                        m_returnList.Add(curView);
                }

            }

            return m_returnList;
        }

        internal static List<View> GetAllViewsByCategoryAndViewTemplate(Document curDoc, string catName, string vtName)
        {
            List<View> m_colViews = GetAllViewsByCategory(curDoc, catName);

            List<View> m_returnList = new List<View>();

            foreach (View curView in m_colViews)
            {
                ElementId vtId = curView.ViewTemplateId;

                if (vtId != ElementId.InvalidElementId)
                {
                    View vt = curDoc.GetElement(vtId) as View;

                    if (vt.Name == vtName)
                        m_returnList.Add(curView);
                }
            }

            return m_returnList;
        }

        #endregion

        #region Viewports

        internal static List<Viewport> GetAllViewports(Document curDoc)
        {
            //get all viewports
            FilteredElementCollector m_vpCollector = new FilteredElementCollector(curDoc);
            m_vpCollector.OfCategory(BuiltInCategory.OST_Viewports);

            //output viewports to list
            List<Viewport> m_vpList = new List<Viewport>();
            foreach (Viewport curVP in m_vpCollector)
            {
                //add to list
                m_vpList.Add(curVP);
            }

            return m_vpList;
        }

        #endregion

        #region View Templates

        public static List<View> GetAllViewTemplates(Document curDoc)
        {
            List<View> returnList = new List<View>();
            List<View> viewList = GetAllNonTemplateViews(curDoc);

            //loop through views and check if is view template
            foreach (View v in viewList)
            {
                if (v.IsTemplate == true)
                {
                    //add view template to list
                    returnList.Add(v);
                }
            }

            return returnList;
        }

        public static List<string> GetAllViewTemplateNames(Document m_doc)
        {
            //returns list of view templates
            List<string> viewTempList = new List<string>();
            List<View> viewList = new List<View>();
            viewList = GetAllNonTemplateViews(m_doc);

            //loop through views and check if is view template
            foreach (View v in viewList)
            {
                if (v.IsTemplate == true)
                {
                    //add view template to list
                    viewTempList.Add(v.Name);
                }
            }

            return viewTempList;
        }

        public static View GetViewTemplateByName(Document curDoc, string viewTemplateName)
        {
            List<View> viewTemplateList = GetAllViewTemplates(curDoc);

            foreach (View v in viewTemplateList)
            {
                if (v.Name == viewTemplateName)
                {
                    return v;
                }
            }

            return null;
        }

        internal static ElementId ImportViewTemplates(Document sourceDoc, View sourceTemplate, Document targetDoc)
        {
            CopyPasteOptions copyPasteOptions = new CopyPasteOptions();

            ElementId sourceTemplateId = sourceTemplate.Id;

            List<ElementId> elementIds = new List<ElementId>();
            elementIds.Add(sourceTemplate.Id);

            ElementTransformUtils.CopyElements(sourceDoc, elementIds, targetDoc, Autodesk.Revit.DB.Transform.Identity, copyPasteOptions);

            return sourceTemplate.Id;
        }

        internal static View GetViewTemplateByNameContains(Document curDoc, string vtName)
        {
            List<View> m_colVTs = Utils.GetAllViewTemplates(curDoc);

            foreach (View curVT in m_colVTs)
            {
                if (curVT.Name.Contains(vtName))
                    return curVT;
            }

            return null;
        }

        internal static View GetViewTemplateByCategoryEquals(Document curDoc, string vtName)
        {
            List<View> m_colVTs = Utils.GetAllViewTemplates(curDoc);

            foreach (View curVT in m_colVTs)
            {
                if (curVT.Category.Equals(vtName))
                    return curVT;
            }

            return null;
        }

        #endregion

        internal static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T foundChild = FindVisualChild<T>(child);
                    if (foundChild != null)
                        return foundChild;
                }
            }
            return null;
        }
    }
}