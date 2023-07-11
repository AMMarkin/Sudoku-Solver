using System.Collections.Generic;

namespace SolverLibrary.model
{
    public class Buffer
    {
        public int[][] sudoku;

        public List<int[][]> fieldStorage;     //пачки изменений поля(по пачке на шаг)

        public List<int[]> fieldChanges;         //пачка изменений


        //список ключей и исключенных кандидатов
        public List<int[]> clues;        //i,j -- где ключ,         k -- что ключ
        public List<int[]> removed;      //i,j -- откуда исключаем, k -- что исключаем


        //для цепных техник
        //список связей и звеньев цепи 
        //ind = 9 * i + j
        public List<int[]> chain;        //ind, k => ind, k    сильные связи !A =>  B
        public List<int[]> weak;         //ind, k => ind, k     слабые связи  A => !B
        public List<int[]> chainUnits;   //ind, k

        //цепи по двум цветам
        public List<int[]> ON;           //ind, k
        public List<int[]> OFF;          //ind, k

        //очистка заполненных частей 
        public void ClearChainBuffer()
        {
            clues?.Clear();
            clues = clues ?? new List<int[]>();

            removed?.Clear();
            removed = removed ?? new List<int[]>();

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
            fieldChanges = fieldChanges ?? new List<int[]>();
            fieldChanges?.Clear();

            fieldStorage = fieldStorage ?? new List<int[][]>();
            fieldStorage?.Clear();
        }

        //ind = 9*i+j
        //k
        // flag = 1 -  удаление кандидата
        // flag = -1 - установка значения

        //было установлено значение по i j
        public void AddSettingValueChange(int i, int j, int k)
        {
            fieldChanges.Add(new int[] { 9 * i + j, k, -1 });
        }
        //было установлено значение по ind
        public void AddSettingValueChange(int ind, int k)
        {
            fieldChanges.Add(new int[] { ind, k, -1 });
        }
        //был исключен кандидат по i j
        public void AddRemovingChange(int i, int j, int k)
        {
            fieldChanges.Add(new int[] { 9 * i + j, k, 1 });
        }
        //был исключен кандидат по ind
        public void AddRemovingChange(int ind, int k)
        {
            fieldChanges.Add(new int[] { ind, k, 1 });
        }


        //сохранение пачки изменений(за шаг)
        public void SaveChanges()
        {
            fieldStorage.Add(fieldChanges.ToArray());
            fieldChanges.Clear();
        }

        //полечение списка последней пачки изменений(за шаг)
        public int[][] GetLastChanges()
        {
            //если в текущей пачке есть изменения то отправляю их
            if (fieldChanges.Count != 0)
            {
                return fieldChanges.ToArray();
            }
            else
            {
                return fieldStorage[fieldStorage.Count - 1];
            }
        }

        public void RemoveLastChanges()
        {
            fieldStorage.RemoveAt(fieldStorage.Count - 1);
        }
    }
}
