﻿using System;
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
        Field field;                    //массив ячеек
        int[][] sudoku;                 //вспомогательный массив для загрузки из файла
              

        Button stepButton;              //кнопка шага
        Label console;                  //консоль
        GroupBox tecniquesPanel;        //панель включения техник
        CheckBox[] tecniques;           //кнопки включения техник
        GroupBox highlightingPanel;     //панель подсветки
        Button[] highlightingButtons;   //кнопки включения подсветки

        
        MenuStrip menu;

        //перенести в верхнюю панель
        //сделать выбор файла для загрузки судоку

        Constructor constructor;        //Форма конструктора судоку
        Loader loader;                  //Форма загрузчика судоку

        bool needRefresh;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //создание окна
            this.Size = new Size(1220,680);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Location = new Point(10, 10);
            
            //this.DoubleBuffered = true;

            //создание интерфейса сетки
            grid = new Grid(this);

            //создание поля
            field = new Field();

            //загрузка судоку
            sudoku = new int[9][];
            for(int i = 0; i < sudoku.Length; i++)
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


            //загрузка судоку в поле
            //удаление лишних очевидных кандидатов
            //отрисовка сетки
            field.updateField(sudoku);
            Logic.SimpleRestriction(ref field);
            grid.updateGrid(ref field);


            //создание консоли
            CreateConsole();

            //настройка кнопки шага
            CreateStepButton();

            //панель подсветки
            CreateHighlightingPanel();

            //панель выбора техник
            CreateTecniquesPanel();

            //создание панели меню
            CreateMenu();

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
            

            this.MainMenuStrip = menu;
            this.Controls.Add(menu);
        }


        //----------------------------------------------------------------------------------------------------------------------
        //следующий шаг решения
        int step = 0;
        bool[] usedTecniques;
        private void stepButtonClick(object sender, EventArgs e)
        {

            grid.updateGrid(ref field);

            usedTecniques = new bool[tecniques.Length];

            switch (step)
            {
                case 0:
                    

                    for(int i = 0; i < tecniques.Length; i++)
                    {
                        usedTecniques[i] = tecniques[i].Checked;
                    }

                    string answer = Logic.findElimination(ref field, usedTecniques);
                    printToConsole(answer);

                       

                    

                    grid.HighlighteRemoved(Logic.clues,Logic.removed);

                    if(needRefresh || (Logic.chain!=null && Logic.chain.Count != 0))
                    {
                        this.Refresh();
                        if(Logic.chain==null || (Logic.chain!=null && Logic.chain.Count == 0))
                        {
                            needRefresh = false;
                        }
                        else
                        {
                            needRefresh = true;
                        }
                    }

                    break;
            }
        }

        //----------------------------------------------------------------------------------------------------------------------
        //нажатие кнопки вызова загрузчика
        private void loaderOpenButton_Click(object sender,EventArgs e)
        {
            loaderOpen();
        }
        
        //вызов окна загрузки
        private void loaderOpen()
        {
            loader = new Loader(this);
        }

        //----------------------------------------------------------------------------------------------------------------------
        //нажатие на кнопку вызова конструктора
        private void constrOpenClick(object sender, EventArgs e)
        {
            LoadConstructor();
        }

        //вызов конструктора
        private void LoadConstructor()
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

            tecniques = new CheckBox[Logic.tecniques.Length];
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
            }

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
        //создание панели подстветки
        private void CreateHighlightingPanel()
        {
            highlightingPanel = new GroupBox();
            highlightingPanel.Location = new Point(stepButton.Location.X + stepButton.Width + 20, stepButton.Location.Y);
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
        private void CreateStepButton()
        {
            stepButton = new Button();
            stepButton.Size = new Size(70, 30);
            stepButton.Location = new Point(console.Location.X, console.Location.Y + console.Size.Height + 30);
            stepButton.Text = "Далее";
            stepButton.Visible = true;
            stepButton.Click += stepButtonClick;

            this.AcceptButton = stepButton;
            this.Controls.Add(stepButton);
        }


        //----------------------------------------------------------------------------------------------------------------------
        //создание консоли
        private void CreateConsole()
        {
            console = new Label();
            console.Visible = true;
            console.Size = new Size(300, 400);
            console.Location = new Point(grid.startX + grid.sizeGrid + 30, grid.startY);
            console.BorderStyle = BorderStyle.FixedSingle;
            console.BackColor = Color.FromArgb(255, 228, 181);
            console.Text = "";
            this.Controls.Add(console);
        }

        //очистить консоль
        void clearConsole()
        {
            console.Text = "";
        }

        //написать в консоль
        void printToConsole(string s)
        {
            console.Text = s + "\n" + console.Text;
        }

        //----------------------------------------------------------------------------------------------------------------------
        //отрисовка вспомогательных элементов
        //private void Form1_Paint(object sender, PaintEventArgs e)
        //{
            //base.OnPaint(e);

            
        //}

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

                MessageBox.Show("Загружено тестовое судоку", "Файл не существует",MessageBoxButtons.OK,MessageBoxIcon.Error);
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

            //обновляю поле
            field = new Field();
            field.updateField(sudoku);
            Logic.SimpleRestriction(ref field);
            grid.reloadGrid();
            grid.updateGrid(ref field);

        }

        public void loadSudokuFromBuffer()
        {
            //переписываю судоку

            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {
                    sudoku[i][j] = Buffer.sudoku[i][j];
                }
            }

            //очищаю консоль
            clearConsole();

            //обновляю поле
            field = new Field();
            field.updateField(sudoku);
            Logic.SimpleRestriction(ref field);
            grid.reloadGrid();
            grid.updateGrid(ref field);

        }

    }
}
