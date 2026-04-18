#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-18 Creation (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using LeTian.JxProps;
using JxPointF = LeTian.JxProps.JxBase<System.Drawing.PointF>;

namespace LaserAlignDX.AoiModel.Calib
{
    public class JxInkerMotorSettings : JxContainer
    {
        #region GLOBAL_JSON_FILE_NAME
        public static string INKER_SETTINGS_JSON_FILE => 
            System.IO.Path.Combine(GaMvcPaths.CALIBRATION_PATH, "InkerMotorSettings.json");
        #endregion

        public JxPointF IdlePosC1S1 = new JxPointF("InkerIdlePosC1S1(隱藏)");
        public JxPointF IdlePosC1S2 = new JxPointF("InkerIdlePosC1S2(隱藏)");
        public JxPointF IdlePosC2S1 = new JxPointF("InkerIdlePosC2S1(隱藏)");
        public JxPointF IdlePosC2S2 = new JxPointF("InkerIdlePosC2S2(隱藏)");
        public JxNumber zDownS1 = new JxNumber("zInkerDownS1(隱藏)");
        public JxNumber zDownS2 = new JxNumber("zInkerDownS2(隱藏)");

        #region SINGLETON
        static JxInkerMotorSettings _instance;
        protected JxInkerMotorSettings() : base("InkerMotorSettings", "(隱藏)")
        {
        }
        #endregion

        public static JxInkerMotorSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new JxInkerMotorSettings();
                    _instance.Load(null);
                }
                return _instance;
            }
        }

        #region OVERRIDES
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                zDownS1,
                zDownS2,
                IdlePosC1S1,
                IdlePosC1S2,
                IdlePosC2S1,
                IdlePosC2S2,
            });
            base.OnBindingSubItems();
        }
        public override void Load(string fileName)
        {
            fileName = INKER_SETTINGS_JSON_FILE;
            if (System.IO.File.Exists(fileName))
                base.Load(fileName);
        }
        public override void Save(string fileName)
        {
            fileName = INKER_SETTINGS_JSON_FILE;
            base.Save(fileName);
        }
        #endregion

        //=== PUBLIC HELPER FUNCTIONS =======================================
        public JxPointF GetInkerIdlePosXY(CarrierEnum C, SuckerRowEnum S)
        {
            JxPointF pt;

            if (C == CarrierEnum.C1 && S == SuckerRowEnum.S1)
                pt = IdlePosC1S1;
            else if (C == CarrierEnum.C1 && S == SuckerRowEnum.S2)
                pt = IdlePosC1S2;
            else if (C == CarrierEnum.C2 && S == SuckerRowEnum.S1)
                pt = IdlePosC2S1;
            else
                pt = IdlePosC2S2;

            return pt;
        }
        public double GetInkerDownZ(SuckerRowEnum S)
        {
            if (S == SuckerRowEnum.S1)
                return (double)zDownS1.Value;
            else
                return (double)zDownS2.Value;
        }
    }
}
