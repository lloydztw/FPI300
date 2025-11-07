#region AUTHOR
/*
 * 
 * Copyright (c) 2025 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2025-11-05 整理架構 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using LaserAlignDX.AoiModel;
using LaserAlignDX.OPSpace;
using LaserAlignDX.OPSpace.RecipeSpace;
using System.Collections.Generic;
using System.Text;


namespace LaserAlignDX.Model
{
    /// <summary>
    /// PLC 返回數據 打包器
    /// </summary>
    public class PlcDataPacker
    {
        #region GLOBAL_MESS
        static RecipeFPIX3Class _xRecipe => RecipeFPIX3Class.Instance;
        #endregion

        /// <summary>
        /// 单颗的线扫结果(预留300个) PLC用此信号来将每颗产品放到对应的Tray盘
        /// </summary>
        /// <returns>ARRAY[0..299] OF INT PC->PLC 单颗结果, 1:OK 2:外观NG, 3:空, 4:读码NG, 9:切割NG </returns>
        public static int[] GetSingleResult(bool forceAllPass)
        {
            VerifyFinalResultCells();

            //---------------------------------------------------------------------------------------------------------------------
            // PC->PLC 单颗结果:
            //  1-Ok, 2-外观Ng, 3-空, 4-读码NG, 8-切割偏移NG, 9-切割NG
            //---------------------------------------------------------------------------------------------------------------------

            int i = 0;
            int[] states = new int[_xRecipe.xRegionCells.Count];

            foreach (RegionCellX3Class cell in IterFinalResultCells())
            {
                if (forceAllPass)
                {
                    var go = (cell != null && 
                              cell.IsLocated() && 
                             !cell.IsEmptyPlaceHold() && 
                             !cell.IsAmbiguousBloc());

                    // 1 (當成 PASS 讓 PLC 吸走)
                    // 3 (回報 NG_EMPTY 讓 PLC "不" 吸走 該區塊)
                    states[i] = (int)(go ? PlcResultCode.OK : PlcResultCode.NG_EMPTY);
                }
                else
                {
                    //(A) PASS
                    if (cell.IsResultPass()) 
                    {
                        states[i] = (int)PlcResultCode.OK;                          // 1
                    }
                    //(B) 空格
                    else if (cell.IsEmptyPlaceHold())
                    {
                        states[i] = (int)PlcResultCode.NG_EMPTY;                    // 3 (回報 NG_EMPTY 讓 PLC "不" 吸走 該區塊)
                    }
                    //(C) 疑似有料之不明區塊
                    else if (cell.IsAmbiguousBloc())
                    {
                        states[i] = (int)PlcResultCode.NG_EMPTY;                    // 3 (回報 NG_EMPTY 讓 PLC "不吸" 該區塊)
                    }
                    //(D) 其他 結果
                    else
                    {
                        switch (cell.FinalInspectResult)
                        {
                            // 二維碼 NG
                            //case InspectReason.INS_2DERR:
                            //case InspectReason.INS_2DMAPNG:
                            case InspectReason.NG_QRCODE_ERR:
                            case InspectReason.NG_QRCODE_COMPARE:
                                states[i] = (int)PlcResultCode.NG_QRCODE_ERR;       // 4
                                break;

                            // 尺寸量測 NG
                            //case InspectReason.INS_CUTTINGERR:
                            case InspectReason.NG_CUT:
                                states[i] = (int)PlcResultCode.NG_CUT;              // 9
                                break;

                            // 邊隙 NG
                            //case InspectReason.INS_PADEDGEGAPERR:
                            case InspectReason.NG_EDGE_GAP:
                                states[i] = (int)PlcResultCode.NG_EDGE_GAP;         // 8
                                break;

                            // 其他 NG
                            default:
                                states[i] = (int)PlcResultCode.NG_APPEARANCE;       // 2
                                break;
                        }
                    }
                }

                i++;
            }

            return states;
        }

        /// <summary>
        /// 单颗产品的读码比对结果(预留300个) 视觉软件需要将读码结果保存在本地或服务器
        /// </summary>
        /// <returns>ARRAY[0..299] OF INT PC->PLC 读码结果, 1:OK, 2:比对NG, 3:空, 4:有码未读到</returns>
        public static int[] GetQrResult()
        {
            //PC->PLC 读码结果, 1-Ok, 2-比对Ng, 3-空, 4-有码未读到

            int i = 0;
            int[] states = new int[_xRecipe.xRegionCells.Count];

            foreach (RegionCellX3Class cell in IterFinalResultCells(_xRecipe.xRegionCells))
            {
                //if (cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                //    states[i] = (int)PlcResultCode.OK;                                // 1;
                //else if (cell.inspectReason == InspectReason.INS_ALIGNERR)
                //    states[i] = (int)PlcResultCode.NG_EMPTY;                          // 3;
                //else if (cell.inspectReason == InspectReason.INS_2DERR)
                //    states[i] = (int)PlcResultCode.NG_QRCODE_ERR;                     // 4;
                //else if (cell.inspectReason == InspectReason.INS_2DMAPNG)
                //    states[i] = (int)PlcResultCode.NG_APPEARANCE;                     // 2; (why ???)
                //else
                //    states[i] = (int)PlcResultCode.OK;                                // 1;

                //(A) PASS
                if (cell.IsResultPass())
                {
                    states[i] = (int)PlcResultCode.OK;                                  // 1
                }
                //(B) 空格
                else if (cell.IsEmptyPlaceHold())
                {
                    states[i] = (int)PlcResultCode.NG_EMPTY;                            // 3 (回報 NG_EMPTY 讓 PLC "不" 吸走 該區塊)
                }
                //(C) 疑似有料之不明區塊
                else if (cell.IsAmbiguousBloc())
                {
                    states[i] = (int)PlcResultCode.NG_EMPTY;                            // 3 (回報 NG_EMPTY 讓 PLC "不吸" 該區塊)
                }
                //(D) 其他 結果
                else
                {
                    switch (cell.FinalInspectResult)
                    {
                        // 二維碼 讀取錯誤
                        // case InspectReason.INS_2DERR:
                        case InspectReason.NG_QRCODE_ERR:
                            states[i] = (int)PlcResultCode.NG_QRCODE_ERR;               // 4
                            break;

                        // 二維碼 比對錯誤
                        // case InspectReason.INS_2DMAPNG:
                        case InspectReason.NG_QRCODE_COMPARE:
                            states[i] = (int)PlcResultCode.NG_APPEARANCE;               // 2
                            break;

                        default:
                            states[i] = (int)PlcResultCode.OK;                          // 1
                            break;
                    }
                }

                i++;
            }
            return states;
        }

        /// <summary>
        /// 单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个) 线扫引导功能启用时PLC需要用到这些值
        /// </summary>
        /// <returns>ARRAY[0..899] OF REAL PC->PLC 线扫偏移值XYR</returns>
        public static float[] GetScanOffset()
        {
            //PC->PLC 线扫偏移值XYR
            //单颗产品的偏移值([0]-X,[1]-Y,[2]-R，[3]-X,[4]-Y,[5]-R…依次共300个)

            int i = 0;
            float[] states = new float[_xRecipe.xRegionCells.Count * 3];

            foreach (RegionCellX3Class cell in IterFinalResultCells(_xRecipe.xRegionCells))
            {
                if (cell.IsResultPass())                    //cell.inspectReason == InspectReason.PASS && cell.inspectReasons.Count == 0)
                {
                    states[i] = cell.RunX;
                    states[i + 1] = cell.RunY;
                    states[i + 2] = cell.RunAngle;
                }
                else if (cell.IsEmptyPlaceHold())     //cell.inspectReason == InspectReason.INS_ALIGNERR)
                {
                    states[i] = 0;
                    states[i + 1] = 0;
                    states[i + 2] = 0;

                }
                else
                {
                    states[i] = cell.RunX;
                    states[i + 1] = cell.RunY;
                    states[i + 2] = cell.RunAngle;
                }
                i += 3;
            }
            return states;
        }

        /// <summary>
        /// 調試用: 檢查最後 cell (row,col) 是否正確
        /// </summary>
        public static bool VerifyFinalResultCells()
        {
            // 實機跑線版本: 不執行 此調試驗證函式
            if (!Traveller106.Universal.IsNoUseCCD)
                return true;

            StringBuilder errors = new StringBuilder();
            int rows = _xRecipe.xCamGrid1.Rows;
            int cols = _xRecipe.xCamGrid1.Cols;
            var table = new RegionCellX3Class[rows, cols];
            int index = -1;
            bool ok = true;

            foreach (var cell in IterFinalResultCells())
            {
                ++index;

                if (cell == null)
                {
                    errors.AppendLine($"[{index}] Null cell");
                    ok = false;
                    continue;
                }

                var r = cell.CellRow;
                var c = cell.CellCol;
                if (r >= rows && c >= cols)
                {
                    errors.AppendLine($"[{index}] 異常 [r={r}, c={c}] 超出範圍 [{rows}, {cols}]");
                    ok = false;
                    continue;
                }

                if (table[r, c] != null)
                {
                    errors.AppendLine($"[{index}] 重複 [r={r}, c={c}]");
                    ok = false;
                    continue;
                }

                table[r, c] = cell;
            }

            // 檢查是否按照 Zigzag 順序
            index = -1;
            foreach ((int r, int c) in Zigzag.IterZigzag(rows, cols))
            {
                ++index;
                var cell = table[r, c];
                if (cell == null)
                {
                    errors.AppendLine($"[r={r}, c={c}] 缺少 cell");
                    ok = false;
                    break;
                }
                if(cell.Index != index)
                {
                    errors.AppendLine($"[r={r}, c={c}] Zigzag 順序錯誤!");
                    ok = false;
                    break;
                }

            }

            if (!ok)
            {
                var errMsg = "PLC 返回數據異常 :\n\r" + errors.ToString();
                throw new System.Exception(errMsg);
            }
            
            return ok;
        }

        /// <summary>
        /// 枚舉 檢測後  實際 持有量測數據的 Cell
        /// (以 OutGridLink 優先)
        /// </summary>
        public static IEnumerable<RegionCellX3Class> IterFinalResultCells(IEnumerable<RegionCellX3Class> cells = null)
        {
            if (cells == null)
                cells = _xRecipe.xRegionCells;

            foreach (var cell in cells)
            {
                var outGridCell = cell?.OutGridLink;
                if (outGridCell != null)
                    yield return outGridCell;
                else
                    yield return cell;
            }
        }
    }
}
