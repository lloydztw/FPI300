#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-04-13 LeTian Chang, Creation
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using AX.Gui;
using JetEazy.ControlSpace.MotionSpace;
using JetEazy.FormSpace;
using JetEazy.Interface;
using JetEazy.Utils;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.Mvc.Gui;
using LaserAlignDX.OPSpace.RecipeSpace;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using INI = Traveller106.INI;
using Universal = Traveller106.Universal;


namespace LaserAlignDX.Mvc.Ctrl
{
    public enum ZPosDataSrc : int
    {
        [Description("對焦在校正平面")]
        Calib = -1,
        [Description("對焦在晶粒表面")]
        FocusOnChip = 0,
        [Description("對焦在空載台")]
        FocusOnCarrier,
        [Description("S1 點墨高度 (下壓)")]
        InkerS1,
        [Description("S2 點墨高度 (下壓)")]
        InkerS2,
    }


    public class GaMotorZCtrl
    {
        public event EventHandler OnPosDataSrcModified;

        #region PRIVATE_KERNEL_DATA
        PLCMotionClass _motor;
        IMotorPosHolder _posHolder;
        ZPosDataSrc _posHolderID;
        #endregion

        #region GUI_LINKS
        GwMotorSimpleGoPanel _view;
        Form frmOwner => _view.FindForm();
        Control lblMotorName => _view.lblAxisName;
        Control lblMotorPos => _view.lblCurrentMotorPos;
        #endregion

        string DisplayName
        {
            get => $"Motor ({GaUtil.GetEnumDescription(_posHolderID)})";
        }

        public void Attach(GwMotorSimpleGoPanel panel)
        {
            _view = panel;
            _view.btnMotorGo.Click += (s, e) => MoveMotorToPosHolder();
            _view.btnSettings.Click += (s, e) => OpenMotorJogWindow();
        }
        public void SetDataSrc(SuckerRowEnum sucker)
        {
            SetDataSrc(sucker == SuckerRowEnum.S1 ? ZPosDataSrc.InkerS1 : ZPosDataSrc.InkerS2);
        }
        public void SetDataSrc(ZPosDataSrc src)
        {
            switch (src)
            {
                case ZPosDataSrc.Calib:
                    _posHolder = new ZPosHolder_Focus_On_Calib();
                    _motor = Universal.GetBigScanCameraFocusMotor();
                    break;
                case ZPosDataSrc.FocusOnChip:
                    _posHolder = new ZPosHolder_Focus_On_Chip();
                    _motor = Universal.GetBigScanCameraFocusMotor();
                    break;
                case ZPosDataSrc.FocusOnCarrier:
                    _posHolder = new ZPosHolder_Focus_On_Carrier();
                    _motor = Universal.GetBigScanCameraFocusMotor();
                    break;
                case ZPosDataSrc.InkerS1:
                    _posHolder = new ZPosHolder_Inker_S1();
                    _motor = Universal.GetInkerMotor(SuckerRowEnum.S1);
                    break;
                case ZPosDataSrc.InkerS2:
                    _posHolder = new ZPosHolder_Inker_S2 ();
                    _motor = Universal.GetInkerMotor(SuckerRowEnum.S2);
                    break;
                default:
                    _posHolder = null;
                    break;
            }

            if(_posHolderID!=src)
            {
                _posHolderID = src;
                updateMotorPosToGui(_posHolder);
            }
        }

        public void UpdatePosHolderToGui()
        {
            updateMotorPosToGui(_posHolder);
        }
        public void MoveMotorToPosHolder()
        {
            if (_posHolder != null)
            {
                _motor.PromptMoveTo(_posHolder.Value, DisplayName);
            }
        }
        public void OpenMotorJogWindow()
        {
            var motor = this._motor;
            if (motor == null)
            {
                VsMessageBox.Warning($"No Motor!");
                return;
            }

            // 備份當下 相機馬達 的位置
            double backupMotorPos = motor.GetPos();

            using (var dlg = new FormMotorZ())
            {
                dlg.Text = lblMotorName.Text;
                dlg.StartPosition = FormStartPosition.CenterParent;

                dlg.Attach(motor);
                if (dlg.ShowDialog(frmOwner) == DialogResult.OK)
                {
                    bool isChanged = updateMotorPos(motor, _posHolder);
                    updateMotorPosToGui(_posHolder);

                    if (isChanged)
                        OnPosDataSrcModified?.Invoke(this, null);
                }
                else
                {
                    // 返回 相機馬達 之前的位置
                    motor?.PromptMoveTo(backupMotorPos, DisplayName);
                }
            }
        }

        #region PRIVATE_UPDATE_FUNCTIONS
        bool updateMotorPos(IAxis srcMotor, IMotorPosHolder dstPosHolder)
        {
            if (srcMotor == null || dstPosHolder == null)
                return false;

            double currentMotorPos = srcMotor.GetPos();
            double delta = currentMotorPos - dstPosHolder.Value;
            if (!GaBasicMotorUtil.IsTinyDelta(delta))
            {
                dstPosHolder.Value = currentMotorPos;
                return true;
            }

            return false;
        }
        void updateMotorPosToGui(IMotorPosHolder src)
        {
            var text = src != null ? $"{src.Value:0.000}" : "NA";
            updateText(lblMotorPos, text);
        }
        void updateText(Control c, string text)
        {
            if (c != null && text != null)
                c.Text = text;
        }
        #endregion
    }


    interface IMotorPosHolder
    {
        double Value { get; set; }
    }
    class ZPosHolder_Focus_On_Calib : IMotorPosHolder
    {
        ITravellerTransforms _commonBaseTrf => TravellerTransformFactory.CommonBase;
        public double Value
        {
            get
            {
                if (_commonBaseTrf == null)
                    return 0;
                return _commonBaseTrf.CameraWorkDist;
            }
            set
            {
                if (_commonBaseTrf != null)
                {
                    _commonBaseTrf.CameraWorkDist = value;
                    //_isCoordModified = true;
                }
            }
        }
    }
    class ZPosHolder_Focus_On_Carrier : IMotorPosHolder
    {
        RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        public double Value
        {
            get => _xRecipe.zFocusOnCarrier;
            set => _xRecipe.zFocusOnCarrier = (float)value;
        }
    }
    class ZPosHolder_Focus_On_Chip : IMotorPosHolder
    {
        RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        public double Value
        {
            get => _xRecipe.zFocusOnChip;
            set => _xRecipe.zFocusOnChip = (float)value;
        }
    }
    class ZPosHolder_Inker_S1 : IMotorPosHolder
    {
        public double Value
        {
            get => INI.Instance.zInkerS1;
            set => INI.Instance.zInkerS1 = (float)value;
        }
    }
    class ZPosHolder_Inker_S2 : IMotorPosHolder
    {
        public double Value
        {
            get => INI.Instance.zInkerS2;
            set => INI.Instance.zInkerS2 = (float)value;
        }
    }
}
