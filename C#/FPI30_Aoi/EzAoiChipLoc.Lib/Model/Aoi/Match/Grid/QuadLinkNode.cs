#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-10-03 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.QMath;
using JetEazy.QxCollections;
using T = JetEazy.Match.EzBloc;


namespace JetEazy.Match
{
    public class QuadLinkNode
    {
        public enum Dir : int
        {
            Right = 0,
            Down = 1,
            Left = 2,
            Up = 3,
        }

        public class Link
        {
            public T Next;
            public QVector Vector;

            public Link() { }
            public Link(T next, QVector vect)
            {
                Next = next;
                Vector = vect;
            }
            public static implicit operator T(Link link)
            {
                return link != null ? link.Next : default(T);
            }
            public static implicit operator QVector(Link link)
            {
                var v = link != null ? link.Vector : null;
                return v;
            }
            public void Set(EzBloc next)
            {
                this.Next = next;
            }
            public void Set(QVector vect)
            {
                this.Vector = vect;
            }
        }

        #region PRIVATE_DATA
        Link[] _links = new Link[4];
        #endregion

        public Link this[Dir dir]
        {
            get => _links[(int)dir];
            set => _links[(int)dir] = value;
        }
        public Link Right { get => this[Dir.Right]; set => this[Dir.Right] = value; }
        public Link Down { get => this[Dir.Down]; set => this[Dir.Down] = value; }
        public Link Left { get => this[Dir.Left]; set => this[Dir.Left] = value; }
        public Link Up { get => this[Dir.Up]; set => this[Dir.Up] = value; }
        public Link R { get => this[Dir.Right]; set => this[Dir.Right] = value; }
        public Link D { get => this[Dir.Down]; set => this[Dir.Down] = value; }
        public Link L { get => this[Dir.Left]; set => this[Dir.Left] = value; }
        public Link U { get => this[Dir.Up]; set => this[Dir.Up] = value; }

        public QxRowCol rowCol;
    }
}
