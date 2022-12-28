using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RoofFrameSchedule
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]
    class App : IExternalApplication
    {
        static string AddInPath = typeof(App).Assembly.Location;
        public Result OnStartup(UIControlledApplication app)
        {
            FrameUpdater updater = new FrameUpdater(app.ActiveAddInId);
            UpdaterRegistry.RegisterUpdater(updater);

            ElementClassFilter frameFilter = new ElementClassFilter(typeof(FamilyInstance));

            FilterableValueProvider valueProvider = new ParameterValueProvider(new ElementId(BuiltInParameter.ALL_MODEL_MODEL));
            FilterRule filterRule = new FilterStringRule(valueProvider, new FilterStringEquals(), "Рамки", true);
            //FilterableValueProvider valueProvider1 = new ParameterValueProvider(new ElementId(BuiltInParameter.INVALID));
            //FilterRule filterRule1 = new FilterStringRule(valueProvider1, new FilterStringContains(), )
            SharedParameterApplicableRule paramRule = new SharedParameterApplicableRule("КР_Рамки_Bf_Осн");
            IList<FilterRule> rules = new List<FilterRule>();
            rules.Add(filterRule);
            rules.Add(paramRule);
            ElementParameterFilter elementParameterFilter = new ElementParameterFilter(rules);

            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), /*frameFilter*/elementParameterFilter, Element.GetChangeTypeGeometry());
            CreateRibbon(app);
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication app)
        {
            FrameUpdater updater = new FrameUpdater(app.ActiveAddInId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
            return Result.Succeeded;
        }
        private void CreateRibbon(UIControlledApplication app)
        {
            string PanelName = "Сталь";
            string TabName = "АО Казанский Гипронииавиапром";

            try
            {
                app.CreateRibbonTab(TabName);
            }
            catch { }
            RibbonPanel ribbonAOKazGAP = null;

            List<RibbonPanel> panels = app.GetRibbonPanels(TabName);
            foreach (RibbonPanel rb in panels)
            {
                if (rb.Name == PanelName)
                {
                    ribbonAOKazGAP = rb;
                    continue;
                }
            }
            if (ribbonAOKazGAP == null)
            {
                ribbonAOKazGAP = app.CreateRibbonPanel(TabName, PanelName);
            }
            ribbonAOKazGAP.Visible = true;

            PushButtonData pushButtonData = new PushButtonData("RoofFrameSchedule", "Спецификация\nрамок", AddInPath, "RoofFrameSchedule.CommandRFS");
            pushButtonData.LargeImage = convertFromBitmap(RoofFrameSchedule.Properties.Resources.Icon);
            pushButtonData.ToolTip = "Создание спецификации по семействам стальных рамок. Спецификация формируется текстов в заголовке, по принципу" +
                " работы плагина" + @"""Спецификация металлопроката"""+".";
            ribbonAOKazGAP.AddItem(pushButtonData);
        }
        BitmapSource convertFromBitmap(System.Drawing.Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
