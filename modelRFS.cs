using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RVTAppServ = Autodesk.Revit.ApplicationServices.Application;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Autodesk.Revit.Attributes;

namespace RoofFrameSchedule
{
    [Transaction(TransactionMode.Manual)]
    class modelRFS
    {
        private UIApplication UIAPP = null;
        private RVTAppServ APP = null;
        private UIDocument UIDOC = null;
        private Document DOC = null;

        public modelRFS(UIApplication uiapp)
        {
            UIAPP = uiapp;
            APP = UIAPP.Application;
            UIDOC = UIAPP.ActiveUIDocument;
            DOC = UIDOC.Document;
        }

        public bool Create(ObservableCollection<MyFrameType> colSelectedFrames)
        {
            try
            {
                using (Transaction tx = new Transaction(DOC))
                {
                    tx.Start("CreateSchedule");
                    double[] widths = { 15, 60, 65, 10, 15, 20 };
                    string[] headers = { "Поз.", "Обозначение", "Наименование", "Кол.", "Масса\nед., кг", "Приме-\nчание" };

                    int countRows = 0;

                    Dictionary<FamilySymbol, string[][]> dictValues = new Dictionary<FamilySymbol, string[][]>();
                    int num = 0;

                    foreach (MyFrameType mft in colSelectedFrames)
                    {
                        int containsExtra = 0;
                        if (mft.ContainsExtra)
                        {
                            containsExtra = 6;
                            countRows += containsExtra;
                        }
                        else
                        {
                            containsExtra = 4;
                            countRows += containsExtra;
                        }
                        
                        FamilySymbol fs = mft.FamilySymbol;
                        string[][] arrayValues = new string[containsExtra-1][];

                        for (int i = 1; i < containsExtra; i++)
                        {
                            string i2 = "";
                            string i3 = "";
                            string i4 = "";
                            string i5 = "";
                            switch (i)
                            {
                                case 1:
                                    i2 = "ГОСТ 30245-2003";
                                    i3 = (DOC.GetElement(fs.LookupParameter("Основной элемент").AsElementId()) as NestedFamilyTypeReference).TypeName +
                                        " L=" + fs.LookupParameter("L_Осн").AsValueString();
                                    i4 = "2";
                                    i5 = Math.Round(fs.LookupParameter("L_Осн").AsDouble() * fs.LookupParameter("КР_Рамки_Масса п.м._Осн").AsDouble(), 2).ToString();
                                    break;
                                case 2:
                                    i2 = "ГОСТ 30245-2003";
                                    i3 = (DOC.GetElement(fs.LookupParameter("Поперечный элемент").AsElementId()) as NestedFamilyTypeReference).TypeName +
                                        " L=" + fs.LookupParameter("L_Попереч").AsValueString();
                                    i4 = "2";
                                    if (fs.FamilyName.Contains("с 1 поперечным профилем"))
                                        i4 = "1";
                                    i5 = Math.Round(fs.LookupParameter("L_Попереч").AsDouble() * fs.LookupParameter("КР_Рамки_Масса п.м._Попереч").AsDouble(), 2).ToString();
                                    break;
                                case 3:
                                    i2 = "ГОСТ 8510-86";
                                    i3 = (DOC.GetElement(fs.LookupParameter("Опорный элемент").AsElementId()) as NestedFamilyTypeReference).TypeName +
                                        " L=" + fs.LookupParameter("L_Опора").AsValueString();
                                    i4 = "4";
                                    i5 = Math.Round(fs.LookupParameter("L_Опора").AsDouble() * fs.LookupParameter("КР_Рамки_Масса п.м._Опора").AsDouble(), 2).ToString();
                                    break;
                                case 4:
                                    i2 = "ГОСТ 8509-93";
                                    i3 = (DOC.GetElement(fs.LookupParameter("Дополнительный элемент").AsElementId()) as NestedFamilyTypeReference).TypeName +
                                        " L=" + fs.LookupParameter("L_Доп45").AsValueString();
                                    i4 = "4";
                                    i5 = Math.Round(fs.LookupParameter("L_Доп45").AsDouble() * fs.LookupParameter("КР_Рамки_Масса п.м._Доп").AsDouble(), 2).ToString();
                                    break;
                                case 5:
                                    i2 = "ГОСТ 8509-93";
                                    i3 = (DOC.GetElement(fs.LookupParameter("Дополнительный элемент").AsElementId()) as NestedFamilyTypeReference).TypeName +
                                        " L=" + fs.LookupParameter("L_Доп").AsValueString();
                                    i4 = "4";
                                    i5 = Math.Round(fs.LookupParameter("L_Доп").AsDouble() * fs.LookupParameter("КР_Рамки_Масса п.м._Доп").AsDouble(), 2).ToString();
                                    break;
                            }

                            string[] values = { i.ToString(), i2, i3, i4, i5 };
                            arrayValues.SetValue(values,i-1);
                        }

                        dictValues.Add(fs, arrayValues);
                    }

                    ViewSchedule schedule = CreateRevitSchedule(widths); //6 столбцов для типовой спецификации на элемент
                    TableData tableData = schedule.GetTableData();
                    TableSectionData tsd = tableData.GetSectionData(SectionType.Header);

                    CreateColumnsAndRows(countRows, tsd, widths);
                    CreateHeading(tsd, headers);

                    SetValues(tsd, dictValues);

                    schedule.Definition.ShowHeaders = false;
                    tx.Commit();
                    
                    UIDOC.ActiveView = schedule;
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }
        private ViewSchedule CreateRevitSchedule(double[] widths)
        {
            ViewSchedule schedule = ViewSchedule.CreateSchedule(DOC, new ElementId(BuiltInCategory.OST_Windows), ElementId.InvalidElementId);

            string name = "Спецификация на рамки" + Regex.Replace(DateTime.Now.ToString(), ":", "-");
            schedule.Name = name;

            ScheduleDefinition definition = schedule.Definition;
            SchedulableField schedulableField = definition.GetSchedulableFields()
                .FirstOrDefault<SchedulableField>(x => x.ParameterId == new ElementId(BuiltInParameter.WINDOW_HEIGHT));
            ScheduleField scheduleField = definition.AddField(schedulableField);
            ScheduleFilter filter = new ScheduleFilter(scheduleField.FieldId, ScheduleFilterType.Equal, (double)12);
            definition.AddFilter(filter);

            TableData tableData = schedule.GetTableData();
            TableSectionData tsdRFS = tableData.GetSectionData(SectionType.Body);

            List<double> columnWidths = new List<double>();

            foreach (double d in widths)
            {
                columnWidths.Add(UnitUtils.Convert(d, DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES));
            }

            tsdRFS.SetColumnWidth(0, columnWidths.Sum());

            return schedule;
        }

        private bool CreateColumnsAndRows(int rowsCount, TableSectionData tsd, double[] widths)
        {
            tsd.RemoveRow(0);
            tsd.RemoveColumn(0);

            for (int i = 0; i < widths.Count(); i++)
            {
                tsd.InsertColumn(i);
                double width = UnitUtils.Convert(widths[i], DisplayUnitType.DUT_MILLIMETERS, DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES);
                tsd.SetColumnWidth(i, width);
            }

            for (int i = 0; i < rowsCount/* + 1*/; i++)
            {
                tsd.InsertRow(i);
                tsd.SetRowHeight(i, 0.026);
            }

            return true;
        }

        private bool CreateHeading(TableSectionData tsd, string[] headers)
        {
            tsd.InsertRow(0);
            tsd.SetRowHeight(0, 0.049);
            for (int i = 0; i < headers.Count(); i++)
            {
                tsd.SetCellText(0, i, headers[i]);
            }
            return true;
        }

        private bool SetValues(TableSectionData tsd, Dictionary<FamilySymbol, string[][]> dictValues)
        {
            int startRow = 1;
            foreach (KeyValuePair<FamilySymbol, string[][]> kvp in dictValues)
            {
                SharedParameterApplicableRule paramRule = new SharedParameterApplicableRule("КР_Рамки_Bf_Осн");
                ElementParameterFilter filter = new ElementParameterFilter(paramRule);

                int countFI = new FilteredElementCollector(DOC)
                    .WhereElementIsNotElementType()
                    .WherePasses(filter)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .Where(x => DOC.GetElement(x.GetTypeId()).get_Parameter(BuiltInParameter.ALL_MODEL_MODEL).AsString() == "Рамки")
                    .Where(x => x.Symbol.Id == kvp.Key.Id)
                    .ToList()
                    .Count;
                tsd.SetCellText(startRow, 2, kvp.Key.Name + " (" + countFI.ToString() + " шт.)");
                startRow++;
                for (int i = 0; i < kvp.Value.Length; i++)
                {
                    string[] values = kvp.Value[i];
                    int startColumn = 0;
                    foreach (string str in values)
                    {
                        tsd.SetCellText(startRow, startColumn, str);
                        startColumn++;                        
                    }
                    //for (int j = 0; j < 5; i++)
                    //{
                    //    tsd.SetCellText(startIndex, i, kvp.Value[i][j]);
                    //}
                    startRow++;
                }
            }
            return true;
        }
    }
}
