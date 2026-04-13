using System;

namespace Paso.Model
{
    public class CxPasoRvPoint
    {
        public int ID;
        public object Owner;
        public object[] Participators;
        public string Description;
        public CxPasoMotorVector MotorVector = new CxPasoMotorVector();
    }
}
