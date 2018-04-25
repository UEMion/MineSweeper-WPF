using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineSweeper
{
    public class MineHelper
    {
        public static int _minesCount = 0;

        public static List<int[]> minesList = new List<int[]>();

        //-1:Mine   0-∞:safe
        //y是横坐标 x是纵坐标
        public static int[,] CreateMines(int x, int y)
        {
            _minesCount = 0;
            minesList.Clear();
            int[,] mines = new int[y, x];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    mines[j, i] = 0;
                }
            }
            Random random = new Random();
            int count = random.Next(6, 11);
            int mine_x;
            int mine_y;
            for (int i = 0; i < count; i++)
            {
                mine_x = random.Next(0, x);
                mine_y = random.Next(0, y);
                if (mines[mine_y, mine_x] == 0)
                {
                    mines[mine_y, mine_x] = -1;
                    minesList.Add(new int[] { mine_y, mine_x });
                    if (mine_y + 1 < y && mines[mine_y + 1, mine_x] != -1)
                    {
                        mines[mine_y + 1, mine_x]++;
                    }
                    if (mine_x + 1 < x && mines[mine_y, mine_x + 1] != -1)
                    {
                        mines[mine_y, mine_x + 1]++;
                    }
                    if (mine_y - 1 >= 0 && mines[mine_y - 1, mine_x] != -1)
                    {
                        mines[mine_y - 1, mine_x]++;
                    }
                    if (mine_x - 1 >= 0 && mines[mine_y, mine_x - 1] != -1)
                    {
                        mines[mine_y, mine_x - 1]++;
                    }

                    if (mine_x - 1 >= 0 && mine_y - 1 >= 0 && mines[mine_y - 1, mine_x - 1] != -1)
                    {
                        mines[mine_y - 1, mine_x - 1]++;
                    }
                    if (mine_x - 1 >= 0 && mine_y + 1 < y && mines[mine_y + 1, mine_x - 1] != -1)
                    {
                        mines[mine_y + 1, mine_x - 1]++;
                    }
                    if (mine_x + 1 < x && mine_y - 1 >= 0 && mines[mine_y - 1, mine_x + 1] != -1)
                    {
                        mines[mine_y - 1, mine_x + 1]++;
                    }
                    if (mine_x + 1 < x && mine_y + 1 < y && mines[mine_y + 1, mine_x + 1] != -1)
                    {
                        mines[mine_y + 1, mine_x + 1]++;
                    }
                    MineHelper._minesCount++;
                }
                else
                {
                    i--;
                }
            }
            return mines;
        }
    }
}
