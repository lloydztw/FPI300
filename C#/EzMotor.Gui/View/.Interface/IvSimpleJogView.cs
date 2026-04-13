using System.Windows.Forms;

namespace AX.Gui
{
    public interface IvSimpleJogView
    {
        string AxisName { get; set; }
        Button btnJogBackward { get; }
        Button btnJogForward { get; }
        Button btnJogHome { get; }
        Control lblPos { get; }
    }
}
