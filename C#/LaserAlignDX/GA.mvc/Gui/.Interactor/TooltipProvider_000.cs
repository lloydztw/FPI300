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

using JetEazy.Match;
using JetEazy.Transform;
using LaserAlignDX.Model.Coords;
using System.Text;

namespace LaserAlignDX.Mvc.Gui.Tooltips.V0
{
    internal class TooltipProvider
    {
        #region PRIVATE_KERNEL_DATA
        ITravellerTransforms _trfModel;
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
        public string ComposeTooltipText(EzBloc cursor, EzBloc cursor2, int row =-1, int col=-1)
        {
            if (cursor == null)
                return "";

            var cursorBloc = cursor;
            var cursorBloc2 = cursor2;

            bool showScore = true;

            var sb = new StringBuilder();

            if (row < 0 || col < 0)
                getRowCol(cursorBloc, out row, out col);

            if (row >= 0 && col >= 0)
                sb.Append("格點: [").AppendValues(row, col).AppendLine("]");

            appendCameraCoords(sb, cursorBloc, cursorBloc2);

            if (TransCameraToWorld != null)
            {
                appendWorldCoords(sb, cursorBloc, cursorBloc2);
                showScore = false;
            }

            if (TransCameraToMotor != null || TransCameraToMotor2 != null)
            {
                appendMotorCoords(sb, cursorBloc, cursorBloc2);
                showScore = false;
            }

            if (TransCameraToMotor != null && cursorBloc2 == null && row >= 0 && col >= 0)
            {
                appendPlcCompensation(sb, cursorBloc, row, col);
                showScore = false;
            }

            if (showScore || IsEmptyTrayMode)
            {
                sb.AppendLine();
                sb.AppendLine($"Score= {cursorBloc.Score:0.00}");
                sb.AppendLine($"Size= {cursorBloc.Rect.Width}x{cursorBloc.Rect.Height}");
            }

            return sb.ToString();
        }

        void appendCameraCoords(StringBuilder sb, EzBloc bloc, EzBloc bloc2)
        {
            if (bloc == null)
                return;

            sb.AppendLine($"相機座標 (X,Y) = ({bloc.Center.X:0.0}, {bloc.Center.Y:0.0}) pix");

            if (bloc != null && bloc2 != null && bloc != bloc2)
            {
                var dv = bloc.Center - bloc2.Center;
                var dist = dv.NormLength;
                sb.AppendLine($"相機座標 差值 (dX,dY) = ({dv.X:0.0}, {dv.Y:0.0}) pix");
                sb.AppendLine($"相機座標 距離 = {dist:0.0} pix");
            }
        }
        void appendWorldCoords(StringBuilder sb, EzBloc bloc, EzBloc bloc2)
        {
            var trfCameraToWorld = TransCameraToWorld;
            if (bloc == null || trfCameraToWorld == null)
                return;

            sb.AppendLine();

            //QVector world_node = null;
            //if (getRowCol(bloc, out int row, out int col))
            //    trfCameraToWorld.GetCalibGridPoints().Get(row, col, out _, out world_node);
            //if (world_node != null)
            //    sb.AppendLine($"World 座標 (X,Y) = ({world_node.X:0.000}, {world_node.Y:0.000}) mm");

            var worldCoord = trfCameraToWorld.Trans(bloc.Center);
            sb.AppendLine($"World座標 (X,Y) = ({worldCoord.X:0.000}, {worldCoord.Y:0.000}) mm");

            if (bloc != null && bloc2 != null && bloc != bloc2)
            {
                var worldCoord2 = trfCameraToWorld.Trans(bloc2.Center);
                var dv = worldCoord - worldCoord2;
                double dist = dv.NormLength;
                sb.AppendLine($"World座標 差值 (dX,dY) = ({dv.X:0.000}, {dv.Y:0.000}) mm");
                sb.AppendLine($"World座標 距離 = {dist:0.000} mm");
            }
        }
        void appendMotorCoords(StringBuilder sb, EzBloc bloc, EzBloc bloc2)
        {
            if (bloc == null)
                return;

            sb.AppendLine();

            bool isDistMeasuring = (bloc != null && bloc2 != null && bloc != bloc2);

            var trfs = new[] { TransCameraToMotor, TransCameraToMotor2 };

            foreach (var trfCamToMotor in trfs)
            {
                if (trfCamToMotor == null)
                    continue;

                var motorName = trfCamToMotor.Name.Contains("S2") ? "S2" : "S1";
                var motorCoord = trfCamToMotor.Trans(bloc.Center);

                // 當下 相機點位 對應之 馬達座標值
                sb.AppendLine($"{motorName}馬達座標 (X,Y) = ({motorCoord.X:0.000}, {motorCoord.Y:0.000}) mm");

                // 距離量測模式
                if (isDistMeasuring)
                {
                    var motorCoord2 = trfCamToMotor.Trans(bloc2.Center);
                    var dv = motorCoord - motorCoord2;
                    double dist = dv.NormLength;
                    sb.AppendLine($"{motorName}馬達座標 差值 (dX,dY) = ({dv.X:0.000}, {dv.Y:0.000}) mm");
                    sb.AppendLine($"{motorName}馬達座標 距離 = {dist:0.000} mm");
                    sb.AppendLine();
                }
                else
                {
                    // NODES
                    if (getRowCol(bloc, out int row, out int col))
                    {
                        //_trfModel.GetNodeCoords(ActiveCarrierID, row, col, out var _, out var _, out var motorCoordS1, out var motorCoordS2);
                        _trfModel.GetPlcExpectedCoords(ActiveCarrierID, row, col, out var motorCoordS1, out var motorCoordS2);
                        var coord = motorName=="S1" ? motorCoordS1 : motorCoordS2;
                        sb.AppendLine($"{motorName}預想座標 (X,Y) = ({coord.X:0.000}, {coord.Y:0.000}) mm");
                    }
                }
            }
        }
        void appendPlcCompensation(StringBuilder sb, EzBloc bloc, int row, int col)
        {
            if (bloc == null || row < 0 || col < 0 || _trfModel == null)
                return;

            (var motorD1, var motorD2, var worldDelta) = _trfModel.CalcPlcCompensation(ActiveCarrierID, bloc.Center, row, col);

            sb.AppendLine();
            sb.AppendLine($"PLC 補償量 S1 (ΔX,ΔY) = ({motorD1.X:0.000}, {motorD1.Y:0.000}) mm");
            sb.AppendLine($"PLC 補償量 S2 (ΔX,ΔY) = ({motorD2.X:0.000}, {motorD2.Y:0.000}) mm");
            //sb.AppendLine($"World 變動量 (ΔX,ΔY) = ({worldDelta.X:0.000}, {worldDelta.Y:0.000}) mm");
        }

        bool getRowCol(EzBloc bloc, out int row, out int col)
        {
            var rowCol = (bloc?.Tag as QuadLinkNode)?.rowCol;
            row = rowCol != null ? rowCol.Row : -1;
            col = rowCol != null ? rowCol.Col : -1;
            return row >= 0 && col >= 0;
        }
    }
}
