using System;
using System.Collections.Generic;
using System.Text;


namespace JetEazy.Diagnostics
{
    public class EzTiming
    {
        DateTime m_tm0 = DateTime.Now;
        List<string> m_tags = new List<string>();
        List<double> m_times = new List<double>();

        public void Reset()
        {
            m_tm0 = DateTime.Now;
            m_tags.Clear();
            m_times.Clear();
        }
        public void Mark(string tag)
        {
            if (tag != null)
            {
                var ts = DateTime.Now - m_tm0;
                m_times.Add(ts.TotalMilliseconds);
                m_tags.Add(tag);
            }
            m_tm0 = DateTime.Now;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < m_tags.Count; i++)
            {
                sb.Append(m_tags[i]);
                sb.Append(" = ");
                sb.Append((int)m_times[i]);
                sb.Append(" ms\n");
            }
            return sb.ToString();
        }
    }
}
