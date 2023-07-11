using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SolverLibrary.Interfaces;
using SolverLibrary.model;

namespace SudokuSolver.controller
{
    internal class WFController : IController
    {

        //ссылки на компоненты
        private Field _field;
        private Grid _grid;                             //поле (VIEW)
        private Solver _solver;                         //форма солвера
        private Form _constructor;               //форма конструктора
        private Form _loader;                         //форма загрузчика

        Field IController.Field { get => _field; set => _field = value; }
        public IGridView Grid { get => _grid; set => _grid = value as Grid; }
        public ISolverView Solver { get => _solver; set => _solver = value as Solver; }
        public ILoader Loader { get => (ILoader)_loader; set => _loader = value as Form; }
        public IConstructor Constructor { get => (IConstructor)_constructor; set => _constructor = value as Form; }


        //массив используемых в решении техник
        private bool[] usedtechs;
        bool[] IController.UsedTechs { get => usedtechs; set => usedtechs = value; }
        
        private readonly bool[] shownLinks;

        private bool needRefresh = false;
        private bool done;
        private readonly string dirpath = @"E:\SudokuSolver\Sudoku\";

        public WFController()
        {
            shownLinks = new bool[9];
        }

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
            if (done) return;

            //применяю записанные на прошлом шаге изменения
            _field.ApplyChanges();

            //нахожу новые исключения
            //пишу найденное в консоль

            if (Logic.SimpleRestriction(_field))
            {
                _field.ApplyChanges();
            }

            string answer = Logic.FindElimination(_field, usedTechs);
            _solver.PrintToConsole(answer);
            
            if(answer.Equals(Logic.done)) done = true;

            //убираю исключенных кандидатов
            _grid.UpdateGrid(_field);
            //выделяю найденные исключения
            _grid.HighlighteRemoved(_field.Buffer.clues, _field.Buffer.removed, _field.Buffer.ON, _field.Buffer.OFF);

            //сохраняю сделанные изменения
            _field.Buffer.SaveChanges();


            //если есть включенные связи то пересчитываю и вывожу
            if (_field.Buffer.chain.Count == 0 && shownLinks.FirstOrDefault(x=>x==true))
            {
                int[] links = shownLinks.Select((x, idx) => new { value = x, id = idx }).Where(x => x.value).Select(x => x.id).ToArray();

                Logic.CreateChain(_field, links);
                Logic.FillWeakLinks(_field);
            }


            //расчет требуется ли перерисовывать цепи
            if (needRefresh || _field.Buffer.chain.Count != 0)
            {
                //если требуется то перерисовываю
                _solver.Refresh();
                if (_field.Buffer.chain == null || _field.Buffer.chain.Count == 0)
                {
                    needRefresh = false;
                }
                else
                {
                    needRefresh = true;
                }
            }

        }

        //предыдущий шаг решения
        public void Undo()
        {
            done = false;
            //если есть куда откатывать
            if (_field.Buffer.fieldStorage.Count > 3)
            {
                //беру последние изменения
                int i;
                int j;
                int[][] changes = _field.Buffer.GetLastChanges();
                _field.Buffer.RemoveLastChanges();
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
                changes = _field.Buffer.GetLastChanges();
                _field.Buffer.RemoveLastChanges();
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

        //подсветка числа
        public void HighlightDigit(int digit)
        {
            _grid.isHighlighted[digit - 1] = !_grid.isHighlighted[digit - 1];
            _grid.HighlighteGrid();
        }

        //подсветка связей
        public void HighlightLinks(int digit)
        {
            //меняю флаг
            shownLinks[digit - 1] = !shownLinks[digit - 1];

            //массив связей по колличеству включенных
            int[] links = shownLinks.Select((x, idx) => new { value = x, id = idx}).Where(x=>x.value).Select(x=>x.id).ToArray();

            Logic.CreateChain(_field, links);
            Logic.FillWeakLinks(_field);
            _solver.Refresh();

        }

        //загрузка головоломки
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
                //создаю если не создано
                _field = _field ?? new Field();
                _field.Buffer.Init();
                //записал в буфер

                for(int i=0;i<lines.Length; i++)
                {
                    _field.Buffer.sudoku[i] = lines[i].Where(c => Char.IsDigit(c)).Select(c => c-48).ToArray();

                }
            }

            //очищаю консоль
            _solver.ClearConsole();

            //очищаю поле
            _field?.ResetField();
            //создаю если не создано
            _field = _field ?? new Field();
            //заполняю значениями
            _field.UpdateField();

            _grid.Controller = this;
            

            //очищаю буфферы цепей
            _field.Buffer.ClearChainBuffer();
            //очищаю буфферы изменений
            _field.Buffer.InitChangeStorage();
            //провожу простые ислкючения
            Logic.SimpleRestriction(_field);
            //сохраняю проведенные исключения
            _field.Buffer.SaveChanges();
            //применяю
            _field.ApplyChanges();
            //обновляю поле
            _grid.UpdateGrid(_field);
            //перерисовываю форму
            _solver.Refresh();
            //ставлю флаг что не решено
            done = false;
        }

        //получить список всех сохраненных головоломок
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

        //удалить сохраненные головоломки
        public void Delete(string[] filenames)
        {
            for(int i = 0; i < filenames.Length; i++)
            {
                File.Delete(dirpath + filenames[i]+".txt");
            }
        }

        //сохранить головоломку
        public void SaveToFile(string filename, string data)
        {
            string fullpath = dirpath + filename + ".txt";

            using (FileStream f = File.Create(fullpath))
            {
                byte[] byteS = Encoding.UTF8.GetBytes(data);
                f.Write(byteS, 0, byteS.Length);
            }
        }

        //рестарт
        public void Restart()
        {
            LoadFrom("BUFFER");
        }

        
    }
}
