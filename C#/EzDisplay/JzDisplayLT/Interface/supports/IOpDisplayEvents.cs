using System.Drawing;

namespace JzDisplay.Interface
{
    public delegate void MoverHandler(MoverOpEnum moverOp, string opString);
    public delegate void AdjustHandler(PointF ptOffset);
    public delegate void CaputreHandler(RectangleF rectf);
    public delegate void DebugHandler(string opString);

    /// <summary>
    /// OPDisplay 的事件與其發射函式
    /// </summary>
    public interface IOpDisplayEvents
    {
        event AdjustHandler AdjustAction;
        event CaputreHandler CaptureAction;
        event DebugHandler DebugAction;
        event MoverHandler MoverAction;

        void OnAdjustAction(PointF ptfoffset);
        void OnCapture(RectangleF rectf);
        void OnDebug(string opstring);
        void OnMover(MoverOpEnum moverop, string opstring);
    }
}
