using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    public class Field
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

        //заполнить поле заданными значениями
        public void UpdateField(int[][] sudoku)
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
        //применение записанных изменений
        public void ApplyChanges(int[][] changes)
        {
            if (changes == null) return;

            int i;
            int j;
            foreach (int[] change in changes)
            {
                //нахожу нужную ячейку по индексу
                i = change[0] / 9;
                j = change[0] % 9;

                //исключение кандидата
                if (change[2] == 1)
                {
                    cells[i][j].RemoveCandidat(change[1]);
                }
                //установка значения
                if (change[2] == -1)
                {
                    cells[i][j].SetValue(change[1]);
                }
            }
        }


        //обнуление поля
        public void ResetField()
        {
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    cells[i][j].value = -1;
                    cells[i][j].remainingCandidates = 9;
                    for(int k = 0; k < 9; k++)
                    {
                        cells[i][j].candidates[k] = true;
                    }
                }
            }
        }


        public class Cell
        {
            //значение в ячейке
            public int value = -1;

            public readonly int row;         //i
            public readonly int column;      //j
            public readonly int ind;         //9*i + j

            //массив кандидатов
            public bool[] candidates;
            public int remainingCandidates = 9;

            //массив индексов ячеек которые видно отсюда
            public int[][] seenInd;
            public Cell[] seenCell;

            public Cell(int row, int column)
            {
                //неизвестное значение
                candidates = new bool[9];

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

                //Buffer.AddSettingValueChange(ind, v);

                //убираю оставшихся кандидатов
                for (int i = 0; i < candidates.Length; i++)
                {
                    //если есть
                    if (candidates[i])
                    {
                        //исключаю
                        RemoveCandidat(i+1);
                        //записываю
                        Buffer.AddRemovingChange(ind, i+1);
                    }
                }
            }

            //удаление значения(откат утановки)
            public void RemoveValue()
            {
                value = -1;
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

            //добавление кандидата (откат исключения)
            public void AddCandidat(int v)
            {
                if (!candidates[v - 1])
                {
                    candidates[v - 1] = true;
                    remainingCandidates++;
                }
            }

            
        }
    }

    
}
