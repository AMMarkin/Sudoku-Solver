using SudokuSolver.controller;
using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SudokuSolver
{
    public class Constructor : Form
    {
        private readonly IController controller;


        private GroupBox inputField;        //поле для ввода чисел
        private TextBox[][] digits;         //текстовые поля
        private Button loadButton;          //кнопка Загрузить в солвер
        private Button saveButton;          //кнопка Сохранить в файл
        private Button exitButton;          //кнопка выхода
        private TextBox fileName;           //поля ввода имени файла для сохранения
        private readonly Solver mainForm;             //основное окно


        public Constructor(Solver mainForm, IController controller)
        {
            this.mainForm = mainForm;
            this.controller = controller;

            //настройка окна
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Size = new Size(400, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Text = "Создание судоку";


            //создание поля ввода судоку
            CreateInputField();

            //создание текстового поля для ввода имени сохраняемого файла
            CreateTextBoxForFileName();

            //создание кнопок загрузки
            CreateLoadButtons();

            //создание кнопки выхода
            CreateExitButton();


            this.Size = new Size(60 + inputField.Width, exitButton.Location.Y + exitButton.Height + 50);

            mainForm.Hide();
            this.ShowDialog();
        }

        //создание поля ввода
        private void CreateInputField()
        {
            Color first = Color.FromArgb(119, 221, 119);     // светло - зеленый 
            Color second = Color.FromArgb(100, 149, 237);   // светло - синий


            //создание контейнера
            int digitsSize = 35;

            inputField = new GroupBox()
            {
                Text = "Поле ввода",
                Location = new Point(20, 40),
                Visible = true,
                Font = new Font(this.Font.Name, 10)


            };
            this.Controls.Add(inputField);


            //создание полей ввода
            digits = new TextBox[9][];
            for (int i = 0; i < 9; i++)
            {
                digits[i] = new TextBox[9];
                for (int j = 0; j < 9; j++)
                {
                    digits[i][j] = new TextBox()
                    {
                        Width = digitsSize,
                        Location = new Point(10 + j * (digitsSize + 5), 30 + i * (digitsSize + 5)),
                        Visible = true,
                        TextAlign = HorizontalAlignment.Center,
                        Font = new Font(inputField.Font.Name, (int)(digitsSize * 0.5))
                    };
                    if ((i / 3 + j / 3) % 2 != 0)
                    {
                        digits[i][j].BackColor = first;
                    }
                    else
                    {
                        digits[i][j].BackColor = second;
                    }
                    digits[i][j].KeyPress += InputField_KeyPress;
                }

                inputField.Controls.AddRange(digits[i]);
            }

            inputField.Size = new Size(20 + 9 * (digits[0][0].Width + 5), 50 + 9 * (digits[0][0].Width + 5));
        }

        //настройка фильтров
        //перевод курсора на следующее поле
        private void InputField_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 32 || number == 48)
            {
                e.Handled = true;

            }
            else
            {
                if (number != 8)
                {
                    if (txt.TextLength < 1)
                    {
                        if (txt.Equals(digits[8][8]))
                        {
                            fileName.Focus();
                        }
                        else
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                for (int j = 0; j < 9; j++)
                                {
                                    //нужно взять фокус на следущий текст бокс
                                    if (digits[i][j].TabIndex == txt.TabIndex + 1)
                                    {
                                        digits[i][j].Focus();
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        txt.Text = "";
                    }
                }

            }

            if (number == 32)
            {
                e.Handled = true;
            }

        }

        //создание текстового поля для ввода имени сохраняемого файла
        private void CreateTextBoxForFileName()
        {
            //текстовое поле
            fileName = new TextBox()
            {
                Text = "",
                TextAlign = HorizontalAlignment.Right,
                Font = new Font(this.Font.Name, 10),
                Width = inputField.Width / 2,
                Location = new Point(inputField.Location.X + inputField.Width / 2, inputField.Location.Y + inputField.Height + 20)

            };

            this.Controls.Add(fileName);

            //подпись
            Label l = new Label()
            {
                Text = "Название файла: ",
                Font = new Font(this.Font.Name, 10),
                Width = inputField.Width / 2,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(inputField.Location.X, fileName.Location.Y)
            };

            this.Controls.Add(l);
        }

        //создание кнопки выхода
        private void CreateExitButton()
        {
            exitButton = new Button()
            {
                Text = "Закрыть",
                Size = new Size(saveButton.Width, saveButton.Height),
                Location = new Point(saveButton.Location.X, saveButton.Location.Y + saveButton.Height + 20),
                Visible = true,
                Font = new Font(this.Font.Name, 10)
            };
            exitButton.Click += ExitButton_Click;
            this.Controls.Add(exitButton);
        }

        //закрытие конструктора
        private void ExitButton_Click(object sender, EventArgs e)
        {
            mainForm.Show();
            this.Close();
        }

        //создание кнопки загрузки судоку
        private void CreateLoadButtons()
        {
            //кнопка
            saveButton = new Button()
            {
                Text = "Сохранить",
                Size = new Size((int)(inputField.Width * 0.45), 40),
                Visible = true,
                Font = new Font(this.Font.Name, 10)
            };
            saveButton.Location = new Point(inputField.Location.X + inputField.Width - saveButton.Width, inputField.Location.Y + inputField.Height + 20 + fileName.Height + 20);
            saveButton.Click += SaveButton_Click;

            this.Controls.Add(saveButton);

            loadButton = new Button()
            {
                Text = "Загрузить в решатель",
                Size = new Size((int)(inputField.Width * 0.45), 40),
                Visible = true,
                Font = new Font(this.Font.Name, 10),
                Location = new Point(inputField.Location.X, saveButton.Location.Y)
            };
            loadButton.Click += LoadButton_Click;

            this.Controls.Add(loadButton);
        }

        //кнопка загрузки в солвер
        private void LoadButton_Click(object sender, EventArgs e)
        {
            Buffer.sudoku = new int[9][];

            for (int i = 0; i < 9; i++)
            {
                Buffer.sudoku[i] = new int[9];
            }

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (digits[i][j].Text.Length == 0)
                    {
                        Buffer.sudoku[i][j] = 0;
                    }
                    else
                    {
                        Buffer.sudoku[i][j] = Convert.ToInt32(digits[i][j].Text);
                    }
                }
            }

            controller.LoadFrom("BUFFER");

        }

        //сохранить судоку в файл
        private void SaveButton_Click(object sender, EventArgs e)
        {
            string name = fileName.Text;

            //если не введено название файла
            if (name.Equals(""))
            {
                MessageBox.Show("Введите название файла", "Ошибка!", MessageBoxButtons.OK);
                fileName.Focus();
            }
            //если введено
            else
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (digits[i][j].Text.Length == 0)
                        {
                            sb.Append("0");
                        }
                        else
                        {
                            sb.Append(digits[i][j].Text);
                        }
                        if (j != 8)
                        {
                            sb.Append(" ");
                        }
                    }
                    sb.Append("\n");
                }

                controller.SaveToFile(name, sb.ToString());

                MessageBox.Show("Файл " + name + " успешно сохранен.", "Сохранение", MessageBoxButtons.OK);
                exitButton.Focus();

            }
        }



    }
}
