namespace SolverLibrary.model.TechsLogic
{
    internal class MatrixBuilder
    {
        public int[][] GetMatrixOfCandidatesFromGroup(Field.Cell[] group, bool IsHidden)
        {
            int[][] matrix = new int[group.Length][];
            for (int i = 0; i < group.Length; i++)
            {
                matrix[i] = new int[group.Length];
            }
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < group.Length; j++)
                {
                    if (group[i].candidates[j])
                    {
                        if (IsHidden)
                        {
                            matrix[j][i] = 1;
                        }
                        else
                        {
                            matrix[i][j] = 1;
                        }
                    }
                }
            }
            return matrix;
        }

        public int[][] GetMatrixFromFieldByDigit(Field field, int digit, bool isTransparent)
        {
            int[][] matrix = new int[Field.Row_Count][];
            for(int i = 0; i < Field.Column_Count; i++)
            {
                matrix[i] = new int[Field.Column_Count];
            }

            for (int i = 0; i < Field.Row_Count; i++)
            {
                for (int j = 0; j < Field.Column_Count; j++)
                {
                    if (field[i, j].candidates[digit])
                    {
                        if (isTransparent)
                        {
                            matrix[j][i] = 1;
                        }
                        else
                        {
                            matrix[i][j] = 1;
                        }
                    }
                }
            }
            return matrix;
        }

    }
}
