
namespace JetEazy
{
    public enum ActionEnum : int
    {
        CAPTUREONCE,
        CAPTURETEST,
        ALLRESET,


        //MAIN_SD
        /// <summary>
        /// 设定马达参数界面
        /// </summary>
        ACT_MOTOR_SETUP,
        /// <summary>
        /// 测试取像
        /// </summary>
        ACT_TEST_GETIMAGE,

        /// <summary>
        /// 用户设定满盒PASS
        /// </summary>
        ACT_USER_FULL_PASS,
        /// <summary>
        /// 用户设定满盒NG
        /// </summary>
        ACT_USER_FULL_NG,

        ACT_SENSOR_FULL_PASS,
        ACT_SENSOR_FULL_NG,

        ACT_ISEMC,
        /// <summary>
        /// 连续NG个数
        /// </summary>
        ACT_CONTINUE_COUNT,
        /// <summary>
        /// 设定相机曝光
        /// </summary>
        ACT_SETCAMEXPOSURE,
    }

    //public enum MotionEnum : int
    //{
    //    COUNT = 16,

    //    M0 = 0,
    //    M1 = 1,
    //    M2 = 2,
    //    M3 = 3,
    //    M4 = 4,
    //    M5 = 5,
    //    M6 = 6,
    //    M7 = 7,
    //    M8 = 8,
    //    M9 = 9,
    //    M10 = 10,
    //    M11 = 11,
    //    M12 = 12,
    //    M13 = 13,
    //    M14 = 14,
    //    M15 = 15,
    //}

    public enum IOEnum : int
    {
        COUNT = 8,

        I1 = 0,
        I2 = 1,
        I3 = 2,
        I4 = 3,
        I5 = 4,
        I6 = 5,
        I7 = 6,
        I8 = 7,
    }

    public enum MotionTypeEnum
    {
        AXIS,
        ROTATION,
    }

    public enum MachineEventEnum : int
    {
        START = 0,
        EMC = 1,
        CURTAIN = 2,
        AUTOSTART = 3,

        /// <summary>
        /// 严重报警
        /// </summary>
        ALARM_SERIOUS = 4,
        /// <summary>
        /// 普通报警
        /// </summary>
        ALARM_COMMON = 5,

        /// <summary>
        /// 警告
        /// </summary>
        ALARM_WARNING = 6,
    }

    public enum MC100IONameEnum : int
    {
        COUNT = 14,

        PA00 = 0,
        PA01 = 1,
        PA02 = 2,
        PA03 = 3,
        PA04 = 4,
        PA05 = 5,
        PA06 = 6,
        PA07 = 7,
        PB00 = 8,
        PC00 = 9,
        PC01 = 10,
        PC02 = 11,
        PC03 = 12,
        PC04 = 13,
    }

    public enum MachineState : int
    {
        /// <summary>
        /// 綠燈-跑線
        /// </summary>
        Running = 1,
        /// <summary>
        /// 黃燈-等待測試
        /// </summary>
        Idle = 2,
        /// <summary>
        /// 藍燈-工程師模式調試參數
        /// </summary>
        Engineering_mode = 3,
        /// <summary>
        /// 白燈-停止使用
        /// </summary>
        Planned_downtime = 4,
        /// <summary>
        /// 紅燈-故障
        /// </summary>
        Error = 5,
    }

    public enum EventActionTypeEnum
    {
        MANUAL,
        MOVP,
        MOVN,
        PORCESS,
        AUTOMATIC,
        TEST,
        EXCEPTION,
    }
}