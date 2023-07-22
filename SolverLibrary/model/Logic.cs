using SolverLibrary.model.TechsLogic.Techs;
using System.Collections.Generic;

namespace SolverLibrary.model
{

    //класс с техниками
    //получает на вход объект класса Field содержащий поле, а так же список используемых техник
    //находит возможные исключения кандидатов или возможные установки значений
    //записывает найденные ходы в буффер поля

    public class Logic
    {

        public const string noFound = "Исключений не найдено";
        public const string done = "Судоку решено!";

        //массив и библиотека названий техник
        public readonly List<string> tecniques = TechsList.techNames;

        private readonly List<TechChecker> techCheckers = new List<TechChecker>();
        private readonly List<FieldStatusChecker> statusCheckers = new List<FieldStatusChecker>()
        {
            new RepeatedValuesChecker(),
            new EmptyCellsChecker(),
            new ComplitionChecker()
        };

        public Dictionary<string, int> tech = new Dictionary<string, int>();

        private readonly List<Change> seted = new List<Change>();
        private readonly List<Change> eliminations = new List<Change>();

        private readonly List<Mark> clues = new List<Mark>();
        private readonly List<Mark> removed = new List<Mark>();


        //заполениен библиотеки
        public void Init()
        {
            for (int i = 0; i < tecniques.Count; i++)
            {
                tech.Add(tecniques[i], i);
            }

            TechCheckerBuilder builder = new TechCheckerBuilder();

            foreach(string techName in tecniques) 
            { 
                TechChecker techChecker = builder.GetTechChecker(TechsList.ConvertNameToType(techName));
                if (techChecker != null)
                {
                    techCheckers.Add(techChecker);
                }
            }
        }

