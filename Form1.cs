using FFMpegCore;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Reflection;

namespace FragmentMaker
{
    public partial class FragmentMaker : Form
    {
        private static readonly String path = @".\Capture";
        private static readonly String _outFileName = @"captured_video.mp4";
        private static int index;
        private static int maxFrames = 720;
        private static int frameRate = 24;
        private static bool isCompilationStarted = false;
        private static System.Threading.Timer timer;
        private static FragmentMaker _instance;

        public FragmentMaker()
        {
            InitializeComponent();
            btnSave.Enabled = false;
            _instance = this;

            button1.Enabled = File.Exists(_outFileName);

            FormClosing += new FormClosingEventHandler(Form1_FormClosing);
        }

        private static FragmentMaker GetInstance()
        {
            return _instance;
        }

        public void StartCapture(Rectangle? rectangle)
        {
            if (!rectangle.HasValue) { return; }

            btnRecord.Enabled = false;
            btnSave.Enabled = true;

            DeleteDirectoryAsync();

            TimerCallback tm = async _ =>
            {
                await saveBitmapsAsync(rectangle.Value);

            };

            index = 0;
            int num = 0;
            timer = new System.Threading.Timer(tm, num, 0, (1000 / frameRate));
        }

        private static void StartCompilation()
        {
            GetInstance().btnSave.Enabled = false;

            if (!isCompilationStarted)
            {
                isCompilationStarted = true;

                timer.Dispose();

                GetInstance().toolStripStatusLabel1.Text = "Compilation started";

                string[] files = Directory.GetFiles(path, "*.png");
                
                Debug.WriteLine("Start compiling: " + DateTime.Now);
               

                if (FFMpeg.JoinImageSequence(_outFileName, frameRate, files))
                {
                    Debug.WriteLine("Finish compiling: " + DateTime.Now);

                    GetInstance().ShowNotification();
                    GetInstance().RestoreState();
                    isCompilationStarted = false;
                }
            }
        }

        private void ShowNotification()
        {
            notifyIcon1.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon1.Visible = true;
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipTitle = "Success";
            notifyIcon1.BalloonTipText = "Capture successful saved. Click to copy link";
            notifyIcon1.ShowBalloonTip(100);
        }

        private void RestoreState()
        {
            index = 0;
            toolStripStatusLabel1.Text = "Compilation finished";
            btnRecord.Enabled = true;
            btnSave.Enabled = false;
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            new CaptureForm(this).Show();
        }

        private async Task saveBitmapsAsync(Rectangle rect)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            try
            {
                GetSreenshot(rect).Save(path + @"\" + $"{index:00000}" + ".png", ImageFormat.Png);

                if (index == maxFrames)
                {
                    StartCompilation();
                }

                index++;
            }
            catch (Exception ex)
            {
                timer.Dispose();
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
            finally
            {
                DeleteDirectoryAsync();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            btnSave.Enabled = false;
            StartCompilation();
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            copyVideoToClipboard();
        }

        private void copyVideoToClipboard()
        {
            var path = new StringCollection
            {
                Path.GetFullPath(_outFileName)
            };
            Clipboard.SetFileDropList(path);
        }

        private Bitmap GetSreenshot(Rectangle rect)
        {
            if (rect.Width % 2 != 0)
            {
                rect.Width = rect.Width - 1;
            }
            if (rect.Height % 2 != 0)
            {
                rect.Height = rect.Height - 1;
            }
            Bitmap captureBitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            Rectangle captureRectangle = Screen.PrimaryScreen.Bounds;
            Graphics captureGraphics = Graphics.FromImage(captureBitmap);
            captureGraphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, captureRectangle.Size);
            return captureBitmap;
        }

        private async Task DeleteDirectoryAsync()
        {
            if (true)
            {
                return;
            }
            var isDeleting = true;
            while (Directory.Exists(path))
            {
                if (isDeleting)
                {
                    isDeleting = false;
                    await Task.Run(() =>
                    {
                        Directory.Delete(path, true);
                    });

                }
            }

            Directory.CreateDirectory(path);
        }

        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            copyVideoToClipboard();
        }

        private void FragmentMaker_KeyPress(object sender, KeyPressEventArgs e)
        {
            Debug.WriteLine("KeyCode: " + e.KeyChar);
            if (e.KeyChar == (char)Keys.Escape)
            {
                Application.Exit();
            }
        }
    }
}