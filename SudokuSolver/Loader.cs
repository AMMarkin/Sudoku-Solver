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
    internal class Loader : Form
    {
        Form1 mainForm;    //основное окно
        ListView list;

        Button load;       //кнопка загрузки в солвер
        Button delete;     //кнопка удаления файла
        Button exit;       //кнопка выхода

        string path;

        public Loader(Form1 mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
            
            //создание списка файлов
            CreateFileList();

            //заполнение списка файлов
            FillList();

            //создание кнопок загрузки, удаления, закрытия
            CreateButtons();

            this.ShowDialog();
        }
        //создание кнопок
        private void CreateButtons()
        {
            load = new Button()
            {
                Text = "Загрузить в решатель",
                Size = new Size((int)(list.Size.Width * 0.4), 40),
                Visible = true
            };
            load.Location = new Point(list.Location.X + list.Width - load.Width, list.Location.Y + list.Height + 20);
            load.Click += loadButton_Click;

            this.Controls.Add(load);

            delete = new Button()
            {
                Text = "Удалить файл",
                Size = load.Size,
                Location = new Point(list.Location.X,load.Location.Y),
                Visible = true
            };

            delete.Click+=deleteButton_Click;
            this.Controls.Add(delete);

            exit = new Button()
            {
                Text = "Закрыть",
                Size = delete.Size,
                Location = new Point(load.Location.X, load.Location.Y + load.Height + 20)
            };

            exit.Click+=exitButton_Click;
            this.Controls.Add(exit);
        }

        //загрузка
        private void loadButton_Click(object sender, EventArgs e)
        {
            if (list.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Выберите файл", "Ошибка!");
            }
            else if(list.SelectedIndices.Count > 1)
            {
                MessageBox.Show("Выберите один файл", "Ошибка!");
            }
            else
            {
                var items = list.SelectedItems;
                foreach (ListViewItem item in items)
                {
                    mainForm.loadSudoku(item.Text);
                }
            }
        }

        //удаление
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (list.SelectedIndices.Count==0)
            {
                MessageBox.Show("Не выбраны файлы для удаления", "Ошибка!");
            }
            else
            {
                if (MessageBox.Show("Подтвердите удаление файла", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var items = list.SelectedItems;
                    string[] names = new string[items.Count];

                    for (int i = 0; i < items.Count; i++)
                    {
                        names[i] = items[i].Text + ".txt";
                        File.Delete(path + names[i]);
                    }

                    foreach (ListViewItem item in items)
                    {
                        list.Items.Remove(item);
                    }

                    MessageBox.Show("Файлы успешно удалены","Успешно",MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        //закрытие
        private void exitButton_Click(object sender,EventArgs e)
        {
            this.Close();
        }
        

        //создание списка файлов
        private void CreateFileList()
        {
            list = new ListView();
            list.Size = new Size(350,310);
            list.Location = new Point(20, 20);
            list.View = View.List;
            list.HeaderStyle = ColumnHeaderStyle.Nonclickable;

            list.MultiSelect = true;
            
            list.GridLines = true;
            list.FullRowSelect = true;
            list.HideSelection = false;
            list.Scrollable = true;
            list.View = View.Details;

            //добавление колонки
            ColumnHeader header = new ColumnHeader();
            header.Width = list.Size.Width-25;
            header.Text = "Название";
            list.Columns.Add(header);

            list.AllowDrop = false;

            list.DoubleClick += loadButton_Click;

            this.Controls.Add(list);
        }

        //заполнение списка файлов
        private void FillList()
        {
            //путь к папке с судоку
            path = @"E:\SudokuSolver\Sudoku\";

            //загружаем все имена
            var a = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).ToList();
            //обрезаем пути
            for(int i = 0; i < a.Count; i++)
            {
                a[i] = Path.GetFileName(a[i]);
            }


            a.ForEach(f => addLine(f));
        }

        private void addLine(string name)
        {
            ListViewItem line = new ListViewItem(name.Split('.')[0]);
            
            list.Items.Add(line);
        }

        //настройка формы
        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            
            // настройка окна
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(400, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Text = "Загрузка судоку";
            // 
            this.Name = "Loader";
            this.ResumeLayout(false);

        }
    }
}
