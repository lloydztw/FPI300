using LaserAlignDX;
using LaserAlignDX.UISpace.UIMVC;
using System.Windows.Forms;


namespace GA.Mvc.Ctrl
{
    public class GaMainCtrl : Abs.GaMainCtrl
    {
        Abs.GaMainCtrl _imp = GaMvcConfig.CreateMainCtrl();

        public override void Attach(Control[] DsMains, MVSUI[] DsFlys, Control lblFlyCameraSerialNo)
        {
            _imp.Attach(DsMains, DsFlys, lblFlyCameraSerialNo);
        }
        public override void Tick()
        {
            _imp.Tick();
        }
    }
}
