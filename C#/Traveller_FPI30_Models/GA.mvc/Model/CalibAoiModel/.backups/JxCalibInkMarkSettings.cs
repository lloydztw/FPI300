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

using LeTian.JxProps;
using System.Drawing;

namespace LaserAlignDX.AoiModel
{
    using JxPointF = JxBase<PointF>;
    using JxRect = JxBase<Rectangle>;

    /// <summary>
    /// 點墨參數
    /// </summary>
    public class JxCalibInkMarkSettings: JxContainer
    {
        public JxCalibInkMarkVision Vision = new JxCalibInkMarkVision(description: "校正塊像測設定");
        public JxCalibInkMarkPoints Marks = new JxCalibInkMarkPoints(description: "點位標記 (唯讀)");

        public override void OnBindingSubItems()
        {
            // 綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                Vision,
                Marks,
            });
            base.OnBindingSubItems();
        }

        public JxPointF GetInkPoint(int cornerID)
        {
            //switch (cornerID)
            //{
            //    case 0: return InkLT;
            //    case 1: return InkRT;
            //    case 2: return InkRD;
            //    case 3: return InkLD;
            //    default: return null;
            //}
            return Marks.GetInkPoint(cornerID);
        }
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
    }

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

    public class JxCalibInkMarkPoints : JxContainer
    {
        public JxRect BoundRect = new JxRect("Boundary", "範圍框 (唯讀)");
        public JxPointF InkLT = new JxPointF("Ink LT", "左上 墨點 (唯讀)");
        public JxPointF InkRT = new JxPointF("Ink RT", "右上 墨點 (唯讀)");
        public JxPointF InkRD = new JxPointF("Ink RD", "右下 墨點 (唯讀)");
        public JxPointF InkLD = new JxPointF("Ink LD", "左下 墨點 (唯讀)");
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
                BoundRect,
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

        public JxPointF GetInkPoint(int cornerID)
        {
            switch (cornerID)
            {
                case 0: return InkLT;
                case 1: return InkRT;
                case 2: return InkRD;
                case 3: return InkLD;
                default: return null;
            }
        }
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
    }
}
