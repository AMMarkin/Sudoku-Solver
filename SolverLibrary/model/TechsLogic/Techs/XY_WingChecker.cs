using System.Collections.Generic;
using SolverLibrary.model.field;


namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class XY_WingChecker : TechChecker
    {
        public override TechType Type => TechType.XY_Wing;




        //Y-Wings
        protected override AnswerOfTech FindElimination(Field field)
        {
            AnswerOfTech answer;

            //ищем все ячейки с двумя кандидатами

            List<Cell> list = new FieldScanner().FindXValueCells(field, 2);

            int counter;

            Cell Y;
            Cell X1;
            Cell X2;

            int a;
            int b;
            int c;

            bool foundet = false;

            for (int i = 0; i < list.Count; i++)
            {
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
                    //Берем вторую точку
                    X1 = Y.seenCell[j];

                    if (X1.remainingCandidates != 2) continue;

                    //если 0 совпадений то flag=false  -- пропускаем
                    //если 1 совпадение то flag=true   -- то что нужно
                    //если 2 совпадения то flag=false  -- пропускаем

                    for (int digit = 0; digit < 9; digit++)
                    {
                        if (X1.candidates[digit] && (digit == a || digit == b))
                        {
                            flag = !flag;
                        }
                    }

                    if (flag == false)
                        continue;

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
                        continue;

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

                                        AddElimination(X1.seenCell[n].ind, c);
                                        AddRemovingMark(X1.seenCell[n].ind, c);
                                    }

                                }
                            }
                        }

                        if (foundet)
                        {
                            AddClueMark(Y.ind, a);
                            AddClueMark(Y.ind, b);
                            AddClueMark(X1.ind, a);
                            AddClueMark(X1.ind, c);
                            AddClueMark(X2.ind, b);
                            AddClueMark(X2.ind, c);

                            answer = new AnswerOfTech($"{Discription} по {(c + 1)} : ({(Y.row + 1)};{(Y.column + 1)}) => ({(X1.row + 1)};{(X1.column + 1)}) - ({(X2.row + 1)};{(X2.column + 1)})");
                            SetLists(answer);
                            return answer;
                        }
                    }
                }
            }
            return null;
        }
    }
}
