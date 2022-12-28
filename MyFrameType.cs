using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoofFrameSchedule
{
    class MyFrameType
    {
        private FamilySymbol _familySymbol;
        private string _typeAndFamily;
        private int _id;
        private string _familyName;
        private bool _containsExtra;

        public MyFrameType(FamilySymbol fs)
        {
            _typeAndFamily = fs.Name + " (" + fs.FamilyName + ")";
            _id = fs.Id.IntegerValue;
            _familyName = fs.FamilyName;
            _containsExtra = fs.ParametersMap.Contains("Дополнительный элемент");
            _familySymbol = fs;
        }

        public string TypeAndFamily
        {
            get { return _typeAndFamily; }
        }

        public int Id
        {
            get { return _id; }
        }

        public string FamilyName
        {
            get { return _familyName; }
        }

        public bool ContainsExtra
        {
            get { return _containsExtra; }
        }

        public FamilySymbol FamilySymbol
        {
            get { return _familySymbol; }
        }
    }
}
