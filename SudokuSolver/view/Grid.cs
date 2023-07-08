using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;


namespace SudokuSolver
{
    public class Grid
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

        private int selectedInd = -1;            //индекс выбранной ячейки
        private int selectedDigitByClick = -1;

        public Graphics g;                  //ссылка на доступ к графике
        public Solver mainForm;              //ссылка на основную форму

        private readonly int merStep = 5;
        private readonly int sizeCell = Cell.size;


        public Grid(Solver f)
        {
            mainForm = f;
            //размеры сетки
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

                //устанавливаю событие клика
                cells[i][j].value.Click += HighlighteEvent;
                cells[i][j].p.Click += HighlighteEvent;
                for (int k = 0; k < 9; k++)
                    cells[i][j].candidates[k].Click += HighlighteEvent;
            }
        }

        internal void HighlighteEvent(object sender, EventArgs e)
        {
            int ind;
            if (sender is Label)
            {

                Label label = sender as Label;
                if (label.Name.Equals("value"))
                {
                    //клик по заполненному лейблу
                    if (!label.Text.Equals(""))
                    {
                        int digit = Convert.ToInt32(label.Text) - 1;

                        //если уже выбранное число кликом поменялось
                        if (selectedDigitByClick != digit)
                        {
                            if (selectedDigitByClick != -1)
                            {
                                //снимаю старое выделение
                                isHighlighted[selectedDigitByClick] = false;
                            }
                        }
                        selectedDigitByClick = digit;
                        //если число в ячейке не выделено
                        if (!isHighlighted[digit])
                        {
                            //выделяем
                            isHighlighted[digit] = true;
                        }
                        HighlighteGrid(isHighlighted);
                        //если уже выделено то ничего не делаем


                        Point location = label.Parent.Location;

                        ind = GetIndexFromLocation(location.X, location.Y);
                        HighlighteSeenCells(ind);
                    }
                }
                else if (label.Name.Equals("candidate"))
                {
                    isHighlighted = new bool[9];
                    HighlighteGrid(isHighlighted);

                    Point location = label.Parent.Location;
                    ind = GetIndexFromLocation(location.X, location.Y);
                    HighlighteSeenCells(ind);
                }
            }
            if (sender is Panel)
            {
                isHighlighted = new bool[9];
                HighlighteGrid(isHighlighted);

                Panel panel = sender as Panel;
                Point location = panel.Location;
                ind = GetIndexFromLocation(location.X, location.Y);
                HighlighteSeenCells(ind);
            }
        }

        //подсветка всех ячеек видимых из данной
        private void HighlighteSeenCells(int index)
        {
            //если не выбрано ничего
            if (selectedInd == -1)
            {
                //запоминаю выбранную ячейку
                selectedInd = index;

                //строка столбец выбранной ячейки
                int i = index / 9;
                int j = index % 9;

                //крашу регион ячейки
                for(int x=0; x < 3; x++)
                {
                    for(int y = 0; y < 3; y++)
                    {
                        cells[3 * (i / 3) + y][3 * (j / 3) + x].HighlightAsSeen(false);
                    }
                }
                //крашу строку и столбец
                for(int x = 0; x < 9; x++)
                {
                    //если не в регионе выбранной
                    if (x / 3 != j / 3)
                    {
                        cells[i][x].HighlightAsSeen(false);
                    }
                    //если не в регионе выбранной
                    if (x / 3 != i / 3)
                    {
                        cells[x][j].HighlightAsSeen(false);
                    }
                }

            }
            //если уже выбрана эта ячейка
            else if(selectedInd == index)
            {
                selectedInd = -1;
                //нужно снять выделение
                //строка столбец выбранной ячейки
                int i = index / 9;
                int j = index % 9;

                //чищу регион ячейки
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        cells[3 * (i / 3) + y][3 * (j / 3) + x].HighlightAsSeen(true);
                    }
                }
                //чищу строку и столбец
                for (int x = 0; x < 9; x++)
                {
                    //если не в регионе выбранной
                    if (x / 3 != j / 3)
                    {
                        cells[i][x].HighlightAsSeen(true);
                    }
                    //если не в регионе выбранной
                    if (x / 3 != i / 3)
                    {
                        cells[x][j].HighlightAsSeen(true);
                    }
                }
            }
            //если выбрана другая
            else
            {
                //перекрашиваю

                //снимаю выделение с выбранной

                //строка столбец выбранной ячейки
                int i = selectedInd / 9;
                int j = selectedInd % 9;

                //чищу регион ячейки
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        cells[3 * (i / 3) + y][3 * (j / 3) + x].HighlightAsSeen(true);
                    }
                }
                //чищу строку и столбец
                for (int x = 0; x < 9; x++)
                {
                    //если не в регионе выбранной
                    if (x / 3 != j / 3)
                    {
                        cells[i][x].HighlightAsSeen(true);
                    }
                    //если не в регионе выбранной
                    if (x / 3 != i / 3)
                    {
                        cells[x][j].HighlightAsSeen(true);
                    }
                }

                //крашу новую
                //запоминаю выбранную ячейку
                selectedInd = index;

                //строка столбец выбранной ячейки
                i = index / 9;
                j = index % 9;

                //крашу регион ячейки
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        cells[3 * (i / 3) + y][3 * (j / 3) + x].HighlightAsSeen(false);
                    }
                }
                //крашу строку и столбец
                for (int x = 0; x < 9; x++)
                {
                    //если не в регионе выбранной
                    if (x / 3 != j / 3)
                    {
                        cells[i][x].HighlightAsSeen(false);
                    }
                    //если не в регионе выбранной
                    if (x / 3 != i / 3)
                    {
                        cells[x][j].HighlightAsSeen(false);
                    }
                }

            }
        }

        //нахождение индекса ячейки из координат левого верхнего угла
        private int GetIndexFromLocation(int x, int y)
        {
            //координаты левого верхнего угла ячейки

            //x = startX + j * (merStep + sizeCell)
            //y = startY + i * (merStep + sizeCell)

            //строка
            //столбец
            int i;
            int j;

            j = (x - startX) / (merStep + sizeCell);
            i = (y - startY) / (merStep + sizeCell);

            //индекс
            return 9 * i + j;
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

        public class Cell
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
            private readonly Color defaultColor = Color.FromArgb(173, 216, 230);        //обычный цвет
            private readonly Color seenColor = Color.FromArgb(135, 206, 235);           //цвет когда видима из выбранной
            private readonly Color highlightedColor = Color.FromArgb(255, 105, 180);    //цвет когда выделена
            private readonly Color clueColor = Color.FromArgb(255, 255, 0);             //цвет ключевой ячейки
            private readonly Color removingColor = Color.FromArgb(255, 215, 0);         //цвет исключаемого кандидата
            private readonly Color clueDigitColor = Color.FromArgb(154, 205, 50);       //цвет ключевого кандидата
            private readonly Color chainColorON = Color.FromArgb(173, 255, 47);         //цвета групп цепей (ON,OFF)
            private readonly Color chainColorOFF = Color.FromArgb(0, 0, 255);

            private bool seen;


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
                    mainForm.Controls.Add(p);
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

                    //добавление на панельку
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

                //обхожу всех кандидатов
                for (int i = 0; i < 9; i++)
                {
                    //выделяю все что есть внутри этой ячейи
                    if (flags[i])
                    {
                        //если ячейка заполнена и нужно выделить
                        if (digit - 1 == i)
                        {
                            //выделяю основное число
                            value.BackColor = highlightedColor;
                        }
                        //если ячейка не заполнена выделяю кандидата
                        //если кандидат есть
                        else if (candidates[i].Visible)
                        {
                            //если кандидат не учавствовал в решении
                            if (candidates[i].BackColor == defaultColor || candidates[i].BackColor == seenColor)
                            {
                                candidates[i].BackColor = highlightedColor;
                            }
                        }

                    }
                    //если нужно снять выделение
                    else
                    {
                        //если ячейка заполнена
                        if (digit - 1 == i)
                        {
                            //снимаю выделение
                            if (!seen)
                            {
                                value.BackColor = defaultColor;
                            }
                            else
                            {
                                value.BackColor = seenColor;
                            }

                        }
                        //снимаю выделение с кандидата
                        //если кандидат есть
                        else if (candidates[i].Visible)
                        {
                            //если кандидат не учавствовал в решении
                            if (candidates[i].BackColor == highlightedColor)
                            {
                                //снимаю выделение
                                candidates[i].BackColor = p.BackColor;
                            }
                        }
                    }
                }
            }

            //подсветка ячейки как видимой из выбранной
            internal void HighlightAsSeen(bool remove)
            {
                //если подсвечиваю
                if (!remove)
                {
                    seen = true;
                    //если ячейка заполнена
                    if (value.Visible)
                    {
                        //подсветка основного лейбла
                        //если не подсвечен и не учавастовал в решении
                        if (value.BackColor == defaultColor)
                        {
                            value.BackColor = seenColor;
                        }
                    }
                    else
                    {
                        //подсветка кандидатов
                        for (int k = 0; k < 9; k++)
                        {
                            //если кандидат есть
                            if (candidates[k].Visible)
                            {
                                //если кандидат не учавствует в решении
                                if (candidates[k].BackColor == defaultColor)
                                {
                                    //выделяю
                                    candidates[k].BackColor = seenColor;
                                }
                            }
                        }
                        if (p.BackColor == defaultColor)
                        {
                            p.BackColor = seenColor;
                        }
                    }
                }
                //иначе снимаю подсветку
                else
                {
                    seen = false;
                    //если ячейка заполнена
                    if (value.Visible)
                    {
                        //подсветка основного лейбла
                        //если не подсвечен и не учавастовал в решении
                        if (value.BackColor == seenColor)
                        {
                            value.BackColor = defaultColor;
                        }
                    }
                    else
                    {
                        //подсветка кандидатов
                        for (int k = 0; k < 9; k++)
                        {
                            //если кандидат есть
                            if (candidates[k].Visible)
                            {
                                //если кандидат не учавствует в решении
                                if (candidates[k].BackColor == seenColor)
                                {
                                    //выделяю
                                    candidates[k].BackColor = defaultColor;
                                }
                            }
                        }
                        if (p.BackColor == seenColor)
                        {
                            p.BackColor = defaultColor;
                        }
                    }


                }
            }

            //сброс раскраски
            internal void ResetHighlighting(bool[] flags)
            {
                if (seen)
                {
                    p.BackColor = seenColor;
                }
                else
                {
                    p.BackColor = defaultColor;
                }

                for (int i = 0; i < flags.Length; i++)
                {
                    if (flags[i])
                    {
                        if (value.Visible)
                        {
                            value.BackColor = highlightedColor;
                        }
                        else
                        {
                            candidates[i].BackColor = highlightedColor;
                        }
                    }
                    else
                    {
                        if (value.Visible)
                        {
                            if (seen)
                            {
                                value.BackColor = seenColor;
                            }
                            else
                            {
                                value.BackColor = defaultColor;
                            }
                        }

                        candidates[i].BackColor = p.BackColor;

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
