using System.Collections.Generic;

namespace SudokuSolver
{
    internal static class Buffer
    {
        public static int[][] sudoku;

        public static List<int[][]> fieldStorage = new List<int[][]>();     //пачки изменений поля(по пачке на шаг)

        public static List<int[]> fieldChanges = new List<int[]>();         //пачка изменений

        //создание буффера 
        public static void Init()
        {
            sudoku = new int[9][];
            for (int i = 0; i < sudoku.Length; i++)
            {
                sudoku[i] = new int[9];
            }
        }


        //ind = 9*i+j
        //k
        // flag = 1 -  удаление кандидата
        // flag = -1 - установка значения

        //было установлено значение по i j
        public static void AddSettingValueChange(int i, int j, int k)
        {
            fieldChanges.Add(new int[] { 9 * i + j, k, -1 });
        }
        //было установлено значение по ind
        public static void AddSettingValueChange(int ind, int k)
        {
            fieldChanges.Add(new int[] { ind, k, -1 });
        }
        //был исключен кандидат по i j
        public static void AddRemovingChange(int i, int j, int k)
        {
            fieldChanges.Add(new int[] { 9 * i + j, k, 1 });
        }
        //был исключен кандидат по ind
        public static void AddRemovingChange(int ind, int k)
        {
            fieldChanges.Add(new int[] { ind, k, 1 });
        }


        //сохранение пачки изменений(за шаг)
        public static void SaveChanges()
        {
            fieldStorage.Add(fieldChanges.ToArray());
            fieldChanges.Clear();
        }

        //полечение списка последней пачки изменений(за шаг)
        public static int[][] GetLastChanges()
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

        public static void RemoveLastChanges()
        {
            fieldStorage.RemoveAt(fieldStorage.Count - 1);
        }
    }
}
