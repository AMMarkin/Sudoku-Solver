using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace SudokuSolver
{
    public partial class Form1 : Form
    {
        Grid grid;                      //нарисованная сетка
        List<Field> field;              //массив ячеек
        int[][] sudoku;                 //вспомогательный массив для загрузки из файла


        Button DoButton;                //кнопка шага
        Button UndoButton;              //кнопка шага назад
        TextBox console;                //консоль
        GroupBox tecniquesPanel;        //панель включения техник
        CheckBox[] tecniques;           //кнопки включения техник
        GroupBox highlightingPanel;     //панель подсветки
        Button[] highlightingButtons;   //кнопки включения подсветки
        GroupBox linksPanel;            //панель отображения связей
        Button[] linksButtons;          //кнопки включения отображения связей


        MenuStrip menu;

        //Constructor constructor;        //Форма конструктора судоку
        //Loader loader;                  //Форма загрузчика судоку


        bool needRefresh;
        bool[] shownLinks;
        bool[] usedTecniques;

        private Size formSize;

        public Form1()
        {
            InitializeComponent();

            Init();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            //загрузка судоку
            sudoku = new int[9][];
            for (int i = 0; i < sudoku.Length; i++)
            {
                sudoku[i] = new int[9];
            }


            //7 X-Wings & NakedTriples
            //12 скрытые тройки
            //13 Swordfish 
            //13 Y-Wings
            //14 Jellyfish
            //------------------------------------------------------------------------------------------------------------
            LoadSudoku("13");
            //------------------------------------------------------------------------------------------------------------


        }

        private void Init()
        {
            //создание окна
            formSize = new Size(1220, 680);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Location = new Point(10, 10);

            //создание интерфейса сетки
            grid = new Grid(this);

            //создание поля
            field = new List<Field>();

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
            LoadSudokuFromBuffer();
        }

        //----------------------------------------------------------------------------------------------------------------------
        //следующий шаг решения

        private void DoButtonClick(object sender, EventArgs e)
        {

            if (Logic.done) return;


            grid.UpdateGrid(field[field.Count - 1]);

            usedTecniques = new bool[tecniques.Length];

            Field newField = new Field();
            newField.CopyFrom(field[field.Count - 1]);

            field.Add(newField);




            for (int i = 0; i < tecniques.Length; i++)
            {
                usedTecniques[i] = tecniques[i].Checked;
            }

            string answer = Logic.findElimination(field[field.Count - 1], usedTecniques);
            PrintToConsole(answer);





            grid.HighlighteRemoved(Logic.clues, Logic.removed);

            if (needRefresh || (Logic.chain != null && Logic.chain.Count != 0))
            {
                this.Refresh();
                if (Logic.chain == null || (Logic.chain != null && Logic.chain.Count == 0))
                {
                    needRefresh = false;
                }
                else
                {
                    needRefresh = true;
                }
            }


        }

        //нажатие кнопки назад
        private void UndoButtonClick(object sender, EventArgs e)
        {
            //выкидываю последнюю запись
            if (field.Count > 2)
            {
                field.RemoveAt(field.Count - 1);
                field.RemoveAt(field.Count - 1);
                grid.UpdateGrid(field[field.Count - 1]);

                Logic.done = false;

                DoButton.PerformClick();

                PrintToConsole("Шаг назад");
            }
        }

        //----------------------------------------------------------------------------------------------------------------------
        //нажатие кнопки вызова загрузчика
        private void LoaderOpenButton_Click(object sender, EventArgs e)
        {
            _ = new Loader(this);
        }

        //----------------------------------------------------------------------------------------------------------------------
        //нажатие на кнопку вызова конструктора
        private void ConstrOpenClick(object sender, EventArgs e)
        {
            _ = new Constructor(this);
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

            tecniques = new CheckBox[Logic.tecniques.Count];
            int checkBoxSize = 20;
            for (int i = 0; i < tecniques.Length; i++)
            {
                tecniques[i] = new CheckBox()
                {
                    Text = Logic.tecniques[i],
                    Size = new Size(150, checkBoxSize),
                    Location = new Point(20, 25 + i * (checkBoxSize + 5))
                };
                tecniques[i].Font = new Font(tecniques[i].Font.Name, tecniques[i].Font.Size, FontStyle.Underline);
                tecniques[i].Checked = true;
                tecniquesPanel.Controls.Add(tecniques[i]);

                Logic.tech.Add(Logic.tecniques[i], i);
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
            shownLinks = new bool[9];

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
            Button b = sender as Button;
            int digit = Convert.ToInt32(b.Text);

            shownLinks[digit - 1] = !shownLinks[digit - 1];
            int counter = 0;
            for (int i = 0; i < shownLinks.Length; i++)
            {
                if (shownLinks[i])
                {
                    counter++;
                }
            }
            int[] links = new int[counter];
            counter = 0;
            for (int i = 0; i < shownLinks.Length; i++)
            {
                if (shownLinks[i])
                {
                    links[counter] = i;
                    counter++;
                }
            }

            Logic.CreateChain(field[field.Count - 1], links);
            Logic.fillWeakLinks();
            this.Refresh();

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
            Button b = sender as Button;
            int digit = Convert.ToInt32(b.Text);
            grid.isHighlighted[digit - 1] = !grid.isHighlighted[digit - 1];
            grid.HighlighteGrid();
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
        void ClearConsole()
        {
            console.Text = "";
            linesCount = 0;
        }

        //написать в консоль
        int linesCount = 0;
        public void PrintToConsole(string s)
        {
            console.Text = s + Environment.NewLine + console.Text;

            //считаю строки
            linesCount++;


            //если строк больше допустимого, обрезаю 
            int maxLineCount = 60;
            if (linesCount > maxLineCount)
            {
                var lines = console.Text.Split('\n');

                lines[linesCount - 1] = "";
                linesCount--;
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string line in lines)
                {
                    stringBuilder.Append(line);
                    if (line.Length > 0)
                        stringBuilder.Append(Environment.NewLine);
                }
                console.Text = stringBuilder.ToString();
            }

        }

        //----------------------------------------------------------------------------------------------------------------------
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            grid.g = e.Graphics;
            grid.DrawLines();
        }
        //----------------------------------------------------------------------------------------------------------------------
        //загрузка судоку
        public void LoadSudoku(string a)
        {
            string path = @"E:\SudokuSolver\Sudoku\";
            path += a + ".txt";
            string[] lines;

            if (File.Exists(path))
            {
                lines = File.ReadAllLines(path);
            }
            else
            {
                string defaultSudoku = "6 0 0 8 0 0 7 0 9\n0 0 4 0 0 2 0 6 0\n0 0 0 0 3 7 0 0 0\n5 0 0 1 0 0 0 8 0\n0 0 1 0 0 0 6 0 0\n0 8 0 0 0 3 0 0 2\n0 0 0 0 1 0 0 0 0\n0 3 0 7 0 0 1 0 0\n4 0 2 0 0 6 0 0 8";
                lines = defaultSudoku.Split('\n');

                MessageBox.Show("Загружено тестовое судоку", "Файл не существует", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //очищаю консоль
            if (console != null)
            {
                ClearConsole();
            }

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].Trim();
                string[] digits = lines[i].Split(' ');

                for (int j = 0; j < digits.Length; j++)
                {
                    sudoku[i][j] = Convert.ToInt32(digits[j]);
                }
            }

            //запоминаю в буфере для рестарта
            Buffer.sudoku = new int[9][];
            for (int i = 0; i < 9; i++)
            {
                Buffer.sudoku[i] = new int[9];
                for (int j = 0; j < 9; j++)
                {
                    Buffer.sudoku[i][j] = sudoku[i][j];
                }
            }

            //обновляю поле
            field.Clear();
            field.Add(new Field());
            field[field.Count - 1].updateField(sudoku);
            Logic.ClearChainBuffer();
            Logic.SimpleRestriction(field[field.Count - 1]);
            grid.ReloadGrid();
            grid.UpdateGrid(field[field.Count - 1]);
            this.Refresh();
            needRefresh = false;
            Logic.done = false;
        }

        public void LoadSudokuFromBuffer()
        {
            //переписываю судоку

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    sudoku[i][j] = Buffer.sudoku[i][j];
                }
            }

            //очищаю консоль
            ClearConsole();

            //обновляю поле
            field.Clear();
            field.Add(new Field());
            field[field.Count - 1].updateField(sudoku);
            Logic.ClearChainBuffer();
            Logic.SimpleRestriction(field[field.Count - 1]);
            grid.ReloadGrid();
            grid.UpdateGrid(field[field.Count - 1]);
            this.Refresh();
            needRefresh = false;
            Logic.done = false;

        }
    }
}
