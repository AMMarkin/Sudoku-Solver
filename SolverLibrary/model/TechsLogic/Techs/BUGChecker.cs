namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class BUGChecker : TechChecker
    {
        public override TechType Type => TechType.BUG;

        protected override string Discription => "BUG";

        protected override AnswerOfTech FindElimination(Field field)
        {
            AnswerOfTech answer;

            int counter = 0;

            int bugI = -1;
            int bugJ = -1;

            //все ячейки кроме одной должны содержать 2 кандидата
            //одна должна содержать 3

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (field[i, j].value != -1) continue;

                    if (field[i, j].remainingCandidates != 2 && field[i, j].remainingCandidates != 3)
                    {
                        return null;
                    }
                    else if (field[i, j].remainingCandidates == 3)
                    {
                        counter++;
                        bugI = i;
                        bugJ = j;
                    }
                }
            }

            //если 3 кандидата более чем в одной ячейке то бага нет
            if (counter != 1)
            {
                return null;
            }

            int[] candidats_counter = new int[9];

            int startX = 3 * (bugJ / 3);
            int startY = 3 * (bugI / 3);

            //обхожу регион с жуком
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    //считаю колличество всех кандидатов
                    for (int k = 0; k < 9; k++)
                    {
                        if (field[startY + y, startX + x].candidates[k])
                        {
                            candidats_counter[k]++;
                        }
                    }
                }
            }

            int bugDigit = -1;
            counter = 0;
            for (int digit = 0; digit < 9; digit++)
            {
                if (candidats_counter[digit] == 3)
                {
                    counter++;
                    bugDigit = digit;
                }
            }

            AddClueMark(9 * bugI + bugJ, bugDigit);
            //устанавливаю значение
            AddSetingValue(9 * bugI + bugJ, bugDigit);
            for (int digit = 0; digit < 9; digit++)
            {
                //обнуляю всех оставшихся кандидатов
                if (field[bugI, bugJ].candidates[digit] && digit != bugDigit)
                {
                    AddElimination(9 * bugI + bugJ, digit);
                }
            }

            answer = MakeAnswer($"BUG: если в ячейке ({(bugI + 1)};{(bugJ + 1)}) установить не {(bugDigit + 1)}, то судоку будет иметь 2 решения");
            return answer;
        }

    }
}
