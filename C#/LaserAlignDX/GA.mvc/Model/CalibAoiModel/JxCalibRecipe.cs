#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-11-20 新增 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion


using AForge.Imaging.Filters;
using EzAoiEmptyTrayInspector.Model;
using JetEazy.Match;
using JetEazy.QvMath;
using LeTian.JxProps;
using System.Drawing;
using System.Windows.Documents;
using JxPointF = LeTian.JxProps.JxBase<System.Drawing.PointF>;
using JxRect = LeTian.JxProps.JxBase<System.Drawing.Rectangle>;


namespace LaserAlignDX.AoiModel.Calib
{
    /// <summary>
    /// 全域座標 校正參數
    /// </summary>
    public class JxCalibRecipe : JxContainer
    {
        public JxCalibGridSettings GridSettings = new JxCalibGridSettings();
        public JxCalibInkMarkSettings InkMarkSettings1 = new JxCalibInkMarkSettings(SuckerRowEnum.S1);
        public JxCalibInkMarkSettings InkMarkSettings2 = new JxCalibInkMarkSettings(SuckerRowEnum.S2);
        public JxRect BoundRect = new JxRect("Boundary", "範圍框 (唯讀)(隱藏)");

        public JxCalibRecipe()
        {
        }
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                GridSettings,
                InkMarkSettings1,
                InkMarkSettings2,
                BoundRect,
            });
            base.OnBindingSubItems();
        }

        #region PUBLIC_HELPER_FUNCTIONS
        public void SyncTraySettings(JxCalibRecipe jxSrc)
        {
            if (jxSrc != null && jxSrc != this)
            {
                var src = jxSrc.GridSettings.EmptyTraySettings;
                var dst = this.GridSettings.EmptyTraySettings;
                dst.FullRows.Value = src.FullRows.Value;
                dst.FullCols.Value = src.FullCols.Value;
                dst.PitchX.Value = src.PitchX;
                dst.PitchY.Value = src.PitchY;
            }
        }
        #endregion
    }


    /// <summary>
    /// 全域座標 校正參數 (格點頁)
    /// </summary>
    public class JxCalibGridSettings : JxContainer
    {
        public JxTrayMiscSettings EmptyTraySettings = new JxTrayMiscSettings() { Description = "空盤規格設定" };
        public JxCalibGridVisionSettings GridVisionSettings = new JxCalibGridVisionSettings() { Description = "格點像測設定" };
        public JxRect BoundRect = new JxRect("Boundary", "範圍框 (唯讀)(隱藏)");

        public JxCalibGridSettings() : base(name: "GridSettings", "校正參數 (格點)")
        {

        }
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                EmptyTraySettings,
                GridVisionSettings,
                BoundRect,
            });
            base.OnBindingSubItems();
        }
    }


    /// <summary>
    /// 格點像測設定
    /// </summary>
    public class JxCalibGridVisionSettings : JxContainer
    {
        public JxInt Threshold = JxInt.C255("Threshold", "格點 門限", 128);
        public JxInt MinSize = new JxInt("Min Size", "格點 最小邊長 (pixels)", 100, new Range(10, 5000));

        public JxCalibGridVisionSettings() : base(name: "Grid Vision")
        {
        }
        public JxCalibGridVisionSettings(string name, string description) : base(name, description)
        {
        }
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                Threshold,
                MinSize,
            });
            base.OnBindingSubItems();
        }
    }


    /// <summary>
    /// 全域座標 校正參數 (點墨頁)
    /// </summary>
    public class JxCalibInkMarkSettings: JxContainer
    {
        public JxCalibInkMarkVision Vision = new JxCalibInkMarkVision(description: "校正塊像測設定");
        public JxCalibInkMarkPoints Marks = new JxCalibInkMarkPoints(description: "點位標記 (唯讀)(隱藏)");
        public JxCalibInkMarkSettings(SuckerRowEnum id) : base($"InkMark {id}", $"校正參數 (點墨 {id})")
        {

        }
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                Vision,
                Marks,
            });
            base.OnBindingSubItems();
        }

        #region PUBLIC_HELPER_FUNCTIONS
        public JxPointF GetRawMotorPoint(int cornerID)
        {
            //switch (cornerID)
            //{
            //    case 0: return RawMotorLT;
            //    case 1: return RawMotorRT;
            //    case 2: return RawMotorRD;
            //    case 3: return RawMotorLD;
            //    default: return null;
            //}
            return Marks.GetRawMotorPoint(cornerID);
        }
        public void GetInkMarks(out EzBloc[] blocs)
        {
            Marks.GetInkMarks(out blocs);
        }
        public void SetInkMarks(EzBloc[] blocs)
        {
            Marks.SetInkMarks(blocs);
        }
        #endregion
    }


    /// <summary>
    /// 校正塊像測設定
    /// </summary>
    public class JxCalibInkMarkVision : JxContainer
    {
        public JxInt BlockThreshold = JxInt.C255("Block Threshold", "校正塊 門限", 200);
        public JxInt BlockMinSize = new JxInt("Block Min Size", "校正塊 最小邊長 (pixels)", 500, new Range(10, 5000));
        public JxCalibInkMarkVision(string name = "Vision", string description = null) : base(name, description)
        {
        }
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                BlockThreshold,
                BlockMinSize,
            });
            base.OnBindingSubItems();
        }
    }


    /// <summary>
    /// 墨點 (Runtime ReadOnly)
    /// </summary>
    public class JxCalibInkMarkPoints : JxContainer
    {
        public JxMark InkLT = new JxMark("Ink LT", "左上 墨點 (唯讀)");
        public JxMark InkRT = new JxMark("Ink RT", "右上 墨點 (唯讀)");
        public JxMark InkRD = new JxMark("Ink RD", "右下 墨點 (唯讀)");
        public JxMark InkLD = new JxMark("Ink LD", "左下 墨點 (唯讀)");

        public JxPointF RawMotorLT = new JxPointF("Raw Motor LT", "左上 馬達輸入點位 (唯讀)");
        public JxPointF RawMotorRT = new JxPointF("Raw Motor RT", "右上 馬達輸入點位 (唯讀)");
        public JxPointF RawMotorRD = new JxPointF("Raw Motor RD", "右下 馬達輸入點位 (唯讀)");
        public JxPointF RawMotorLD = new JxPointF("Raw Motor LD", "左下 馬達輸入點位 (唯讀)");

        public JxCalibInkMarkPoints(string name = "Marks", string description = null) : base(name, description)
        {
        }
        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                InkLT,
                InkRT,
                InkRD,
                InkLD,
                RawMotorLT,
                RawMotorRT,
                RawMotorRD,
                RawMotorLD,
            });
            base.OnBindingSubItems();
        }

        #region PUBLIC_HELPER_FUNCTIONS
        public JxPointF GetRawMotorPoint(int cornerID)
        {
            switch (cornerID)
            {
                case 0: return RawMotorLT;
                case 1: return RawMotorRT;
                case 2: return RawMotorRD;
                case 3: return RawMotorLD;
                default: return null;
            }
        }
        public void GetInkMarks(out EzBloc[] blocs)
        {
            blocs = new EzBloc[]
            {
                InkLT.GetBloc(),
                InkRT.GetBloc(),
                InkRD.GetBloc(),
                InkLD.GetBloc(),
            };
        }
        public void SetInkMarks(EzBloc[] blocs)
        {
            if (blocs == null) return;
            if (blocs.Length > 0) InkLT.SetBloc(blocs[0]);
            if (blocs.Length > 1) InkRT.SetBloc(blocs[1]);
            if (blocs.Length > 2) InkRD.SetBloc(blocs[2]);
            if (blocs.Length > 3) InkLD.SetBloc(blocs[3]);
        }
        #endregion
    }


    public class JxMark : JxContainer
    {
        public JxPointF Center = new JxPointF("InkMark.Center");
        public JxRect Rect = new JxRect("InkMark.Rect");

        public JxMark(string name, string description = null) : base(name, description)
        {
        }
        public JxMark() { }

        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                Center,
                Rect,
            });
            base.OnBindingSubItems();
        }

        #region PUBLIC_HELPER_FUNCTIONS
        public EzBloc GetBloc()
        {
            var bloc = new EzBloc(this.Rect.Value, 1);
            bloc.Center = new JetEazy.QMath.QVector(Center.Value.X, Center.Value.Y);
            return bloc;
        }
        public void SetBloc(EzBloc bloc)
        {
            if (bloc != null)
            {
                Rect.Value = bloc.Rect;
                if (bloc.Center != null)
                    Center.Value = new PointF((float)bloc.Center.X, (float)bloc.Center.Y);
                else
                    Center.Value = PointF.Empty;
            }
            else
            {
                Rect.Value = Rectangle.Empty;
                Center.Value = PointF.Empty;
            }
        }
        #endregion
    }
}
