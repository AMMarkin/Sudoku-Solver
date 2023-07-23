using SolverLibrary.Interfaces;
using SolverLibrary.model;
using SolverLibrary.model.field;
using SolverLibrary.model.Utilits;
using SolverLibrary.storage;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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
        private readonly Logic _logic;
        private readonly Storage storage;


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


        public WFController(Logic logic)
        {
            shownLinks = new bool[9];
            _logic = logic;

            storage = new FileSystemStorage();
        }

        //cледующий шаг решения
        public void Do(bool[] usedTechs)
        {
            //запоминаю используемые техники для UNDO
            usedtechs = usedTechs;

            //если решено то ничего не делаю
            if (done) return;

            //применяю записанные на прошлом шаге изменения
            _field.ApplyChanges();


            AnswerOfTech answer;

            _field.Buffer.ClearChainBuffer();

            answer = _logic.FindElimination(_field, usedTechs);
            _field.ApplyAnswer(answer);


            _solver.PrintToConsole(answer.Message);

            if (answer.Message.Equals(Logic.done)) done = true;

            //убираю исключенных кандидатов
            _grid.UpdateGrid(_field);
            //выделяю найденные исключения
            _grid.HighlighteRemoved(_field.Buffer.Clues, _field.Buffer.Removed, _field.Buffer.ON, _field.Buffer.OFF);

            //сохраняю сделанные изменения
            _field.Buffer.SaveChanges();


            //если есть включенные связи то пересчитываю и вывожу
            if (_field.Buffer.chain.Count == 0 && shownLinks.FirstOrDefault(x => x == true))
            {
                int[] links = shownLinks.Select((x, idx) => new { value = x, id = idx }).Where(x => x.value).Select(x => x.id).ToArray();


                new ChainBuilder().CreateChain(_field, links).FillWeakLinks(_field);
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
            //если есть куда откатывать
            if (_field.Buffer.fieldStorage.Count > 3)
            {
                done = false;
                _field.CancelChanges();
                _field.CancelChanges();
                _grid.UpdateGrid(_field);
                Do(usedtechs);
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
            int[] links = shownLinks.Select((x, idx) => new { value = x, id = idx }).Where(x => x.value).Select(x => x.id).ToArray();

            (new ChainBuilder()).CreateChain(_field, links);
            (new ChainBuilder()).FillWeakLinks(_field);
            _solver.Refresh();

        }

        //загрузка головоломки
        public void LoadFrom(string puzzlename)
        {
            if (!puzzlename.Equals("BUFFER"))
            {
                _field = _field ?? new Field();
                _field.Buffer.Init();

                StorageOperationResult result;
                (_field.Buffer.sudoku, result) = storage.Load(puzzlename);

                if (result != StorageOperationResult.Success)
                {
                    MessageBox.Show("Загружено тестовое судоку", "Файл не существует", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            _solver.ClearConsole();
            _field?.ResetField();
            _field = _field ?? new Field();
            _field.UpdateField();

            _grid.Controller = this;


            _field.Buffer.ClearChainBuffer();
            _field.Buffer.InitChangeStorage();

            _field.ApplyAnswer(_logic.SimpleRestriction(_field));
            _field.Buffer.SaveChanges();
            _field.ApplyChanges();

            _grid.UpdateGrid(_field);

            _solver.Refresh();

            done = false;

        }

        //получить список всех сохраненных головоломок
        public List<string> GetListSaved()
        {
            return storage.GetListOfPuzzles();
        }

        //удалить сохраненные головоломки
        public void Delete(string[] puzzlenames)
        {
            if (storage.Delete(puzzlenames) == StorageOperationResult.Failed)
            {
                MessageBox.Show("Удаление прошло с ошибкой", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //сохранить головоломку
        public void SaveToFile(string name, int[][] puzzle)
        {
            storage.Save(puzzle, name);
        }

        //рестарт
        public void Restart()
        {
            LoadFrom("BUFFER");
        }

    }
}
