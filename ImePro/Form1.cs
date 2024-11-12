using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
//wusing WebCamLib;
//wusing ImageProcess2;

namespace ImePro
{
    public partial class Form1 : Form
    {
        Bitmap loaded, bgloaded, processed;
        //wDevice devices[];
        ConvMatrix convMatrix;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //wdevices = DeviceManager.GetAllDevices();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            loaded = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = loaded;
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            bgloaded = new Bitmap(openFileDialog2.FileName);
            pictureBox2.Image = bgloaded;
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            processed.Save(saveFileDialog1.FileName);
        }

        private void pixelCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x,y);
                    processed.SetPixel(x,y,pixel);
                }
            }
            pictureBox3.Image = processed;
        }

        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            Byte ave;
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    ave = (Byte)(pixel.R + pixel.G + pixel.B / 3);
                    Color grey = Color.FromArgb(ave, ave, ave);
                    processed.SetPixel(x, y, grey);

                }
            }
            pictureBox3.Image = processed;
        }

        private void inversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    Color inv = Color.FromArgb(255-pixel.R, 255-pixel.G, 255-pixel.B);
                    processed.SetPixel(x, y, inv);

                }
            }
            pictureBox3.Image = processed;
        }

        private void mirrorHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    processed.SetPixel(loaded.Width-1-x, y, pixel);
                }
            }
            pictureBox3.Image = processed;
        }

        private void mirrorVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    processed.SetPixel(x, loaded.Height-1-y, pixel);
                }
            }
            pictureBox3.Image = processed;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    int sepR = (int)((pixel.R * 0.393) + (pixel.G * 0.769) + (pixel.B * 0.189));
                    int sepG = (int)((pixel.R * 0.349) + (pixel.G * 0.686) + (pixel.B * 0.168));
                    int sepB = (int)((pixel.R * 0.272) + (pixel.G * 0.534) + (pixel.B * 0.131));
                    Color sep = Color.FromArgb(Math.Min(sepR, 255), Math.Min(sepG, 255), Math.Min(sepB, 255));
                    processed.SetPixel(x, y, sep);
                }
            }
            pictureBox3.Image = processed;

        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Histogram(loaded, ref processed);
            pictureBox3.Image = processed;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            BasicDIP.Brightness(loaded, ref processed, trackBar1.Value);
            pictureBox3.Image = processed;
        }

        private void brightnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Brightness(loaded, ref processed, trackBar1.Value);
            pictureBox3.Image = processed;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            // remove due to lag
        }

        private void contrastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Equalisation(loaded, ref processed, trackBar2.Value / 100);
            pictureBox3.Image = processed;
        }

        private void rotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Rotate(loaded, ref processed, trackBar3.Value);
            pictureBox3.Image = processed;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            BasicDIP.Rotate(loaded, ref processed, trackBar3.Value);
            pictureBox3.Image = processed;
        }

        private void to100pxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Scale(loaded, ref processed, 100, 100);
            pictureBox3.Image = processed;
        }

        private void to1000pxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Scale(loaded, ref processed, 1000, 1000);
            pictureBox3.Image = processed;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color mgreen = Color.FromArgb(0, 0, 255);
            int greygreen = (mgreen.R + mgreen.G + mgreen.B) / 3;
            int threshold = 10;
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    Color pixel = loaded.GetPixel(x, y);
                    Color bgpixel = bgloaded.GetPixel(x, y);
                    int grey = (pixel.R + pixel.G + pixel.B) / 3;
                    int subtractvalue = Math.Abs(grey - greygreen);
                    if (subtractvalue > threshold)
                        processed.SetPixel(x, y, pixel);
                    else
                        processed.SetPixel(x, y, bgpixel);
                }
            }
            pictureBox3.Image = processed;
        }

        private void webcamOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //wdevices[0].ShowWindow(pictureBox1);
        }

        private void webcamOffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //wdevices[0].Stop();
        }

        private void greyscaleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            IDataObject data; // implicit data
            //wImage bmap;
            //wdevices[0].Sendmessage(); // copy a frame
            data = Clipboard.GetDataObject();
            //wbmap = (Image)(data.GetData("System.Drawing.Bitmap", true));
            //wBitmap b = new Bitmap(bmap);
            //wBitmapFilter.GrayScale(b);
            //wpictureBox2.Image = b;
        }

        private void binaryThresholdingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = new Bitmap(loaded.Width, loaded.Height);
            Color pixel;
            int g;
            for (int x = 0; x < loaded.Width; x++)
            {
                for (int y = 0; y < loaded.Height; y++)
                {
                    pixel = loaded.GetPixel(x, y);
                    g = (int)(pixel.R + pixel.G + pixel.B / 3);
                    if (g < 180)
                        processed.SetPixel(x, y, Color.Black);
                    else
                        processed.SetPixel(x, y, Color.White);
                }
            }
            pictureBox3.Image = processed;
        }

        private void rightToLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loaded = processed;
            pictureBox1.Image = loaded;
        }
    }
}
