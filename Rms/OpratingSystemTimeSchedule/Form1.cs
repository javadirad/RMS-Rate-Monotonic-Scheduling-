using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OpratingSystemTimeSchedule
{

    public partial class Form1 : Form
    {
        private int DelayTime { get; } = 10;
        public int CurrentTime { get; set; }
        public int HistoryofCurrentTime { get; set; }
        private List<Process> _processList = new List<Process>();
        private readonly Queue _doneJob = new Queue();
        private readonly List<DoneJobLog> _joblog = new List<DoneJobLog>();
        public int Curstarttext { get; set; }

        public Form1( )
        {
            InitializeComponent();            
        }
        private void ReadJobFromFile()
        {
            int cnt = 0;
            Random rnd = new Random();
            using (var reader = new StreamReader(File.OpenRead("Jobs.csv")))
            {
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    
                    cnt++;
                    Color randomColor;
                    switch (cnt)
                    {
                        case 1:
                            randomColor = Color.Blue; break;
                        case 2:
                            randomColor = Color.DeepPink; break;
                        case 3:
                            randomColor = Color.BlueViolet; break;
                        case 4:
                            randomColor = Color.Chartreuse; break;
                        case 5:
                            randomColor = Color.DarkGreen; break;
                        default:
                            randomColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
                            break;
                    }
                    var line = reader.ReadLine();
                    if (line != null)
                    {
                        var values = line.Split(',');
                        for (int i = 0; i <= Convert.ToInt32(textBox1.Text)-1; i++)
                        {
                            Process p1 = new Process(values[0]+(i+1),i* (Convert.ToInt32(values[1]) + Convert.ToInt32(values[2])), 
                                Convert.ToInt32(values[1]), (i+1) *( Convert.ToInt32(values[1]) + Convert.ToInt32(values[2])),
                                Convert.ToInt32(values[2]), randomColor);                            
                            _processList.Add(p1);
                            listBox4.Items.Add($"processname = {p1.Name} starttime= {p1.Starttime} RemainTime={p1.Remaintime} DeadLineTime={p1.DeadLine}");
                            listBox4.SelectedIndex = listBox4.Items.Count - 1;

                        } 
                    }
                }

            }
        }

        private void ResetAllObjects()
        {
            listBox2.Items.Clear();
            listBox4.Items.Clear();
            _processList.Clear();
            _doneJob.Clear();
            _joblog.Clear();
            CurrentTime = 0;
            HistoryofCurrentTime = 0;
            groupBox3.Visible = false;
        }
        private Process GetProcess()
        {
            //_processList = _processList.OrderBy(p => p.Starttime).ThenBy(p=>p.Period).ThenBy(p=>p.Name).ToList();
            var filteredList = _processList.Where(p => p.Starttime <= CurrentTime).OrderBy(p => p.Period).ThenBy(p => p.Name).ToList();
            if (filteredList.Count > 0)
            {
                Process r = filteredList[0];
                _processList.Remove(r);
                return r;
            }

            return null;

        }

        private void ChangeCurrentTime()
        {
            CurrentTime = _processList.OrderBy(p => p.Starttime).First().Starttime;
        }
        string CheckProcessField()
        {
            if (_processList.Any(p => p.DeadLine < CurrentTime))
                return $"'{_processList.First(p => p.DeadLine < CurrentTime).Name}'    Field in time {CurrentTime}";
            return "";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ResetAllObjects();
            ReadJobFromFile();
            while (true)
            {
                System.Threading.Thread.Sleep(DelayTime);
                if (_processList.Count == 0) break;
                var p = GetProcess();
                if (p == null)
                {
                    if (_processList.Count == 0) break;
                    ChangeCurrentTime();
                    continue;
                }
                if (CurrentTime < p.Starttime)
                    CurrentTime = p.Starttime;
                DoneJobLog tmp = new DoneJobLog();
                tmp.Start = CurrentTime;
                tmp.Processname = p.Name;
                tmp.PrintColor = p.PrintColor;
                var currentTime = CurrentTime;
                p.DoJob(ref currentTime);
                CurrentTime = currentTime;
                tmp.End = currentTime;
                if (CheckProcessField() != "")
                {
                    tmp.Hint = CheckProcessField();
                }

                listBox2.Items.Add(tmp.Processname);
                listBox2.SelectedIndex = listBox2.Items.Count - 1;
                _joblog.Add(tmp);
                Refresh();
                if (tmp.Hint  !=null)
                    break;
            }
            HistoryofCurrentTime = CurrentTime;
            groupBox3.Visible = true;
        }

        private void DrawRotatedTextAt(Graphics gr, float angle,
            string txt, int x, int y, Font theFont, Brush theBrush)
        {
            GraphicsState state = gr.Save();
            gr.ResetTransform();
            gr.RotateTransform(angle);
            gr.TranslateTransform(x, y, MatrixOrder.Append);
            gr.DrawString(txt, theFont, theBrush, 0, 0);
            gr.Restore(state);
        }
        public void PaintLogs()
        { 
        var graphicsObj = CreateGraphics();
            Pen myPen = new Pen(Color.Black, 3);
            Pen clearPen = new Pen(BackColor, 3);
            int startx1 = 100;
            int starty1 = 200;
            int startx2 = 1100;
            int starty2 = 200;

            int startpoint = -15;
            
            graphicsObj.DrawLine(myPen, startx1, starty1, startx2, starty2);
            var myFont = new Font("Tahoma", 8);

            Brush myBrush = new SolidBrush(Color.Red);
             
            int starttext =  (CurrentTime / 100) * 100 ;
            
            int endtext = ((CurrentTime / 100)+ 1)  * 100;

            if (Curstarttext != starttext)
            {
                graphicsObj.DrawLine(clearPen, startx1, starty1+15, startx2, starty2+15);
            }
            Curstarttext = starttext;
            Pen pen1 = new Pen(Color.Blue, 2); 

            DrawRotatedTextAt(graphicsObj, -90, starttext.ToString(), startx1, starty1 + startpoint, myFont, myBrush);
            DrawRotatedTextAt(graphicsObj, -90, endtext.ToString(), startx2, starty2 + startpoint, myFont, myBrush);

        
            if (_joblog != null)
                foreach (var a in _joblog)
                {
                    pen1.Color = a.PrintColor; 
                    if ((a.Start >= starttext && a.Start <= endtext) || (a.End >= starttext && a.End <= endtext))
                    {
                        var fStart = a.Start >= starttext ? a.Start : starttext;
                        var fEnd = a.End <= endtext ? a.End : endtext;
                     
                        Brush aBrush = new SolidBrush( pen1.Color );
                        var x1 = ((startx2 - startx1) / 100) * (fStart % 100) + startx1;
                        DrawRotatedTextAt(graphicsObj, -90, fStart.ToString(), x1, starty1 + startpoint, myFont,
                            aBrush);
                        var x2 = ((startx2 - startx1) / 100) * (fEnd % 100) + startx1;
                        if (x2 < x1) x2 = startx2;
                        DrawRotatedTextAt(graphicsObj, -90, fEnd.ToString(), x2, starty1 + startpoint, myFont, aBrush);
                        graphicsObj.DrawLine( pen1, x1, starty1 + 5, x1, starty1 - 5);
                        graphicsObj.DrawLine( pen1, x2, starty1 + 5, x2, starty1 - 5);

                        graphicsObj.DrawLine( pen1, x1, starty1 - startpoint, x2,
                            starty1 - startpoint);
                        DrawRotatedTextAt(graphicsObj, +90, a.Processname,
                            x1 + (((startx2 - startx1) / 100) * ((fEnd - fStart) / 2)), starty1 + 20, myFont, aBrush);

                        Brush aHintBrush = new SolidBrush(Color.Red);
                        if (a.Hint !="")
                            DrawRotatedTextAt(graphicsObj, +90, a.Hint,
                               20+ x1 + (((startx2 - startx1) / 100) * ((fEnd - fStart) / 2)), starty1 -80, myFont, aHintBrush);
                    }
                }
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            PaintLogs();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (CurrentTime < 100) return;
            CurrentTime -= 100;
            Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (CurrentTime + 100 > HistoryofCurrentTime) return;
            CurrentTime += 100;
            Refresh();
        }
    }
    public class DoneJobLog
    {
        public int Start;
        public int End;
        public string Processname;
        public Color PrintColor;
        public string Hint;
    }
    public class Process
    {
   

        public Process(string name, int starttime, int runningtime, int deadline, int period,Color printColor)
        {
            Runningtime = runningtime;
            Starttime = starttime;
            DeadLine = deadline;
            Remaintime = runningtime;
            Name = name;
            Period = period;
            PrintColor = printColor;
        }
        public void DoJob(ref int currentTime)
        {
            currentTime = currentTime + Runningtime;
        }
        public string Name { get; set; }
        public int Runningtime { get; set; }
        public int Remaintime { get; set; }
        public int Starttime { get; set; }
        public int DeadLine { get; }
        public int Period { get; }
        public Color PrintColor { get; set; }
    }
}
