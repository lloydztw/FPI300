#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-05-07 重整 (by LeTian Chang)
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Lang;
using JetEazy.Match;
using JetEazy.QMath;
using JetEazy.Transform;
using JetEazy.Utils;
using LaserAlignDX.Model.Coords;
using LaserAlignDX.OPSpace;
using System;
using System.Text;
using XCell = LaserAlignDX.OPSpace.RegionCellX3Class;
using XCellBloc = LaserAlignDX.Model.XCellBloc;
using XRecipe = LaserAlignDX.OPSpace.RecipeSpace.RecipeFPIX3Class;

namespace LaserAlignDX.Mvc.Gui.Tooltips
{
    internal class TooltipProvider
    {
        #region GLOBAL_MESS
        XRecipe _xRecipe
        {
            get => XRecipe.Instance;
        }
        bool _withPadGaps = false;
        #endregion

        #region PRIVATE_KERNEL_DATA
        ITravellerTransforms _trfModel;
        #endregion

        #region PRIVATE_RUNTIME_DATA
        bool _isFocusOnChip;
        #endregion

        public CarrierEnum ActiveCarrierID
        {
            get; set;
        }
        public ITransform TransCameraToWorld
        {
            get; set;
        }
        public ITransform TransCameraToMotor
        {
            get; set;
        }
        public ITransform TransCameraToMotor2
        {
            get; set;
        }
        public bool IsEmptyTrayMode
        {
            get;
            set;
        }

