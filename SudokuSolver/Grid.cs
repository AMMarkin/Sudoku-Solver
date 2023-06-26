using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SudokuSolver
{
    internal class Grid
    {
        Cell[][] cells;

        public Cell this[int i, int j]
        {
            get => cells[i][j];
        }

        public int sizeGrid;
        public int startX = 20;
        public int startY = 40;
        public bool[] isHighlighted;



        public Graphics g;
        public Form1 mainForm;
        public Panel canvas;

        public Grid(Form1 f)
        {
            mainForm = f;
            //размеры сетки

            int merStep = 5;

            int sizeCell = 60;

            sizeGrid = 9 * (merStep + sizeCell);
            //создание ячеек

            cells = new Cell[9][];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new Cell[9];
                for (int j = 0; j < cells[i].Length; j++)
                {
                    cells[i][j] = new Cell(startX + j * (merStep + sizeCell), startY + i * (merStep + sizeCell), f, this);
                    cells[i][j].value.Click += HighlighteEvent;
                }
            }

            isHighlighted = new bool[9];

        }

        internal void HighlighteEvent(object sender, EventArgs e)
        {
            Label value = sender as Label;
            if (!value.Text.Equals(""))
            {
                isHighlighted[Convert.ToInt32(value.Text) - 1] = !isHighlighted[Convert.ToInt32(value.Text) - 1];
                HighlighteGrid(isHighlighted);
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
            if(Logic.ON!=null && Logic.ON.Count != 0)
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
        public void updateGrid(int[][] grid)
        {
            for (int i = 0; i < grid.Length; i++)
            {
                for (int j = 0; j < grid[i].Length; j++)
                {
                    if (grid[i][j] != 0)
                    {
                        cells[i][j].setValue(grid[i][j]);
                    }
                }
            }
        }

        public void updateGrid(ref Field field)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = 0; j < cells[i].Length; j++)
                {
                    cells[i][j].updateCell(field[i, j]);
                    cells[i][j].ResetHighlighting(isHighlighted);
                    cells[i][j].HighlighteCell(isHighlighted);

                }
            }
        }

        public void reloadGrid()
        {
            isHighlighted = new bool[isHighlighted.Length];

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    cells[i][j].reloadCell();
                }
            }

        }

        public void drawLines()
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
                        drawChain(-1,-1,null);
                    }
                }

            }

        }

        public void drawChain(int Xshift, int Yshift, PaintEventArgs e)
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

            
            using (Pen p = new Pen(Color.FromArgb(170, Color.Red)))
            {
                p.Width = 3;
                int i1, i2, j1, j2, k1, k2;

                float X1, X2;
                float Y1, Y2;

                int mer = 7;

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
                    }else if (Y1 < Y2)
                    {
                        Y1 += mer;
                        Y2 -= mer;
                    }

                    if (X1 > X2)
                    {
                        X1 -= mer;
                        X2 += mer;
                    }
                    else if(X1<X2)
                    {
                        X1 += mer;
                        X2 -= mer;
                    }

                    gr.DrawLine(p, X1,Y1,X2,Y2);

                }

                if (Logic.weak != null)
                {
                    p.Color = Color.FromArgb(170,Color.Gray);
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

                        gr.DrawLine(p, X1, Y1, X2, Y2);

                    }

                }
            }
        }

        internal class Cell
        {

            Grid grid;
            int size = 60;
            int x;
            int y;

            internal Label[] candidates;
            internal Label value;
            int digit;

            public PointF[] centers;

            Panel p;

            Color defaultColor = Color.FromArgb(173, 216, 230);
            Color highlightedColor = Color.FromArgb(255, 105, 180);
            Color clueColor = Color.FromArgb(255, 255, 0);
            Color removingColor = Color.FromArgb(255, 215, 0);
            Color clueDigitColor = Color.FromArgb(154, 205, 50);
            Color chainColorON = Color.FromArgb(173, 255, 47);
            Color chainColorOFF = Color.FromArgb(0, 0, 255);

            //конструктор
            public Cell(int x, int y, Form f, Grid grid)
            {
                this.grid = grid; 

                this.x = x;
                this.y = y;

                p = new Panel();
                p.Size = new Size(size, size);
                p.Location = new Point(x, y);

                p.BackColor = Color.FromArgb(173, 216, 230);
                p.BorderStyle = BorderStyle.FixedSingle;
                p.Paint += drawChain;
                f.Controls.Add(p);

                value = new Label();
                value.Visible = false;
                value.Size = new Size(size, size);
                value.Location = new Point(0, 0);
                value.TextAlign = ContentAlignment.MiddleCenter;
                value.Font = new Font("Microsoft Sans Serif", Convert.ToInt32(size * 0.6));
                value.Text = "";
                value.Paint += drawChain;

                digit = 0;


                p.Controls.Add(value);

                candidates = new Label[9];
                for (int i = 0; i < candidates.Length; i++)
                {
                    candidates[i] = new Label();
                    candidates[i].Visible = true;
                    candidates[i].TextAlign = ContentAlignment.MiddleCenter;
                    candidates[i].Text = (i + 1).ToString();

                    candidates[i].Size = new Size(size / 3 - 6, size / 3 - 6);
                    candidates[i].Location = new Point(2 + (i % 3) * size / 3, 2 + (i / 3) * size / 3);

                    candidates[i].Paint += drawChain;

                    p.Controls.Add(candidates[i]);

                }

                centers = new PointF[9];
                for (int i = 0; i < 9; i++)
                {
                    centers[i] = new PointF((float)(candidates[i].Location.X + x )+ (float) (candidates[i].Size.Width / 2.0), (float)(candidates[i].Location.Y +y)+(float)( candidates[i].Size.Height/2.0));
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
                            candidates[i].BackColor != chainColorON   && candidates[i].BackColor != chainColorOFF)
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


            internal void HighlighteAsClue()
            {
                p.BackColor = clueColor;
                for (int i = 0; i < candidates.Length; i++)
                {
                    candidates[i].BackColor = clueColor;
                }
            }

            internal void HighlighteDigitsAsClue(int digit)
            {
                candidates[digit].BackColor = clueDigitColor;
            }

            internal void HighlighteRemoving(int digit)
            {
                candidates[digit].BackColor = removingColor;
            }

            internal void setValue(int v)
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


            internal void removeCandidate(int i)
            {
                candidates[i].Visible = false;
                candidates[i].Enabled = false;
            }

            internal void updateCell(Field.Cell cell)
            {
                if (cell.value != -1)
                {
                    setValue(cell.value);
                }
                for (int i = 0; i < cell.candidates.Length; i++)
                {
                    candidates[i].Visible = cell.candidates[i];
                }
            }

            internal void reloadCell()
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

            internal void drawChain(object sender, PaintEventArgs e)
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
                    Graphics gr = e.Graphics;
                    grid.drawChain(Xshift,Yshift,e);
                }
            }

        }

    }
}
