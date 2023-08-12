using System.Collections.Generic;

namespace SolverLibrary.model.field
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
            FillField();

            //кто кого видит
            FillListsOfSeenCell();

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
                        cells[i][j].SetValue(_buffer.sudoku[i][j] - 1);
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
                    cells[i][j].ResetCell();
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

            if (answer.Chain != null)
                foreach (int[] unit in answer.Chain)
                    _buffer.AddStrongLinkToChain(unit);

            if (answer.Weak != null)
                foreach (int[] unit in answer.Weak)
                    _buffer.AddWeakLinkToChain(unit);

            if (answer.ChainUnits != null)
                foreach (int[] unit in answer.ChainUnits)
                    _buffer.AddUnitToChain(unit);

            if (answer.ON != null)
                foreach (int[] unit in answer.ON)
                    _buffer.AddColoredUnit(unit, TechsLogic.Color.ON);

            if (answer.OFF != null)
                foreach (int[] unit in answer.OFF)
                    _buffer.AddColoredUnit(unit, TechsLogic.Color.OFF);
        }

        private void FillListsOfSeenCell()
        {
            int i;
            int j;

            for (int row = 0; row < cells.Length; row++)
            {
                for (int column = 0; column < cells[row].Length; column++)
                {
                    for (int k = 0; k < cells[row][column].seenCell.Length; k++)
                    {
                        i = cells[row][column].seenInd[0][k];
                        j = cells[row][column].seenInd[1][k];

                        cells[row][column].seenCell[k] = cells[i][j];
                    }
                }
            }
        }

        private void FillField()
        {
            cells = new Cell[9][];
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = new Cell[9];
                for (int j = 0; j < cells[i].Length; j++)
                {
                    cells[i][j] = new Cell(i, j);
                }
            }
        }

    }
}
