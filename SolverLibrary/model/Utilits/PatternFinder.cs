using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverLibrary.model.Utilits
{
    internal class PatternFinder
    {
        public int[][] GetNShapeInMatrix(int[][] matrix, int n)
        {
            //основное условие
            Predicate<int> predicate = (x) => ((x > 1) && (x <= n));

            //суммы в строках
            int[] sums = matrix.Select((x) => x.Sum()).ToArray();

            //сколько строк удовлетворяет условию
            int counter = sums.Count((x) => predicate(x));

            //запоминаю номера нужных строк
            int[] digits = sums.
                Select((sum, idSum) => new { index = idSum, Sum = sum }).
                Where(x => predicate(x.Sum)).
                Select(x => x.index).
                ToArray();

            if (digits.Length < n)
                return null;

            if (digits.Length > 1)
            {
                //все сочетания из digits.Length по n (все комбинации digits длины n)
                IEnumerable<int[]> combinations = digits.DifferentCombinations(n).Select(comb => comb.ToArray());

                //для каждого сочетания
                foreach (int[] combination in combinations)
                {
                    //сумма выбранных строк
                    sums = new int[matrix.Length];
                    for (int i = 0; i < matrix[0].Length; i++)
                    {
                        for (int j = 0; j < combination.Length; j++)
                        {
                            sums[i] += matrix[combination[j]][i];
                        }
                    }

                    //проверка полученного вектора
                    //сколько чисел в векторе удовлетворяют условию
                    //counter = sums.Count(x => predicate(x));
                    counter = sums.Count(x => x > 0);

                    //если меньше n идем дальше
                    if (counter != n) continue;

                    //иначе запоминаем фигуру
                    int[] rows = new int[n];
                    combination.CopyTo(rows, 0);

                    int[] colums = sums.
                        Select((sum, idSum) => new { index = idSum, Sum = sum }).
                        Where(x => predicate(x.Sum)).
                        Select(x => x.index).
                        ToArray();

                    if (colums.Length != rows.Length)
                    {
                        continue;
                    }

                    //проверка фигуры на импакт
                    for (int j = 0; j < n; j++)
                    {
                        for (int i = 0; i < matrix.Length; i++)
                        {
                            if (!rows.Contains(i) && matrix[i][colums[j]] != 0)
                            {
                                //если импакт есть то возвращаем фигуру
                                return new int[][] { rows, colums };
                            }
                        }
                    }

                }
            }
            return null;
        }

    }
}