        public AnswerOfTech FindElimination(Field field, bool[] tecFlags)
        {
            AnswerOfTech answer;


            //простые исключения
            TechChecker checker = techCheckers.Find((x) => x.Type == TechType.SimpleRestriction);
            answer = checker.CheckField(field);
            if (answer != null)
            {
                return answer;
            }

            //техники на классах
            for (int i = 0; i < tecFlags.Length; i++)
            {
                if (tecFlags[i] == false) continue;

                checker = techCheckers.Find((x) => x.Type == TechsList.ConvertNameToType(tecniques[i]));

                if (checker == null) continue;

                answer = checker.CheckField(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            //не реализованное ищется в тупую
            answer = FindEliminationByOldMethod(field, tecFlags);

            return answer;
        }

        public AnswerOfTech FindEliminationByOldMethod(Field field, bool[] tecFlags)
        {
            AnswerOfTech answer;
            ClearLists();

            //BUG
            if (tecFlags[tech["BUG"]])
            {
                answer = BUG(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            //XYZ-Wing
            if (tecFlags[tech["XYZ-Wing"]])
            {
                answer = XYZ_Wing(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            //Y-Wings
            if (tecFlags[tech["Y-Wings"]])
            {
                answer = Y_Wings(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            //проверка решения
            foreach(FieldStatusChecker cheker in statusCheckers)
            {
                answer = cheker.Check(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            answer = new AnswerOfTech(noFound);

            return answer;
        }

        //XYZ-Wing
        private AnswerOfTech XYZ_Wing(Field field)
        {
            AnswerOfTech answer;

            //ищем все ячейки с тремя кандидатами

            int counter;
            int i1, j1; //индексы первого крыла
            int i2, j2; //индексы второго крыла

            int rem;    //исключаемое число

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //корень с тремя кандидатами
                    if (field[i, j].remainingCandidates != 3)
                    {
                        continue;
                    }

                    //по региону ищем крыло с двумя кандидатами
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            i1 = 3 * (i / 3) + y;
                            j1 = 3 * (j / 3) + x;

                            if (field[i1, j1].remainingCandidates != 2)
                            {
                                continue;
                            }

                            //должно быть два общих кандидата
                            counter = 0;
                            for (int k = 0; k < 9; k++)
                            {
                                if (field[i, j].candidates[k] && field[i1, j1].candidates[k])
                                {
                                    counter++;
                                }
                            }

                            if (counter != 2)
                            {
                                continue;
                            }

                            //ищем второе крыло по строке

                            for (int col = 0; col < 9; col++)
                            {
                                i2 = i;
                                j2 = col;
                                //пропускаем корень и возможно первое крыло
                                if (j2 == j || (i2 == i1 && j2 == j1))
                                {
                                    continue;
                                }
                                if (field[i2, j2].remainingCandidates != 2)
                                {
                                    continue;
                                }

                                //должно быть два общих кандидата с корнем
                                counter = 0;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i2, j2].candidates[k] && field[i, j].candidates[k])
                                    {
                                        counter++;
                                    }
                                }

                                if (counter != 2)
                                {
                                    continue;
                                }

                                //должен быть один общий кандидат с первым крылом
                                counter = 0;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i2, j2].candidates[k] && field[i1, j1].candidates[k])
                                    {
                                        counter++;
                                    }
                                }

                                if (counter != 1)
                                {
                                    continue;
                                }


                                //ищем исключаемое число
                                //единственное общее число у корня и двух крыльев
                                rem = -1;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i, j].candidates[k] && field[i1, j1].candidates[k] && field[i2, j2].candidates[k])
                                    {
                                        rem = k;
                                        break;
                                    }
                                }

                                //исключения производим в регионе корня, строке второго крыла

                                for (int r = 0; r < 3; r++)
                                {
                                    //пропускаем корень и возможно первое крыло
                                    if ((i1 == i && 3 * (j / 3) + r == j1) || 3 * (j / 3) + r == j || (i2 == i && 3 * (j / 3) + r == j2))
                                    {
                                        continue;
                                    }
                                    //если нашли ячейку видимую из всех трех позиций 
                                    //и она имеет нужный кандидат
                                    //исключаем его
                                    if (field[i, 3 * (j / 3) + r].candidates[rem])
                                    {
                                        eliminations.Add(new Change(i, 3 * (j / 3) + r, rem, ChangeType.RemovingDigit));
                                        removed.Add(new Mark(9 * i + 3 * (j / 3) + r, rem));

                                        for (int k = 0; k < 9; k++)
                                        {
                                            if (field[i, j].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i + j, k));
                                            }
                                            if (field[i1, j1].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i1 + j1, k));
                                            }
                                            if (field[i2, j2].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i2 + j2, k));
                                            }
                                        }

