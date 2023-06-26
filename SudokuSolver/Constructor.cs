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
    internal class Constructor : Form
    {
        GroupBox inputField;        //поле для ввода чисел
        TextBox[][] digits;         //текстовые поля
        Button loadButton;          //кнопка Загрузить в солвер
        Button saveButton;          //кнопка Сохранить в файл
        Button exitButton;          //кнопка выхода
        TextBox fileName;           //поля ввода имени файла для сохранения
        Form1 mainForm;             //основное окно


        public Constructor(Form1 mainForm)
        {
            this.mainForm = mainForm;

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


            this.Size = new Size(60 + inputField.Width, exitButton.Location.Y+exitButton.Height+50);

            this.ShowDialog();
        }

        //создание поля ввода
        private void CreateInputField()
        {
            //Color first = Color.FromArgb(60, 179, 113);     // зеленый 
            Color first = Color.FromArgb(119, 221, 119);     // светло - зеленый 
            //Color second = Color.FromArgb(123, 104, 238);   // синий
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
                        Font = new Font(inputField.Font.Name,(int)(digitsSize*0.5))
                    };
                    if ((i/3 + j/3) %2 != 0)
                    {
                        digits[i][j].BackColor = first;
                    }
                    else
                    {
                        digits[i][j].BackColor = second;
                    }
                    digits[i][j].KeyPress += inputField_KeyPress;
                }

                inputField.Controls.AddRange(digits[i]);
            }

            inputField.Size = new Size(20 + 9 * (digits[0][0].Width + 5), 50 + 9 * (digits[0][0].Width + 5));
        }

        //настройка фильтров
        //перевод курсора на следующее поле
        private void inputField_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox txt = (TextBox)sender;

            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 32 || number==48)
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
                Location = new Point(inputField.Location.X+inputField.Width/2, inputField.Location.Y+inputField.Height+20)
                
            };

            this.Controls.Add(fileName);

            //подпись
            Label l = new Label()
            {
                Text = "Название файла: ",
                Font = new Font(this.Font.Name, 10),
                Width = inputField.Width / 2,
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(inputField.Location.X,fileName.Location.Y)
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
                Font = new Font(this.Font.Name,10)
            };
            saveButton.Location = new Point(inputField.Location.X + inputField.Width - saveButton.Width, inputField.Location.Y + inputField.Height + 20+ fileName.Height+20);
            saveButton.Click += saveButton_Click;
            
            this.Controls.Add(saveButton);

            loadButton = new Button()
            {
                Text = "Загрузить в решатель",
                Size = new Size((int)(inputField.Width * 0.45), 40),
                Visible = true,
                Font = new Font(this.Font.Name, 10),
                Location = new Point(inputField.Location.X, saveButton.Location.Y)
            };
            loadButton.Click += loadButton_Click;

            this.Controls.Add(loadButton);
        }

        //кнопка загрузки в солвер
        private void loadButton_Click(object sender,EventArgs e)
        {
            Buffer.sudoku = new int[9][];

            for(int i = 0; i < 9; i++)
            {
                Buffer.sudoku[i] = new int[9];
            }

            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
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

            mainForm.loadSudokuFromBuffer();
            
        }

        //сохранить судоку в файл
        private void saveButton_Click(object sender, EventArgs e)
        {
            string name = fileName.Text;

            string dir = @"E:\SudokuSolver\Sudoku\";
            string path = dir+  name + ".txt";

            if (name.Equals(""))
            {
                MessageBox.Show("Введите название файла", "Ошибка!", MessageBoxButtons.OK);
                fileName.Focus();
            }
            else
            {
                if (File.Exists(path))
                {
                    MessageBox.Show("Файл "+ name + " уже существует", "Ошибка!", MessageBoxButtons.OK);
                    fileName.Focus();
                }
                else
                {
                    using (FileStream f = File.Create(path))
                    {
                        for(int i = 0; i < 9; i++)
                        {
                            for(int j = 0; j < 9; j++)
                            {
                                if (digits[i][j].Text.Length == 0)
                                {
                                    WriteText(f, "0");
                                }
                                else
                                {
                                    WriteText(f, digits[i][j].Text);
                                }
                                if (j != 8)
                                {
                                    WriteText(f, " ");
                                }
                            }
                            WriteText(f, "\n");
                        }
                    }
                    MessageBox.Show("Файл "+name+" успешно сохранен.","Сохранение",MessageBoxButtons.OK);
                    exitButton.Focus();
                }
            }
        }
        //запись в файл
        private static void WriteText(FileStream f,string s)
        {
            byte[] byteS = Encoding.UTF8.GetBytes(s);
            f.Write(byteS, 0, byteS.Length);
        }


    }
}
