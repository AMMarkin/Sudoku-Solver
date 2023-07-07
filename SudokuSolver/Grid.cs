using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;


namespace SudokuSolver
{
    internal class Grid
    {
        readonly Cell[][] cells;

        public Cell this[int i, int j]
        {
            get => cells[i][j];
        }

        public readonly int sizeGrid;       //размер поля
        public readonly int startX = 20;    //левый верхний угол начала рисования
        public readonly int startY = 40;
        public bool[] isHighlighted;        //массив флагов для подсветки



        public Graphics g;                  //ссылка на доступ к графике
        public Form1 mainForm;              //ссылка на основную форму



        public Grid(Form1 f)
        {
            mainForm = f;
            //размеры сетки

            int merStep = 5;

            int sizeCell = Cell.size;

            sizeGrid = 9 * (merStep + sizeCell);


            //создание ячеек
            //9 строк создаются в 9и потоках
            cells = new Cell[9][];
            Thread[] threads = new Thread[9];

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new Cell[9];
                Info info = new Info()
                {
                    i = i,
                    merStep = merStep,
                    sizeCell = sizeCell,
                    f = f,
                    grid = this
                };
                threads[i] = new Thread(CreateRowOfCell)
                {
                    Name = "Row " + (i + 1)
                };
                threads[i].Start(info);
            }

            for (int i = 0; i < 9; i++)
            {
                threads[i].Join();
            }

            isHighlighted = new bool[9];
        }


        //пакет с инфой для потоков
        private class Info
        {
            internal int i;
            internal int merStep;
            internal int sizeCell;
            internal Form f;
            internal Grid grid;
        }


        private void CreateRowOfCell(object obj)
        {
            Info info = (Info)obj;

            int i = info.i;
            int merStep = info.merStep;
            int sizeCell = info.sizeCell;
            Form f = info.f;
            Grid grid = info.grid;


            for (int j = 0; j < cells[i].Length; j++)
            {
                cells[i][j] = new Cell(startX + j * (merStep + sizeCell), startY + i * (merStep + sizeCell), f, this);
                cells[i][j].value.Click += HighlighteEvent;
                cells[i][j].p.Click += HighlighteEvent;
            }
        }

        internal void HighlighteEvent(object sender, EventArgs e)
        {
            if (sender is Label)
            {

                Label label = sender as Label;
                if (label.Name.Equals("value"))
                {
                    if (!label.Text.Equals(""))
                    {
                        isHighlighted[Convert.ToInt32(label.Text) - 1] = !isHighlighted[Convert.ToInt32(label.Text) - 1];
                        HighlighteGrid(isHighlighted);
                        //найти ячейку по координатам лейбла
                        //вызвать метод подсветки видимых ячеек
                    }
                }
            }
            if (sender is Panel)
            {
                //найти ячейку по координатам лейбла
                //вызвать метод подсветки видимых ячеек
            }
        }

