using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    internal static class Buffer
    {
        public static int[][] sudoku;

        public static List<int[][]> fieldStorage = new List<int[][]>();

        public static List<int[]> fieldChanges = new List<int[]>();

        public static void Init()
        {
            sudoku = new int[9][];
            for(int i=0; i < sudoku.Length; i++)
            {
                sudoku[i] = new int[9];
            }
        }


        //ind = 9*i+j
        //k
        // flag = 1 -  удаление кандидата
        // flag = -1 - установка значения
        public static void AddChange(int i,int j,int k, int flag)
        {
            fieldChanges.Add(new int[] { 9 * i + j, k, flag });
        }

        public static void AddChange(int ind, int k, int flag)
        {
            fieldChanges.Add(new int[] { ind, k ,flag});
        }

        public static void SaveChanges()
        {
            fieldStorage.Add(fieldChanges.ToArray());
            fieldChanges.Clear();
        }

        public static int[][] GetLastChanges()
        {
            int[][] changes = fieldStorage.Last();
            fieldStorage.Remove(changes);
            return changes;
        }
    }
}
