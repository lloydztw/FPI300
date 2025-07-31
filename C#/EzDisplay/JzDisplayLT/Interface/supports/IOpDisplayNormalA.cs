using MoveGraphLibrary;

namespace JzDisplay.Interface
{
    /// <summary>
    /// OPDisplay 常用函式
    /// </summary>
    public interface IOpDisplayNormal
    {
        void SetDisplayType(DisplayTypeEnum displaytype);
        void SetImode(int iMode = 0);
        bool ISMOUSEDOWN { get; }

        void DefaultView();
        void ReDraw();

        void ClearAll();
        void ClearMover();
        void ClearStaticMover();

        void SetMover(Mover mover);
        void SetStaticMover(Mover staticmover);
        void MoveMover(int x, int y);
        void SizeMover(int x, int y);
        void Lock(int level, bool isonly);

        /// <summary>
        /// 將所有的 GeoFigure 進行座標轉換:
        /// <br/> .ToMovingObject : 轉換到 Viewport Coordinate (順)
        /// </summary>
        void RefreshDisplayShape();
    }
}