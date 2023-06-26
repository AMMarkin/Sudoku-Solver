using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    internal static class Logic
    {

        public static string noFound = "Исключений не найдено";

        //массив названий техник
        public static string[] tecniques = new string[] {
            "Открытые одиночки", "Скрытые одиночки","Виртуальные одиночки",
            "Открытые пары", "Cкрытые пары",
            "Открытые тройки", "Скрытые тройки",
            "Открытые четверки", "Скрытые четверки",
            "X-Wings","Swordfish","Jellyfish",
            "Y-Wings",
            "Simple Coloring"
        };

        //simple coloring

        public static List<int[]> clues;    //i,j -- где ключ,         k -- что ключ
        public static List<int[]> removed;  //i,j -- откуда исключаем, k -- что исключаем

        public static List<int[]> chain;    //ind, k => ind, k

        
        public static string findElimination(ref Field field, bool[] tecFlags)
        {

            string answer = noFound;


            string tmp = "";
            clues = new List<int[]>();
            removed = new List<int[]>();
            chain = new List<int[]>();

            //открытые одиночки
            if (tecFlags[0])
            {
                tmp = NakedSingle(ref field);

                //проверка простых исключений
                SimpleRestriction(ref field);

                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //скрытые одиночки
            if (tecFlags[1])
            {
                tmp = HiddenSingle(ref field);

                //проверка простых исключений
                SimpleRestriction(ref field);

                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //виртуальные одиночки
            if (tecFlags[2])
            {
                tmp = VirtualSingle(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //открытые пары
            if (tecFlags[3])
            {
                tmp = NakedPairs(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //скрытые пары
            if (tecFlags[4])
            {
                tmp = HiddenPairs(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //открытые тройки
            if (tecFlags[5])
            {
                tmp = NakedTriples(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }
            //скрытые тройки
            if (tecFlags[6])
            {
                tmp = HiddenTriples(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Открытые четверки
            if (tecFlags[7])
            {
                tmp = NakedQuads(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Скрытые четверки
            if (tecFlags[8])
            {
                tmp = HiddenQuads(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //X-Wings
            if (tecFlags[9])
            {
                tmp = X_Wings(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Swordfish
            if (tecFlags[10])
            {
                tmp = Swordfish(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Jellyfish
            if (tecFlags[11])
            {
                tmp = Jellyfish(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Y-Wings
            if (tecFlags[12])
            {
                tmp = Y_Wings(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Simple Coloring
            if (tecFlags[13])
            {
                tmp = SimpleColoring(ref field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }


            //проверка решения
            tmp = check(ref field);
            if (!tmp.Equals(""))
            {
                return tmp;
            }

            return answer;
        }

        //Simple Coloring
        private static string SimpleColoring(ref Field field)
        {
            string answer = "";
            int[][] matrix = new int[9][];

            for(int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }

            int counter = 0;
            int a = -1;
            int b = -1;


            //для теста ищем только по единицам
            for (int k = 0; k < 1; k++)
            {
                chain = new List<int[]>();
                //переписываю матрицу
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        matrix[i][j] = field[i, j].candidates[k] ? 1 : 0;

                    }
                }

                //создание цепи
                //----------------------------------------------------------------------------------------------------------------------------
                //обхожу все строки
                for(int i = 0; i < 9; i++)
                {
                    counter = 0;
                    for(int j = 0; j < 9; j++)
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
                        for(int j=0; j < 9; j++)
                        {
                            if(matrix[i][j] != 0)
                            {
                                if (a < 0)
                                {
                                    a = 9*i+j;
                                }
                                else
                                {
                                    b = 9*i+j;
                                }
                            }
                        }

                        //добавил в цепь
                        AddToChain(a, b, k);
                    }
                }

                //столбцы
                for(int j = 0; j < 9; j++)
                {

                    counter = 0;
                    for(int i = 0; i < 9; i++)
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
                        for(int i = 0; i < 9; i++)
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
                        AddToChain(a, b, k);

                    }



                }

                //регионы
                for(int y = 0; y < 3; y++)
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
                            AddToChain(a, b, k);
                        }
                    }
                }
                //----------------------------------------------------------------------------------------------------------------------------

                //поиск исключений

                //если исключения есть то исключаем и формируем ответ

            }

            if (chain.Count!=0)
                foreach (int[] item in chain)
                {
                    int i1 = item[0] / 9;
                    int j1 = item[0] % 9;
                    int i2 = item[2] / 9;
                    int j2 = item[2] % 9;

                    answer += (item[1] + 1) + "в (" + (i1 + 1) + ";" + (j1 + 1) + ") => " + (item[3] + 1) + " в (" + (i2 + 1) + ";" + (j2 + 1) + ")\n";
                }


            return answer;
        }

        //добавляем в цепь новую связь
        private static void AddToChain(int ind1,int ind2,int k)
        {
            bool contains = false;
            foreach (int[] item in chain)
            {
                if (item[0] == ind1 && item[1] == k && item[2]==ind2 && item[3] == k)
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                chain.Add(new int[] {ind1,k,ind2,k});
            }
        }

        //Y-Wings
        private static string Y_Wings(ref Field field)
        {
            string answer = "";

            //ищем все ячейки с двумя кандидатами

            List<Field.Cell> list = new List<Field.Cell>();

            int counter = 0;


            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    counter = 0;
                    for(int k = 0; k < 9; k++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            counter++;
                        }
                    }
                    if (counter == 2)
                    {
                        list.Add(field[i, j]);
                    }
                }
            }

            Field.Cell Y;
            Field.Cell X1;
            Field.Cell X2;

            int a = 0;
            int b = 0;
            int c = 0;


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
                for(int k = 0; k < 9; k++)
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
                for(int j = 0; j < Y.seenCell.Length; j++)
                {
                    if (foundet)
                        break;

                    //Берем вторую точку
                    X1 = Y.seenCell[j];

                    counter = 0;
                    for(int k = 0; k < 9; k++)
                    {
                        if (X1.candidates[k])
                        {
                            counter++;
                        }
                    }
                    if (counter != 2)
                        continue;

                    //если 0 совпадений то flag=false  -- пропускаем
                    //если 1 совпадение то flag=true   -- то что нужно
                    //если 2 совпадения то flag=false  -- пропускаем

                    for(int k = 0; k < 9; k++)
                    {
                        if(X1.candidates[k] && (k== a || k == b))
                        {
                            flag = !flag;
                        }
                    }

                    if (flag)
                    {
                        //нужно чтобы совпало именно a
                        flag = false;
                        for(int k=0; k< 9; k++)
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
                        for(int k = 0; k < 9; k++)
                        {
                            if (X1.candidates[k] && k != a)
                            {
                                c = k;
                            }
                        }

                        //ищем второе "крыло"
                        for(int j2=0; j2 < Y.seenCell.Length; j2++)
                        {
                            if (j2 == j)
                                continue;

                            X2 = Y.seenCell[j2];

                            counter = 0;
                            for(int k = 0; k < 9; k++)
                            {
                                if (X2.candidates[k])
                                {
                                    counter++;
                                }
                            }
                            if (counter != 2)
                                continue;

                            counter = 0;

                            flag = false;
                            //считаем совпадения
                            for(int k = 0; k < 9; k++)
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

                            for(int n = 0; n < X1.seenCell.Length; n++)
                            {
                                for(int m=0; m < X2.seenCell.Length; m++)
                                {
                                    //если нашли ячейку видимую из двух крыльев
                                    //ищем есть ли в ней c
                                    if (X1.seenCell[n].Equals(X2.seenCell[m]))
                                    {
                                        bool flagC = false;
                                        for(int k = 0; k < 9; k++)
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

                                            removed.Add(new int[] { X1.seenCell[n].row, X1.seenCell[n].column,c });
                                            X1.seenCell[n].RemoveCandidat(c + 1);
                                            
                                        }

                                    }
                                }
                            }

                            if (foundet)
                            {
                                clues.Add(new int[] { Y.row, Y.column, a });
                                clues.Add(new int[] { Y.row, Y.column, b });
                                clues.Add(new int[] { X1.row, X1.column, a });
                                clues.Add(new int[] { X1.row, X1.column, c });
                                clues.Add(new int[] { X2.row, X2.column, b });
                                clues.Add(new int[] { X2.row, X2.column, c });

                                answer = "Y-Wings по "+(c+1) +" : " +
                                         "(" + (Y.row+1)+";"+(Y.column+1)+") => "+
                                         "(" + (X1.row+1)+";"+(X1.column+1)+") - "+
                                         "(" + (X2.row+1)+";"+(X2.column+1)+")"
                                         ;
                                return answer;

                            }

                        }

                    }

                }
            }
            


            return answer;
        }

        //Jellyfish
        private static string Jellyfish(ref Field field)
        {
            string answer = "";

            int[][] matrix = new int[9][];
            int[][] shape;
            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }
            //по всем числам
            for (int k = 0; k < 9; k++)
            {
                //смотрю строки
                //исключаю в колонках

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[i][j] = 1;
                        }
                        else
                        {
                            matrix[i][j] = 0;
                        }
                    }
                }

                shape = Shape4InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] строки
                //[1] колонки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2] && i != shape[0][3])
                        {
                            field[i, shape[1][0]].RemoveCandidat(k + 1);
                            field[i, shape[1][1]].RemoveCandidat(k + 1);
                            field[i, shape[1][2]].RemoveCandidat(k + 1);
                            field[i, shape[1][3]].RemoveCandidat(k + 1);

                            removed.Add(new int[] { i, shape[1][0],k });
                            removed.Add(new int[] { i, shape[1][1],k });
                            removed.Add(new int[] { i, shape[1][2],k });
                            removed.Add(new int[] { i, shape[1][3],k });
                        }
                    }

                    for (int x = 0; x < 4; x++)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            clues.Add(new int[] { shape[0][x], shape[1][y], k });
                        }
                    }

                    answer = "Jellyfish по " + (k + 1) +
                            " в (" + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) + "-" + (shape[0][2] + 1) + "-" + (shape[0][3] + 1) + ";"
                                   + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) + "-" + (shape[1][2] + 1) + "-" + (shape[1][3] + 1) + ")"
                            ;
                    return answer;

                }


                //смотрю столбцы
                //исключаю строки

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[j][i] = 1;
                        }
                        else
                        {
                            matrix[j][i] = 0;
                        }
                    }
                }

                shape = Shape4InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] колонки
                //[1] строки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2] && i != shape[0][3])
                        {
                            field[shape[1][0], i].RemoveCandidat(k + 1);
                            field[shape[1][1], i].RemoveCandidat(k + 1);
                            field[shape[1][2], i].RemoveCandidat(k + 1);
                            field[shape[1][3], i].RemoveCandidat(k + 1);

                            removed.Add(new int[] { shape[1][0],i,k });
                            removed.Add(new int[] { shape[1][1],i,k });
                            removed.Add(new int[] { shape[1][2],i,k });
                            removed.Add(new int[] { shape[1][3],i,k });
                        }
                    }

                    for (int x = 0; x < 4; x++)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            clues.Add(new int[] { shape[1][x], shape[0][y], k });
                        }
                    }

                    answer = "Jellyfish по" + (k + 1) +
                             " в (" + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) + "-" + (shape[1][2] + 1) + "-" + (shape[1][3] + 1) + ";"
                                    + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) + "-" + (shape[0][2] + 1) + "-" + (shape[0][3] + 1) + ")";
                    return answer;

                }

            }


            return answer;
        }

        //скрытые четверки
        private static string HiddenQuads(ref Field field)
        {
            string answer = "";


            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = HiddenQuadsInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = HiddenQuadsInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = HiddenQuadsInGroup(ref group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //скрытые четверки в группе
        private static string HiddenQuadsInGroup(ref Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
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

                        matrix[j][i] = 1;
                    }
                }
            }

            int[][] shape = Shape4InMatrix(matrix);

            //числа в строках
            //ячейки в колонках

            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2] && i != shape[0][3])
                    {
                        group[shape[1][0]].RemoveCandidat(i + 1);
                        group[shape[1][1]].RemoveCandidat(i + 1);
                        group[shape[1][2]].RemoveCandidat(i + 1);
                        group[shape[1][3]].RemoveCandidat(i + 1);

                        removed.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, i });
                        removed.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, i });
                        removed.Add(new int[] { group[shape[1][2]].row, group[shape[1][2]].column, i });
                        removed.Add(new int[] { group[shape[1][3]].row, group[shape[1][3]].column, i });
                    }
                }
                for (int x = 0; x < shape[1].Length; x++)
                {
                    clues.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, shape[0][x] });
                    clues.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, shape[0][x] });
                    clues.Add(new int[] { group[shape[1][2]].row, group[shape[1][2]].column, shape[0][x] });
                    clues.Add(new int[] { group[shape[1][3]].row, group[shape[1][3]].column, shape[0][x] });
                }

                answer = "Скрытая четверка " + (shape[0][0] + 1) + "/" + (shape[0][1] + 1) + "/" + (shape[0][2] + 1) + "/" + (shape[0][3] + 1) +
                         " в (" + (group[shape[1][0]].row + 1) + ";" + (group[shape[1][0]].column + 1) +
                         ") и (" + (group[shape[1][1]].row + 1) + ";" + (group[shape[1][1]].column + 1) +
                         ") и (" + (group[shape[1][2]].row + 1) + ";" + (group[shape[1][2]].column + 1) +
                         ") и (" + (group[shape[1][3]].row + 1) + ";" + (group[shape[1][3]].column + 1) + ")"
                         ;
            }


            return answer;
        }

        //открытые четверки
        private static string NakedQuads(ref Field field)
        {
            string answer = "";

            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = NakedQuadsInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = NakedQuadsInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = NakedQuadsInGroup(ref group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //открытые тройки в группе
        private static string NakedQuadsInGroup(ref Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
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

                        matrix[i][j] = 1;
                    }
                }
            }

            int[][] shape = Shape4InMatrix(matrix);

            //[0] ячейки
            //[1] кандидаты

            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2] && i != shape[0][3])
                    {
                        group[i].RemoveCandidat(shape[1][0] + 1);
                        group[i].RemoveCandidat(shape[1][1] + 1);
                        group[i].RemoveCandidat(shape[1][2] + 1);
                        group[i].RemoveCandidat(shape[1][3] + 1);

                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][0] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][1] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][2] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][3] });
                    }
                }

                for (int x = 0; x < shape[1].Length; x++)
                {
                    clues.Add(new int[] { group[shape[0][0]].row, group[shape[0][0]].column, shape[1][x] });
                    clues.Add(new int[] { group[shape[0][1]].row, group[shape[0][1]].column, shape[1][x] });
                    clues.Add(new int[] { group[shape[0][2]].row, group[shape[0][2]].column, shape[1][x] });
                    clues.Add(new int[] { group[shape[0][3]].row, group[shape[0][3]].column, shape[1][x] });
                }

                answer = "Открытая четверка " + (shape[1][0] + 1) + "/" + (shape[1][1] + 1) + "/" + (shape[1][2] + 1) + "/" + (shape[1][3] + 1) +
                         " в (" + (group[shape[0][0]].row + 1) + ";" + (group[shape[0][0]].column + 1) +
                         ") и (" + (group[shape[0][1]].row + 1) + ";" + (group[shape[0][1]].column + 1) +
                         ") и (" + (group[shape[0][2]].row + 1) + ";" + (group[shape[0][2]].column + 1) +
                         ") и (" + (group[shape[0][3]].row + 1) + ";" + (group[shape[0][3]].column + 1) + ")"
                         ;
            }


            return answer;
        }

        //нахождение фигуры 4х4 в строках
        private static int[][] Shape4InMatrix(int[][] a)
        {
            //для четверок n=4
            int n = 4;
            int[] rows = new int[n];
            int[] colums = new int[n];

            for (int i = 0; i < n; i++)
            {
                rows[i] = -1;
                colums[i] = -1;
            }

            int[][] answer = new int[][] { rows, colums };
            //считаю колличество строк в которые n вхождений
            int[] sums = new int[a.Length];
            int counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[i].Length; j++)
                {
                    if (a[i][j] != 0)
                    {
                        sums[i]++;
                    }
                }
                if (sums[i] > 1 && sums[i] <= n)
                {
                    counter++;
                }
            }
            //если таких строк меньше n то нужной комбинации нет
            if (counter < n)
            {
                return null;
            }

            //запоминаю номера нужных строк
            int[] digits = new int[counter];
            counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (sums[i] > 1 && sums[i] <= n)
                {
                    digits[counter] = i;
                    counter++;
                }
            }

            counter = 0;
            sums = new int[a.Length];
            if (digits.Length > 1)
            {
                //перебираю все комбинации нужных строк
                for (int k = 0; k < digits.Length; k++)
                {
                    for (int l = k + 1; l < digits.Length; l++)
                    {
                        for (int h = l + 1; h < digits.Length; h++)
                        {
                            for(int g = h+1; g < digits.Length; g++) 
                            {
                                sums = new int[a.Length];
                                //подсчитываю колличество вхождений в выбранные строки
                                for (int j = 0; j < a.Length; j++)
                                {
                                    if (a[digits[k]][j] > 0)
                                    {
                                        sums[j]++;
                                    }
                                    if (a[digits[l]][j] > 0)
                                    {
                                        sums[j]++;
                                    }
                                    if (a[digits[h]][j] > 0)
                                    {
                                        sums[j]++;
                                    }
                                    if (a[digits[g]][j] > 0)
                                    {
                                        sums[j]++;
                                    }
                                }
                                counter = 0;
                                for (int j = 0; j < a.Length; j++)
                                {
                                    if (sums[j] > 0)
                                    {
                                        counter++;
                                    }
                                }

                                if (counter != n)
                                {
                                    continue;
                                }

                                //запоминаю найденную фигуру
                                rows[0] = digits[k];
                                rows[1] = digits[l];
                                rows[2] = digits[h];
                                rows[3] = digits[g];

                                counter = 0;

                                for (int j = 0; j < a.Length; j++)
                                {
                                    if (sums[j] > 1 && sums[j] <= n)
                                    {
                                        colums[counter] = j;
                                        counter++;
                                    }
                                }

                                //смотрю есть ли исключения
                                bool impact = false;
                                for (int j = 0; j < n; j++)
                                {
                                    for (int i = 0; i < a.Length; i++)
                                    {
                                        if (i != rows[0] && i != rows[1] && i != rows[2] && i != rows[3] && a[i][colums[j]] > 0)
                                        {
                                            impact = true;
                                            return new int[][] { rows, colums };
                                        }
                                    }
                                }

                            }
                        }
                    }

                }
            }

            return null;
        }

        //Swordfish
        private static string Swordfish(ref Field field)
        {
            string answer = "";

            int[][] matrix = new int[9][];
            int[][] shape;
            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }
            //по всем числам
            for (int k = 0; k < 9; k++)
            {
                //смотрю строки
                //исключаю в колонках

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[i][j] = 1;
                        }
                        else
                        {
                            matrix[i][j] = 0;
                        }
                    }
                }

                shape = Shape3InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] строки
                //[1] колонки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2])
                        {
                            field[i, shape[1][0]].RemoveCandidat(k + 1);
                            field[i, shape[1][1]].RemoveCandidat(k + 1);
                            field[i, shape[1][2]].RemoveCandidat(k + 1);

                            removed.Add(new int[] { i, shape[1][0],k });
                            removed.Add(new int[] { i, shape[1][1],k });
                            removed.Add(new int[] { i, shape[1][2],k });
                        }
                    }

                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            clues.Add(new int[] { shape[0][x], shape[1][y], k });
                        }
                    }

                    answer = "Swordfish по " + (k + 1) + 
                            " в (" + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) + "-" + (shape[0][2] + 1) + ";" 
                                   + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) + "-" + (shape[1][2] + 1) + ")"
                            ;
                    return answer;

                }


                //смотрю столбцы
                //исключаю строки

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[j][i] = 1;
                        }
                        else
                        {
                            matrix[j][i] = 0;
                        }
                    }
                }

                shape = Shape3InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] колонки
                //[1] строки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2])
                        {
                            field[shape[1][0], i].RemoveCandidat(k + 1);
                            field[shape[1][1], i].RemoveCandidat(k + 1);
                            field[shape[1][2], i].RemoveCandidat(k + 1);

                            removed.Add(new int[] { shape[1][0], i, k });
                            removed.Add(new int[] { shape[1][1], i, k });
                            removed.Add(new int[] { shape[1][2], i, k });
                        }
                    }

                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            clues.Add(new int[] { shape[1][x], shape[0][y], k });
                        }
                    }

                    answer = "Swordfish по" + (k + 1) + 
                             " в (" + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) + "-" + (shape[1][2] + 1) + ";" 
                                    + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) + "-" + (shape[0][2] + 1) + ")";
                    return answer;

                }

            }


            return answer;
        }

        //скрытые тройки
        private static string HiddenTriples(ref Field field)
        {
            string answer = "";


            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = HiddenTriplesInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = HiddenTriplesInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = HiddenTriplesInGroup(ref group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //скрытые пары в группе
        private static string HiddenTriplesInGroup(ref Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
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

                        matrix[j][i] = 1;
                    }
                }
            }

            int[][] shape = Shape3InMatrix(matrix);

            //числа в строках
            //ячейки в колонках

            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for(int i = 0; i < 9; i++)
                {
                    if(i != shape[0][0] && i != shape[0][1] && i != shape[0][2])
                    {
                        group[shape[1][0]].RemoveCandidat(i + 1);
                        group[shape[1][1]].RemoveCandidat(i + 1);
                        group[shape[1][2]].RemoveCandidat(i + 1);

                        removed.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, i });
                        removed.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, i });
                        removed.Add(new int[] { group[shape[1][2]].row, group[shape[1][2]].column, i });
                    }
                }

                for (int x = 0; x < shape[1].Length; x++)
                {
                    clues.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, shape[0][x] });
                    clues.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, shape[0][x] });
                    clues.Add(new int[] { group[shape[1][2]].row, group[shape[1][2]].column, shape[0][x] });
                }


                answer = "Скрытая тройка " + (shape[0][0] + 1) + "/" + (shape[0][1] + 1) + "/" + (shape[0][2] + 1) +
                         " в (" + (group[shape[1][0]].row + 1) + ";" + (group[shape[1][0]].column + 1) +
                         ") и (" + (group[shape[1][1]].row + 1) + ";" + (group[shape[1][1]].column + 1) +
                         ") и (" + (group[shape[1][2]].row + 1) + ";" + (group[shape[1][2]].column + 1) + ")"
                         ;
            }


            return answer;
        }

        //открытые тройки
        private static string NakedTriples(ref Field field)
        {
            string answer = "";


            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = NakedTripesInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = NakedTripesInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = NakedTripesInGroup(ref group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //открытые тройки в группе
        private static string NakedTripesInGroup(ref Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
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

                        matrix[i][j] = 1;
                    }
                }
            }

            int[][] shape = Shape3InMatrix(matrix);

            //ячейки в строках
            //кандидаты в колонках

            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2])
                    {
                        group[i].RemoveCandidat(shape[1][0] + 1);
                        group[i].RemoveCandidat(shape[1][1] + 1);
                        group[i].RemoveCandidat(shape[1][2] + 1);

                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][0] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][1] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][2] });
                    }
                }

                for(int x = 0; x < shape[1].Length; x++)
                {
                    clues.Add(new int[] { group[shape[0][0]].row, group[shape[0][0]].column, shape[1][x] });
                    clues.Add(new int[] { group[shape[0][1]].row, group[shape[0][1]].column, shape[1][x] });
                    clues.Add(new int[] { group[shape[0][2]].row, group[shape[0][2]].column, shape[1][x] });
                }

                answer = "Открытая тройка " + (shape[1][0] + 1) + "/" + (shape[1][1] + 1) + "/"+ (shape[1][2] + 1) +
                         " в (" + (group[shape[0][0]].row + 1) + ";" + (group[shape[0][0]].column + 1) +
                         ") и (" + (group[shape[0][1]].row + 1) + ";" + (group[shape[0][1]].column + 1)+
                         ") и (" + (group[shape[0][2]].row + 1) + ";" + (group[shape[0][2]].column + 1)+")"
                         ;
            }


            return answer;
        }

        //нахождение фигуры 3х3 в строках
        private static int[][] Shape3InMatrix(int[][] a)
        {
            //для троек n=2
            int n = 3;
            int[] rows = new int[n];
            int[] colums = new int[n];

            for (int i = 0; i < n; i++)
            {
                rows[i] = -1;
                colums[i] = -1;
            }

            int[][] answer = new int[][] { rows, colums };
            //считаю колличество строк в которые n вхождений
            int[] sums = new int[a.Length];
            int counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[i].Length; j++)
                {
                    if (a[i][j] != 0)
                    {
                        sums[i]++;
                    }
                }
                if (sums[i] > 1 && sums[i] <= n)
                {
                    counter++;
                }
            }
            //если таких строк меньше n то нужной комбинации нет
            if (counter < n)
            {
                return null;
            }

            //запоминаю номера нужных строк
            int[] digits = new int[counter];
            counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (sums[i] > 1 && sums[i] <= n)
                {
                    digits[counter] = i;
                    counter++;
                }
            }

            counter = 0;
            sums = new int[a.Length];
            if (digits.Length > 1)
            {
                //перебираю все комбинации нужных строк
                for (int k = 0; k < digits.Length; k++)
                {
                    for (int l = k + 1; l < digits.Length; l++)
                    {
                        for (int h = l + 1; h < digits.Length; h++)
                        {
                            sums = new int[a.Length];
                            //подсчитываю колличество вхождений в выбранные строки
                            for (int j = 0; j < a.Length; j++)
                            {
                                if (a[digits[k]][j] > 0)
                                {
                                    sums[j]++;
                                }
                                if (a[digits[l]][j] > 0)
                                {
                                    sums[j]++;
                                }
                                if (a[digits[h]][j] > 0)
                                {
                                    sums[j]++;
                                }
                            }
                            counter = 0;
                            for (int j = 0; j < a.Length; j++)
                            {
                                if (sums[j] >0 )
                                {
                                    counter++;
                                }
                            }

                            if (counter!=n)
                            {
                                continue;
                            }

                            //запоминаю найденную фигуру
                            rows[0] = digits[k];
                            rows[1] = digits[l];
                            rows[2] = digits[h];
                            counter = 0;

                            for (int j = 0; j < a.Length; j++)
                            {
                                if (sums[j] > 1 && sums[j] <= n)
                                {
                                    colums[counter] = j;
                                    counter++;
                                }
                            }

                            //смотрю есть ли исключения
                            bool impact = false;
                            for (int j = 0; j < n; j++)
                            {
                                for (int i = 0; i < a.Length; i++)
                                {
                                    if (i != rows[0] && i != rows[1] && i != rows[2] && a[i][colums[j]] > 0)
                                    {
                                        impact = true;
                                        return new int[][] { rows, colums };
                                    }
                                }
                            }

                        }
                    }

                }
            }

            return null;
        }

        //X-Wings
        private static string X_Wings(ref Field field)
        {
            string answer = "";

            int[][] matrix = new int[9][];
            int[][] shape;
            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }
            //по всем числам
            for (int k = 0; k < 9; k++)
            {
                //смотрю строки
                //исключаю в колонках

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[i][j] = 1;
                        }
                        else
                        {
                            matrix[i][j] = 0;
                        }
                    }
                }

                shape = Shape2InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] строки
                //[1] колонки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1])
                        {
                            field[i, shape[1][0]].RemoveCandidat(k + 1);
                            field[i, shape[1][1]].RemoveCandidat(k + 1);

                            removed.Add(new int[] { i, shape[1][0],k });
                            removed.Add(new int[] { i, shape[1][1],k });
                        }
                    }
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            clues.Add(new int[] { shape[0][x], shape[1][y], k });
                        }
                    }
                    answer = "X-Wings по " + (k + 1) + " в (" + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) +
                             ";" + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) + ")";
                    return answer;

                }


                //смотрю столбцы
                //исключаю строки

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[j][i] = 1;
                        }
                        else
                        {
                            matrix[j][i] = 0;
                        }
                    }
                }

                shape = Shape2InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] колонки
                //[1] строки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1])
                        {
                            field[shape[1][0], i].RemoveCandidat(k + 1);
                            field[shape[1][1], i].RemoveCandidat(k + 1);

                            removed.Add(new int[] { shape[1][0], i, k });
                            removed.Add(new int[] { shape[1][1], i, k });
                        }
                    }

                    for(int x = 0; x < 2; x++)
                    {
                        for(int y = 0; y < 2; y++)
                        {
                            clues.Add(new int[] { shape[1][x], shape[0][y], k });
                        }
                    }

                    answer = "X-Wings по" + (k + 1) + " в (" + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) +
                             ";" + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) + ")";
                    return answer;

                }

            }


            return answer;
        }

        //скрытые пары
        private static string HiddenPairs(ref Field field)
        {
            string answer = "";


            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = HiddenPairsInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = HiddenPairsInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = HiddenPairsInGroup(ref group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //скрытые пары в группе

        private static string HiddenPairsInGroup(ref Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
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

                        matrix[j][i] = 1;
                    }
                }
            }


            int[][] shape = Shape2InMatrix(matrix);
            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i != shape[0][0] && i != shape[0][1])
                    {
                        group[shape[1][0]].RemoveCandidat(i + 1);
                        group[shape[1][1]].RemoveCandidat(i + 1);

                        removed.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column,i });
                        removed.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column,i });
                    }
                }

                clues.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, shape[0][0] });
                clues.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, shape[0][1] });
                clues.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, shape[0][0] });
                clues.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, shape[0][1] });

                answer = "Скрытая пара " + (shape[0][0] + 1) + "/" + (shape[0][1] + 1) +
                         " в (" + (group[shape[1][0]].row + 1) + ";" + (group[shape[1][0]].column + 1) +
                         ") и (" + (group[shape[1][1]].row + 1) + ";" + (group[shape[1][1]].column + 1) + ")"
                         ;
            }

            return answer;
        }

        //нахождение фигуры 2х2 в строках
        private static int[][] Shape2InMatrix(int[][] a)
        {
            //для пар n=2
            int n = 2;
            int[] rows = new int[n];
            int[] colums = new int[n];

            for (int i = 0; i < n; i++)
            {
                rows[i] = -1;
                colums[i] = -1;
            }

            int[][] answer = new int[][] { rows, colums };
            //считаю колличество строк в которые n вхождений
            int[] sums = new int[a.Length];
            int counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[i].Length; j++)
                {
                    if (a[i][j] != 0)
                    {
                        sums[i]++;
                    }
                }
                if (sums[i] > 1 && sums[i] <= n)
                {
                    counter++;
                }
            }
            //если таких строк меньше n то нужной комбинации нет
            if (counter < n)
            {
                return null;
            }

            //запоминаю номера нужных строк
            int[] digits = new int[counter];
            counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (sums[i] > 1 && sums[i] <= n)
                {
                    digits[counter] = i;
                    counter++;
                }
            }

            counter = 0;
            sums = new int[a.Length];
            if (digits.Length > 1)
            {
                //перебираю все комбинации нужных строк
                for (int k = 0; k < digits.Length; k++)
                {
                    for (int l = k + 1; l < digits.Length; l++)
                    {
                        sums = new int[a.Length];
                        //подсчитываю колличество вхождений в выбранные строки
                        for (int j = 0; j < a.Length; j++)
                        {
                            if (a[digits[k]][j] > 0)
                            {
                                sums[j]++;
                            }
                            if (a[digits[l]][j] > 0)
                            {
                                sums[j]++;
                            }
                        }
                        counter = 0;
                        for (int j = 0; j < a.Length; j++)
                        {
                            if (sums[j] > 1 && sums[j] <= n)
                            {
                                counter++;
                            }
                        }

                        if (counter < n)
                        {
                            continue;
                        }

                        //запоминаю найденную фигуру
                        rows[0] = digits[k];
                        rows[1] = digits[l];
                        counter = 0;

                        for (int j = 0; j < a.Length; j++)
                        {
                            if (sums[j] > 1 && sums[j] <= n)
                            {
                                colums[counter] = j;
                                counter++;
                            }
                        }

                        //смотрю есть ли исключения
                        bool impact = false;
                        for (int j = 0; j < n; j++)
                        {
                            for (int i = 0; i < a.Length; i++)
                            {
                                if (i != rows[0] && i != rows[1] && a[i][colums[j]] > 0)
                                {
                                    impact = true;
                                    return new int[][] { rows, colums };
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        //виртуальные одиночки
        private static string VirtualSingle(ref Field field)
        {
            string answer = "";
            /*строки
             * по виртуальной одиночке в строке можно исключить только кандидатов в регионе
             * обходим все строки
             * смотрим все числа
             * если кандидаты только в одном регионе только исключаем кандидатов из региона
             */

            //строка
            for (int i = 0; i < 9; i++)
            {
                //число
                for (int k = 0; k < 9; k++)
                {
                    int count = 0;
                    //столбец
                    //подсчет кандидатов
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            count++;
                        }
                    }
                    //если кандидатов по строке больше 3 то пропускаем число
                    if (count != 2 && count != 3)
                        continue;
                    int[] flags = new int[count];
                    int[] cluesInd = new int[count];
                    //проверяю пренадлежность к одному региону
                    int ind = 0;
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            flags[ind] = j / 3;
                            cluesInd[ind] = j;
                            ind++;
                        }
                    }
                    bool flag = true;
                    for (int x = 1; x < flags.Length; x++)
                    {
                        if (flags[x] != flags[x - 1])
                        {
                            flag = false;
                        }
                    }
                    //если число в строке может стоять только в одном регионе, то в этом регионе оно может стоять только в этой строке
                    if (flag)
                    {
                        //рассматриваемое число k
                        //номер текущей строки i
                        //номер региона по строке лежит в flags
                        //значит левая верхняя ячейка нужного региона (3*flags[0] ; 3*i/3)
                        //от нее смотрим 9 ячеек кроме найденной строки и пытаемся исключить кандидатов
                        //если было хоть одно исключение то формируем ответ

                        int startX = 3 * flags[0];
                        int startY = 3 * (i / 3);

                        bool impact = false;
                        for (int y = 0; y < 3; y++)
                        {
                            if ((startY + y) != i)
                            {
                                for (int x = 0; x < 3; x++)
                                {
                                    if (field[startY + y, startX + x].value < 1)
                                    {
                                        if (field[startY + y, startX + x].candidates[k])
                                        {
                                            field[startY + y, startX + x].RemoveCandidat(k + 1);
                                            impact = true;

                                            
                                            removed.Add(new int[] { startY + y, startX + x, k });
                                        }
                                        if (impact)
                                        {
                                            for(int n = 0; n < cluesInd.Length; n++)
                                            {
                                                clues.Add(new int[] { i, cluesInd[n],k });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //если найдено исключение то формируем ответ
                        if (impact)
                        {
                            answer = "Cтрока  " + (i + 1) + ": виртуальная одиночка " + (k + 1) + " в регионе " + (3 * startY / 3 + startX / 3 + 1)
                                + "";
                            return answer;
                        }
                    }

                }

            }


            //столбец
            for (int j = 0; j < 9; j++)
            {
                //число
                for (int k = 0; k < 9; k++)
                {
                    int count = 0;
                    //строка
                    //подсчет кандидатов
                    for (int i = 0; i < 9; i++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            count++;
                        }
                    }
                    //если кандидатов по столбце больше 3 то пропускаем число
                    if (count != 2 && count != 3)
                        continue;
                    int[] flags = new int[count];
                    int[] cluesInd = new int[count];

                    //проверяю пренадлежность к одному региону
                    int ind = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            flags[ind] = i / 3;
                            cluesInd[ind] = i;
                            ind++;
                        }
                    }
                    bool flag = true;
                    for (int x = 1; x < flags.Length; x++)
                    {
                        if (flags[x] != flags[x - 1])
                        {
                            flag = false;
                        }
                    }

                    if (flag)
                    {
                        //рассматриваемое число k
                        //номер текущего столбца j
                        //номер региона по столбцу лежит в flags
                        //значит левая верхняя ячейка нужного региона (3*j/3 ; 3*flags[0])
                        //от нее смотрим 9 ячеек кроме найденного столбца и пытаемся исключить кандидатов
                        //если было хоть одно исключение то формируем ответ

                        int startX = 3 * (j / 3);
                        int startY = 3 * flags[0];

                        bool impact = false;
                        for (int x = 0; x < 3; x++)
                        {
                            if ((startX + x) != j)
                            {
                                for (int y = 0; y < 3; y++)
                                {
                                    if (field[startY + y, startX + x].value < 1)
                                    {
                                        if (field[startY + y, startX + x].candidates[k])
                                        {
                                            field[startY + y, startX + x].RemoveCandidat(k + 1);
                                            impact = true;

                                            removed.Add(new int[] { startY + y, startX + x, k });
                                        }
                                        if (impact)
                                        {
                                            for (int n = 0; n < cluesInd.Length; n++)
                                            {
                                                clues.Add(new int[] {cluesInd[n],j,k });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //если найдено исключение то формируем ответ
                        if (impact)
                        {
                            answer = "Cтолбец " + (j + 1) + ": виртуальная одиночка " + (k + 1) + " в регионе " + (3 * startY / 3 + startX / 3 + 1)
                                + "";
                            return answer;
                        }
                    }

                }

            }


            //регион
            //номера регионов
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    //число
                    for (int k = 0; k < 9; k++)
                    {

                        //посчет кандидатов
                        int count = 0;
                        //обход региона
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (field[3 * y + i, 3 * x + j].candidates[k])
                                {
                                    count++;
                                }
                            }
                        }
                        //если более трех то пропускаем
                        if (count != 2 && count != 3)
                            continue;


                        //проверка в одной ли они строке

                        int[] indexes = new int[count];
                        int[] cluesInd = new int[count];
                        count = 0;
                        //обходим регион
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (field[3 * y + i, 3 * x + j].candidates[k])
                                {
                                    //запоминаем строку
                                    indexes[count] = 3 * y + i;
                                    cluesInd[count] = 3 * x + j;
                                    count++;
                                }
                            }
                        }
                        //флаг что в одной строке
                        bool flag = true;
                        for (int i = 1; i < indexes.Length; i++)
                        {
                            if (indexes[i] != indexes[i - 1])
                            {
                                flag = false;
                            }
                        }
                        //номер региона x,y
                        //номер строки лежит в indexes[0]

                        

                        //исключение из строки
                        if (flag)
                        {
                            //флаг что было исключение
                            bool impact = false;

                            //идем по всем столбцам найденной строки
                            for (int n = 0; n < 9; n++)
                            {
                                //пропуская текущий регион
                                if (n / 3 != x)
                                {
                                    if (field[indexes[0], n].value < 1)
                                    {
                                        if (field[indexes[0], n].candidates[k])
                                        {
                                            field[indexes[0], n].RemoveCandidat(k + 1);
                                            impact = true;

                                            removed.Add(new int[] { indexes[0], n, k });
                                        }
                                    }
                                }
                            }
                            //если было исключение то формируем ответ
                            if (impact)
                            {
                                for(int n = 0; n < cluesInd.Length; n++)
                                {
                                    clues.Add(new int[] { indexes[0], cluesInd[n],k });
                                }
                                answer = "Регион  " + (y * 3 + x + 1) + ": виртуальная одиночка " + (k + 1) + " в строке " + (indexes[0] + 1) + "";
                                return answer;
                            }

                        }



                        //проверка в одном ли они столбце
                        //исключение из столбца
                        indexes = new int[count];
                        cluesInd = new int[count];
                        count = 0;
                        //обходим регион
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (field[3 * y + i, 3 * x + j].candidates[k])
                                {
                                    //запоминаем стобец
                                    indexes[count] = 3 * x + j;
                                    cluesInd[count] = 3 * y + i;
                                    count++;
                                }
                            }
                        }
                        //флаг что в одном столбце
                        flag = true;
                        for (int i = 1; i < indexes.Length; i++)
                        {
                            if (indexes[i] != indexes[i - 1])
                            {
                                flag = false;
                            }
                        }
                        //номер региона x,y
                        //номер столбца лежит в indexes[0]


                        //исключение из столбца
                        if (flag)
                        {
                            //флаг что было исключение
                            bool impact = false;

                            //идем по всем строкам найденного столбца
                            for (int n = 0; n < 9; n++)
                            {
                                //пропуская текущий регион
                                if (n / 3 != y)
                                {
                                    if (field[n, indexes[0]].value < 1)
                                    {
                                        if (field[n, indexes[0]].candidates[k])
                                        {
                                            field[n, indexes[0]].RemoveCandidat(k + 1);
                                            impact = true;

                                            removed.Add(new int[] { n, indexes[0], k });
                                        }
                                    }
                                }
                            }
                            //если было исключение то формируем ответ
                            if (impact)
                            {
                                for(int n = 0; n < cluesInd.Length; n++)
                                {
                                    clues.Add(new int[] { cluesInd[n], indexes[0],k });
                                }
                                answer = "Регион " + (y * 3 + x + 1) + ": виртуальная одиночка " + (k + 1) + " в столбце " + (indexes[0] + 1) + "";
                                return answer;
                            }

                        }

                    }
                }
            }
            return answer;
        }

        //открытые пары
        private static string NakedPairs(ref Field field)
        {

            string answer = "";

            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = NakedPairsInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = NakedPairsInGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = NakedPairsInGroup(ref group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }
            return answer;
        }

        //способ на матрицах для масштабирования
        private static string NakedPairsInGroup(ref Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
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

                        matrix[i][j] = 1;
                    }
                }
            }


            int[][] shape = Shape2InMatrix(matrix);

            //ячейки в строках
            //кандидаты в колонках

            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i != shape[0][0] && i != shape[0][1])
                    {
                        group[i].RemoveCandidat(shape[1][0] + 1);
                        group[i].RemoveCandidat(shape[1][1] + 1);

                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][0] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][1] });
                    }
                }

                clues.Add(new int[] { group[shape[0][0]].row, group[shape[0][0]].column, shape[1][0] });
                clues.Add(new int[] { group[shape[0][0]].row, group[shape[0][0]].column, shape[1][1] });
                clues.Add(new int[] { group[shape[0][1]].row, group[shape[0][1]].column, shape[1][0] });
                clues.Add(new int[] { group[shape[0][1]].row, group[shape[0][1]].column, shape[1][1] });

                answer = "Открытая пара " + (shape[1][0] + 1) + "/" + (shape[1][1] + 1) +
                         " в (" + (group[shape[0][0]].row + 1) + ";" + (group[shape[0][0]].column + 1) +
                         ") и (" + (group[shape[0][1]].row + 1) + ";" + (group[shape[0][1]].column + 1) + ")"
                         ;
            }

            return answer;
        }

        //скрытые одиночки
        private static string HiddenSingle(ref Field field)
        {
            string answer = "";

            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = HiddenSingleGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = HiddenSingleGroup(ref group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = HiddenSingleGroup(ref group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //скрытые одиночки в группе
        private static string HiddenSingleGroup(ref Field.Cell[] group)
        {
            string answer = "";

            int count = 0;
            int index = -1;
            int v = 0;
            for (int k = 0; k < 9; k++)
            {
                count = 0;
                for (int i = 0; i < 9; i++)
                {
                    if (group[i].candidates[k])
                    {
                        count++;
                        index = i;
                    }
                }
                if (count == 1)
                {
                    v = k + 1;
                    group[index].SetValue(v);
                    clues.Add(new int[] { group[index].row, group[index].column, v-1});
                    answer = "Найдена скрытая одиночка: " + v.ToString() + " в (" + (group[index].row + 1).ToString() + ";" + (group[index].column + 1).ToString() + ")";
                    return answer;
                }
            }


            return answer;
        }

        //открытые одиночки
        private static string NakedSingle(ref Field field)
        {
            string answer = "";
            //обход всех ячеек
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //подсчет колличества кандидатов
                    int count = 0;
                    if (field[i, j].value == -1)
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            if (field[i, j].candidates[k])
                            {
                                count++;
                            }
                        }
                    }
                    //если остался один кандидат, выставляем значение
                    if (count == 1)
                    {
                        int v = 0;
                        for (int k = 0; k < 9; k++)
                        {
                            if (field[i, j].candidates[k])
                            {
                                v = k + 1;
                            }
                        }

                        field[i, j].SetValue(v);

                        clues.Add(new int[] {i,j,v-1});

                        answer = "Найдена открытая одиночка: " + v.ToString() + " в (" + (i + 1).ToString() + ";" + (j + 1).ToString() + ")";
                        return answer;
                    }
                }
            }
            return answer;
        }

        //простые ислючения
        //способ на списке
        public static void SimpleRestriction(ref Field field)
        {
            int value = 0;
            //обход всего поля
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    value = field[i, j].value;
                    //если известно число
                    if(value > 0)
                    {
                        //обходим все видимые ячейки
                        for(int k = 0; k < field[i, j].seenCell.Length;k++) 
                        {

                            //если есть исключение то исключаем
                            if(field[i, j].seenCell[k].candidates[value-1])
                            {
                                field[i, j].seenCell[k].RemoveCandidat(value);
                            }
                        }
                    }
                }
            }
        }

        //проверка
        private static string check(ref Field field)
        {
            string answer = "";
            bool right = true;

            //проверка на решение
            bool full = true;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (field[i, j].value < 1)
                    {
                        full = false;
                    }
                }
            }
            if (full)
            {
                answer = "Судоку решено!";
                return answer;
            }

            //проверка на ошибки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (field[i, j].value != -1)
                    {
                        //обход по столбцу
                        for (int x = 0; x < 9; x++)
                        {
                            if (x != i & field[x, j].value == field[i, j].value)
                            {
                                right = false;
                                answer = "Ошибка! Совпадение в столбце " + (j + 1).ToString() + ": (" + (x + 1).ToString() + ";" + (j + 1).ToString() + ") и (" + (i + 1).ToString() + ";" + (j + 1).ToString() + ")!";
                                return answer;
                            }
                        }
                        //обход по строке
                        for (int x = 0; x < 9; x++)
                        {
                            if (x != j & field[i, x].value == field[i, j].value)
                            {
                                right = false;
                                answer = "Ошибка! Совпадение в строке " + (i + 1).ToString() + ": (" + (i + 1).ToString() + ";" + (x + 1).ToString() + ") и (" + (i + 1).ToString() + ";" + (j + 1).ToString() + ")!";
                                return answer;
                            }
                        }
                        //обход в региону
                        //левый верхний угол региона
                        int indX = 3 * (i / 3);
                        int indY = 3 * (j / 3);

                        for (int x = 0; x < 3; x++)
                        {
                            for (int y = 0; y < 3; y++)
                            {
                                if ((indX + x) != i & (indY + y) != j & field[(indX + x), (indY + y)].value == field[i, j].value)
                                {
                                    right = false;
                                    answer = "Ошибка! Совпадение в регионе " + (indY * 3 + indX + 1).ToString() + ": (" + ((indX + x) + 1).ToString() + ";" + ((indY + y) + 1).ToString() + ") и (" + (i + 1).ToString() + ";" + (j + 1).ToString() + ")!";
                                    return answer;
                                }
                            }
                        }
                    }
                }
            }


            return answer;
        }
        

        public static string makeAnswer(int i, int j, int v, ref Field field)
        {
            return "Исключена " + v.ToString() + " из ячейки (" + (i + 1).ToString() + ";" + (j + 1).ToString() + ")";
        }
    }
}
