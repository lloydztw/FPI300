using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaserAlignDX.BasicSpace
{
    public class ShowRectClass
    {
        public int Index { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RectangleF Bounds { get; set; } = new RectangleF(0, 0, 100, 100);
        public int Result { get; set; } = 0;
        public bool IsPass { get; set; } = false;
    }
}
