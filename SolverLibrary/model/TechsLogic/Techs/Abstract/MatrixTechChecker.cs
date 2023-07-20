using SolverLibrary.model.Utilits;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SolverLibrary.model.TechsLogic.Techs
{
    internal abstract class MatrixTechChecker : GroupedTechChecker
    {
        protected abstract int Order { get; }

        protected abstract bool IsHidden { get; }

        protected override AnswerOfTech FindEliminationInGroup(Field.Cell[] group)
        {
            AnswerOfTech answer;

            //собрать матрицу
            int[][] matrix = (new MatrixBuilder()).GetMatrixOfCandidatesFromGroup(group, IsHidden);

            //найти в матрице нужную фигуру
            int[][] shape = (new PatternFinder()).GetNShapeInMatrix(matrix, Order);



            //если фигура есть то провести исключения
            if (shape == null) return null;


            for (int i = 0; i < group.Length; i++)
            {
                if (!shape[0].Contains(i))
                {
                    for (int j = 0; j < Order; j++)
                    {
                        int cellIndex = i;
                        int digitIndex = shape[1][j];

                        if (IsHidden)
                        {
                            (cellIndex, digitIndex) = (digitIndex, cellIndex);
                        }

                        if (group[cellIndex].candidates[digitIndex])
                        {
                            AddElimination(group[cellIndex].ind, digitIndex);
                            AddRemovingMark(group[cellIndex].ind, digitIndex);
                        }
                    }
                }
            }
            for (int j = 0; j < Order; j++)
            {
                for (int k = 0; k < Order; k++)
                {
                    int cellIndex = shape[0][k];
                    int digitIndex = shape[1][j];

                    if (IsHidden)
                    {
                        (cellIndex, digitIndex) = (digitIndex, cellIndex);
                    }

                    AddClueMark(group[cellIndex].ind, digitIndex);
                }
            }
            answer = MakeAnswerToMatrixTech(group, shape);
            return answer;
        }

        private AnswerOfTech MakeAnswerToMatrixTech(Field.Cell[] group, int[][] shape)
        {
            AnswerOfTech answer;
            if (IsHidden)
            {
                for (int j = 0; j < Order; j++)
                {
                    (shape[0][j], shape[1][j]) = (shape[1][j], shape[0][j]);
                }
            }
            string digits = shape[1].Select(x => (x + 1).ToString()).Aggregate((a, b) => a + "/" + b);

            string[] coordinates = new string[Order];

            for (int j = 0; j < Order; j++)
            {
                coordinates[j] = $"({(group[shape[0][j]].row + 1)};{(group[shape[0][j]].column + 1)})";
            }

            string cells = coordinates.Aggregate((a, b) => a + " и " + b);

            answer = MakeAnswer($"{Discription}: {digits} в {cells}");
            return answer;
        }

    }
}
