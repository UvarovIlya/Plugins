using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoofFrameSchedule
{
    [Transaction(TransactionMode.Manual)]
    class CommandRFS : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            SharedParameterApplicableRule paramRule = new SharedParameterApplicableRule("КР_Рамки_Bf_Осн");
            ElementParameterFilter filter = new ElementParameterFilter(paramRule);

            try
            {
                List<FamilyInstance> listFrames = new List<FamilyInstance>();
                listFrames = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .WherePasses(filter)
                    .OfClass(typeof(FamilyInstance))
                    .OfCategory(BuiltInCategory.OST_GenericModel)
                    .Cast<FamilyInstance>()
                    .Where(x => doc.GetElement(x.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).AsString() == "Рамки")
                    .ToList();
                List<FamilySymbol> listFrameTypes = new List<FamilySymbol>();
                foreach (FamilyInstance fi in listFrames)
                {
                    listFrameTypes.Add(doc.GetElement(fi.GetTypeId()) as FamilySymbol);
                }
                
                listFrameTypes = listFrameTypes.Distinct(new FrameComparer()).OrderBy(x => x.FamilyName).ToList();

                ObservableCollection<MyFrameType> colMyFrameTypes = new ObservableCollection<MyFrameType>();
                foreach (FamilySymbol fs in listFrameTypes)
                {
                    MyFrameType mfs = new MyFrameType(fs);
                    colMyFrameTypes.Add(mfs);
                }

                FilteredElementCollector fecDraftingViews = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfClass(typeof(ViewDrafting));
                Dictionary<int, string> dictDraftingView = fecDraftingViews.Cast<ViewDrafting>()
                    .ToDictionary(x => x.Id.IntegerValue, y => y.Name);
                fecDraftingViews.Dispose();
                
                
                viewModelRFS vmod = new viewModelRFS();
                
                vmod.ColFrames = colMyFrameTypes;

                if (dictDraftingView.Count > 0)
                {
                    vmod.DictDraftingViews = dictDraftingView;
                    vmod.SelectedDraftingView = dictDraftingView.First().Key;
                }

                vmod.ColSelectedFrames = new ObservableCollection<MyFrameType>();

                vmod.RevitModel = new modelRFS(uiapp);

                System.Diagnostics.Process proc = System.Diagnostics.Process.GetCurrentProcess();

                using (ViewRFS view = new ViewRFS())
                {
                    System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(view);
                    helper.Owner = proc.MainWindowHandle;

                    vmod.View = view;

                    view.DataContext = vmod;
                    
                    if (view.ShowDialog() != true)
                    {
                        return Result.Cancelled;
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message + ex.StackTrace;
                return Result.Failed;
            }
        }
    }
}
