using System.Collections.Generic;

namespace SolverLibrary.model
{
    public class Field
    {
        public Cell[][] cells;

        public static int Row_Count => 9;
        public static int Column_Count => 9;
        public static int Digits_Count => 9;
        public static int Regions_Count => 9;

        public Cell this[int i, int j]
        {
            get => cells[i][j];
        }

        private Buffer _buffer;
        public Buffer Buffer { get => _buffer; set => _buffer = value; }

        //Создание поля
        public Field()
        {
            //создаю буффер для поля
            _buffer = new Buffer();
            _buffer.Init();
            _buffer.InitChangeStorage();

            //создание ячеек
            cells = new Cell[9][];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new Cell[9];
                for (int j = 0; j < cells[i].Length; j++)
                {
                    cells[i][j] = new Cell(i, j);

                }
            }

            //кто кого видит
            for (int i = 0; i < cells.Length; i++)
            {
                for (int j = 0; j < cells[i].Length; j++)
                {
                    for (int k = 0; k < cells[i][j].seenCell.Length; k++)
                    {
                        cells[i][j].seenCell[k] = cells[cells[i][j].seenInd[0][k]][cells[i][j].seenInd[1][k]];
                    }

                }
            }

        }

        //заполнить поле заданными значениями
        public void UpdateField()
        {
            for (int i = 0; i < _buffer.sudoku.Length; i++)
            {
                for (int j = 0; j < _buffer.sudoku[i].Length; j++)
                {
                    if (_buffer.sudoku[i][j] != 0)
                    {
                        cells[i][j].SetValue(_buffer.sudoku[i][j]-1);
                        for (int k = 0; k < 9; k++)
                        {
                            cells[i][j].RemoveCandidat(k);
                        }
                    }
                }
            }
        }

        //применение записанных изменений
        public void ApplyChanges()
        {
            IEnumerable<Change> changes = _buffer.GetLastChanges();

            if (changes == null) return;

            int i;
            int j;
            foreach (Change change in changes)
            {
                //нахожу нужную ячейку по индексу
                i = change.I;
                j = change.J;
                

                //исключение кандидата
                if (change.Type == ChangeType.RemovingDigit)
                {
                    cells[i][j].RemoveCandidat(change.Digit);
                }
                //установка значения
                if (change.Type == ChangeType.SettingValue)
                {
                    cells[i][j].SetValue(change.Digit);
                }
            }
        }

        public void CancelChanges()
        {
            int i;
            int j;
            IEnumerable<Change> changes = _buffer.GetLastChanges();
            foreach (Change change in changes)
            {
                //нахожу нужную ячейку
                i = change.I;
                j = change.J;
                if (change.Type == ChangeType.RemovingDigit)
                {
                    //возвращаю кандидата
                    cells[i][j].AddCandidat(change.Digit);
                }
                if (change.Type == ChangeType.SettingValue)
                {
                    cells[i][j].RemoveValue();
                }
            }
            _buffer.RemoveLastChanges();
        }

        //обнуление поля
        public void ResetField()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    cells[i][j].value = -1;
                    cells[i][j].remainingCandidates = 9;
                    for (int k = 0; k < 9; k++)
                    {
                        cells[i][j].candidates[k] = true;
                    }
                }
            }
        }

        public void ApplyAnswer(AnswerOfTech answer)
        {
            if (answer == null) return;

            if (answer.Eliminations != null)
                foreach (Change change in answer.Eliminations)
                    _buffer.AddRemovingChange(change.Ind, change.Digit);

            if (answer.Seted != null)
                foreach (Change change in answer.Seted)
                    _buffer.AddSettingValueChange(change.Ind, change.Digit);

            if (answer.Clues != null)
                foreach (Mark clue in answer.Clues)
                    _buffer.AddClueMark(clue.Ind, clue.Digit);

            if (answer.Removed != null)
                foreach (Mark remove in answer.Removed)
                    _buffer.AddRemovedMark(remove.Ind, remove.Digit);

        }

        public class Cell
        {
            //значение в ячейке
            public int value = -1;

            public readonly int row;         //i
            public readonly int column;      //j
            public readonly int ind;         //9*i + j
            public readonly int region;      

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
                this.region = 3 * (row / 3) + (column / 3);

                
                //заполнение массива видимых ячеек
                seenInd = new int[2][];
                seenCell = new Cell[20];
                seenInd[0] = new int[20]; //строки
                seenInd[1] = new int[20]; //колонки


                int counter = 0;



                //8 ячеек региона
                //левая верхняя ячейка региона
                int startCol = 3 * (column / 3);
                int startRow = 3 * (row / 3);
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if ((startRow + i) != row || (startCol + j) != column)
                        {
                            seenInd[0][counter] = startRow + i;
                            seenInd[1][counter] = startCol + j;
                            counter++;
                        }
                    }
                }

                //6 ячеек строки
                for (int j = 0; j < 9; j++)
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
                value = v;

            }

            //удаление значения(откат утановки)
            public void RemoveValue()
            {
                value = -1;
            }

            //исключение кандидата
            public void RemoveCandidat(int v)
            {
                if (candidates[v])
                {
                    candidates[v] = false;
                    remainingCandidates--;
                }
            }

            //добавление кандидата (откат исключения)
            public void AddCandidat(int v)
            {
                if (!candidates[v])
                {
                    candidates[v] = true;
                    remainingCandidates++;
                }
            }
        }
    }


}
