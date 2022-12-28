using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RoofFrameSchedule
{
    class FrameUpdater : IUpdater
    {
        static AddInId _appId;
        static UpdaterId _updaterId;
        FamilySymbol _frameType;

        public FrameUpdater(AddInId id)
        {
            _appId = id;
            _updaterId = new UpdaterId(_appId, new Guid("F8410623-F98F-4305-9590-A25494A80C52"));
        }

        public void Execute(UpdaterData data)
        {
            try
            {
                Document doc = data.GetDocument();

                ICollection<ElementId> elemIds = data.GetModifiedElementIds();
                List<FamilyInstance> fis = new List<FamilyInstance>();
                foreach (ElementId elId in elemIds)
                {
                    FamilyInstance elem = doc.GetElement(elId) as FamilyInstance;
                    fis.Add(elem);
                }
                fis = fis.TakeWhile(x=>x != null).Distinct(new FrameInstanceComparer()).ToList();

                //if (fis.Count == 1)
                //{
                    //TaskDialog.Show("Внимание", "Обратитесь к разработчику плагина, необходимо проверить количество изменяемых элементов");

                    FamilyInstance fi = fis[0];
                    FamilySymbol fs = fis[0].Symbol;

                    Dictionary<string, FamilySymbol> framings = new FilteredElementCollector(doc)
                        .WhereElementIsElementType()
                        .OfCategory(BuiltInCategory.OST_StructuralFraming)
                        .Cast<FamilySymbol>()
                        .ToDictionary(x => x.FamilyName + x.Name, y => y);

                    ParameterSet fsParams = fs.Parameters;
                    List<Parameter> parameters = new List<Parameter>();

                    foreach (Parameter p in fsParams)
                    {
                        parameters.Add(p);
                    }

                    ElementId primeId = fs.LookupParameter("Основной элемент").AsElementId();
                    ElementId transomId = fs.LookupParameter("Поперечный элемент").AsElementId();
                    ElementId backboneId = fs.LookupParameter("Опорный элемент").AsElementId();
                    ElementId extraId = null;

                    NestedFamilyTypeReference nestPrime = doc.GetElement(primeId) as NestedFamilyTypeReference;
                    NestedFamilyTypeReference nestTransom = doc.GetElement(transomId) as NestedFamilyTypeReference;
                    NestedFamilyTypeReference nestBackbone = doc.GetElement(backboneId) as NestedFamilyTypeReference;

                    try
                    {
                        extraId = fs.LookupParameter("Дополнительный элемент").AsElementId();
                    }
                    catch { }

                    NestedFamilyTypeReference nestExtra = null;

                    if (extraId != null)
                    {
                        nestExtra = doc.GetElement(extraId) as NestedFamilyTypeReference;
                    }

                    FamilySymbol fiPrime = null;// dictSubFis[primeId];
                    FamilySymbol fiTransom = null;// dictSubFis[transomId];
                    FamilySymbol fiBackbone = null;// dictSubFis[backboneId];
                    FamilySymbol fiExtra = null;// dictSubFis[extraId];
                    try
                    {
                        fiPrime = framings[nestPrime.FamilyName + nestPrime.TypeName];
                        fiTransom = framings[nestTransom.FamilyName + nestTransom.TypeName];
                        fiBackbone = framings[nestBackbone.FamilyName + nestBackbone.TypeName];
                        if (extraId != null)
                            fiExtra = framings[nestExtra.FamilyName + nestExtra.TypeName];

                        parameters = parameters.Where(x => x.Definition.Name.Contains("КР_Рамки_")).ToList();

                        doc.Regenerate();
                        foreach (Parameter p in parameters)
                        {
                            switch (p.Definition.Name)
                            {
                                case "КР_Рамки_Bf_Осн":
                                    double bfPrime = fiPrime.LookupParameter("bf").AsDouble();
                                    p.Set(bfPrime);
                                    doc.Regenerate();
                                    break;
                                case "КР_Рамки_Bf_Попереч":
                                    double bfTransom = fiTransom.LookupParameter("bf").AsDouble();
                                    p.Set(bfTransom);
                                    doc.Regenerate();
                                    break;
                                case "КР_Рамки_x0_Доп.Уголок":
                                    double x0Extra = fiExtra.LookupParameter("Vpy").AsDouble();
                                    p.Set(x0Extra);
                                    doc.Regenerate();
                                    break;
                                case "КР_Рамки_x0_Опора":
                                    double x0Backbone = fiBackbone.LookupParameter("Vpy").AsDouble();
                                    p.Set(x0Backbone);
                                    doc.Regenerate();
                                    break;
                                case "КР_Рамки_Масса п.м._Осн":
                                    double massPrime = fiPrime.LookupParameter("Масса погонного метра").AsDouble();
                                    p.Set(massPrime);
                                    doc.Regenerate();
                                    break;
                                case "КР_Рамки_Масса п.м._Попереч":
                                    double massTransom = fiTransom.LookupParameter("Масса погонного метра").AsDouble();
                                    p.Set(massTransom);
                                    doc.Regenerate();
                                    break;
                                case "КР_Рамки_Масса п.м._Доп":
                                    double massExtra = fiExtra.LookupParameter("Масса погонного метра").AsDouble();
                                    p.Set(massExtra);
                                    doc.Regenerate();
                                    break;
                                case "КР_Рамки_Масса п.м._Опора":
                                    double massBackbone = fiBackbone.LookupParameter("Масса погонного метра").AsDouble();
                                    p.Set(massBackbone);
                                    doc.Regenerate();
                                    break;
                            }
                        }
                    }
                    catch
                    {
                        TaskDialog.Show("Внимание!", "Проверьте наличие загруженных в проекте семейств и их типов:" + "\n" +
                            nestPrime.FamilyName + nestPrime.TypeName + "\n" +
                            nestTransom.FamilyName + nestTransom.TypeName + "\n" +
                            nestBackbone.FamilyName + nestBackbone.TypeName + "\n" +
                            nestExtra.FamilyName + nestExtra.TypeName);
                    }
                //}
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message + ex.StackTrace + ex.Source);
            }
        }

        public string GetAdditionalInformation()
        {
            return "Frame Type Updater обновляет некоторые параметры типоразмеров семейств рамок";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FreeStandingComponents;
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public string GetUpdaterName()
        {
            return "Frame Type Updater";
        }
    }
}
