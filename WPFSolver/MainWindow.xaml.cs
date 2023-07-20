using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFSolver
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Init();    
        }

        private void Init()
        {
            FillGrid();
        }


        //private int cellSize = 15;

        private void FillGrid()
        {
            for(int i = 1; i < gridview.RowDefinitions.Count; i++)
            {
                for(int j = 1; j < gridview.ColumnDefinitions.Count; j++)
                {
                    Label label = new Label()
                    {
                        Content = $"({i};{j})",
                        FontSize = 5
                    };
                    Grid.SetRow(label, i);
                    Grid.SetColumn(label, j);
                    gridview.Children.Add(label);
                }
            }

            for(int i = 1; i < gridview.RowDefinitions.Count; i++)
            {
                Label label = new Label()
                {
                    Content = i,
                    FontSize = 5
                };

                Grid.SetRow(label, i);
                Grid.SetColumn(label, 0);
                gridview.Children.Add(label);
            }
            for (int j = 1; j < gridview.ColumnDefinitions.Count; j++)
            {
                Label label = new Label()
                {
                    Content = j,
                    FontSize = 5
                };

                Grid.SetRow(label, 0);
                Grid.SetColumn(label, j);
                gridview.Children.Add(label);
            }

        }
        
    }
}
