using JetEazy.Drivers.Motor;

namespace AX.Gui
{
    public interface IvAxisStatusView
    {
        void UpdateAxisStatus(IDrvMotorAxis axis);
    }
}
