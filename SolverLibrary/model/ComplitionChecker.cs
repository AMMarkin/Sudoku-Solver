namespace SolverLibrary.model
{
    internal class ComplitionChecker : FieldStatusChecker
    {
        public override AnswerOfTech Check(Field field)
        {
            AnswerOfTech answer = null;

            for (int row = 0; row < Field.Row_Count; row++)
            {
                for (int col = 0; col < Field.Column_Count; col++)
                {
                    if (field[row, col].value < 0) return answer;
                }
            }

            answer = new AnswerOfTech("Судоку решено!");

            return answer;
        }
    }
}
