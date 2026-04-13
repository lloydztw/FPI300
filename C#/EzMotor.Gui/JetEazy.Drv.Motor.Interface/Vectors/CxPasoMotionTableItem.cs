using System;

namespace Paso.Model
{
    public class CxPasoMotionTableItem
    {
        public int ID;
        public string ClassName
        {
            get { return ((PasoRvPointID)ID).ToString(); }
        }
        public string Description;
        public CxPasoMotorVector MotorVector = new CxPasoMotorVector();
    }
}
