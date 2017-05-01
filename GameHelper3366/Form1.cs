using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GameHelper3366
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer tim = new System.Windows.Forms.Timer();
            tim.Interval = 200;
            tim.Tick += delegate
            {
                Point p = new Point(MousePosition.X, MousePosition.Y);//取置顶点坐标 
                label1.Text = $"({p.X},{p.Y})";
                Color color = GetPointColor(MousePosition.X, MousePosition.Y);
                label4.Text = $"Rgb({color.R},{color.G},{color.B})";
            };
            tim.Start();
        }

        private string Lettetr { get; set; } = "ABCDEFGHOJKLMNOPQRSTUVWXYX1234567890";
        private string GetLetterFrom(int index)
        {
            if (index < Lettetr.Length) return Lettetr.Substring(index, 1);
            return "我";
        }

        private Color GetPointColor(int x,int y)
        {
            Point p = new Point(x, y);//取置顶点坐标 
            IntPtr hdc = GetDC(new IntPtr(0));//取到设备场景(0就是全屏的设备场景) 
            int c = GetPixel(hdc, p);//取指定点颜色 
            int r = (c & 0xFF);//转换R 
            int g = (c & 0xFF00) / 256;//转换G 
            int b = (c & 0xFF0000) / 65536;//转换B 
            return Color.FromArgb(r, g, b);
        }


        [DllImport("user32.dll")]//取设备场景 
        private static extern IntPtr GetDC(IntPtr hwnd);//返回设备场景句柄 
        [DllImport("gdi32.dll")]//取指定点颜色 
        private static extern int GetPixel(IntPtr hdc, Point p);

        private Bitmap GetString(string str)
        {
            Bitmap bitmap = new Bitmap(460, 300);
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);
            StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(str, Font, Brushes.Black, new Rectangle(0, 0, 460, 300), sf);
            sf.Dispose();
            g.Dispose();
            return bitmap;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("还未输入数据");
                return;
            }
            pictureBox1.Image = GetString("开始获取数据...");
            Thread thread = new Thread(new ThreadStart(ThreadDeal));
            thread.IsBackground = true;
            thread.Start();
            label5.Text = "开始";
        }

        private Bitmap GetString(List<Color> colors)
        {
            Bitmap bitmap = new Bitmap(460, 300);
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);

            int height = 12;
            for (int i = 0; i < colors.Count; i++)
            {
                Rectangle rec = new Rectangle(0, i * height, 20, height);
                Brush brush = new SolidBrush(colors[i]);
                g.FillRectangle(brush, rec);
                brush.Dispose();
                g.DrawString($"({colors[i].R},{colors[i].G},{colors[i].B})", Font, Brushes.Black, new Point(25, i * height));
            }


            g.Dispose();
            return bitmap;
        }

        private Bitmap GetString(List<Color> colors, int[,] chess, int x, int y)
        {
            Bitmap bitmap = new Bitmap(460, 300);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);
            StringFormat sf = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            for (int i = 0; i < 23; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    Rectangle rec = new Rectangle(20 * i, 20 * j, 20, 20);
                    if (chess[i, j] > 0)
                    {
                        Brush brush = new SolidBrush(colors[chess[i, j] - 1]);
                        g.FillRectangle(brush, rec);
                        brush.Dispose();
                        g.DrawString(GetLetterFrom(chess[i, j] - 1), Font, Brushes.Black, rec, sf);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.WhiteSmoke, rec);
                    }
                    g.DrawRectangle(Pens.LightGray, rec);
                }
            }
            if (x >= 0 && y >= 0)
            {
                Rectangle rectage = new Rectangle(20 * x + 5, 20 * y + 5, 10, 10);
                g.FillEllipse(Brushes.DimGray, rectage);
            }

            sf.Dispose();
            g.Dispose();
            return bitmap;
        }
        private Bitmap GetAllString(List<Color> colors)
        {
            Bitmap bitmap = new Bitmap(460, 300);
            Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);

            int location_x = 0;
            int height = -20;
            for (int i = 0; i < colors.Count; i++)
            {
                if (i != 0 && i % 15 == 0)
                {
                    location_x += 20;
                    height = 0;
                }
                else
                {
                    height += 20;
                }
                Rectangle rec = new Rectangle(location_x, height, 20, 20);
                Brush brush = new SolidBrush(colors[i]);
                g.FillRectangle(brush, rec);
                brush.Dispose();
            }
            g.Dispose();
            return bitmap;
        }

        private bool 判断是否粘情况(int[,] chess, int x, int y)
        {
            if (x > 0 && chess[x - 1, y] == chess[x, y]) return true;
            if (x < 22 && chess[x + 1, y] == chess[x, y]) return true;
            if (y > 0 && chess[x, y - 1] == chess[x, y]) return true;
            if (y < 14 && chess[x, y + 1] == chess[x, y]) return true;
            return false;
        }

        private void ThreadDeal()
        {
            List<Color> all_color = new List<Color>();
            List<Color> all_single_color = new List<Color>();
            int x = Convert.ToInt32(textBox1.Text);
            int y = Convert.ToInt32(textBox2.Text);



            int[,] int_chess = new int[23, 15];

            //获取所有的颜色
            for (int i = 0; i < 23; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    Color color = GetPointColor(x + 25 * i, y + 25 * j);
                    all_color.Add(color);

                    if (color != Color.FromArgb(247, 247, 247) &&
                        color != Color.FromArgb(237, 237, 237))
                    {
                        if (!all_single_color.Contains(color))
                        {
                            all_single_color.Add(color);
                        }
                        int_chess[i, j] = all_single_color.IndexOf(color) + 1;
                    }
                }
            }

            Invoke(new Action(() =>
            {
                pictureBox1.Image = GetString(all_single_color, int_chess,-1,-1);

                if (all_single_color.Count != 10)
                {
                    label5.Text = "错误";
                }
                
            }));
            if (all_single_color.Count == 10)
            {
                int game_round = 0;
                Thread.Sleep(2000);
                while (!IsGameOver(int_chess))
                {
                    game_round++;
                    int is_game_over = 0;
                    for (int i = 0; i < 23; i++)
                    {
                        for (int j = 0; j < 15; j++)
                        {
                            if (int_chess[i, j] == 0)
                            {
                                //开始寻找
                                int left = 0;
                                int k_left = i - 1;
                                while (k_left >= 0)
                                {
                                    if (int_chess[k_left, j] > 0)
                                    {
                                        left = int_chess[k_left, j];
                                        break;
                                    }
                                    k_left--;
                                }
                                int right = 0;
                                int k_right = i + 1;
                                while (k_right < 23)
                                {
                                    if (int_chess[k_right, j] > 0)
                                    {
                                        right = int_chess[k_right, j];
                                        break;
                                    }
                                    k_right++;
                                }
                                int up = 0;
                                int k_up = j - 1;
                                while (k_up >= 0)
                                {
                                    if (int_chess[i, k_up] > 0)
                                    {
                                        up = int_chess[i, k_up];
                                        break;
                                    }
                                    k_up--;
                                }
                                int down = 0;
                                int k_dowm = j + 1;
                                while (k_dowm < 15)
                                {
                                    if (int_chess[i, k_dowm] > 0)
                                    {
                                        down = int_chess[i, k_dowm];
                                        break;
                                    }
                                    k_dowm++;
                                }



                                int time_sleep = Convert.ToInt32(textBox3.Text);

                                if (left == right && left == down && left == up && left != 0)
                                {
                                    is_game_over++;
                                    int_chess[k_left, j] = 0;
                                    int_chess[k_right, j] = 0;
                                    int_chess[i, k_up] = 0;
                                    int_chess[i, k_dowm] = 0;
                                    SetPointAndClick(x + 25 * i, y + 25 * j);
                                    Invoke(new Action(() =>
                                    {
                                        pictureBox1.Image = GetString(all_single_color, int_chess, i, j);
                                    }));
                                    Thread.Sleep(time_sleep);
                                    continue;
                                }

                                if (left == right && left != 0 && left != up && left != down)
                                {
                                    is_game_over++;

                                    if (判断是否粘情况(int_chess, k_left, j) || 判断是否粘情况(int_chess, k_right, j) || game_round % 2 == 0)
                                    {
                                        int_chess[k_left, j] = 0;
                                        int_chess[k_right, j] = 0;
                                        if (up == down && up != 0)
                                        {
                                            int_chess[i, k_up] = 0;
                                            int_chess[i, k_dowm] = 0;
                                        }
                                        SetPointAndClick(x + 25 * i, y + 25 * j);
                                        Invoke(new Action(() =>
                                        {
                                            pictureBox1.Image = GetString(all_single_color, int_chess, i, j);
                                        }));
                                        Thread.Sleep(time_sleep);
                                    }
                                    continue;
                                }
                                if (left == up && left != 0 && left != down && left != right)
                                {
                                    is_game_over++;

                                    if (判断是否粘情况(int_chess, k_left, j) || 判断是否粘情况(int_chess, i, k_up) || game_round % 2 == 0)
                                    {
                                        int_chess[k_left, j] = 0;
                                        int_chess[i, k_up] = 0;
                                        if (right == down && right != 0)
                                        {
                                            int_chess[i, k_dowm] = 0;
                                            int_chess[k_right, j] = 0;
                                        }
                                        SetPointAndClick(x + 25 * i, y + 25 * j);
                                        Invoke(new Action(() =>
                                        {
                                            pictureBox1.Image = GetString(all_single_color, int_chess, i, j);
                                        }));
                                        Thread.Sleep(time_sleep);
                                    }
                                    continue;
                                }
                                if (left == down && left != 0 && left != up && left != right)
                                {
                                    is_game_over++;
                                    if (判断是否粘情况(int_chess, k_left, j) || 判断是否粘情况(int_chess, i, k_dowm) || game_round % 2 == 0)
                                    {
                                        int_chess[k_left, j] = 0;
                                        int_chess[i, k_dowm] = 0;
                                        if (right == up && right != 0)
                                        {
                                            int_chess[i, k_up] = 0;
                                            int_chess[k_right, j] = 0;
                                        }
                                        SetPointAndClick(x + 25 * i, y + 25 * j);
                                        Invoke(new Action(() =>
                                        {
                                            pictureBox1.Image = GetString(all_single_color, int_chess, i, j);
                                        }));
                                        Thread.Sleep(time_sleep);
                                    }
                                    continue;
                                }

                                if (up == down && up != 0 && up != left && up != right)
                                {
                                    is_game_over++;
                                    if (判断是否粘情况(int_chess, i, k_up) || 判断是否粘情况(int_chess, i, k_dowm) || game_round % 2 == 0)
                                    {
                                        int_chess[i, k_up] = 0;
                                        int_chess[i, k_dowm] = 0;
                                        if (right == left && right != 0)
                                        {
                                            int_chess[k_left, j] = 0;
                                            int_chess[k_right, j] = 0;
                                        }
                                        SetPointAndClick(x + 25 * i, y + 25 * j);
                                        Invoke(new Action(() =>
                                        {
                                            pictureBox1.Image = GetString(all_single_color, int_chess, i, j);
                                        }));
                                        Thread.Sleep(time_sleep);
                                    }
                                    continue;
                                }
                                if (up == right && up != 0 && up != left && up != down)
                                {
                                    is_game_over++;
                                    if (判断是否粘情况(int_chess, i, k_up) || 判断是否粘情况(int_chess, k_right, j) || game_round % 2 == 0)
                                    {
                                        int_chess[i, k_up] = 0;
                                        int_chess[k_right, j] = 0;
                                        if (down == left && down != 0)
                                        {
                                            int_chess[k_left, j] = 0;
                                            int_chess[i, k_dowm] = 0;
                                        }
                                        SetPointAndClick(x + 25 * i, y + 25 * j);
                                        Invoke(new Action(() =>
                                        {
                                            pictureBox1.Image = GetString(all_single_color, int_chess, i, j);
                                        }));
                                        Thread.Sleep(time_sleep);
                                    }
                                    continue;
                                }
                                if (down == right && down != 0 && down != left && down != up)
                                {
                                    is_game_over++;
                                    if (判断是否粘情况(int_chess, i, k_dowm) || 判断是否粘情况(int_chess, k_right, j) || game_round % 2 == 0)
                                    {
                                        int_chess[i, k_dowm] = 0;
                                        int_chess[k_right, j] = 0;
                                        if (up == left && up != 0)
                                        {
                                            int_chess[k_left, j] = 0;
                                            int_chess[i, k_up] = 0;
                                        }
                                        SetPointAndClick(x + 25 * i, y + 25 * j);
                                        Invoke(new Action(() =>
                                        {
                                            pictureBox1.Image = GetString(all_single_color, int_chess, i, j);
                                        }));
                                        Thread.Sleep(time_sleep);
                                    }
                                    continue;
                                }
                            }
                        }
                    }
                    if (is_game_over == 0)
                    {
                        break;
                    }
                }
                Invoke(new Action(() =>
                {
                    label5.Text = "结束";
                }));
            }
        }
        
        private void SetPointAndClick(int x,int y)
        {
            if (checkBox1.Checked)
            {
                SetCursorPos(x, y);
                Thread.Sleep(20);
                mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
        }

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
        //该函数用于设置鼠标的位置,其中X和Y是相对于屏幕左上角的绝对位置.
        [DllImport("user32")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        private bool IsGameOver(int[,] chess)
        {
            for (int i = 0; i < 23; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (chess[i, j] > 0) return false;
                }
            }
            return true;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                textBox1.Text = MousePosition.X.ToString();
                textBox2.Text = MousePosition.Y.ToString();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(label5.Text=="开始")
            {
                e.Cancel = true;
            }
        }
    }
}
