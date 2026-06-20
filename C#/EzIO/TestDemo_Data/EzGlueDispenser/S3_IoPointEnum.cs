#region AUTHOR
/*
 * EzGlueDispenser.IO.S3
 * 
 * Copyright (C) 2026
 * 
 * 2026-04-22 revised by LeTian Chang
 * 
 * http://www.jeteazy.com
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Mem;

namespace EzGlueDispenser.IO.S3
{
    /// <summary>
    /// 點膠機 點位
    /// </summary>
    /// <remarks>
    /// 簡單取名 EzPointEnum 即可, 站別利用 namespace 區隔.
    /// <br/> 例如 S3 代表 Station 3
    /// <br/> 第1,2,4站 可以依例取名 namespace 為 EzGlueDispenser.IO.S1, EzGlueDispenser.IO.S2, EzGlueDispenser.IO.S3
    /// </remarks>
    public enum EzPointEnum : int
    {
        #region DISCRETE_INPUTS
        [EzPointEnumAttribute("急停", "IX0.0", autoScan:true)]
        EMG = 0,
        [EzPointEnumAttribute("UV灯上", "IX0.1", autoScan: true)]
        UV_TOP,
        [EzPointEnumAttribute("UV灯下", "IX0.2", autoScan: true)]
        UV_BOTTOM,
        [EzPointEnumAttribute("散热片上", "IX0.3", autoScan: true)]
        FIN_UP,
        [EzPointEnumAttribute("散热片下", "IX0.4", autoScan: true)]
        FIN_DOWN,
        [EzPointEnumAttribute("总真空表", "IX0.5", autoScan: true)]
        VAC_ALL,
        [EzPointEnumAttribute("产品真空表", "IX0.6", autoScan: true)]
        VAC_PRODUCT,
        [EzPointEnumAttribute("料座真空表", "IX0.7", autoScan: true)]  //<<< 進料座 ???
        VAC_FEEDING_SITE,
        [EzPointEnumAttribute("总气压表", "IX1.0", autoScan: true)]
        AIR_PRESSURE,
        [EzPointEnumAttribute("光幕", "IX1.1", autoScan: true)]
        LIGHT_GATE,
        [EzPointEnumAttribute("速度", "IX1.2", autoScan: true)]
        SPEED,
        [EzPointEnumAttribute("速度", "IX1.3", autoScan: true)]
        SPEED_2,
        [EzPointEnumAttribute("手动/自动", "IX1.4", autoScan: true)]
        MANUAL_AUTO,
        [EzPointEnumAttribute("吸嘴/点胶", "IX1.5", autoScan: true)]
        SUCKER_DISPENSER,
        [EzPointEnumAttribute("按钮1", "IX1.6", autoScan: true)]
        BUTTON_1,
        [EzPointEnumAttribute("按钮2", "IX1.7", autoScan: true)]
        BUTTON_2,
        [EzPointEnumAttribute("按钮3", "IX2.0", autoScan: true)]
        BUTTON_3,
        [EzPointEnumAttribute("按钮4", "IX2.1", autoScan: true)]
        BUTTON_4,
        [EzPointEnumAttribute("按钮5", "IX2.2", autoScan: true)]
        BUTTON_5,
        [EzPointEnumAttribute("按钮6", "IX2.3", autoScan: true)]
        BUTTON_6,
        [EzPointEnumAttribute("按钮7", "IX2.4", autoScan: true)]
        BUTTON_7,
        [EzPointEnumAttribute("按钮8", "IX2.5", autoScan: true)]
        BUTTON_8,
        [EzPointEnumAttribute("按钮9", "IX2.6", autoScan: true)]
        BUTTON_9,
        [EzPointEnumAttribute("按钮10", "IX2.7", autoScan: true)]
        BUTTON_10,
        [EzPointEnumAttribute("按钮11", "IX3.0", autoScan: true)]
        BUTTON_11,
        [EzPointEnumAttribute("按钮12", "IX3.1", autoScan: true)]
        BUTTON_12,
        [EzPointEnumAttribute("镭射测距", "IX3.2", autoScan: true)]
        LASER,
        [EzPointEnumAttribute("门禁", "IX3.3", autoScan: true)]
        DOOR,
        [EzPointEnumAttribute("POGO-ON", "QB1547.0", autoScan: true)]
        POGO_ON,
        [EzPointEnumAttribute("POGO-OFF", "QB1548.0", autoScan: true)]
        POGO_OFF,
        [EzPointEnumAttribute("预留", "IX3.6")]
        RESERVED,
        [EzPointEnumAttribute("预留", "IX3.7")]
        RESERVED_2,
        #endregion

        #region QX_POINTS (Coils)
        [EzPointEnumAttribute("UV上", "QX0.0")]
        ADR_UVTOP,
        [EzPointEnumAttribute("UV下", "QX0.1")]
        ADR_UVBOTTOM,
        [EzPointEnumAttribute("散热上", "QX0.2")]
        ADR_FINTOP,
        [EzPointEnumAttribute("散热下", "QX0.3")]
        ADR_FINBOTTOM,
        [EzPointEnumAttribute("真空", "QX0.4")]
        ADR_SWITCH_ATTRACT,
        [EzPointEnumAttribute("料座", "QX0.5")]
        ADR_SWITCH_MATERIALBASE,

        [EzPointEnumAttribute("UV", "QB1553.0")]
        ADR_SWITCH_UV,
        [EzPointEnumAttribute("点胶阀", "QB1554.0")]
        ADR_SWITCH_DISPENSING,

        [EzPointEnumAttribute("日光灯", "QX1.0")]
        ADR_SWITCH_LIGHT,
        [EzPointEnumAttribute("刹车", "QX1.1")]
        ADR_AXIS_BREAK,
        [EzPointEnumAttribute("红灯", "QX1.2")]
        ADR_RED,
        [EzPointEnumAttribute("黄灯", "QX1.3")]
        ADR_YELLOW,
        [EzPointEnumAttribute("绿灯", "QX1.4")]
        ADR_GREEN,
        [EzPointEnumAttribute("蜂鸣器", "QX1.5")]
        ADR_BUZZER,
        [EzPointEnumAttribute("电磁铁", "QX1.6")]
        ADR_ELECTROMAGNET,
        [EzPointEnumAttribute("风扇", "QX1.7")]
        ADR_FAN,

        [EzPointEnumAttribute("小灯条", "QX2.0")]
        ADR_SMALL_LIGHT,

        [EzPointEnumAttribute("手柄", "QB1540.0")]
        ADR_CONTROLBOX,

        /// <summary>
        /// ADR_POGO_PIN 在 Gaara 代碼中會用到 QB1545.0, QB1546.0 兩點
        /// </summary>
        [EzPointEnumAttribute("POGO", "QB1545.0")]
        ADR_POGO_PIN,
        [EzPointEnumAttribute("POGO_2", "QB1546.0")]
        ADR_POGO_PIN2,

        [EzPointEnumAttribute("回原開始", "QB1520.0")]
        ADR_RESET_START,
        [EzPointEnumAttribute("回原中", "QB1521.0")]
        ADR_RESETING,
        [EzPointEnumAttribute("回原完成", "QB1522.0")]
        ADR_RESET_COMPLETE,

        [EzPointEnumAttribute("UV_LEVEL", "QX0.6")]
        ADR_UV_Level,
        #endregion

        #region QB_POINTS (Coils)
        /// <summary>
        /// 清除报警
        /// </summary>
        [EzPointEnumAttribute("清除报警", "QB1523", autoScan: true)]
        QB1523,

        /// <summary>
        /// 点胶启动 ON启动 OFF完成
        /// </summary>
        [EzPointEnumAttribute("点胶启动", "QB1541")]
        QB1541,

        /// <summary>
        /// 吸嘴吸料和到达相机测试玻璃偏移位置启动 ON启动 OFF完成
        /// </summary>
        [EzPointEnumAttribute("位置启动 (1)", "QB1542")]
        QB1542,

        /// <summary>
        /// 吸嘴吸料和到达放入位置启动 ON启动 OFF完成
        /// </summary>
        [EzPointEnumAttribute("位置启动 (2)", "QB1543")]
        QB1543,

        /// <summary>
        /// 避光槽位置启动 ON启动 OFF完成
        /// </summary>
        [EzPointEnumAttribute("位置启动 (避光槽)", "QB1550")]
        QB1550,

        /// <summary>
        /// 一鍵點膠測試 ON启动 OFF完成
        /// </summary>
        [EzPointEnumAttribute("一鍵點膠測試", "QB1551")]
        QB1551,

        /// <summary>
        /// 其他 (1)
        /// </summary>
        [EzPointEnumAttribute("其他 (1)", "QB1625")]
        QB1625,

        /// <summary>
        /// 其他 (2)
        /// </summary>
        [EzPointEnumAttribute("其他 (2)", "QB1665")]
        QB1665,
        #endregion

        #region MW_POINTS (Holding Registers)
        //注释:
        //  6 初始化位置
        //  7 避光槽下
        //  8 点胶位置
        //  9 避光槽上
        //  4 相机计算玻璃偏移位置
        //  5 pick吸嘴吸料位置
        //  3 放入位置

        /// <summary>
        /// "手动自动模式切换地址 0手动 1自动"
        /// </summary>
        [EzPointEnumAttribute("手动自动模式切换", "MW1090", autoScan: true)]
        MW1090,

        /// <summary>
        /// 点胶时间 单位ms
        /// </summary>
        [EzPointEnumAttribute("点胶时间", "MW1091")]
        MW1091,

        /// <summary>
        /// UV时间 单位s
        /// </summary>
        [EzPointEnumAttribute("UV时间", "MW1092")]
        MW1092,
        #endregion
    }
}
