

using System;
using System.Drawing;
using System.Windows.Forms;
using static FragmentMaker.FragmentMaker;

namespace FragmentMaker
{
    public partial class CaptureForm : Form
    {
        private Point startDragPoint;
        private bool isMouseDown;
        private readonly Graphics graphics;
        private readonly FragmentMaker mainWindow;
        private Rectangle? rectangle;
        private int left = 0, top = 0, right = 0, bottom = 0, width = 0, height = 0;

        public CaptureForm(FragmentMaker mainWindow)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            graphics = CreateGraphics();
        }

        private void CaptureForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                Close();
            }
        }

        private void CaptureForm_MouseMove(object sender, MouseEventArgs mouseEvent)
        {
          
            if (isMouseDown)
            {
                left = startDragPoint.X;
                top = startDragPoint.Y;
                bottom = mouseEvent.Y;
                right = mouseEvent.X;
                width = right - left;
                height = bottom - top;

                Refresh();
                rectangle = new Rectangle(
                     Math.Min(right, left),
                     Math.Min(bottom, top),
                     (width > 0) ? width : width * -1,
                     (height > 0) ? height : height * -1);
                if (rectangle.HasValue) {
                    graphics.FillRectangle(Brushes.Aqua, rectangle.Value);
                }
                
            }
        }

        private void CaptureForm_MouseUp(object sender, MouseEventArgs e)
        {
            Refresh();
            graphics.Dispose();
            isMouseDown = false;
            if (rectangle != null)
            {

            }
            Close();

            mainWindow.StartCapture(rectangle);
        }

        private void CaptureForm_MouseDown(object sender, MouseEventArgs e)
        {
            startDragPoint = new Point(e.X, e.Y);
            isMouseDown = true;
        }
    }
}
