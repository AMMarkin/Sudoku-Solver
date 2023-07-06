using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    internal class Field
    {
        public Cell[][] cells;

        public Cell this[int i, int j]
        {
            get => cells[i][j];
        }
        //Создание поля
        public Field()
        {

            //создание ячеек
            cells = new Cell[9][];
            for(int i = 0; i < cells.Length; i++)
            {
                cells[i] = new Cell[9];
                for(int j = 0; j < cells[i].Length; j++)
                {
                    cells[i][j] = new Cell(i,j);

                }
            }

            //кто кого видит
            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = 0; j < cells[i].Length; j++)
                {
                    for(int k = 0; k < cells[i][j].seenCell.Length; k++)
                    {
                        cells[i][j].seenCell[k] = cells[cells[i][j].seenInd[0][k]][cells[i][j].seenInd[1][k]];
                    }

                }
            }

        }

        public void CopyFrom(Field sourse)
        {
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    cells[i][j].CopyFrom(sourse.cells[i][j]);
                }
            }
        }

        public void updateField(int[][] sudoku)
        {
            for(int i = 0; i < sudoku.Length; i++)
            {
                for(int j = 0; j < sudoku[i].Length; j++)
                {
                    if (sudoku[i][j] != 0)
                    {
                        cells[i][j].SetValue(sudoku[i][j]);
                    }
                }
            }
        }
        
        internal class Cell
        {
            //значение в ячейке
            public int value;

            public readonly int row;         //i
            public readonly int column;      //j
            public readonly int ind;         //9*i + j

            //массив кандидатов
            public bool[] candidates;
            public int remainingCandidates;

            //массив индексов ячеек которые видно отсюда
            public int[][] seenInd;
            public Field.Cell[] seenCell;

            public Cell(int row, int column)
            {
                //неизвестное значение
                value = -1;
                candidates = new bool[9];
                remainingCandidates = 9;

                //добавление всех кандидатов
                for (int i = 0; i < candidates.Length; i++)
                {
                    candidates[i] = true;
                }

                this.row = row;
                this.column = column;
                this.ind = 9 * row + column;


                //заполнение массива видимых ячеек
                seenInd = new int[2][];
                seenCell = new Cell[20];
                seenInd[0] = new int[20]; //строки
                seenInd[1] = new int[20]; //колонки


                int counter = 0;



                //8 ячеек региона
                    //левая верхняя ячейка региона
                int startCol = 3 * (column / 3);
                int startRow = 3 * (row/ 3);
                for(int i = 0; i < 3; i++)
                {
                    for(int j = 0; j < 3; j++)
                    {
                        if((startRow+i)!=row || (startCol+j)!= column)
                        {
                            seenInd[0][counter] = startRow + i;
                            seenInd[1][counter] = startCol + j;
                            counter++;
                        }
                    }
                }

                //6 ячеек строки
                for(int j = 0; j < 9; j++)
                {
                    if (j / 3 != startCol / 3)
                    {
                        seenInd[0][counter] = row;
                        seenInd[1][counter] = j;
                        counter++;
                    }
                }
                //6 ячеек столбца
                for (int i = 0; i < 9; i++)
                {
                    if (i / 3 != startRow / 3)
                    {
                        seenInd[0][counter] = i;
                        seenInd[1][counter] = column;
                        counter++;
                    }
                }
            }

            //установка значения
            public void SetValue(int v)
            {
                //установил значение
                value = v;

                //убрал кандидатов
                for (int i = 0; i < candidates.Length; i++)
                {
                    candidates[i] = false;
                }
            }

            //исключение кандидата
            public void RemoveCandidat(int v)
            {
                if (candidates[v - 1])
                {
                    candidates[v - 1] = false;
                    remainingCandidates--;
                }
            }

            internal void CopyFrom(Cell sourse)
            {
                value = sourse.value;
                remainingCandidates = sourse.remainingCandidates;

                for(int k = 0; k < 9; k++)
                {
                    candidates[k] = sourse.candidates[k];
                }
            }
        }
    }

    
}
