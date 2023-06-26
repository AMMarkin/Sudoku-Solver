using System;
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
    internal class DrawingPanel : Form
    {

        public DrawingPanel()
        {
            TopMost = true;
            TransparencyKey = SystemColors.Control;

            
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawEllipse(Pens.Red, 100, 100, 300, 300);
        }
    }
}
