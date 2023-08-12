namespace SolverLibrary.model.field
{

    public class Cell
    {
        //значение в ячейке
        public int value = -1;

        public readonly int row;         //i
        public readonly int column;      //j
        public readonly int ind;         //9*i + j
        public readonly int region;

        //массив кандидатов
        public readonly bool[] candidates;


        public int remainingCandidates = 9;

        //массив индексов ячеек которые видно отсюда
        public int[][] seenInd;
        public Cell[] seenCell;

        public Cell(int row, int column)
        {
            this.row = row;
            this.column = column;
            this.ind = 9 * row + column;
            this.region = 3 * (row / 3) + (column / 3);


            //добавление всех кандидатов
            candidates = new bool[9];
            for (int i = 0; i < candidates.Length; i++)
            {
                candidates[i] = true;
            }

            FillListOfSeenIndexes(row, column);
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

        internal void ResetCell()
        {
            value = -1;
            remainingCandidates = 9;
            for (int k = 0; k < 9; k++)
            {
                candidates[k] = true;
            }
        }

        private void FillListOfSeenIndexes(int row, int column)
        {
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

    }
}
