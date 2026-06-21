#region AUTHOR
/*
 * 
 * Copyright (c) 2024 JetEazy Corp. All rights reserved.
 * 
 * REVISION:
 *      2024-09-25 初稿 (by LeTian Chang)
 * 
 * http://www.jeteazy.com
 * https://github.com/lloydztw
 * https://lloydztw.github.io/mysite/
 * 
 */
#endregion

using JetEazy.Match;
using LeTian.JxProps;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace EzAoiChipLocQC.Model
{
    public class JxGoldenGrid : JxContainer
    {
        public JxText GridData = new JxText("GridData", "", description: "(Hidden)");

        public JxGoldenGrid(string name) : base(name, "(Hidden)")
        {
        }
        public JxGoldenGrid()
        {
            Description = "(Hidden)";
        }

        public override void OnBindingSubItems()
        {
            //綁定以下成員, 會自動顯示在GUI編輯視窗.
            BindItems(new IProp[] {
                GridData,
            });
            base.OnBindingSubItems();
        }
        protected override void OnDisposing()
        {
            base.OnDisposing();
            _cache?.Dispose();
            _cache = null;
        }

        /// <summary>
        /// Runtime Helper Property
        /// </summary>
        [JsonIgnore]
        public EzBlocsGrid GoldenGrid
        {
            get
            {
                if (_cache == null)
                    _cache = get_goldenGrid_from_jx();
                return _cache;
            }
            set
            {
                if (_cache != value)
                {
                    _cache?.Dispose();
                    _cache = value;
                    set_goldenGrid_to_jx(_cache);
                }
            }
        }

        #region PRIVATE_MEMBERS
        EzBlocsGrid _cache = null;
        EzBlocsGrid get_goldenGrid_from_jx()
        {
            var s = new EzBlocsGridSerializer();
            s.Deserialize(GridData.Value, out EzBlocsGrid grid);
            return grid;
        }
        void set_goldenGrid_to_jx(EzBlocsGrid grid)
        {
            var s = new EzBlocsGridSerializer();
            GridData.Value = s.Serialize(grid);
        }
        #endregion
    }


    class EzBlocsGridSerializer
    {
        const char SEP_B = ':';

        public string Serialize(EzBlocsGrid grid)
        {
            if (grid == null)
                return "";
            return Serialize(grid.IterBlocs());
        }
        public bool Deserialize(string str, out EzBlocsGrid grid)
        {
            grid = null;
            try
            {
                if (string.IsNullOrEmpty(str))
                    return false;

                bool ok = Deserialize(str, out List<EzBloc> blocs);
                if (!ok)
                    return false;

                EzBlocsGridBuilder builder = new EzBlocsGridBuilder();
                grid = builder.Build(blocs);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return false;
        }

        public string Serialize(IEnumerable<EzBloc> blocs)
        {
            int count = 0;
            var sb = new StringBuilder();

            foreach (var bloc in blocs)
            {
                if (bloc != null)
                {
                    if (count > 0)
                        sb.Append(SEP_B);
                    string str = Serialize(bloc);
                    sb.Append(str);
                    count++;
                }
            }
            if (count == 0)
                return "";

            return sb.ToString().Trim(SEP_B);
        }
        public bool Deserialize(string str, out List<EzBloc> blocs)
        {
            blocs = null;

            if (string.IsNullOrEmpty(str))
                return false;

            blocs = new List<EzBloc>();

            var tokens = str.Split(SEP_B);
            foreach (var token in tokens)
            {
                if (Deserialize(token, out EzBloc b))
                    blocs.Add(b);
            }

            return blocs.Count > 0;
        }

        public string Serialize(EzBloc bloc)
        {
            if (bloc != null)
                return $"{bloc.Rect.X},{bloc.Rect.Y},{bloc.Rect.Width},{bloc.Rect.Height},{bloc.Score:0.000}";
            return "";
        }
        public bool Deserialize(string str, out EzBloc bloc)
        {
            bloc = null;
            
            if (string.IsNullOrEmpty(str))
                return false; 
            
            var strs = str.Split(',');
            if (strs.Length < 5)
                return false;

            bool ok = true;
            ok &= int.TryParse(strs[0], out int x);
            ok &= int.TryParse(strs[1], out int y);
            ok &= int.TryParse(strs[2], out int w);
            ok &= int.TryParse(strs[3], out int h);
            ok &= double.TryParse(strs[4], out double score);
            if (ok)
                bloc = new EzBloc(new System.Drawing.Rectangle(x, y, w, h), score);

            return ok;
        }
    }
}
