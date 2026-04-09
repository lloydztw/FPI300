#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-09 LeTian Chang, 修改自 萬子 的 FormMotor
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using Common;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.Interface;
using System;
using System.Windows.Forms;


namespace LaserAlignDX.Mvc.Gui
{
    public partial class FormMotorOne : Form
    {
        #region PRIVATE_DATA
        PLCMotionClass _motorModel;
        MotionTouchPanelUIClass _motorUiCtrl;
        #endregion

        public FormMotorOne()
        {
            InitializeComponent();
            TopMost = true;
            Load += (s, e) => Init();
            FormClosed += (s, e) => CleanUp();
            btnOK.Click += (s, e) => DoConfirm();
            btnCancel.Click += (s, e) => DoCancel();
        }
        public void Attach(IAxis motor)
        {
            _motorModel = motor as PLCMotionClass;
        }
        public void Attach(PLCMotionClass motor)
        {
            _motorModel = motor;
        }

        #region PRIVATE_FUNCTIONS
        void Init()
        {
            //-----------------------------------------------------------------------------
            // 以下程式碼, 拆解成 MVC (model-view-control), 演示分層.

            //-----------------------------------------------------------------------------
            //(1) MODEL
            //    PLCMotionClass 繼承自 IAxis (其生命週期 由 Univeral 維持)
            var plcMotor = _motorModel;
            if (plcMotor == null)
                return;

            //-----------------------------------------------------------------------------
            //(2) VIEW
            var motorView = vsTouchMotorUI1;

            //-----------------------------------------------------------------------------
            //(3) CONTROL
            //    MotionTouchPanelUIClass 扮演著 MVC 中的 control 角色
            //(3.1) 綁定了 Control 與 View
            var motorUiCtrl = new MotionTouchPanelUIClass(motorView);
            //(3.2) 綁定了 Control 與 Model
            motorUiCtrl.Initial(plcMotor);

            //-----------------------------------------------------------------------------
            //(4) 將 Control 記入成員
            //    後續讓 timer 可以調用
            _motorUiCtrl = motorUiCtrl;

            //-----------------------------------------------------------------------------
            //(5) 啟動 timer
            //    注意: 如果是程序員自己 new 出來的 timer, 必須自己負責調用其 Dispose !!!
            timer1.Tick += (s, e) => DoTick();
            timer1.Enabled = true;
        }
        void CleanUp()
        {
            // 因為 timer1 是用 Visual Studio 開發工具包 拖拉入 Form,
            // Net Framework 會自動調用其 Dispose().
            // 所以在此不用手動 Dispose()
            timer1.Enabled = false;

            // 請萬子 排查 MotionTouchPanelUIClass 使用完畢, 是否需要釋放資源.
            // 如果需要釋放資源, 請實作 IDisposable
            if (_motorUiCtrl is IDisposable d)
                d.Dispose();
            _motorUiCtrl = null;
        }
        void DoTick()
        {
            _motorUiCtrl?.Tick();
        }
        void DoConfirm()
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        void DoCancel()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        #endregion
    }
}
