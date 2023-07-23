using SolverLibrary.model.TechsLogic;
using System.Collections.Generic;

namespace SolverLibrary.model
{
    public class Buffer
    {
        public int[][] sudoku;

        public List<Change[]> fieldStorage;     //пачки изменений поля(по пачке на шаг)

        public List<Change> fieldChanges;         //пачка изменений


        //список ключей и исключенных кандидатов
        private List<Mark> _clues;        //i,j -- где ключ,         k -- что ключ
        private List<Mark> _removed;      //i,j -- откуда исключаем, k -- что исключаем


        //для цепных техник
        //список связей и звеньев цепи 
        //ind = 9 * i + j
        public List<int[]> chain;        //ind, k => ind, k    сильные связи !A =>  B
        public List<int[]> weak;         //ind, k => ind, k     слабые связи  A => !B
        public List<int[]> chainUnits;   //ind, k

        //цепи по двум цветам
        public List<int[]> ON;           //ind, k
        public List<int[]> OFF;          //ind, k


        public IEnumerable<Mark> Clues => _clues;
        public IEnumerable<Mark> Removed => _removed;

        

        //создание буффера 
        public void Init()
        {
            sudoku = new int[9][];
            for (int i = 0; i < sudoku.Length; i++)
            {
                sudoku[i] = new int[9];
            }
        }

        public void InitChangeStorage()
        {
            fieldChanges = fieldChanges ?? new List<Change>();
            fieldChanges?.Clear();

            fieldStorage = fieldStorage ?? new List<Change[]>();
            fieldStorage?.Clear();
        }


        public void ClearChainBuffer()
        {
            _clues?.Clear();
            _clues = _clues ?? new List<Mark>();

            _removed?.Clear();
            _removed = _removed ?? new List<Mark>();

            chain?.Clear();
            chain = chain ?? new List<int[]>();

            weak?.Clear();
            weak = weak ?? new List<int[]>();

            chainUnits?.Clear();
            chainUnits = chainUnits ?? new List<int[]>();

            ON?.Clear();
            ON = ON ?? new List<int[]>();

            OFF?.Clear();
            OFF = OFF ?? new List<int[]>();
        }

        //было установлено значение по ind
        public void AddSettingValueChange(int ind, int digit)
        {
            fieldChanges.Add(new Change(ind, digit, ChangeType.SettingValue));
        }

        //был исключен кандидат по ind
        public void AddRemovingChange(int ind, int digit)
        {
            fieldChanges.Add(new Change(ind, digit, ChangeType.RemovingDigit));
        }

        public void AddClueMark(int index, int digit = -1)
        {
            Mark.MarkType type;

            if (digit != -1)
            {
                type = Mark.MarkType.Digit;
            }
            else
            {
                type = Mark.MarkType.Cell;
            }

            _clues.Add(new Mark(index, digit, type));
        }

        public void AddRemovedMark(int index, int digit)
        {
            _removed.Add(new Mark(index, digit, Mark.MarkType.Digit));
        }

        public void AddStrongLinkToChain(int[] strongLink)
        {
            chain.Add(strongLink);
        }

        public void AddWeakLinkToChain(int[] weakLink)
        {
            weak.Add(weakLink);
        }
        
        public void AddUnitToChain(int[] unit)
        {
            chainUnits.Add(unit);
        }

        public void AddColoredUnit(int[] unit, Color color)
        {
            if(color == Color.ON)
            {
                ON.Add(unit);
            }
            else
            {
                OFF.Add(unit);
            }
        }

        public void SaveChanges()
        {
            fieldStorage.Add(fieldChanges.ToArray());
            fieldChanges.Clear();
        }

        public IEnumerable<Change> GetLastChanges()
        {
            return fieldStorage[fieldStorage.Count - 1];
        }

        public void RemoveLastChanges()
        {
            fieldStorage.RemoveAt(fieldStorage.Count - 1);
        }
    }
}
