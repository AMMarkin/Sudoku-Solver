using System;
using System.Drawing;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using SudokuSolver.controller;
using SolverLibrary.model;
using SolverLibrary.Interfaces;

namespace SudokuSolver
{
    public partial class Solver : Form, ISolverView
    {
        private IController _controller;
        IController ISolverView.Controller { get => _controller; set => _controller = value; }


        private Grid grid;                      //нарисованная сетка
        
        private Button DoButton;                //кнопка шага
        private Button UndoButton;              //кнопка шага назад
        private TextBox console;                //консоль
        private GroupBox tecniquesPanel;        //панель включения техник
        private CheckBox[] tecniques;           //кнопки включения техник
        private GroupBox highlightingPanel;     //панель подсветки
        private Button[] highlightingButtons;   //кнопки включения подсветки
        private GroupBox linksPanel;            //панель отображения связей
        private Button[] linksButtons;          //кнопки включения отображения связей
        private MenuStrip menu;                 //меню

        private Size formSize;                  //размер формы
        

        private readonly string[] tech_names;   //список названия техник
        private int linesCount = 0;
        private Queue<string> output_lines = new Queue<string>();
        private Queue<int> lines_in_output = new Queue<int>();

        //передаю ссылку на контроллер и 
        public Solver(IController controller, string[] tech_names)
        {
            this._controller = controller;
            this.tech_names = tech_names;
            
            InitializeComponent();

            Init();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //загрузка судоку
            
            //7 X-Wings & NakedTriples
            //12 скрытые тройки
            //13 Swordfish 
            //13 Y-Wings
            //14 Jellyfish
            //------------------------------------------------------------------------------------------------------------
            _controller.LoadFrom("13");
            //------------------------------------------------------------------------------------------------------------

        }

        private void Init()
        {
            //создание окна
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            
            //создание интерфейса сетки
            grid = new Grid(this);
            

            //добавляю ссылки в контроллер
            _controller.Grid = grid;
            _controller.Solver = this;


            //создание консоли
            CreateConsole();

            //настройка кнопок шага
            CreateButtons();

            //панель подсветки
            CreateHighlightingPanel();

            //панель связей
            CreateLinksPanel();

            //панель выбора техник
            CreateTecniquesPanel();

            //создание панели меню
            CreateMenu();

            formSize.Width = tecniquesPanel.Location.X + tecniquesPanel.Width + 40;
            formSize.Height = grid.startY + grid.sizeGrid + 60;

            this.Size = formSize;
        }


        //создание меню
        private void CreateMenu()
        {
            //само меню
            menu = new MenuStrip();

            ToolStripMenuItem sud = new ToolStripMenuItem()
            {
                Text = "Судоку"
            };

            menu.Items.Add(sud);



            ToolStripMenuItem make = new ToolStripMenuItem()
            {
                Text = "Создать"
            };

            make.Click += ConstrOpenClick;

            menu.Items.Add(make);



            sud.DropDownItems.Add(make);

            ToolStripMenuItem load = new ToolStripMenuItem()
            {
                Text = "Открыть"
            };

            load.Click += LoaderOpenButton_Click;
            sud.DropDownItems.Add(load);


            ToolStripMenuItem restart = new ToolStripMenuItem()
            {
                Text = "Заново"
            };
            restart.Click += RestartButton_Click;
            sud.DropDownItems.Add(restart);

            this.MainMenuStrip = menu;
            this.Controls.Add(menu);
        }

        //рестарт
        private void RestartButton_Click(object sender, EventArgs e)
        {
            _controller.Restart();
        }
        //----------------------------------------------------------------------------------------------------------------------
        //следующий шаг решения

        private void DoButtonClick(object sender, EventArgs e)
        {

            //записываю выбранные техники
            bool[] flags = new bool[tecniques.Length];
            for(int i = 0; i < tecniques.Length; i++)
            {
                flags[i] = tecniques[i].Checked;
            }

            //отправляю 
            _controller.Do(flags);
        }

        //нажатие кнопки назад
        private void UndoButtonClick(object sender, EventArgs e)
        {
            _controller.Undo();
        }

        //----------------------------------------------------------------------------------------------------------------------
        //нажатие кнопки вызова загрузчика
        private void LoaderOpenButton_Click(object sender, EventArgs e)
        {
            _ = new Loader(_controller);
        }

