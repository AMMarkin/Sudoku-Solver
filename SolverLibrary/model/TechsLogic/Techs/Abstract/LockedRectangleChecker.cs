using SolverLibrary.model.Utilits;
using System.Linq;
using SolverLibrary.model.field;


namespace SolverLibrary.model.TechsLogic.Techs
{
    internal abstract class LockedRectangleChecker : TechChecker
    {
        protected abstract int Order { get; }

        protected enum PatternDirection
        {
            Rows,
            Columns
        }

        protected override AnswerOfTech FindElimination(Field field)
        {
            AnswerOfTech answer;
            answer = FindEliminationInDirection(field, PatternDirection.Rows);

            if (answer != null) return answer;

            answer = FindEliminationInDirection(field, PatternDirection.Columns);

            return answer;
        }


        protected AnswerOfTech FindEliminationInDirection(Field field, PatternDirection direction)
        {
            AnswerOfTech answer = null;
            int[][] matrix;
            int[][] shape;
            for (int digit = 0; digit < Field.Digits_Count; digit++)
            {

                matrix = (new MatrixBuilder()).GetMatrixFromFieldByDigit(field, digit, direction==PatternDirection.Columns);

                shape = (new PatternFinder()).GetNShapeInMatrix(matrix, Order);

                if (shape == null) continue;

                for (int i = 0; i < 9; i++)
                {
                    if (!shape[0].Contains(i))
                    {
                        for (int j = 0; j < Order; j++)
                        {
                            int rowIndex = i;
                            int colIndex = shape[1][j];

                            if (direction == PatternDirection.Columns)
                            {
                                (rowIndex, colIndex) = (colIndex, rowIndex);
                            }

                            if (field[rowIndex, colIndex].candidates[digit])
                            {
                                AddElimination(9 * rowIndex + colIndex, digit);
                                AddRemovingMark(9 * rowIndex + colIndex, digit);
                            }
                        }

                    }
                }

                if(direction == PatternDirection.Columns)
                {
                    for(int i = 0; i < Order; i++)
                    {
                        (shape[0][i], shape[1][i]) = (shape[1][i], shape[0][i]);
                    }
                }

                for (int x = 0; x < Order; x++)
                {
                    for (int y = 0; y < Order; y++)
                    {
                        AddClueMark(9 * shape[0][x] + shape[1][y], digit);
                    }
                }

                string rows = shape[0]
                    .Select((x) => (x + 1).ToString())
                    .Aggregate((a, b) => $"{a}-{b}");

                string columns = shape[1]
                    .Select((x) => (x + 1).ToString())
                    .Aggregate((a, b) => $"{a}-{b}");

                answer = MakeAnswer($"{Discription} по {digit + 1} в ({rows};{columns})");
                return answer;
            }
            return answer;
        }
    }
}
