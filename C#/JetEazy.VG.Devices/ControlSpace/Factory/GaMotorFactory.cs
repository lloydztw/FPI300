using JetEazy.ControlSpace.MotionSpace;
using JetEazy.ControlSpace.PLCSpace;

namespace JetEazy.ControlSpace
{
    public static class GaMotorFactory
    {
        public static PLCMotionClass Instance(string path, MotionEnum motionname, VsCommPLC[] plc, bool isnousemotor)
        {
            var plcMotor = isnousemotor ? new PLCMotionSim() : new PLCMotionClass();
            plcMotor?.Intial(path, motionname, plc, isnousemotor);
            return plcMotor;
        }
    }
}
