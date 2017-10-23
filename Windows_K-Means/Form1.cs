using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows_K_Means
{
    public partial class Form1 : Form
    {
        static Graphics g_panel;
        List<tDot> lstDot;
        List<tDot> lstCenter;

        public string _log(string message, bool isError = false)
        {
            message = string.Format("\r\n{0}>> {1}", DateTime.Now.ToShortTimeString(), message);
            txtLog.Select(txtLog.Text.Length, message.Length);
            txtLog.SelectionColor = isError ? Color.Red : Color.Green;
            txtLog.AppendText(message);
            return message;
        }

        public Form1()
        {
            InitializeComponent();
            g_panel = panel1.CreateGraphics();
            lstDot = new List<tDot>();
            lstCenter = new List<tDot>();
            txtLog.ReadOnly = true;
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (lstDot.Any(q => q.has(e.Location)))
            {
                _log("trùng" + e.Location, true);
            }
            else
            {
                lstDot.Add(new tDot(e.Location));
                _log("paint" + e.Location);
                lblCount.Text = lstDot.Count.ToString();
            }
        }

        public class tDot
        {
            public Point Location { get; set; }
            public Color color { get; set; }
            public eType type { get; set; }

            public tDot(Point location)
            {
                this.Location = location;
                type = eType.Default;
                color = Color.Black;
                _paint();
            }

            public float KhoangCach(Point p)
            {
                return (float)Math.Sqrt(Math.Pow(Location.X - p.X, 2) + Math.Pow(Location.Y - p.Y, 2));
            }

            public void _paint(bool isCenter = false)
            {
                SolidBrush b = new SolidBrush(color);
                g_panel.FillEllipse(b, Location.X, Location.Y, isCenter ? 15 : 9, isCenter ? 15 : 9);
            }

            public bool has(Point p)
            {
                return p.Equals(Location);
            }

            public void Clear(bool isCenter = false)
            {
                SolidBrush b = new SolidBrush(SystemColors.Control);
                g_panel.FillEllipse(b, Location.X, Location.Y, isCenter ? 15 : 9, isCenter ? 15 : 9);
            }

            public tDot Update(tDot center)
            {
                color = center.color;
                type = center.type;
                _paint();
                return this;
            }

            public int SelectCenter(List<tDot> lstCenter)
            {
                if (lstCenter == null || lstCenter.Count <= 0) return -1;
                int id = 0;
                float kc = KhoangCach(lstCenter[id].Location);
                for (int i = 1; i < lstCenter.Count; i++)
                {
                    float k = KhoangCach(lstCenter[i].Location);
                    if (kc > k)
                    {
                        kc = k;
                        id = i;
                    }
                }
                Update(lstCenter[id]);
                return id;
            }
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            txtLog.ScrollToCaret();
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            foreach (var item in lstDot)
            {
                item.Clear(true);
            }
            lstDot.Clear();
            lblCount.Text = lstDot.Count.ToString();
            panel1.BackColor = SystemColors.Control;
            _log("Clear");
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            run_kMeans();
        }

        private void run_kMeans()
        {
            var rd = new Random();

            bool isEnd = false;

            int sW = panel1.Location.X;
            int eW = panel1.Width + sW;
            int sH = panel1.Location.Y;
            int eH = panel1.Height + sH;

            int k = (int)numK.Value;
            int asd = rd.Next();
            foreach (var item in lstCenter)
            {
                item.Clear(true);
            }

            lstCenter = new List<tDot>();
            for (int i = 0; i < k; i++)
            {
                tDot ob = new tDot(new Point()
                {
                    X = rd.Next(sW, eW),
                    Y = rd.Next(sH, eH)
                });
                ob.type = (eType)Enum.GetValues(typeof(eType)).GetValue(1 + k % 3);

                //KnownColor[] names = (KnownColor[])Enum.GetValues(typeof(KnownColor));
                //KnownColor randomColorName = names[rd.Next(names.Length)];
                //Color randomColor = Color.FromKnownColor(randomColorName);
                Color[] c = new Color[] { Color.Green, Color.Red, Color.Blue, Color.Yellow };
                asd = (asd + 1) % c.Length;
                ob.color = c[asd];
                ob._paint(true);
                _log(string.Format("Center {0}: {1}", i, ob.Location.ToString()));
                lstCenter.Add(ob);
            }

            int count = 0;

            List<int> lstIndex = new List<int>();
            for (int i = 0; i < lstDot.Count; i++)
            {
                lstIndex.Add(-1);
            }

            while (!isEnd)
            {
                isEnd = true;
                count++;
                _log("K-Means: " + count);

                for (int i = 0; i < lstDot.Count; i++)
                {
                    int id = lstDot[i].SelectCenter(lstCenter);
                    if (lstIndex[i] != id)
                    {
                        lstIndex[i] = id;
                        isEnd = false;
                    }
                }

                for (int i = 0; i < lstCenter.Count; i++)
                {
                    int sx = 0, sy = 0, cc = 0;
                    for (int j = 0; j < lstIndex.Count; j++)
                    {
                        if (lstIndex[j] == i)
                        {
                            sx += lstDot[j].Location.X;
                            sy += lstDot[j].Location.Y;
                            cc++;
                        }
                    }
                    if (cc <= 0) cc = 1;
                    lstCenter[i].Clear(true);
                    lstCenter[i].Location = new Point(sx / cc, sy / cc);
                    lstCenter[i]._paint(true);
                    _log(string.Format("Center_{0}: [{3} == {1} - {4}=={2}]", i, lstCenter[i].Location.X, lstCenter[i].Location.Y, sx / cc, sy / cc));
                }

                Thread.Sleep(1000);
            }
            _log("Done");
        }
    }

    public enum eType
    {
        Default, Tron, Vuong, TamGiac
    }
}
