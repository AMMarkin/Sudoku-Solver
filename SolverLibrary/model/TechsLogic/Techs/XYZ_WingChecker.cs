using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class XYZ_WingChecker : TechChecker
    {
        public override TechType Type => TechType.XYZ_Wing;

        protected override string Discription => "XYZ-Wing";


        //XYZ-Wing
        //TODO вынести поиск второго крыла в отдельный метод
        protected override AnswerOfTech FindElimination(Field field)
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
                                        AddElimination(field[i, 3 * (j / 3) + r].ind, rem);
                                        AddRemovingMark(field[i, 3 * (j / 3) + r].ind, rem);

                                        for (int k = 0; k < 9; k++)
                                        {
                                            if (field[i, j].candidates[k])
                                            {
                                                AddClueMark(9 * i + j, k);
                                            }
                                            if (field[i1, j1].candidates[k])
                                            {
                                                AddClueMark(9 * i1 + j1, k);
                                            }
                                            if (field[i2, j2].candidates[k])
                                            {
                                                AddClueMark(9 * i2 + j2, k);
                                            }
                                        }

                                        answer = MakeAnswer($"{Discription}: исключена {(rem + 1)} из ({(i + 1)};{(3 * (j / 3) + r + 1)})");
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
                                        AddElimination(field[3 * (i / 3) + r, j].ind, rem);
                                        AddRemovingMark(field[3 * (i / 3) + r, j].ind, rem);

                                        for (int k = 0; k < 9; k++)
                                        {
                                            if (field[i, j].candidates[k])
                                            {
                                                AddClueMark(9 * i + j, k);
                                            }
                                            if (field[i1, j1].candidates[k])
                                            {
                                                AddClueMark(9 * i1 + j1, k);
                                            }
                                            if (field[i2, j2].candidates[k])
                                            {
                                                AddClueMark(9 * i2 + j2, k);
                                            }
                                        }

                                        answer = MakeAnswer($"{Discription}: исключена {(rem + 1)} из ({(3 * (i / 3) + r + 1)};{(j + 1)})");
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

    }
}
