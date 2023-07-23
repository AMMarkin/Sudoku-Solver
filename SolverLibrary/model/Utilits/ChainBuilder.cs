using System.Collections.Generic;
using SolverLibrary.model.field;


namespace SolverLibrary.model.Utilits
{
    public class ChainBuilder
    {
        //проверка видят ли ячейки друг друга
        private bool IsSeen(int ind1, int ind2)
        {
            bool res = false;
            int i1, i2;
            int j1, j2;

            i1 = ind1 / 9;
            j1 = ind1 % 9;

            i2 = ind2 / 9;
            j2 = ind2 % 9;

            //видят по строке
            if (i1 == i2)
            {
                res = true;
            }
            //видят по столбцу
            if (j1 == j2)
            {
                res = true;
            }
            //видят по региону
            if ((3 * (i1 / 3) + j1 / 3) == (3 * (i2 / 3) + j2 / 3))
            {
                res = true;
            }

            return res;
        }

        //создание сильной цепи 
        public ChainBuilder CreateChain(Field field, params int[] kArray)
        {
            int[][] matrix = new int[9][];

            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }

            int counter;
            int a;
            int b;

            if (field.Buffer.chain != null)
            {
                field.Buffer.chain.Clear();
            }
            else
            {
                field.Buffer.chain = new List<int[]>();
            }
            if (field.Buffer.weak != null)
            {
                field.Buffer.weak.Clear();
            }
            else
            {
                field.Buffer.weak = new List<int[]>();
            }
            if (field.Buffer.chainUnits != null)
            {
                field.Buffer.chainUnits.Clear();
            }
            else
            {
                field.Buffer.chainUnits = new List<int[]>();
            }
            //заполняем цепи для каждого числа отдельно
            foreach (int k in kArray)
            {

                //переписываю матрицу
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        matrix[i][j] = field[i, j].candidates[k] ? 1 : 0;

                    }
                }

                //создание цепи 
                //сильные связи
                //----------------------------------------------------------------------------------------------------------------------------
                //обхожу все строки
                for (int i = 0; i < 9; i++)
                {
                    counter = 0;
                    for (int j = 0; j < 9; j++)
                    {
                        if (matrix[i][j] != 0)
                        {
                            counter++;
                        }
                    }

                    //если нашли сильную связь
                    //записываем в цепь
                    if (counter == 2)
                    {
                        a = -1;
                        b = -1;
                        //записал индексы
                        for (int j = 0; j < 9; j++)
                        {
                            if (matrix[i][j] != 0)
                            {
                                if (a < 0)
                                {
                                    a = 9 * i + j;
                                }
                                else
                                {
                                    b = 9 * i + j;
                                }
                            }
                        }

                        //добавил в цепь
                        AddLinkToChain(a, b, k, k, field);
                        AddUnitToChain(a, k, field);
                        AddUnitToChain(b, k, field);
                    }
                }

