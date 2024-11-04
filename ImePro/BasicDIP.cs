using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImePro
{
    static class BasicDIP
    {
        public static void Brightness(Bitmap a, ref Bitmap b, int value)
        {
            b = new Bitmap(a.Width, a.Height);
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    Color temp = a.GetPixel(x, y);
                    Color changed;
                    if (value > 0)
                        changed = Color.FromArgb(Math.Min(temp.R + value, 255), Math.Min(temp.G + value, 255), Math.Min(temp.B + value, 255));
                    else
                        changed = Color.FromArgb(Math.Max(temp.R + value, 0), Math.Max(temp.G + value, 0), Math.Max(temp.B + value, 0));
                    b.SetPixel(x, y, changed);
                }
            }
        }

        public static void Equalisation(Bitmap a, ref Bitmap b, int degree)
        {
            int height = a.Height;
            int width = a.Width;
            int numSamples, histSum;
            int[] Ymap = new int[256];
            int[] hist = new int[256];
            int percent = degree;

            Color taken;
            Color grey;
            Byte greydata;

            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    taken = a.GetPixel(x, y);
                    greydata = (Byte)(taken.R + taken.G + taken.B / 3);
                    grey = Color.FromArgb(greydata, greydata, greydata);
                    a.SetPixel(x, y, grey);
                }
            }

            // histogram id
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    taken = a.GetPixel(x, y);
                    hist[taken.B]++;
                }
            }

            // remap Ys, use max contrast
            // based on histogram qualization
            numSamples = (a.Width * a.Height);
            histSum = 0;
            for (int h = 0; h < 256; h++)
            {
                histSum += hist[h];
                Ymap[h] = histSum * 255 / numSamples;
            }

            // if not max, adjust
            if (percent < 100)
            {
                for (int h = 0; h < 256; h++)
                {
                    Ymap[h] = h + ((int)Ymap[h] - h) * percent / 100;
                }
            }

            b = new Bitmap(a.Width, a.Height);
            // remap intensities
            for (int y = 0; y < a.Height; y++)
            {
                for (int x = 0; x < a.Width; x++)
                {
                    // set new value of grey value
                    Color temp = Color.FromArgb(Ymap[a.GetPixel(x, y).R], Ymap[a.GetPixel(x, y).G], Ymap[a.GetPixel(x, y).B]);
                    b.SetPixel(x, y, temp);
                }
            }
        }

        public static void Rotate(Bitmap a, ref Bitmap b, int value)
        {
            float angleRadians = (float)(value * Math.PI / 180);
            int xCenter = (int)(a.Width / 2);
            int yCenter = (int)(a.Height / 2);
            int width, height, xs, ys, xp, yp, x0, y0;
            float cosA, sinA;
            cosA = (float)Math.Cos(angleRadians);
            sinA = (float)Math.Sin(angleRadians);
            width = a.Width;
            height = a.Height;
            b = new Bitmap(width, height);
            for (xp = 0; xp < width; xp++)
            {
                for (yp = 0; yp < height; yp++)
                {
                    x0 = xp - xCenter;
                    y0 = yp - yCenter;
                    xs = (int)(x0 * cosA + y0 * sinA);
                    ys = (int)(-x0 * sinA + y0 * cosA); 
                    xs = (int)(xs + xCenter);
                    ys = (int)(ys + yCenter);
                    xs = Math.Max(0, Math.Min(width - 1, xs));
                    ys = Math.Max(0, Math.Min(height - 1, ys));
                    b.SetPixel(xp, yp, a.GetPixel(xs, ys));
                }
            }
        }

        public static void Scale(Bitmap a, ref Bitmap b, int nwidth, int nheight)
        {
            int targetWidth = nwidth;
            int targetHeight = nheight;
            int xTarget, yTarget, xSource, ySource;
            int width = a.Width, height = a.Height;
            b = new Bitmap(targetWidth, targetHeight);

            for (xTarget = 0; xTarget < targetWidth; xTarget++)
            {
                for (yTarget = 0; yTarget < targetHeight; yTarget++)
                {
                    xSource = xTarget * width / targetWidth;
                    ySource = yTarget * height / targetHeight;
                    b.SetPixel(xTarget, yTarget, a.GetPixel(xSource, ySource));
                }
            }
        }

        public static void Histogram(Bitmap a, ref Bitmap b)
        {
            Color sample;
            Color grey;
            Byte greydata;
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    sample = a.GetPixel(x, y);
                    greydata = (Byte)((sample.R + sample.G + sample.B) / 3);
                    grey = Color.FromArgb(greydata, greydata, greydata);
                    a.SetPixel(x, y, grey);
                }
            }

            int[] histdata = new int[256];
            for (int x = 0; x < a.Width; x++)
            {
                for (int y = 0; y < a.Height; y++)
                {
                    sample = a.GetPixel(x, y);
                    histdata[sample.R]++; // any RGB since its divided in the grayscale process
                }
            }

            // set bitmap and fill bg
            int bx = 256;
            int by = 800;
            b = new Bitmap(bx, by);
            for (int x = 0; x < bx; x++)
            {
                for (int y = 0; y < by; y++)
                {
                    b.SetPixel(x,y,Color.White);
                }
            }

            // plotting points
            for (int x = 0; x < bx; x++)
            {
                for (int y = 0; y < Math.Min(histdata[x] / 5, b.Height - 1); y++)
                {
                    b.SetPixel(x, (b.Height - 1) - y, Color.Black);
                }
            }
        }
    }
}
