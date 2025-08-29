using Eazy_Project_III;
using LaserAlignDX.UISpace.UIMVC;
using System;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Ctrl
{
    using IMPLEMENT = V2.GaMainCtrl;

    public class GaMainCtrl : Abs.GaMainCtrl
    {
        IMPLEMENT _imp = new IMPLEMENT();

        public void Attach(MVSUI[] DsMains, MVSUI[] DsFlys, Control lblFlyCameraSerialNo)
        {
            _imp.Attach(DsMains, DsFlys, lblFlyCameraSerialNo);
        }
        public override void Tick()
        {
            _imp.Tick();
        }
    }
}