        //подсветка кандидатов
        public void HighlighteGrid(bool[] flags)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    cells[i][j].HighlighteCell(flags);
                }
            }
        }

        public void HighlighteGrid()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    cells[i][j].HighlighteCell(isHighlighted);
                }
            }
        }

        //подсветка исключений
        public void HighlighteRemoved(List<int[]> clues, List<int[]> removed)
        {


            for (int i = 0; i < clues.Count; i++)
            {
                cells[clues[i][0]][clues[i][1]].HighlighteAsClue();
            }
            for (int i = 0; i < clues.Count; i++)
            {
                cells[clues[i][0]][clues[i][1]].HighlighteDigitsAsClue(clues[i][2]);
            }

            for (int i = 0; i < removed.Count; i++)
            {
                cells[removed[i][0]][removed[i][1]].HighlighteRemoving(removed[i][2]);
            }

            //если цепь не null и не пустая то отмечаем звенья
            if (Logic.ON != null && Logic.ON.Count != 0)
            {
                foreach (int[] unit in Logic.ON)
                {
                    int i1 = unit[0] / 9;
                    int j1 = unit[0] % 9;
                    cells[i1][j1].HighlighteUnit(unit[1], true);
                }
            }
            if (Logic.OFF != null && Logic.OFF.Count != 0)
            {
                foreach (int[] unit in Logic.OFF)
                {
                    int i1 = unit[0] / 9;
                    int j1 = unit[0] % 9;
                    cells[i1][j1].HighlighteUnit(unit[1], false);
                }
            }
        }
        //обновить сетку
        public void UpdateGrid(int[][] grid)
        {
            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid[i].Length; j++)
                {
                    if (grid[i][j] != 0)
                    {
                        cells[i][j].SetValue(grid[i][j]);
                    }
                }
            }
        }

        public void UpdateGrid(Field field)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = 0; j < cells[i].Length; j++)
                {
                    cells[i][j].UpdateCell(field[i, j]);
                    cells[i][j].ResetHighlighting(isHighlighted);
                    cells[i][j].HighlighteCell(isHighlighted);
                }
            }
        }

        public void ReloadGrid()
        {
            isHighlighted = new bool[isHighlighted.Length];

            for (int i = 0; i < 9; i++)
            {
                ReloadRow(new Info() { i = i });
            }

        }

        private void ReloadRow(Object obj)
        {

            int i = ((Info)obj).i;

            for (int j = 0; j < 9; j++)
            {
                cells[i][j].ReloadCell();
            }
        }


        public void DrawLines()
        {

            using (Pen p = new Pen(Color.Black))
            {
                p.Width = 3;
                int X = startX - 3;
                int Y = startY - 3;

                for (int i = 0; i < 4; i++)
                {
                    g.DrawLine(p, X + i * sizeGrid / 3, Y, X + i * sizeGrid / 3, Y + sizeGrid);
                    g.DrawLine(p, X, Y + i * sizeGrid / 3, X + sizeGrid, Y + i * sizeGrid / 3);
                }

                //если есть цепь, то рисуем цепь
                if (Logic.chain != null)
                {
                    if (Logic.chain.Count != 0)
                    {
                        DrawChain(-1, -1, null);
                    }
                }

            }

        }
        //отрисовка найденных связей
        public void DrawChain(int Xshift, int Yshift, PaintEventArgs e)
        {
            Graphics gr;
            if (e != null)
            {
                gr = e.Graphics;
            }
            else
            {
                gr = g;
            }

            int mer = 6;
            float dist = 20;

            using (Pen p = new Pen(Color.FromArgb(170, Color.Red)))
            {
                p.Width = 2;
                int i1, i2, j1, j2, k1, k2;

                float X1, X2;
                float Y1, Y2;

                float PX, PY;

                for (int i = 0; i < Logic.chain.Count; i++)
                {
                    i1 = Logic.chain[i][0] / 9;
                    j1 = Logic.chain[i][0] % 9;
                    k1 = Logic.chain[i][1];
                    i2 = Logic.chain[i][2] / 9;
                    j2 = Logic.chain[i][2] % 9;
                    k2 = Logic.chain[i][3];

                    X1 = cells[i1][j1].centers[k1].X - Xshift;
                    Y1 = cells[i1][j1].centers[k1].Y - Yshift;
                    X2 = cells[i2][j2].centers[k2].X - Xshift;
                    Y2 = cells[i2][j2].centers[k2].Y - Yshift;


                    //если линия сверху вниз
                    if (Y1 > Y2)
                    {
                        Y1 -= mer;
                        Y2 += mer;
                    }
                    else if (Y1 < Y2)
                    {
                        Y1 += mer;
                        Y2 -= mer;
                    }

                    if (X1 > X2)
                    {
                        X1 -= mer;
                        X2 += mer;
                    }
                    else if (X1 < X2)
                    {
                        X1 += mer;
                        X2 -= mer;
                    }

                    //если линии достаточно отдаленные
                    //искривляю
                    //иначе рисуем прямую
                    if (Math.Sqrt((X2 - X1) * (X2 - X1) + (Y2 - Y1) * (Y2 - Y1)) > dist)
                    {
                        if (X2 - X1 != 0)
                        {
                            PY = 2 * mer;
                            PX = -(PY * (Y2 - Y1)) / (X2 - X1);
                        }
                        else
                        {
                            PX = 2 * mer;
                            PY = -(PX * (X2 - X1)) / (Y2 - Y1);
                        }

                        PX += (X1 + X2) / (float)2.0;
                        PY += (Y1 + Y2) / (float)2.0;

                        gr.DrawBezier(p, X1, Y1, PX, PY, PX, PY, X2, Y2);
                    }
                    else
                    {
                        gr.DrawLine(p, X1, Y1, X2, Y2);
                    }
                }
                //отрисовка слабых связей
                if (Logic.weak != null)
                {
                    p.Color = Color.FromArgb(170, Color.Gray);
                    for (int i = 0; i < Logic.weak.Count; i++)
                    {
                        i1 = Logic.weak[i][0] / 9;
                        j1 = Logic.weak[i][0] % 9;
                        k1 = Logic.weak[i][1];
                        i2 = Logic.weak[i][2] / 9;
                        j2 = Logic.weak[i][2] % 9;
                        k2 = Logic.weak[i][3];

                        X1 = cells[i1][j1].centers[k1].X - Xshift;
                        Y1 = cells[i1][j1].centers[k1].Y - Yshift;
                        X2 = cells[i2][j2].centers[k2].X - Xshift;
                        Y2 = cells[i2][j2].centers[k2].Y - Yshift;


                        //если линия сверху вниз
                        if (Y1 > Y2)
                        {
                            Y1 -= mer;
                            Y2 += mer;
                        }
                        else if (Y1 < Y2)
                        {
                            Y1 += mer;
                            Y2 -= mer;
                        }

                        if (X1 > X2)
                        {
                            X1 -= mer;
                            X2 += mer;
                        }
                        else if (X1 < X2)
                        {
                            X1 += mer;
                            X2 -= mer;
                        }

                        if (Math.Sqrt((X2 - X1) * (X2 - X1) + (Y2 - Y1) * (Y2 - Y1)) > dist)
                        {
                            if (X2 - X1 != 0)
                            {
                                PY = 2 * mer;
                                PX = -(PY * (Y2 - Y1)) / (X2 - X1);
                            }
                            else
                            {
                                PX = 2 * mer;
                                PY = -(PX * (X2 - X1)) / (Y2 - Y1);
                            }

                            PX += (X1 + X2) / (float)2.0;
                            PY += (Y1 + Y2) / (float)2.0;

                            gr.DrawBezier(p, X1, Y1, PX, PY, PX, PY, X2, Y2);
                        }
                        else
                        {
                            gr.DrawLine(p, X1, Y1, X2, Y2);
                        }
                    }

                }
            }
        }

        internal class Cell
        {

            private readonly Grid grid;                                      //ссылка на основное поле
            public static readonly int size = 55;                   //размер ячейки
            readonly int x;                                         //координаты ячейки
            readonly int y;

            internal Label[] candidates;                            //лейблы для отображения кандидатов
            internal Label value;                                   //лейбл для отображения числа
            private int digit;                                      //число в ячейке

            public PointF[] centers;                                //координаты центра ячейки

            internal readonly Panel p;                               //панелька ячейки
            private readonly Form mainForm;                         //ссылка на основную форму

            //цвета на все случаи жизни
            private readonly Color defaultColor = Color.FromArgb(173, 216, 230);
            private readonly Color highlightedColor = Color.FromArgb(255, 105, 180);
            private readonly Color clueColor = Color.FromArgb(255, 255, 0);
            private readonly Color removingColor = Color.FromArgb(255, 215, 0);
            private readonly Color clueDigitColor = Color.FromArgb(154, 205, 50);
            private readonly Color chainColorON = Color.FromArgb(173, 255, 47);
            private readonly Color chainColorOFF = Color.FromArgb(0, 0, 255);


            //заглушка для лока потоков
            private static readonly object locker = new object();

            //конструктор
            public Cell(int x, int y, Form f, Grid grid)
            {
                //переписываю параметры
                this.grid = grid;
                this.mainForm = f;
                this.x = x;
                this.y = y;

                //панелька ячейки
                p = new Panel()
                {
                    Size = new Size(size, size),
                    Location = new Point(x, y),
                    BackColor = Color.FromArgb(173, 216, 230),
                    BorderStyle = BorderStyle.FixedSingle,
                };
                p.Paint += DrawChain;

                //добавляем панельки на форму
                lock (locker)
                {
                    f.Controls.Add(p);
                }

                //лейбл значения
                value = new Label()
                {
                    Name = "value",
                    Visible = false,
                    Size = new Size(size, size),
                    Location = new Point(0, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Microsoft Sans Serif", Convert.ToInt32(size * 0.6)),
                    Text = ""
                };
                value.Paint += DrawChain;


                //значение в ячейке
                digit = 0;
                p.Controls.Add(value);

                //лейблы кандидатов
                candidates = new Label[9];
                for (int i = 0; i < candidates.Length; i++)
                {
                    //общие настройки
                    candidates[i] = new Label()
                    {
                        Name = "candidate",
                        Visible = true,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Text = (i + 1).ToString(),
                        //настройки размера и шрифта
                        Size = new Size(size / 3 - 6, size / 3 - 6),
                        Font = new Font("Microsoft Sans Serif", Convert.ToInt32((size / 3 - 6) * 0.6)),
                        //настройки позиции
                        Location = new Point(2 + (i % 3) * size / 3, 2 + (i / 3) * size / 3)
                    };
                    //подключение отрисовки
                    candidates[i].Paint += DrawChain;
                    //добавления на панельку
                    p.Controls.Add(candidates[i]);
                }

                //нахожу коордитаны центров лейблов кандидатов
                centers = new PointF[9];
                for (int i = 0; i < 9; i++)
                {
                    centers[i] = new PointF((float)(candidates[i].Location.X + x) + (float)(candidates[i].Size.Width / 2.0),
                                            (float)(candidates[i].Location.Y + y) + (float)(candidates[i].Size.Height / 2.0));
                }
            }

            //подсветка цепи
            internal void HighlighteUnit(int digit, bool flag)
            {
                //если ON то 
                if (flag)
                {
                    candidates[digit].BackColor = chainColorON;
                }

                //если OFF то
                if (!flag)
                {
                    candidates[digit].BackColor = chainColorOFF;
                }

            }

            //подсветка кандидатов
            internal void HighlighteCell(bool[] flags)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (digit - 1 == i && flags[i])
                    {

                        value.BackColor = highlightedColor;


                    }
                    if (digit - 1 == i && !flags[i])
                    {

                        value.BackColor = defaultColor;

                    }



                    if (flags[i])
                    {
                        if (candidates[i].BackColor != clueDigitColor && candidates[i].BackColor != removingColor &&
                            candidates[i].BackColor != chainColorON && candidates[i].BackColor != chainColorOFF)
                        {
                            candidates[i].BackColor = highlightedColor;
                        }
                    }
                    else
                    {
                        if (candidates[i].BackColor != clueDigitColor && candidates[i].BackColor != removingColor &&
                            candidates[i].BackColor != chainColorON && candidates[i].BackColor != chainColorOFF)
                        {
                            candidates[i].BackColor = p.BackColor;

                        }
                        if (p.BackColor != clueColor)
                        {
                            p.BackColor = defaultColor;
                        }
                    }
                }
            }

            //сброс раскраски
            internal void ResetHighlighting(bool[] flags)
            {
                p.BackColor = defaultColor;
                for (int i = 0; i < flags.Length; i++)
                {
                    if (digit - 1 == i && flags[i])
                    {
                        value.BackColor = highlightedColor;
                    }
                    if (digit - 1 == i && !flags[i])
                    {
                        value.BackColor = defaultColor;
                    }

                    if (flags[i])
                    {
                        candidates[i].BackColor = highlightedColor;
                    }
                    else
                    {
                        candidates[i].BackColor = defaultColor;

                    }
                }
            }

            //подсветить ячейку как ключевую
            internal void HighlighteAsClue()
            {
                p.BackColor = clueColor;
                for (int i = 0; i < candidates.Length; i++)
                {
                    candidates[i].BackColor = clueColor;
                }
            }

            //подсветить кандидата как ключевого
            internal void HighlighteDigitsAsClue(int digit)
            {
                candidates[digit].BackColor = clueDigitColor;
            }

            //подсветить кандидата как исключаемого
            internal void HighlighteRemoving(int digit)
            {
                candidates[digit].BackColor = removingColor;
            }

            //вывести значение в ячейке
            internal void SetValue(int v)
            {
                for (int i = 0; i < candidates.Length; i++)
                {
                    candidates[i].Visible = false;
                }
                value.Text = v.ToString();
                if (candidates[v - 1].BackColor == highlightedColor)
                {
                    value.BackColor = highlightedColor;
                }
                value.Visible = true;
                digit = v;
            }

            //скрыть кандидата
            internal void RemoveCandidate(int i)
            {
                candidates[i].Visible = false;
                candidates[i].Enabled = false;
            }

            //обновить ячейку в соответствии с ячейкой поля
            internal void UpdateCell(Field.Cell cell)
            {
                if (cell.value != -1)
                {
                    SetValue(cell.value);
                }
                else
                {
                    value.Visible = false;
                    digit = 0;
                }

                for (int i = 0; i < cell.candidates.Length; i++)
                {
                    candidates[i].Visible = cell.candidates[i];
                }
            }

            //полный сброс ячейки
            internal void ReloadCell()
            {
                digit = 0;
                p.BackColor = defaultColor;
                value.BackColor = defaultColor;
                value.Visible = false;

                for (int i = 0; i < candidates.Length; i++)
                {
                    candidates[i].BackColor = defaultColor;
                    candidates[i].Visible = true;
                }
            }

            //отрисовка всего что нужно поверх элемента
            internal void DrawChain(object sender, PaintEventArgs e)
            {
                int Xshift = x;
                int Yshift = y;

                Control c = sender as Control;

                //если не панель ячейки то сдвигаем
                if (!c.Equals(p))
                {
                    Xshift += c.Left;
                    Yshift += c.Top;
                }

                if (Logic.chain != null && Logic.chain.Count != 0)
                {
                    grid.DrawChain(Xshift, Yshift, e);
                }
            }

        }

    }
}
