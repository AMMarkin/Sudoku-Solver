using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SudokuSolver.controller
{
    internal class WFController : IController
    {
        private Field _field;                           //поле (данные)
        private Grid _grid;                             //поле (VIEW)
        private Solver _solver;                         //форма солвера
        private Constructor _constructor;               //форма конструктора
        private Loader _loader;                         //форма загрузчика
        private bool[] usedtechs;

        Field IController.Field { get => _field; set => _field = value; }
        Grid IController.Grid { get => _grid; set => _grid = value; }
        Solver IController.Solver { get => _solver; set => _solver = value; }
        Loader IController.Loader { get => _loader; set => _loader = value; }
        Constructor IController.Constructor { get => _constructor; set => _constructor = value; }
        bool[] IController.UsedTechs { get => usedtechs; set => usedtechs = value; }

        private bool needRefresh = false;

        private readonly string dirpath = @"E:\SudokuSolver\Sudoku\";

        //cледующий шаг решения
        public void Do(bool[] usedTechs)
        {
            //запоминаю используемые техники для UNDO
            if (usedTechs == null)
            {
                usedTechs = usedtechs;
            }
            else
            {
                usedtechs = usedTechs;
            }

            //если решено то ничего не делаю
            if (Logic.done) return;

            //убираю исключенных кандидатов
            _grid.UpdateGrid(_field);

            //нахожу исключения
            //пишу найденное в консоль
            string answer = Logic.findElimination(_field, usedTechs);
            _solver.PrintToConsole(answer);

            //выделяю найденные исключения
            _grid.HighlighteRemoved(Logic.clues, Logic.removed);

            //сохраняю сделанные изменения
            Buffer.SaveChanges();


            //расчет требуется ли перерисовывать цепи
            if (needRefresh || Logic.chain.Count != 0)
            {
                //если требуется то перерисовываю
                _solver.Refresh();
                if (Logic.chain == null || Logic.chain.Count == 0)
                {
                    needRefresh = false;
                }
                else
                {
                    needRefresh = true;
                }
            }

        }

        public void Undo()
        {
            Logic.done = false;
            //если есть куда откатывать
            if (Buffer.fieldStorage.Count > 3)
            {
                //беру последние изменения
                int i;
                int j;
                int[][] changes = Buffer.GetLastChanges();
                foreach (int[] change in changes)
                {
                    //нахожу нужную ячейку
                    i = change[0] / 9;
                    j = change[0] % 9;
                    if (change[2] == 1)
                    {
                        //возвращаю кандидата
                        _field[i, j].AddCandidat(change[1]);
                    }
                    if (change[2] == -1)
                    {
                        _field[i, j].RemoveValue();
                    }
                }
                changes = Buffer.GetLastChanges();
                foreach (int[] change in changes)
                {
                    //нахожу нужную ячейку
                    i = change[0] / 9;
                    j = change[0] % 9;
                    if (change[2] == 1)
                    {
                        //возвращаю кандидата
                        _field[i, j].AddCandidat(change[1]);
                    }
                    if (change[2] == -1)
                    {
                        _field[i, j].RemoveValue();
                    }
                }


                Do(null);

            }
        }

        public void HighlightDigit(int digit)
        {
            _grid.isHighlighted[digit - 1] = !_grid.isHighlighted[digit - 1];
            _grid.HighlighteGrid();
        }

        public void HighlightLinks(int[] links)
        {
            Logic.CreateChain(_field, links);
            Logic.fillWeakLinks();
            _solver.Refresh();

        }

        public void LoadFrom(string filename)
        {
            if (!filename.Equals("BUFFER"))
            {

                string fullpath = dirpath + filename + ".txt";
                string[] lines;

                if (File.Exists(fullpath))
                {
                    lines = File.ReadAllLines(fullpath);
                }
                else
                {
                    string defaultSudoku = "6 0 0 8 0 0 7 0 9\n0 0 4 0 0 2 0 6 0\n0 0 0 0 3 7 0 0 0\n5 0 0 1 0 0 0 8 0\n0 0 1 0 0 0 6 0 0\n0 8 0 0 0 3 0 0 2\n0 0 0 0 1 0 0 0 0\n0 3 0 7 0 0 1 0 0\n4 0 2 0 0 6 0 0 8";
                    lines = defaultSudoku.Split('\n');

                    MessageBox.Show("Загружено тестовое судоку", "Файл не существует", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Buffer.Init();
                //записал в буфер
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i].Trim();
                    string[] digits = lines[i].Split(' ');

                    for (int j = 0; j < digits.Length; j++)
                    {
                        Buffer.sudoku[i][j] = Convert.ToInt32(digits[j]);
                    }
                }
            }

            //очищаю консоль
            _solver.ClearConsole();

            //обновляю поле
            _field = new Field();
            _field.updateField(Buffer.sudoku);

            Logic.ClearChainBuffer();
            Logic.SimpleRestriction(_field);
            _grid.ReloadGrid();
            _grid.UpdateGrid(_field);
            _solver.Refresh();
            Logic.done = false;
        }

        public List<string> GetListSaved()
        {
            //загружаем все имена
            var filenames = Directory.GetFiles(dirpath, "*", SearchOption.TopDirectoryOnly).ToList();
            //обрезаем пути
            for (int i = 0; i < filenames.Count; i++)
            {
                filenames[i] = Path.GetFileName(filenames[i]);
            }

            return filenames;
        }

        public void Delete(string[] filenames)
        {
            for(int i = 0; i < filenames.Length; i++)
            {
                File.Delete(dirpath + filenames[i]+".txt");
            }
        }

        public void SaveToFile(string filename, string data)
        {
            string fullpath = dirpath + filename + ".txt";

            using (FileStream f = File.Create(fullpath))
            {
                byte[] byteS = Encoding.UTF8.GetBytes(data);
                f.Write(byteS, 0, byteS.Length);
            }
        }

        public void Restart()
        {
            LoadFrom("BUFFER");
        }

        
    }
}
