using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaserAlignDX.BasicSpace
{
    public class CollectResultClass
    {
        public CollectResultClass() { }
        public RectangleF loc = new RectangleF(0, 0, 10, 10);
        public bool ispass = false;
        public string desc = string.Empty;
        public CollectResultClass Clone()
        {
            CollectResultClass cl = new CollectResultClass();
            cl.loc = new RectangleF(this.loc.X, this.loc.Y, loc.Width, loc.Height);
            cl.desc = desc;
            cl.ispass = ispass;

            return cl;
        }
    }
}
