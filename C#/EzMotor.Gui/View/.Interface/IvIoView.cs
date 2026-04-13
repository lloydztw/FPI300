using System.Windows.Forms;

namespace AX.Gui
{
    public interface IvIoView
    {
        Form frmOwner { get; }

        Button btnServOn { get; }
        Control btnServAlarm { get; }

        Button btnBrake { get; }
        Control btnEMG { get; }
        Control btnAir { get; }
        Button btnBuzzer { get; }
        Button btnLampR { get; }
        Button btnLampY { get; }
        Button btnLampG { get; }
        Button btnLightCurtainTriggered { get; }
        Button btnSettings { get; }
    }
}
