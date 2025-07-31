using System.Collections.Generic;

namespace JzDisplay.Interface
{
    /// <summary>
    /// OPDisplay 實驗函式 (Victor 所標記的 Experiments)
    /// </summary>
    public interface IOpDisplayExp
    {
        void AddRect();
        void AddShape(ShapeEnum shape);
        void DelShape();
        void HoldSelect();
        void ReleaseSelect();

        void MappingLsbSelect(List<int> lsbSelectList);
        void MappingSelect();
        //void SimWheel(int delta = 1);
    }
}