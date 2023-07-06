using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        Constructor constructor;        //Форма конструктора судоку
        Loader loader;                  //Форма загрузчика судоку


        bool needRefresh;
        bool[] shownLinks;
        bool[] usedTecniques;

        List<string> consoleLogs;



        public Form1()
        {
            InitializeComponent();


            //создание окна
            this.Size = new Size(1220, 680);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Location = new Point(10, 10);

            //this.DoubleBuffered = true;

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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            //загрузка судоку
            sudoku = new int[9][];
            for (int i = 0; i < sudoku.Length; i++)
            {
                sudoku[i] = new int[9];
            }

            needRefresh = false;

            //7 X-Wings & NakedTriples
            //12 скрытые тройки
            //13 Swordfish 
            //13 Y-Wings
            //14 Jellyfish
            //------------------------------------------------------------------------------------------------------------
            loadSudoku("13");
            //------------------------------------------------------------------------------------------------------------

            

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

            make.Click += constrOpenClick;

            menu.Items.Add(make);



            sud.DropDownItems.Add(make);

            ToolStripMenuItem load = new ToolStripMenuItem()
            {
                Text = "Открыть"
            };

            load.Click += loaderOpenButton_Click;
            sud.DropDownItems.Add(load);


            ToolStripMenuItem restart = new ToolStripMenuItem()
            {
                Text = "Заново"
            };
            restart.Click += restartButton_Click;
            sud.DropDownItems.Add(restart);

            this.MainMenuStrip = menu;
            this.Controls.Add(menu);
        }

        //рестарт
        private void restartButton_Click(object sender, EventArgs e)
        {
            loadSudokuFromBuffer();
        }

        //----------------------------------------------------------------------------------------------------------------------
        //следующий шаг решения

        private void DoButtonClick(object sender, EventArgs e)
        {

            if (Logic.done) return;


            grid.updateGrid(field[field.Count - 1]);

            usedTecniques = new bool[tecniques.Length];

            Field newField = new Field();
            newField.CopyFrom(field[field.Count - 1]);

            field.Add(newField);


            

            for (int i = 0; i < tecniques.Length; i++)
            {
                usedTecniques[i] = tecniques[i].Checked;
            }

            string answer = Logic.findElimination(field[field.Count - 1], usedTecniques);
            printToConsole(answer);





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
                grid.updateGrid(field[field.Count - 1]);

                Logic.done = false;

                DoButton.PerformClick();

                printToConsole("Шаг назад");
            }
        }

        //----------------------------------------------------------------------------------------------------------------------
        //нажатие кнопки вызова загрузчика
        private void loaderOpenButton_Click(object sender, EventArgs e)
        {
            loader = new Loader(this);
        }

        //----------------------------------------------------------------------------------------------------------------------
        //нажатие на кнопку вызова конструктора
        private void constrOpenClick(object sender, EventArgs e)
        {
            constructor = new Constructor(this);
        }

        //----------------------------------------------------------------------------------------------------------------------
        //создание панели техник
        private void CreateTecniquesPanel()
        {
            tecniquesPanel = new GroupBox();
            tecniquesPanel.Location = new Point(console.Location.X + console.Width + 20, console.Location.Y);
            tecniquesPanel.Visible = true;
            tecniquesPanel.BackColor = Color.FromArgb(173, 216, 230);

            tecniquesPanel.Text = "Техники";
            tecniquesPanel.Font = new Font(tecniquesPanel.Font.Name, 10);
            this.Controls.Add(tecniquesPanel);

            tecniques = new CheckBox[Logic.tecniques.Count];
            int checkBoxSize = 20;
            for (int i = 0; i < tecniques.Length; i++)
            {
                tecniques[i] = new CheckBox();
                tecniques[i].Text = Logic.tecniques[i];
                tecniques[i].Size = new Size(150, checkBoxSize);
                tecniques[i].Checked = true;
                tecniques[i].Font = new Font(tecniques[i].Font.Name, tecniques[i].Font.Size, FontStyle.Underline);
                tecniques[i].Location = new Point(20, 25 + i * (checkBoxSize + 5));
                tecniquesPanel.Controls.Add(tecniques[i]);

                Logic.tech.Add(Logic.tecniques[i], i);
            }

            for (int i = 0; i < tecniques.Length; i++)
            {
                if (tecniques[i].Text.Equals("X-Cycles"))
                {
                    tecniques[i].Checked = true;
                }
            }

            //----------------------------------------------------------------------------------------------------------------------
            //tecniques[tecniques.Length - 1].Checked = true;
            //----------------------------------------------------------------------------------------------------------------------




            tecniquesPanel.Size = new Size(tecniques[0].Width + 30, (checkBoxSize + 5) * tecniques.Length + 20);

            //создание кнопки вкл-выкл всех техник
            CheckBox all = new CheckBox();
            all.Size = new Size(15, 15);
            all.Location = new Point(tecniquesPanel.Size.Width - all.Width - 20, all.Size.Height / 2 + 2); ;
            all.Checked = true;
            all.Click += allCheck;
            tecniquesPanel.Controls.Add(all);

        }

        //вкл-выкл всего списка техник
        private void allCheck(object sender, EventArgs e)
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

            linksPanel = new GroupBox();
            linksPanel.Location = new Point(highlightingPanel.Location.X + highlightingPanel.Width + 20, highlightingPanel.Location.Y);
            linksPanel.Text = "Связи";
            linksPanel.Visible = true;

            linksButtons = new Button[9];
            for (int i = 0; i < linksButtons.Length; i++)
            {
                linksButtons[i] = new Button();
                linksButtons[i].Text = (i + 1).ToString();
                linksButtons[i].Size = new Size(25, 20);
                linksButtons[i].Location = new Point(10 + (i % 3) * (linksButtons[i].Size.Width + 5), 15 + (i / 3) * (linksButtons[i].Size.Height + 5));
                linksButtons[i].Visible = true;
                linksButtons[i].Click += linksButtonClick;

                linksPanel.Controls.Add(linksButtons[i]);
            }

            linksPanel.Size = new Size(10 * 2 + 3 * (linksButtons[0].Width + 5) - 5, 10 * 2 + 3 * (linksButtons[0].Height + 5));
            this.Controls.Add(linksPanel);
        }

        //включение связей
        private void linksButtonClick(object sender, EventArgs e)
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
            highlightingPanel = new GroupBox();
            highlightingPanel.Location = new Point(DoButton.Location.X + DoButton.Width + 20, DoButton.Location.Y);
            highlightingPanel.Text = "Подсветка";
            highlightingPanel.Visible = true;

            highlightingButtons = new Button[9];
            for (int i = 0; i < highlightingButtons.Length; i++)
            {
                highlightingButtons[i] = new Button();
                highlightingButtons[i].Text = (i + 1).ToString();
                highlightingButtons[i].Size = new Size(25, 20);
                highlightingButtons[i].Location = new Point(10 + (i % 3) * (highlightingButtons[i].Size.Width + 5), 15 + (i / 3) * (highlightingButtons[i].Size.Height + 5));
                highlightingButtons[i].Visible = true;
                highlightingButtons[i].Click += highlightButtonClick;

                highlightingPanel.Controls.Add(highlightingButtons[i]);
            }

            highlightingPanel.Size = new Size(10 * 2 + 3 * (highlightingButtons[0].Width + 5) - 5, 10 * 2 + 3 * (highlightingButtons[0].Height + 5));
            this.Controls.Add(highlightingPanel);
        }

        //включение подсветки
        private void highlightButtonClick(object sender, EventArgs e)
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

            UndoButton = new Button();
            UndoButton.Size = new Size(30, 30);
            UndoButton.Location = new Point(console.Location.X, console.Location.Y + console.Size.Height + 30);
            UndoButton.Text = "";
            UndoButton.Visible = true;
            UndoButton.Click += UndoButtonClick;

            Image UndoButtonIcon = Image.FromFile(iconsDir + @"\DoButton.jpg");
            UndoButtonIcon.RotateFlip(RotateFlipType.RotateNoneFlipX);
            UndoButton.BackgroundImage = UndoButtonIcon;
            UndoButton.BackgroundImageLayout = ImageLayout.Stretch;

            this.Controls.Add(UndoButton);

            DoButton = new Button();
            DoButton.Size = new Size(30, 30);
            DoButton.Location = new Point(UndoButton.Location.X + UndoButton.Width + 10, UndoButton.Location.Y);
            DoButton.Text = "";
            DoButton.Visible = true;
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
            console = new TextBox();
            console.Visible = true;
            console.Size = new Size(320, 400);
            console.Location = new Point(grid.startX + grid.sizeGrid + 30, grid.startY);
            console.BorderStyle = BorderStyle.FixedSingle;
            console.BackColor = Color.FromArgb(255, 228, 181);
            console.Text = "";

            console.WordWrap = true;
            console.Multiline = true;
            console.ScrollBars = ScrollBars.Vertical;
            console.ReadOnly = true;

            console.Cursor = Cursors.Default;

            //отключаю возможность получения фокуса
            console.GotFocus += delegate (object sender, EventArgs e) { this.ActiveControl = null; };

            this.Controls.Add(console);


            consoleLogs = new List<string>();
        }

        //очистить консоль
        void clearConsole()
        {
            console.Text = "";
            consoleLogs.Clear();
        }

        //написать в консоль
        void printToConsole(string s)
        {
            //consoleLogs.Add(s);
            console.Text = s + Environment.NewLine + console.Text;
            if (console.Text.Length > 2000)
            {
                console.Text = console.Text.Remove(1000, console.Text.Length - 1000);
            }

        }

        //удалить из консоли
        void removeLastLogFromConsole()
        {
            consoleLogs.RemoveAt(consoleLogs.Count - 1);
        }

        //----------------------------------------------------------------------------------------------------------------------

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            grid.g = e.Graphics;
            grid.drawLines();
        }

        //----------------------------------------------------------------------------------------------------------------------
        //загрузка судоку
        public void loadSudoku(string a)
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
                clearConsole();
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

            Logic.SimpleRestriction(field[field.Count - 1]);
            grid.reloadGrid();
            grid.updateGrid(field[field.Count - 1]);

        }

        public void loadSudokuFromBuffer()
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
            clearConsole();

            //обновляю поле
            field.Clear();
            field.Add(new Field());
            field[field.Count - 1].updateField(sudoku);

            Logic.SimpleRestriction(field[field.Count - 1]);
            grid.reloadGrid();
            grid.updateGrid(field[field.Count - 1]);

        }

    }
}
