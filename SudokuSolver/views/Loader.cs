using System;
using System.Drawing;
using System.Windows.Forms;
using SolverLibrary.Interfaces;

namespace SudokuSolver
{
    public class Loader : Form
    {
        private readonly IController controller;

        ListView list;
        Button load;       //кнопка загрузки в солвер
        Button delete;     //кнопка удаления файла
        Button exit;       //кнопка выхода


        public Loader(IController controller)
        {
            this.controller = controller;

            InitializeComponent();

            //создание списка файлов
            CreateFileList();

            //заполнение списка файлов
            FillList();

            //создание кнопок загрузки, удаления, закрытия
            CreateButtons();

            this.ShowDialog();
        }



        private void LoadButton_Click(object sender, EventArgs e)
        {
            if (list.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Выберите файл", "Ошибка!");
            }
            else if (list.SelectedIndices.Count > 1)
            {
                MessageBox.Show("Выберите один файл", "Ошибка!");
            }
            else
            {
                var items = list.SelectedItems;
                foreach (ListViewItem item in items)
                {
                    controller.LoadFrom(item.Text);
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            //если ничего не выбрано
            if (list.SelectedIndices.Count == 0)
            {
                MessageBox.Show("Не выбраны файлы для удаления", "Ошибка!");
            }
            //иначе
            else
            {
                //спрашиваю подтверждение
                if (MessageBox.Show("Подтвердите удаление файла", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //составляю массив имен файлов
                    var items = list.SelectedItems;
                    string[] names = new string[items.Count];

                    for (int i = 0; i < items.Count; i++)
                    {
                        names[i] = items[i].Text;

                    }
                    //удаляю выбранные 
                    controller.Delete(names);

                    //убираю из списка
                    foreach (ListViewItem item in items)
                    {
                        list.Items.Remove(item);
                    }

                    MessageBox.Show("Файлы успешно удалены", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        private void CreateButtons()
        {
            load = new Button()
            {
                Text = "Загрузить в решатель",
                Size = new Size((int)(list.Size.Width * 0.4), 40),
                Visible = true
            };
            load.Location = new Point(list.Location.X + list.Width - load.Width, list.Location.Y + list.Height + 20);
            load.Click += LoadButton_Click;

            this.Controls.Add(load);

            delete = new Button()
            {
                Text = "Удалить файл",
                Size = load.Size,
                Location = new Point(list.Location.X, load.Location.Y),
                Visible = true
            };

            delete.Click += DeleteButton_Click;
            this.Controls.Add(delete);

            exit = new Button()
            {
                Text = "Закрыть",
                Size = delete.Size,
                Location = new Point(load.Location.X, load.Location.Y + load.Height + 20)
            };

            exit.Click += ExitButton_Click;
            this.Controls.Add(exit);
        }



        private void CreateFileList()
        {
            list = new ListView
            {
                Size = new Size(350, 310),
                Location = new Point(20, 20),
                View = View.List,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,

                MultiSelect = true,

                GridLines = true,
                FullRowSelect = true,
                HideSelection = false,
                Scrollable = true
            };
            list.View = View.Details;

            //добавление колонки
            ColumnHeader header = new ColumnHeader
            {
                Width = list.Size.Width - 25,
                Text = "Название"
            };
            list.Columns.Add(header);

            list.AllowDrop = false;

            list.DoubleClick += LoadButton_Click;

            this.Controls.Add(list);
        }

        private void FillList()
        {
            var filenames = controller.GetListSaved();

            filenames.ForEach(f => AddLine(f));
        }

        private void AddLine(string name)
        {
            ListViewItem line = new ListViewItem(name);

            list.Items.Add(line);
        }



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
