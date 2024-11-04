using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImePro
{
    public partial class Form1 : Form
    {
        Bitmap loaded, processed;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            loaded = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = loaded;
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
            pictureBox2.Image = processed;
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
            pictureBox2.Image = processed;
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
            pictureBox2.Image = processed;
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
            pictureBox2.Image = processed;
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
            pictureBox2.Image = processed;
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
            pictureBox2.Image = processed;

        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Histogram(loaded, ref processed);
            pictureBox2.Image = processed;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
        }

        private void brightnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Brightness(loaded, ref processed, trackBar1.Value);
            pictureBox2.Image = processed;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {

        }

        private void contrastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Equalisation(loaded, ref processed, trackBar2.Value / 100);
            pictureBox2.Image = processed;
        }

        private void rotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Rotate(loaded, ref processed, trackBar3.Value);
            pictureBox2.Image = processed;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            BasicDIP.Rotate(loaded, ref processed, trackBar3.Value);
            pictureBox2.Image = processed;
        }

        private void to100pxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Scale(loaded, ref processed, 100, 100);
            pictureBox2.Image = processed;
        }

        private void to1000pxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BasicDIP.Scale(loaded, ref processed, 1000, 1000);
            pictureBox2.Image = processed;
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
            pictureBox2.Image = processed;
        }

        private void rightToLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loaded = processed;
            pictureBox1.Image = loaded;
        }
    }
}
