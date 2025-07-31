using System;

namespace JzDisplay.Interface
{
    /// <summary>
    /// OPDisplay 抽出來的 公開介面,
    /// DispUI 以外的模塊 可以調用.
    /// </summary>
    public interface IOpDisplay : 
                IOpDisplayNormal,
                IOpDisplayNormalB,
                IOpDisplayExp, 
                IOpDisplayEvents, 
                IDisposable
    {
    }

    /// <summary>
    /// OPDisplay 只給 DispUI 內部 使用的介面,
    /// 目前禁止 DispUI 以外的模塊 調用!
    /// </summary>
    internal interface IOpDisplayPriv : IOpDisplay
    {
        void FromStatusString(string statusstr);
        string ToStatusString();

        /// <summary>
        /// 將所有的 GeoFigures 的座標還原到 World Coordinate ?
        /// </summary>
        void MappingShape();
        void SimWheel(int delta = 1);
    }
}