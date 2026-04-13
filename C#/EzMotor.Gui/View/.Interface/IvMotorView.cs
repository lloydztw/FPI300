using System.Windows.Forms;

namespace AX.Gui
{
    public interface IvMotorView
    {
        //Form frmOwner { get; }
        Control wndOwnerPanel { get; }
        //Timer timTick { get; }

        Button btnExit { get; }
        Button btnStop { get; }
        Button btnServOn { get; }
        Control btnServAlarm { get; }

        IvAxisStatusView[] quickViews { get; }        // 0:X, 1:Y
        NumericUpDown[] numsMoveTo { get; }           // 0:X, 1:Y
        Button btnMoveTo { get; }

        RadioButton rdoSpeedModeHI { get; }
        RadioButton rdoSpeedModeLO { get; }
        RadioButton rdoMicroStep { get; }

        Control lblSpeedHI { get; }
        Control lblSpeedLO { get; }
        Button btnSpeedsConfig { get; }

        Button btnHome { get; }
        Button[] btnsJogForwards { get; }            // 0:X, 1:Y  
        Button[] btnsJogBackwards { get; }           // 0:X, 1:Y

        /// <summary>
        /// loc: (L)eft, (R)ight, (U)p, (D)own
        /// </summary>
        //Button btnJogL { get; }
        //Button btnJogR { get; }
        //Button btnJogU { get; }
        //Button btnJogD { get; }
        //Button btnJogCW { get; }
        //Button btnJogCCW { get; }
    }
}