        //----------------------------------------------------------------------------------------------------------------------
        //нажатие на кнопку вызова конструктора
        private void ConstrOpenClick(object sender, EventArgs e)
        {
            _ = new Constructor(this, _controller);
        }

        //----------------------------------------------------------------------------------------------------------------------
        //создание панели техник
        private void CreateTecniquesPanel()
        {
            tecniquesPanel = new GroupBox()
            {
                Location = new Point(console.Location.X + console.Width + 20, console.Location.Y),
                Visible = true,
                BackColor = Color.FromArgb(173, 216, 230),
                Text = "Техники",
                Font = new Font(this.Font.Name, 10)
            };
            this.Controls.Add(tecniquesPanel);

            tecniques = new CheckBox[tech_names.Length];
            int checkBoxSize = 20;
            for (int i = 0; i < tecniques.Length; i++)
            {
                tecniques[i] = new CheckBox()
                {
                    Text = tech_names[i],
                    Size = new Size(150, checkBoxSize),
                    Location = new Point(20, 25 + i * (checkBoxSize + 5))
                };
                tecniques[i].Font = new Font(tecniques[i].Font.Name, tecniques[i].Font.Size, FontStyle.Underline);
                tecniques[i].Checked = true;
                tecniquesPanel.Controls.Add(tecniques[i]);
            }

            //проверка последней техники
            //----------------------------------------------------------------------------------------------------------------------
            //tecniques[tecniques.Length - 1].Checked = true;
            //----------------------------------------------------------------------------------------------------------------------


            tecniquesPanel.Size = new Size(tecniques[0].Width + 30, (checkBoxSize + 5) * tecniques.Length + 20);

            //создание кнопки вкл-выкл всех техник
            CheckBox all = new CheckBox()
            {
                Size = new Size(15, 15),
                Checked = tecniques[0].Checked
            };
            all.Location = new Point(tecniquesPanel.Size.Width - all.Width - 20, all.Size.Height / 2 + 2);
            all.Click += AllCheck;
            tecniquesPanel.Controls.Add(all);

        }

        //вкл-выкл всего списка техник
        private void AllCheck(object sender, EventArgs e)
        {
            CheckBox all = sender as CheckBox;
            for (int i = 0; i < tecniques.Length; i++)
            {
                tecniques[i].Checked = all.Checked;
            }
        }

        //----------------------------------------------------------------------------------------------------------------------
        //создание панели связей
        private void CreateLinksPanel()
        {
            linksPanel = new GroupBox()
            {
                Location = new Point(highlightingPanel.Location.X + highlightingPanel.Width + 20, highlightingPanel.Location.Y),
                Text = "Связи",
                Visible = true
            };

            linksButtons = new Button[9];
            for (int i = 0; i < linksButtons.Length; i++)
            {
                linksButtons[i] = new Button()
                {
                    Text = (i + 1).ToString(),
                    Size = new Size(25, 20),
                    Visible = true
                };
                linksButtons[i].Location = new Point(10 + (i % 3) * (linksButtons[i].Size.Width + 5), 15 + (i / 3) * (linksButtons[i].Size.Height + 5));
                linksButtons[i].Click += LinksButtonClick;

                linksPanel.Controls.Add(linksButtons[i]);
            }

            linksPanel.Size = new Size(10 * 2 + 3 * (linksButtons[0].Width + 5) - 5, 10 * 2 + 3 * (linksButtons[0].Height + 5));
            this.Controls.Add(linksPanel);
        }

        //включение связей
        private void LinksButtonClick(object sender, EventArgs e)
        {
            //нахожу нажатое число
            Button b = sender as Button;
            int digit = Convert.ToInt32(b.Text);

            
            //передаю в контроллер
            _controller.HighlightLinks(digit);
        }

        //----------------------------------------------------------------------------------------------------------------------
        //создание панели подстветки
        private void CreateHighlightingPanel()
        {
            highlightingPanel = new GroupBox()
            {
                Location = new Point(DoButton.Location.X + DoButton.Width + 20, DoButton.Location.Y),
                Text = "Подсветка",
                Visible = true
            };

            highlightingButtons = new Button[9];
            for (int i = 0; i < highlightingButtons.Length; i++)
            {
                highlightingButtons[i] = new Button()
                {
                    Text = (i + 1).ToString(),
                    Size = new Size(25, 20),
                    Visible = true
                };
                highlightingButtons[i].Location = new Point(10 + (i % 3) * (highlightingButtons[i].Size.Width + 5), 15 + (i / 3) * (highlightingButtons[i].Size.Height + 5));
                highlightingButtons[i].Click += HighlightButtonClick;

                highlightingPanel.Controls.Add(highlightingButtons[i]);
            }

            highlightingPanel.Size = new Size(10 * 2 + 3 * (highlightingButtons[0].Width + 5) - 5, 10 * 2 + 3 * (highlightingButtons[0].Height + 5));
            this.Controls.Add(highlightingPanel);
        }