                //столбцы
                for (int j = 0; j < 9; j++)
                {

                    counter = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        if (matrix[i][j] != 0)
                        {
                            counter++;
                        }
                    }
                    //если нашел сильную связь
                    if (counter == 2)
                    {
                        //записал индексы
                        a = -1;
                        b = -1;
                        for (int i = 0; i < 9; i++)
                        {
                            if (matrix[i][j] != 0)
                            {
                                if (a < 0)
                                {
                                    a = i * 9 + j;
                                }
                                else
                                {
                                    b = i * 9 + j;
                                }
                            }
                        }
                        //добавил в цепь
                        AddLinkToChain(a, b, k, k, field);
                        AddUnitToChain(a, k, field);
                        AddUnitToChain(b, k, field);

                    }



                }

                //регионы
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {

                        counter = 0;
                        //считаю кандидатов
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (matrix[3 * y + i][3 * x + j] != 0)
                                {
                                    counter++;
                                }

                            }
                        }

                        //если нашел сильную связь
                        if (counter == 2)
                        {
                            //записал индексы
                            a = -1;
                            b = -1;
                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    if (matrix[3 * y + i][3 * x + j] != 0)
                                    {
                                        if (a < 0)
                                        {
                                            a = 9 * (3 * y + i) + (3 * x + j);
                                        }
                                        else
                                        {
                                            b = 9 * (3 * y + i) + (3 * x + j);
                                        }

                                    }
                                }
                            }
                            //добавил в цепь
                            AddLinkToChain(a, b, k, k, field);
                            AddUnitToChain(a, k, field);
                            AddUnitToChain(b, k, field);
                        }
                    }
                }
                //----------------------------------------------------------------------------------------------------------------------------
                //слабые связи
                //fillWeakLinks();
            }

            return this;
        }

        //заполнение слабых связей
        public ChainBuilder FillWeakLinks(Field field)
        {
            //полный перебор

            foreach (int[] unit1 in field.Buffer.chainUnits)
            {
                foreach (int[] unit2 in field.Buffer.chainUnits)
                {
                    if (!unit1.Equals(unit2))
                    {
                        bool linked = false;
                        foreach (int[] link in field.Buffer.chain)
                        {
                            //если сильно связаны то пропускаем
                            if ((link[0] == unit1[0] && link[1] == unit1[1] && link[2] == unit2[0] && link[3] == unit2[1])
                                ||
                                (link[0] == unit2[0] && link[1] == unit2[1] && link[2] == unit1[0] && link[3] == unit1[1]))
                            {
                                linked = true;
                                break;
                            }
                        }
                        //если не связаны сильно то
                        //проверяем одно ли число содержат
                        //проверяем видят ли они друг друга
                        if (!linked)
                        {
                            if ((unit1[1] == unit2[1]) && IsSeen(unit1[0], unit2[0]))
                            {
                                //добавляю слабую связь
                                field.Buffer.weak.Add(new int[] { unit1[0], unit1[1], unit2[0], unit2[1] });
                            }

                        }
                    }
                }
            }

            return this;
        }

        //дфс по сильным и слабым связям
        private void DfsWeakStrong(ref bool[] visited, ref int[] component, ref int components, int v, Field field)
        {
            visited[v] = true;
            component[v] = components;
            foreach (int[] link in field.Buffer.chain)
            {
                //нашли ребро

                if (field.Buffer.chainUnits[v][0] == link[0] && field.Buffer.chainUnits[v][1] == link[1])
                {
                    //нахожу номер конца ребра в field.Buffer.chainUnits
                    int count = 0;
                    foreach (int[] unit in field.Buffer.chainUnits)
                    {
                        if (unit[0] == link[2] && unit[1] == link[3])
                        {
                            if (!visited[count])
                            {
                                DfsWeakStrong(ref visited, ref component, ref components, count, field);
                            }
                        }
                        else
                        {
                            count++;
                        }

                    }
                }
            }

            foreach (int[] link in field.Buffer.weak)
            {
                //нашли ребро

                if (field.Buffer.chainUnits[v][0] == link[0] && field.Buffer.chainUnits[v][1] == link[1])
                {
                    //нахожу номер конца ребра в field.Buffer.chainUnits
                    int count = 0;
                    foreach (int[] unit in field.Buffer.chainUnits)
                    {
                        if (unit[0] == link[2] && unit[1] == link[3])
                        {
                            if (!visited[count])
                            {
                                DfsWeakStrong(ref visited, ref component, ref components, count, field);
                            }
                        }
                        else
                        {
                            count++;
                        }

                    }
                }
            }
        }

        //дфс по сильным связям
        private void DfsStrong(ref bool[] visited, ref int[] component, ref int components, int v, Field field)
        {
            visited[v] = true;
            component[v] = components;
            foreach (int[] link in field.Buffer.chain)
            {
                //нашли ребро

                if (field.Buffer.chainUnits[v][0] == link[0] && field.Buffer.chainUnits[v][1] == link[1])
                {
                    //нахожу номер конца ребра в field.Buffer.chainUnits
                    int count = 0;
                    foreach (int[] unit in field.Buffer.chainUnits)
                    {
                        if (unit[0] == link[2] && unit[1] == link[3])
                        {
                            if (!visited[count])
                            {
                                DfsStrong(ref visited, ref component, ref components, count, field);
                            }
                        }
                        else
                        {
                            count++;
                        }

                    }
                }
            }
        }

        //добавление в цепь новой связь
        private void AddLinkToChain(int ind1, int ind2, int k1, int k2, Field field)
        {
            bool contains = false;
            foreach (int[] item in field.Buffer.chain)
            {
                if (item[0] == ind1 && item[1] == k1 && item[2] == ind2 && item[3] == k2)
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                field.Buffer.chain.Add(new int[] { ind1, k1, ind2, k2 });
            }
            contains = false;
            foreach (int[] item in field.Buffer.chain)
            {
                if (item[0] == ind2 && item[1] == k2 && item[2] == ind1 && item[3] == k1)
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                field.Buffer.chain.Add(new int[] { ind2, k2, ind1, k1 });
            }
        }

        //добавление в цепь нового звена
        private void AddUnitToChain(int ind, int k, Field field)
        {
            bool contains = false;
            foreach (int[] unit in field.Buffer.chainUnits)
            {
                if (unit[0] == ind && unit[1] == k)
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                field.Buffer.chainUnits.Add(new int[] { ind, k });
            }
        }
        //--------------------------------------------------------------------------------------------------------

    }
}
