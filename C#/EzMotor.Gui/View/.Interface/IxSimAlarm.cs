namespace AX.Gui.Ctrl
{
    public interface IxSimAlarm
    {
        int TotalMotorsCount { get; }
        void simMotorDriverAlarm(int motorID, bool? on = null);

        void simEMG(bool? on = null);
        void simAirOK(bool? on = null);
        void simLightCurtain(bool? on = null);
        bool LightGateEnabled { get; set; }
    }
}