        public void Attach(ITravellerTransforms trfModel)
        {
            _trfModel = trfModel;
        }
        public string ComposeTooltipText(EzBloc cursor, EzBloc cursor2)
        {
            if (cursor == null)
                return null;

            try
            {
                var sb = new StringBuilder();
                bool isShowScore = true;

                getRowCol(cursor, out int row, out int col, out XCell cell);
                checkResult(cell, out bool isPass, out bool isEmpty);

                //(1) 顯示 PASS/NG  (如果是 Cell 而且有 ChipData)
                if (!IsEmptyTrayMode && !isEmpty && cell != null)
                    appendPassNG(sb, cell);

                //(2) 格點(index) : [row, col]
                string idxTag = cell != null ? $"({cell.Index})" : "";
                sb.Append(QMSG.T("格點")).Append(idxTag).Append(" : [").AppendValues(row, col).AppendLine("]");

                //(3) Camera Coords
                appendCameraCoords(sb, cursor, cursor2);

                //(4) World Coords
                if (TransCameraToWorld != null)
                {
                    appendWorldCoords(sb, cursor, cursor2);
                    isShowScore = false;
                }

                //(5) Motor Coords
                if (TransCameraToMotor != null || TransCameraToMotor2 != null)
                {
                    appendMotorCoords(sb, cursor, cursor2, row, col);
                    isShowScore = false;
                }

                //(6) Score
                if (isShowScore || IsEmptyTrayMode)
                {
                    sb.AppendLine();
                    sb.AppendLine($"Score= {cursor.Score:0.00}");
                    sb.AppendLine($"Size= {cursor.Rect.Width}x{cursor.Rect.Height}");
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        void appendPassNG(StringBuilder sb, XCell cell)
        {
            if (cell == null)
                return;

            if (cell.IsResultPass())
            {
                sb.AppendLine($"{UnicodeTags.PASS} PASS").AppendLine();
            }
            else
            {
                foreach (InspectReason ng in cell.IterNgResults())
                {
                    var ngText = GaUtil.GetEnumDescription(ng);
                    sb.AppendLine($"{UnicodeTags.NG} {ngText}");
                }
                sb.AppendLine();
            }
        }
        void appendCameraCoords(StringBuilder sb, EzBloc bloc, EzBloc bloc2)
        {
            var camPt = getCamCoord(bloc);
            if (camPt == null)
                return;

            var tag = QMSG.T("相機座標");
            sb.AppendLine($"{tag} (X,Y) = ({camPt.X:0.0}, {camPt.Y:0.0}) pix");
            var camPt2 = getCamCoord(bloc2);
            if (camPt2 != null && bloc2 != bloc)
            {
                var dv = camPt - camPt2;
                var dist = dv.NormLength;
                sb.AppendLine($"{tag} Diff (ΔX,ΔY) = ({dv.X:0.0}, {dv.Y:0.0}) pix");
                sb.AppendLine($"{tag} Dist. = {dist:0.0} pix");
            }
        }
        void appendWorldCoords(StringBuilder sb, EzBloc bloc, EzBloc bloc2)
        {
            var camPt = getCamCoord(bloc);
            if (camPt == null)
                return;

            var trfCameraToWorld = TransCameraToWorld;
            if (trfCameraToWorld == null)
                return;

            sb.AppendLine();

            var tag = QMSG.T("World 座標");
            var worldCoord = trfCameraToWorld.Trans(camPt);
            sb.AppendLine($"{tag} (X,Y) = ({worldCoord.X:0.000}, {worldCoord.Y:0.000}) mm");

            var camPt2 = getCamCoord(bloc2);
            if (camPt2 != null && bloc2 != bloc)
            {
                var worldCoord2 = trfCameraToWorld.Trans(camPt2);
                var dv = worldCoord - worldCoord2;
                double dist = dv.NormLength;
                sb.AppendLine($"{tag} Diff (ΔX,ΔY) = ({dv.X:0.000}, {dv.Y:0.000}) mm");
                sb.AppendLine($"{tag} Dist. = {dist:0.000} mm");
            }
        }
        void appendMotorCoords(StringBuilder sb, EzBloc bloc, EzBloc bloc2, int row, int col)
        {
#if(OPT_QC)
            return;
#endif

            if (bloc == null)
                return;

            var camPt = getCamCoord(bloc);
            var camPt2 = getCamCoord(bloc2);

            bool isDistMeasuring = (camPt != null && camPt2 != null && bloc != bloc2);

            var trfModel = _trfModel;
            var trfs = new[] { TransCameraToMotor, TransCameraToMotor2 };

            foreach (var trfCamToMotor in trfs)
            {
                if (trfCamToMotor == null)
                    continue;

                SuckerRowEnum SID = trfCamToMotor.Name.Contains("S1") ? SuckerRowEnum.S1 : SuckerRowEnum.S2;
                var motorCoord = trfCamToMotor.Trans(camPt);

                // 距離量測模式
                if (isDistMeasuring)
                {
                    var motorCoord2 = trfCamToMotor.Trans(camPt2);
                    var dv = motorCoord - motorCoord2;
                    double dist = dv.NormLength;

                    var visionTag = QMSG.T("像測");
                    var motorTag = QMSG.T("馬達座標");
                    sb.AppendLine();
                    sb.AppendLine($"{SID} {visionTag} {motorTag} (X,Y) = ({motorCoord.X:0.000}, {motorCoord.Y:0.000}) mm");
                    sb.AppendLine($"{SID} {visionTag} {motorTag} Diff (ΔX,ΔY) = ({dv.X:0.000}, {dv.Y:0.000}) mm");
                    sb.AppendLine($"{SID} {visionTag} {motorTag} Dist. = {dist:0.000} mm");
                }
                else
                {
                    // 德龍 PLC 預期座標
                    if (trfModel != null && row >= 0 && col >= 0)
                    {
                        var predict = QMSG.T("預期");
                        var vision = QMSG.T("像測");
                        var motorTag = QMSG.T("馬達座標");

                        trfModel.GetPlcExpectedCoords(ActiveCarrierID, 0, 0, out var s1_org, out var s2_org);
                        trfModel.GetPlcExpectedCoords(ActiveCarrierID, row, col, out var s1_expected, out var s2_expected);
                        var plc_coord = SID == SuckerRowEnum.S1 ? s1_expected : s2_expected;
                        
                        sb.AppendLine();
                        sb.AppendLine($"{SID} {predict} {motorTag} (X,Y) = ({plc_coord.X:0.000}, {plc_coord.Y:0.000}) mm");
                        sb.AppendLine($"{SID} {vision} {motorTag} (X,Y) = ({motorCoord.X:0.000}, {motorCoord.Y:0.000}) mm");

                        trfModel.GetNodeCoords(ActiveCarrierID, 0, 0, out var camOrg, out var _, out var _, out var _);
                        var motorCoord0 = trfCamToMotor.Trans(camOrg);
                        var motor_relative = motorCoord - motorCoord0;
                        var plc_org = SID == SuckerRowEnum.S1 ? s1_org : s2_org;
                        var plc_relative = plc_coord - plc_org;
                        sb.AppendLine($"{SID} {predict} {motorTag} (relative) (X,Y) = ({plc_relative.X:0.000}, {plc_relative.Y:0.000}) mm");
                        sb.AppendLine($"{SID} {vision} {motorTag} (relative) (X,Y) = ({motor_relative.X:0.000}, {motor_relative.Y:0.000}) mm");
                    }
                }
            }

            if (!isDistMeasuring)
            {
                appendPlcCompensation(sb, camPt, row, col);
                appendCellResult(sb, (bloc as XCellBloc)?.Cell);
            }
        }
        void appendPlcCompensation(StringBuilder sb, QVector camPt, int row, int col)
        {
#if (OPT_QC)
            return;
#endif

            if (camPt == null || row < 0 || col < 0 || _trfModel == null)
                return;

            (var motorD1, var motorD2, var worldDelta) = _trfModel.CalcPlcCompensation(ActiveCarrierID, camPt, row, col);

            var tag = QMSG.T("補償量");
            sb.AppendLine();
            sb.AppendLine($"PLC {tag} S1 (ΔX,ΔY) = ({motorD1.X:0.000}, {motorD1.Y:0.000}) mm");
            sb.AppendLine($"PLC {tag} S2 (ΔX,ΔY) = ({motorD2.X:0.000}, {motorD2.Y:0.000}) mm");

            //sb.AppendLine($"World 變動量 (ΔX,ΔY) = ({worldDelta.X:0.000}, {worldDelta.Y:0.000}) mm");
        }
        void appendCellResult(StringBuilder sb, XCell cell)
        {
#if (OPT_QC)
            return;
#endif
            if (cell == null)
                return;

            #region 最後補償量
            sb.AppendLine();
            sb.AppendLine($"RunX = {cell.RunX:0.000} mm");
            sb.AppendLine($"RunY = {cell.RunY:0.000} mm");
            sb.AppendLine($"Angle = {cell.RunAngle:0.00}°");
            #endregion

            if (_xRecipe.InspectParams.optTiltDetectEnabled)
            {
                #region 傾斜(踩腳)
                var chipCoords = cell?.ChipData?.ChipCoords;
                if (chipCoords != null)
                {
                    sb.AppendLine();
                    sb.AppendLine(QMSG.T("傾斜(踩腳)程度")).Append($" = {chipCoords.TiltRatio:0.000}");
                }
                #endregion
            }

            if (_xRecipe.InspectParams.optChipMeasurement)
            {
                var tagX = QMSG.T("晶粒.尺寸X");
                var tagY = QMSG.T("晶粒.尺寸Y");

                #region 尺寸量測結果
                var cW = _xRecipe.InspectParams.mWidthStand;
                var cH = _xRecipe.InspectParams.mHeightStand;
                var dx = Math.Round(cell.RunWidth - cW, 3);
                var dy = Math.Round(cell.RunHeight - cH, 3);
                sb.AppendLine().Append($"{tagX} = {cell.RunWidth:0.000} mm").Append($" (Δ = {dx:0.000} mm)");
                sb.AppendLine().Append($"{tagY} = {cell.RunHeight:0.000} mm").Append($" (Δ = {dy:0.000} mm)");
                #endregion

                #region 尺寸量測詳細點位
                var chipDim = cell?.ChipData?.ChipDimension;
                if (chipDim != null && chipDim.GetPixelSize(out double dpX, out double dpY))
                {
                    sb.AppendLine();
                    sb.AppendLine().Append($"{tagX} = {dpX:0.0} pix");
                    sb.AppendLine().Append($"{tagY} = {dpY:0.0} pix");
                }
                #endregion

                #region 晶粒_PAD_跨距
                if (_xRecipe.InspectParams.xAlgorithm == MatchAlgorithmEnum.GridMatch)
                {
                    var padsGrid = cell?.ChipData?.PadsGrid;
                    if (getChipPadsSpan(padsGrid, out double padSpanW, out double padSpanH))
                    {
                        tagX = QMSG.T("PAD.跨距.尺寸X");
                        tagY = QMSG.T("PAD.跨距.尺寸Y");
                        sb.AppendLine();
                        sb.AppendLine().Append($"{tagX}= {padSpanW:0.0} pix");
                        sb.AppendLine().Append($"{tagY} = {padSpanH:0.0} pix");
                    }
                }
                #endregion

                #region PAD_邊隙
                if (_withPadGaps)
                {
                    var gaps = cell?.ChipData?.PadEdgeGaps;
                    if (gaps != null)
                    {
                        if (GlobalConfig.OPT_USING_GAPS_4)
                        {
                            sb.AppendLine();
                            //sb.AppendLine().Append($"邊隙(左) = {gaps.GetAveGap(BasicSpace.EdgeBorder.Left):0.000} mm");
                            //sb.AppendLine().Append($"邊隙(上) = {gaps.GetAveGap(BasicSpace.EdgeBorder.Top):0.000} mm");
                            //sb.AppendLine().Append($"邊隙(右) = {gaps.GetAveGap(BasicSpace.EdgeBorder.Right):0.000} mm");
                            //sb.AppendLine().Append($"邊隙(下) = {gaps.GetAveGap(BasicSpace.EdgeBorder.Bottom):0.000} mm");
                            //sb.AppendLine().Append($"邊隙差值(左右) = {gaps.GetAveGapDiff():0.000} mm");
                            sb.AppendLine().Append(QMSG.T("邊隙(左)")).Append($" = {gaps.GetAveGap(BasicSpace.EdgeBorder.Left):0.000} mm");
                            sb.AppendLine().Append(QMSG.T("邊隙(上)")).Append($" = {gaps.GetAveGap(BasicSpace.EdgeBorder.Top):0.000} mm");
                            sb.AppendLine().Append(QMSG.T("邊隙(右)")).Append($" = {gaps.GetAveGap(BasicSpace.EdgeBorder.Right):0.000} mm");
                            sb.AppendLine().Append(QMSG.T("邊隙(下)")).Append($" = {gaps.GetAveGap(BasicSpace.EdgeBorder.Bottom):0.000} mm");
                            sb.AppendLine().Append(QMSG.T("邊隙差值(左右)")).Append($" = {gaps.GetAveGapDiff():0.000} mm");
                        }
                        else
                        {
                            sb.AppendLine();
                            sb.AppendLine().Append($"LUX = {gaps.LU.X:0.000} mm");
                            sb.AppendLine().Append($"RUX = {gaps.RU.X:0.000} mm");
                            sb.AppendLine().Append($"RDX = {gaps.RD.X:0.000} mm");
                            sb.AppendLine().Append($"LDX = {gaps.LD.X:0.000} mm");
                            sb.AppendLine();
                            sb.AppendLine().Append($"LUY = {gaps.LU.Y:0.000} mm");
                            sb.AppendLine().Append($"RUY = {gaps.RU.Y:0.000} mm");
                            sb.AppendLine().Append($"RDY = {gaps.RD.Y:0.000} mm");
                            sb.AppendLine().Append($"LDY = {gaps.LD.Y:0.000} mm");
                        }
                    }
                }
                #endregion
            }
        }

        bool getRowCol(EzBloc cursor, out int row, out int col, out XCell cell)
        {
            var curCellBloc = cursor as XCellBloc ?? cursor.Tag as XCellBloc;
            cell = curCellBloc?.Cell;
            if (cell != null)
            {
                row = cell.CellRow;
                col = cell.CellCol;
            }
            else
            {
                var rowCol = (cursor?.Tag as QuadLinkNode)?.rowCol;
                row = rowCol != null ? rowCol.Row : -1;
                col = rowCol != null ? rowCol.Col : -1;
            }
            return row >= 0 && col >= 0;
        }
        bool checkResult(XCell cell, out bool isPass, out bool isEmpty)
        {
            isPass = false;
            isEmpty = false;

            if (cell == null)
                return false;

            if (IsEmptyTrayMode)
            {
                string abnormalStr = cell.GetNoTrayDesc();
                if (string.IsNullOrEmpty(abnormalStr))
                    isEmpty = true;
                else
                    isEmpty = false;
                isPass = isEmpty;
            }
            else
            {
                isEmpty = false;
                if (cell.IsResultPass())
                    isPass = true;
                else if (!cell.IsEmptyPlaceHold())
                    isPass = false;
                else
                    isEmpty = true;
            }

            return true;
        }
        bool getChipPadsSpan(EzBlocsGrid padsGrid, out double spanWidth, out double spanHeight, int digits = 1)
        {
            spanWidth = 0;
            spanHeight = 0;
            if (padsGrid != null)
            {
                //------------------------------------------------------------------------------------------
                // 0 1
                // 3 2
                //------------------------------------------------------------------------------------------
                var pts = Array.ConvertAll(padsGrid.GetCornerBlocs(), pb => pb?.Center);
                if (pts[0] != null && pts[1] != null && pts[2] != null && pts[3] != null)
                {
                    var L = (pts[0] + pts[3]) / 2;
                    var R = (pts[1] + pts[2]) / 2;
                    var T = (pts[0] + pts[1]) / 2;
                    var B = (pts[3] + pts[2]) / 2;
                    spanWidth = Math.Round((R - L).NormLength, digits);
                    spanHeight = Math.Round((B - T).NormLength, digits);
                    return true;
                }
            }
            return false;
        }

        QVector getCamCoord(EzBloc bloc)
        {
            var cell = (bloc as XCellBloc)?.Cell;
            if (cell != null)
            {
                //if (cell == null)
                //    return bloc?.Center;
                //var cx = cell.xFindResult.fCenterX;
                //var cy = cell.xFindResult.fCenterY;
                //if (cx > 0 && cy > 0)
                //    return new QVector(cx, cy);
                //return bloc.Center;

                var chipQuad = cell?.ChipData?.ChipQuad2D;
                if (chipQuad != null)
                    return chipQuad.Center;
            }

            var camPt = bloc?.Center;
            return camPt;
        }
    }
}
