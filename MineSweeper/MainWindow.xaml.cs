using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace MineSweeper
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [System.Runtime.InteropServices.DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        private int[,] mines = null;
        private List<int[]> marks = new List<int[]>();
        private int clickedPoints = 0;
        private bool[,] visited = new bool[8, 8];

        public MainWindow()
        {
            InitializeComponent();
            mines = MineHelper.CreateMines(8, 8);
            CountTextBlock.Text = ":" + MineHelper._minesCount;
            //初始化visited数组用于后面的函数递归
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (mines[i, j] == -1)
                    {
                        visited[i, j] = true;
                    }
                    else
                    {
                        visited[i, j] = false;
                    }
                }
            }
        }

        /// <summary>
        /// 关闭窗口的动画
        /// </summary>
        bool _closinganimation = true;
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = _closinganimation;
            _closinganimation = false;

            Storyboard sb = new Storyboard();
            DoubleAnimation dh = new DoubleAnimation();
            DoubleAnimation dw = new DoubleAnimation();
            DoubleAnimation dop = new DoubleAnimation();
            dop.Duration = dh.Duration = dw.Duration = sb.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            dop.To = dh.To = dw.To = 0;
            Storyboard.SetTarget(dop, this);
            Storyboard.SetTarget(dh, this);
            Storyboard.SetTarget(dw, this);
            Storyboard.SetTargetProperty(dop, new PropertyPath("Opacity", new object[] { }));
            Storyboard.SetTargetProperty(dh, new PropertyPath("Height", new object[] { }));
            Storyboard.SetTargetProperty(dw, new PropertyPath("Width", new object[] { }));
            sb.Children.Add(dh);
            sb.Children.Add(dw);
            sb.Children.Add(dop);
            sb.Completed += new EventHandler(Sb_Completed); //(a, b) => { this.Close(); };
            sb.Begin();
        }
        void Sb_Completed(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 退出应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 窗口拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// 左键点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            button.Background = new SolidColorBrush(Colors.Chocolate);
            int y = Convert.ToInt32(button.Name[6].ToString());
            int x = Convert.ToInt32(button.Name[8].ToString());
            string content = KnowTheNumToShow(mines[x, y]);
            button.Content = content;
            if (visited[x, y] == false)
            {
                clickedPoints++;
            }
            visited[x, y] = true;
            if (clickedPoints == 64)
            {
                if (CheckSucceed())
                {
                    WPGrid.Visibility = Visibility.Visible;
                }
            }
            if (mines[x, y] == 0)
            {
                visited[x, y] = false;
                clickedPoints--;
                FloodFill2Buttons(x, y, visited);
            }
            else if (mines[x, y] == -1)
            {
                GGGrid.Visibility = Visibility.Visible;
                button.Background = new SolidColorBrush(Colors.Tomato);
            }
        }

        /// <summary>
        /// 右键标记
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Button button = (Button)sender;
            int y = Convert.ToInt32(button.Name[6].ToString());
            int x = Convert.ToInt32(button.Name[8].ToString());
            if (visited[x, y] == true && ContainPoint(x, y) == -1 && mines[x, y] != -1)
            {
                return;
            }
            int index = ContainPoint(x, y);
            if (index != -1)
            {
                button.Background = new SolidColorBrush(Colors.Transparent);
                button.Content = "";
                marks.RemoveAt(index);
                visited[x, y] = false;
                clickedPoints--;
            }
            else
            {
                button.Background = new SolidColorBrush(Colors.Sienna);
                button.Content = "\u26F3";
                marks.Add(new int[] { x, y });
                visited[x, y] = true;
                clickedPoints++;
                if (clickedPoints == 64)
                {
                    if (CheckSucceed())
                    {
                        WPGrid.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        /// <summary>
        /// 判断marks列表中是否存在这个点
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int ContainPoint(int x, int y)
        {
            for (int i = 0; i < marks.Count; i++)
            {
                if (marks[i][0] == x && marks[i][1] == y)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 当clickedPoints到达了64时就说明每一个点都被操作过了，
        /// 检查结果是否正确，也就是检查一下marks列表是不是有且只有正确答案（雷）
        /// </summary>
        /// <returns></returns>
        public bool CheckSucceed()
        {
            if (marks.Count == MineHelper.minesList.Count)
            {
                for (int i = 0; i < MineHelper.minesList.Count; i++)
                {
                    if (ContainPoint(MineHelper.minesList[i][0], MineHelper.minesList[i][1]) == -1)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 重新开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < GameGrid.Children.Count; i++)
            {
                Button btn = GameGrid.Children[i] as Button;
                btn.Background = new SolidColorBrush(Colors.Transparent);
                btn.Content = "";
            }
            mines = MineHelper.CreateMines(8, 8);
            CountTextBlock.Text = ":" + MineHelper._minesCount;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (mines[i, j] == -1)
                    {
                        visited[i, j] = true;
                    }
                    else
                    {
                        visited[i, j] = false;
                    }
                }
            }
            clickedPoints = 0;
            marks.Clear();
            GGGrid.Visibility = Visibility.Collapsed;
            WPGrid.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 点击到的格子是空白时对button们进行floodfill算法处理
        /// </summary>
        public void FloodFill2Buttons(int x, int y, bool[,] visted)
        {
            if (x >= 0 && y >= 0 && x < 8 && y < 8)
            {
                if (visted[x, y])
                {
                    return;
                }
                clickedPoints++;
                visted[x, y] = true;
                ChangeButton(x, y, mines[x, y]);
                if (mines[x, y] != 0)
                {
                    return;
                }
                FloodFill2Buttons(x, y - 1, visted);
                FloodFill2Buttons(x - 1, y, visted);
                FloodFill2Buttons(x + 1, y, visted);
                FloodFill2Buttons(x, y + 1, visted);
            }
        }

        /// <summary>
        /// 把你要改变为“已点击过”状态的按钮的横纵坐标传进来，我将把他变
        /// 颜色为chocolate，内容为show的空格子，特殊颜色的格子只有两种，
        /// 棋子和雷，但是这两个都不可能传入到这个方法
        /// </summary>
        /// <param name="column_value"></param>
        /// <param name="row_value"></param>
        /// <param name="show"></param>
        public void ChangeButton(int x, int y, int show)
        {
            for (int i = 0; i < GameGrid.Children.Count; i++)
            {
                Button btn = GameGrid.Children[i] as Button;
                if (Grid.GetRow(btn) == x && Grid.GetColumn(btn) == y)
                {
                    btn.Background = new SolidColorBrush(Colors.Chocolate);
                    btn.Content = KnowTheNumToShow(show);
                    if (clickedPoints == 64)
                    {
                        if (CheckSucceed())
                        {
                            WPGrid.Visibility = Visibility.Visible;
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据传进来的数字返回一个Segoe UI Symbol对应样式的代码
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string KnowTheNumToShow(int i)
        {
            switch (i)
            {
                case 0:
                    return "";
                case -1:
                    return "\u26ED";
                case 1:
                    return "\u2776";
                case 2:
                    return "\u2777";
                case 3:
                    return "\u2778";
                case 4:
                    return "\u2779";
                case 5:
                    return "\u277A";
                case 6:
                    return "\u277B";
                case 7:
                    return "\u277C";
                case 8:
                    return "\u277D";
                default:
                    return "";
            }
        }
    }
}
