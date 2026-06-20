#region AUTHOR
/*
 * 
 * Copyright (c) 2026 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2026-06-21 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using EzIO.Device;
using EzIO.Mem;
using System.Drawing;

namespace EzAoiChipLocQC.Drivers.IO
{
    public interface IPlcAtm20
    {
        IoDevice Device { get; }
        IAutoScan AutoScan { get; }
        IoMemory IoMem { get; }

        void ResetPlc(bool resetRecipeNum = false);

        /// <summary>
        /// 軟件準備
        /// </summary>
        bool bSoftwareReady { get; set; }

        /// <summary>
        /// 心跳
        /// </summary>
        bool bSyncClock { get; set; }

        /// <summary>
        /// 批號 (PLC -> PC) 
        /// </summary>
        string sLotID { get; }

        /// <summary>
        /// 子號 (PLC -> PC)
        /// </summary>
        string sStripID { get; }

        /// <summary>
        /// 機台配方名稱 (PLC -> PC)
        /// </summary>
        string sRecipeName { get; set; }

        /// <summary>
        /// 機台配方名稱切換確認 (PC->PLC)
        /// </summary>
        /// <remarks>
        /// 設定值: 1 == OK, 2 == NG
        /// </remarks>
        int iRecipeNum { get; set; }


        /// <summary>
        /// PLC -> PC 马达移动到开始位通知pc信号
        /// </summary>
        bool bScanStart { get; }
        
        /// <summary>
        /// PC -> PLC 线扫准备就绪
        /// </summary>
        bool bScanReady { get; set; }
        
        /// <summary>
        /// PC -> PLC完成信号和结果一起给
        /// </summary>
        bool bScanDone { get; set; }
        
        /// <summary>
        /// PLC->PC 线扫状态
        /// 1: 尺寸外观
        /// 2: 读码,
        /// 3: 空载台
        /// </summary>
        int iScanStatus { get; }
        
        /// <summary>
        /// 1: ok 
        /// 2: ng
        /// </summary>
        int iScanResult { get; set; }
        
        /// <summary>
        /// PC->PLC 单颗结果,1-Ok,2-外观Ng,3-空,4-读码NG,9-切割NG
        /// 单颗的线扫结果(预留300个)
        /// PLC用此信号来将每颗产品放到对应的Tray盘
        /// </summary>
        /// <param name="singleResults">ARRAY[0..299] OF INT</param>
        void iSingleResult(int[] singleResults);
        
        /// <summary>
        /// PC->PLC 读码结果,1-Ok,2-比对Ng,3-空,4-有码未读到
        /// 单颗产品的读码比对结果(预留300个)
        /// 视觉软件需要将读码结果保存在本地或服务器
        /// </summary>
        /// <param name="QRResults">ARRAY[0..299] OF INT</param>
        void iQRResult(int[] QRResults);
        
        /// <summary>
        /// PC->PLC 线扫偏移值XYR
        /// 单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个)
        /// 线扫引导功能启用时PLC需要用到这些值
        /// </summary>
        /// <param name="scanOffsets">ARRAY[0..899] OF REAL</param>
        void rScanOffset(float[] scanOffsets);

        #region 其他
        int iFlyStart { get; }
        bool bFlyDone { get; set; }
        bool bFlyReady { get; set; }

        /// <summary>
        /// PC->PLC 飞拍结果,1-Ok,2-Ng,3-空 单颗的飞拍结果
        /// </summary>
        /// <param name="flyResults">ARRAY[0..3] OF INT</param>
        void iFlyResult(int[] flyResults);
        /// <summary>
        /// PC->PLC 飞拍补偿(X,Y,R) 单颗的补偿结果([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共4个) 
        /// </summary>
        /// <param name="Offsets">ARRAY[0..11] OF REAL</param>
        void rOffset(float[] Offsets);

        /// <summary>
        /// 载台一纠偏计算位置
        /// </summary>
        /// <param name="pt0">第一排吸嘴</param>
        /// <param name="pt1">第二排吸嘴</param>
        void SetStage1(int eIndex, PointF pt0, PointF pt1);
        /// <summary>
        /// 载台二纠偏计算位置
        /// </summary>
        /// <param name="pt0">第一排吸嘴</param>
        /// <param name="pt1">第二排吸嘴</param>
        void SetStage2(int eIndex, PointF pt0, PointF pt1);
        /// <summary>
        /// 目前 PLC 使用那个线扫平台
        /// 1: 平台一
        /// 2: 平台二
        /// </summary>
        int iScanStage { get; }

#if (false)
        /// <summary>
        /// 控制 吸嘴排1的真空 ON / OFF
        /// </summary>
        bool VacuumSucker1 { get; set; }
        /// <summary>
        /// 取得 吸嘴排的安全高度Z (PLC 配方設定)
        /// </summary>
        double GetSafeZ(SuckerRowEnum suckerID);
        /// <summary>
        /// 取得 飛拍相機的對焦高度Z (PLC 配方設定)
        /// </summary>
        double GetFlyCamFocusZ();
        /// <summary>
        /// 取得 Sucker1 (或 Sucker2) 的 飛拍相機 觸發位置 (PLC 配方設定)
        /// </summary>
        double GetFlyCamTriggerX(SuckerRowEnum suckerID = SuckerRowEnum.S1);
        /// <summary>
        /// 取得 飛拍相機 拍照時的 Y軸 位置 (PLC 配方設定)
        /// </summary>
        double GetFlyCamSnapshotY();
#endif

        #endregion
    }
}