                                        answer = new AnswerOfTech($"XYZ-Wing: исключена {(rem + 1)} из ({(i + 1)};{(3 * (j / 3) + r + 1)})");
                                        SetLists(answer);
                                        return answer;
                                    }
                                }
                            }


                            //ищем второе крыло по столбцу

                            for (int row = 0; row < 9; row++)
                            {
                                i2 = row;
                                j2 = j;
                                //пропускаем корень и возможно первое крыло
                                if (i2 == i || (i2 == i1 && j2 == j1))
                                {
                                    continue;
                                }
                                if (field[i2, j2].remainingCandidates != 2)
                                {
                                    continue;
                                }

                                //должно быть два общих кандидата с корнем
                                counter = 0;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i2, j2].candidates[k] && field[i, j].candidates[k])
                                    {
                                        counter++;
                                    }
                                }

                                if (counter != 2)
                                {
                                    continue;
                                }

                                //должен быть один общий кандидат с первым крылом
                                counter = 0;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i2, j2].candidates[k] && field[i1, j1].candidates[k])
                                    {
                                        counter++;
                                    }
                                }

                                if (counter != 1)
                                {
                                    continue;
                                }


                                //ищем исключаемое число
                                //единственное общее число у корня и двух крыльев
                                rem = -1;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i, j].candidates[k] && field[i1, j1].candidates[k] && field[i2, j2].candidates[k])
                                    {
                                        rem = k;
                                        break;
                                    }
                                }


                                //исключения производим в регионе корня, столбце второго крыла

                                for (int r = 0; r < 3; r++)
                                {
                                    //пропускаем корень и возможно первое крыло
                                    if ((j1 == j && 3 * (i / 3) + r == i1) || 3 * (i / 3) + r == i || (3 * (i / 3) + r == i2 && j2 == j))
                                    {
                                        continue;
                                    }
                                    //если нашли ячейку видимую из всех трех позиций 
                                    //и она имеет нужный кандидат
                                    //исключаем его
                                    if (field[3 * (i / 3) + r, j].candidates[rem])
                                    {
                                        eliminations.Add(new Change(3 * (i / 3) + r, j, rem, ChangeType.RemovingDigit));
                                        removed.Add(new Mark(9 * (3 * (i / 3) + r) + j, rem));

                                        for (int k = 0; k < 9; k++)
                                        {
                                            if (field[i, j].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i + j, k));
                                            }
                                            if (field[i1, j1].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i1 + j1, k));
                                            }
                                            if (field[i2, j2].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i2 + j2, k));
                                            }
                                        }

                                        answer = new AnswerOfTech($"XYZ-Wing: исключена {(rem + 1)} из ({(3 * (i / 3) + r + 1)};{(j + 1)})");
                                        SetLists(answer);
                                        return answer;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        //Y-Wings
        private AnswerOfTech Y_Wings(Field field)
        {
            AnswerOfTech answer;

            //ищем все ячейки с двумя кандидатами

            List<Field.Cell> list = new List<Field.Cell>();

            int counter;


            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (field[i, j].remainingCandidates == 2)
                    {
                        list.Add(field[i, j]);
                    }
                }
            }

            Field.Cell Y;
            Field.Cell X1;
            Field.Cell X2;

            int a;
            int b;
            int c;


            bool foundet = false;

            //встаем в точку 
            for (int i = 0; i < list.Count; i++)
            {
                if (foundet)
                    break;

                Y = list[i];

                a = -1;
                b = 0;
                c = 0;

                //записываем кандидатов в a и b
                for (int k = 0; k < 9; k++)
                {
                    if (Y.candidates[k])
                    {
                        if (a == -1)
                        {
                            a = k;
                        }
                        else
                        {
                            b = k;
                        }
                    }
                }

                //ищем X1 
                //крыло у которого только a или только b

                bool flag = false;
                for (int j = 0; j < Y.seenCell.Length; j++)
                {
                    if (foundet)
                        break;

                    //Берем вторую точку
                    X1 = Y.seenCell[j];

                    if (X1.remainingCandidates != 2) continue;

                    //если 0 совпадений то flag=false  -- пропускаем
                    //если 1 совпадение то flag=true   -- то что нужно
                    //если 2 совпадения то flag=false  -- пропускаем

                    for (int k = 0; k < 9; k++)
                    {
                        if (X1.candidates[k] && (k == a || k == b))
                        {
                            flag = !flag;
                        }
                    }

                    if (flag)
                    {
                        //нужно чтобы совпало именно a
                        flag = false;
                        for (int k = 0; k < 9; k++)
                        {
                            if (X1.candidates[k] && k == a)
                            {
                                flag = true;
                            }
                        }
                        //если совпало не а -- пропускаем
                        if (!flag)
                        {
                            continue;
                        }

                        //запоминаем c
                        for (int k = 0; k < 9; k++)
                        {
                            if (X1.candidates[k] && k != a)
                            {
                                c = k;
                            }
                        }

                        //ищем второе "крыло"
                        for (int j2 = 0; j2 < Y.seenCell.Length; j2++)
                        {
                            if (j2 == j) continue;

                            X2 = Y.seenCell[j2];

                            if (X2.remainingCandidates != 2) continue;

                            counter = 0;

                            flag = false;
                            //считаем совпадения
                            for (int k = 0; k < 9; k++)
                            {
                                if (X2.candidates[k] && k == a)
                                {
                                    flag = true;
                                    break;
                                }
                                if (X2.candidates[k] && k == b)
                                {
                                    counter++;
                                }
                                if (X2.candidates[k] && k == c)
                                {
                                    counter++;
                                }
                            }

                            if (counter != 2)
                                flag = true;

                            if (flag)
                                continue;

                            //если совпало и b и с
                            //то ищем ячейки которая видна из X1 и X2

                            for (int n = 0; n < X1.seenCell.Length; n++)
                            {
                                for (int m = 0; m < X2.seenCell.Length; m++)
                                {
                                    //если нашли ячейку видимую из двух крыльев
                                    //ищем есть ли в ней c
                                    if (X1.seenCell[n].Equals(X2.seenCell[m]))
                                    {
                                        bool flagC = false;
                                        for (int k = 0; k < 9; k++)
                                        {
                                            if (X1.seenCell[n].candidates[k] && k == c)
                                            {
                                                flagC = true;
                                            }
                                        }
                                        //если найдено исключение
                                        //исключаем
                                        //записываем ключи и исключения

                                        if (flagC)
                                        {
                                            foundet = true;

                                            eliminations.Add(new Change(X1.seenCell[n].ind, c, ChangeType.RemovingDigit));
                                            removed.Add(new Mark(X1.seenCell[n].ind, c));
                                        }

                                    }
                                }
                            }

                            if (foundet)
                            {
                                clues.Add(new Mark(Y.ind, a));
                                clues.Add(new Mark(Y.ind, b));
                                clues.Add(new Mark(X1.ind, a));
                                clues.Add(new Mark(X1.ind, c));
                                clues.Add(new Mark(X2.ind, b));
                                clues.Add(new Mark(X2.ind, c));

                                answer = new AnswerOfTech($"Y-Wings по {(c + 1)} : ({(Y.row + 1)};{(Y.column + 1)}) => ({(X1.row + 1)};{(X1.column + 1)}) - ({(X2.row + 1)};{(X2.column + 1)})");
                                SetLists(answer);
                                return answer;

                            }

                        }

                    }

                }
            }

            return null;
        }

        //BUG
        private AnswerOfTech BUG(Field field)
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
            for (int k = 0; k < 9; k++)
            {
                if (candidats_counter[k] != 2 && candidats_counter[k] != 3 && candidats_counter[k] != 0)
                {
                    return new AnswerOfTech("BUG: ОШИБКА ПОДСЧЕТА КАНДИДАТОВ");
                }
                if (candidats_counter[k] == 3)
                {
                    counter++;
                    bugDigit = k;
                }
            }

            if (counter != 1)
            {
                return new AnswerOfTech("BUG: ОШИБКА");
            }

            clues.Add(new Mark(9 * bugI + bugJ, bugDigit));

            for (int k = 0; k < 9; k++)
            {
                if (k == bugDigit) continue;

                if (field[bugI, bugJ].candidates[k])
                {
                    removed.Add(new Mark(9 * bugI + bugJ, k));
                }
            }
            //устанавливаю значение
            seted.Add(new Change(bugI, bugJ, bugDigit, ChangeType.SettingValue));
            for (int k = 0; k < 9; k++)
            {
                //обнуляю всех оставшихся кандидатов
                if (field[bugI, bugJ].candidates[k])
                {
                    eliminations.Add(new Change(bugI, bugJ, k, ChangeType.RemovingDigit));
                }
            }

            answer = new AnswerOfTech($"BUG: если в ячейке ({(bugI + 1)};{(bugJ + 1)}) установить не {(bugDigit + 1)}, то судоку будет иметь 2 решения");
            SetLists(answer);
            return answer;
        }

        //простые ислючения
        public AnswerOfTech SimpleRestriction(Field field)
        {
            return techCheckers.Find((x) => x.Type == TechType.SimpleRestriction).CheckField(field);
        }

        private void SetLists(AnswerOfTech answer)
        {
            answer.Seted = seted;
            answer.Eliminations = eliminations;

            answer.Clues = clues;
            answer.Removed = removed;
        }

        private void ClearLists()
        {
            seted.Clear();
            removed.Clear();
            clues.Clear();
            eliminations.Clear();
        }
    }
}