        //включение подсветки
        private void HighlightButtonClick(object sender, EventArgs e)
        {
            //нашел выбранное число
            Button b = sender as Button;
            int digit = Convert.ToInt32(b.Text);
            //подсветил
            _controller.HighlightDigit(digit);
        }

        //----------------------------------------------------------------------------------------------------------------------
        //создание кнопки следующего шага
        private void CreateButtons()
        {
            string iconsDir = @"E:\SudokuSolver\SudokuSolver\Icons";

            UndoButton = new Button()
            {
                Size = new Size(30, 30),
                Location = new Point(console.Location.X, console.Location.Y + console.Size.Height + 30),
                Text = "",
                Visible = true
            };
            UndoButton.Click += UndoButtonClick;

            Image UndoButtonIcon = Image.FromFile(iconsDir + @"\DoButton.jpg");
            UndoButtonIcon.RotateFlip(RotateFlipType.RotateNoneFlipX);
            UndoButton.BackgroundImage = UndoButtonIcon;
            UndoButton.BackgroundImageLayout = ImageLayout.Stretch;

            this.Controls.Add(UndoButton);

            DoButton = new Button()
            {
                Size = new Size(30, 30),
                Location = new Point(UndoButton.Location.X + UndoButton.Width + 10, UndoButton.Location.Y),
                Text = "",
                Visible = true
            };
            DoButton.Click += DoButtonClick;

            Image DoButtonIcon = Image.FromFile(iconsDir + @"\DoButton.jpg");

            DoButton.BackgroundImage = DoButtonIcon;
            DoButton.BackgroundImageLayout = ImageLayout.Stretch;


            this.AcceptButton = DoButton;
            this.Controls.Add(DoButton);
            DoButton.Focus();
        }

        //----------------------------------------------------------------------------------------------------------------------
        //создание консоли
        private void CreateConsole()
        {
            console = new TextBox()
            {
                Visible = true,
                Size = new Size(320, 400),
                Location = new Point(grid.startX + grid.sizeGrid + 30, grid.startY),
                BackColor = Color.FromArgb(255, 228, 181),
                Text = "",

                WordWrap = true,
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical,
                Cursor = Cursors.Default
            };

            //отключаю возможность получения фокуса
            console.GotFocus += delegate (object sender, EventArgs e)
            {
                this.ActiveControl = null;
            };

            this.Controls.Add(console);
        }

        //очистить консоль
        public void ClearConsole()
        {
            console.Text = "";
            linesCount = 0;
            output_lines.Clear();
            lines_in_output.Clear();
        }

        //написать в консоль
        public void PrintToConsole(string output)
        {

            //считаю строки в поступившем сообщении
            int line_counter =output.Split('\n').Length;

            //добавляю в очередь сообщение
            output_lines.Enqueue(output);
            //запоминаю сколько в нем строк
            lines_in_output.Enqueue(line_counter);
            //считаю общее колличество строк
            linesCount+=line_counter;

            //максимальное колличество строк
            int maxLineCount = 60;

            //если строк больше допустимого, обрезаю 
            if (linesCount > maxLineCount)
            {
                //пока не влезает
                while(linesCount > maxLineCount)
                {
                    //удаляю то последний
                    output_lines.Dequeue();
                    //считаю сколько строк осталось
                    linesCount -= lines_in_output.Dequeue();
                }
            }

            //собираю общий вывод
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string line in output_lines)
            {
                //переписываю очередь
                //так как вывод сверху вниз то вставляю сообщения в начало строки
                stringBuilder.Insert(0, Environment.NewLine);
                stringBuilder.Insert(0, line);
            }
            //вывожу ответ
            console.Text = stringBuilder.ToString();
        }

        //----------------------------------------------------------------------------------------------------------------------
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            grid.g = e.Graphics;
        }
        //----------------------------------------------------------------------------------------------------------------------
    }
}
