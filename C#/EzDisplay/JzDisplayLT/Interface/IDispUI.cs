using System.Drawing;
using System.Windows.Forms;

namespace JzDisplay.Interface
{
    /// <summary>
    /// DispUI 對外的介面, 
    /// 基本上涵蓋所有 IOpDisplay
    /// 加上幾個額外特有的 Functions
    /// </summary>
    public interface IDispUI : IOpDisplay
    {
        int DisplayWidth{ get; }
        int DisplayHeight{ get; }

        void Initial(float maxratio = 10f, float minratio = 0.1f);
        bool DispUIload(Form myForm = null);

        void SaveStatus(string pathname);
        void LoadStatus(string pathname);


    }
}