namespace SolverLibrary.model
{
    internal class RepeatedValuesChecker : FieldStatusChecker
    {
        public override AnswerOfTech Check(Field field)
        {
            AnswerOfTech answer = null;

            Field.Cell first;
            Field.Cell second;

            //внутри каждой строки
            for (int row = 0; row < Field.Row_Count; row++)
            {
                for (int col = 0; col < Field.Column_Count; col++)
                {
                    first = field[row, col];

                    if (first.value < 0) continue;

                    for (int i = col + 1; i < Field.Column_Count; i++)
                    {
                        second = field[row, i];

                        if (second.value < 0) continue;

                        if (first.value != second.value) continue;

                        answer = new AnswerOfTech($"Ошибка! Совпадение значений в строке {row + 1}: ({first.row + 1};{first.column + 1}) и ({second.row + 1};{second.column + 1})");
                        return answer;
                    }
                }
            }

            //внутри каждого столбца
            for (int col = 0; col < Field.Column_Count; col++)
            {
                for (int row = 0; row < Field.Row_Count; row++)
                {
                    first = field[row, col];

                    if (first.value < 0) continue;

                    for (int i = row + 1; i < Field.Row_Count; i++)
                    {
                        second = field[i, col];

                        if (second.value < 0) continue;

                        if (first.value != second.value) continue;

                        answer = new AnswerOfTech($"Ошибка! Совпадение значений в столбце {col + 1}: ({first.row + 1};{first.column + 1}) и ({second.row + 1};{second.column + 1})");
                        return answer;
                    }
                }
            }

            //внутри каждого региона
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    //x y - индексы региона
                    //i   - индекс внутри региона
                    for (int i = 0; i < Field.Digits_Count; i++)
                    {
                        first = field[3 * y + i / 3, 3 * x + i % 3];

                        if (first.value < 0) continue;

                        for (int j = i + 1; j < Field.Digits_Count; j++)
                        {
                            second = field[3 * y + j / 3, 3 * x + j % 3];

                            if (second.value < 0) continue;

                            if (first.value != second.value) continue;

                            answer = new AnswerOfTech($"Ошибка! Совпадение значений в регионе {3 * y + x}: ({first.row + 1};{first.column + 1}) и ({second.row + 1};{second.column + 1})");
                            return answer;
                        }
                    }

                }
            }

            return answer;
        }
    }
}
