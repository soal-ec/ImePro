﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AForge.Imaging;

//wusing WebCamLib;
//wusing ImageProcess2;
using AForge.Video;
using AForge.Video.DirectShow;
//using OpenCvSharp;
using AForge.Imaging.Filters;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using static System.Windows.Forms.AxHost;
using System.Security.Policy;

namespace ImePro
{
    public partial class Form1 : Form
    {
        Bitmap loaded, bgloaded, processed;

        //wDevice devices[];
        private FilterInfoCollection videoDevices; // videoCaptureDevices
        private VideoCaptureDevice videoSource; // finalVideo

        bool isVideoOn = false;
        String videoFilter;

        int rotation;
        int brightness;
        sbyte contrast;
        int[] gammaRGB = new int[3];
        int[] colorRGB = new int[3];
        int smoothWeight;
        int gaussianBlurWeight;
        int meanRemovalWeight;
        int sharpenWeight;
        short edgeDetectConvolutionType;
        byte edgeDetectConvolusionThreshold;
        byte edgeDetectHomogenityThreshold;
        byte edgeDetectDifferenceThreshold;
        byte edgeEnhanceThreshold;
        int[] resizeValues = new int[2];
        bool resizeIfBilinear;
        short jitterDegree;
        double swirlDegree;
        bool swirlSmoothing;
        bool sphereSmoothing;
        byte timeWarpFactor;
        bool timeWarpSmoothing;
        double moireDegree;
        short waterWave;
        bool waterSmoothing;
        short pixelatePixel;
        bool pixelateGrid;

        int framecount = 0;
        int frameskip = 1;

        int ccGaussianBlurWeight = 5;
        int ccSmoothWeight = 1;
        byte ccEdgeEnhanceThreshold = 150;

        int ccMeanFilter = 7;

        bool useSteps = false;
        int steps = 0;

        bool debug = false;

        int btThres = 50;
        int fcThres = 50;
        int sizeThres = 20;

        int[] coinDiaThres = {107, 120, 141, 161};

        Color shapeOutlineColor = Color.Red;
        int circleThickness = 8;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //wdevices = DeviceManager.GetAllDevices();

            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo videoDevice in videoDevices)
            {
                toolStripComboBox1.Items.Add(videoDevice.Name);
            }

            if (toolStripComboBox1.Items.Count > 0) toolStripComboBox1.SelectedIndex = 0;

            loaded = new Bitmap("images/coins.jpeg");
            pictureBox1.Image = loaded;

            if (debug)
            {
                applyAForgeFiltersToolStripMenuItem.Enabled = true;
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.Stop();
            }
            base.OnFormClosing(e);
        }


        // IMAGE FILE SAVE/LOAD

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            loaded = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = loaded;
            stopVideo();
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
        
        private void rightToLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loaded = (Bitmap)processed.Clone();
            pictureBox1.Image = loaded;
        }



        // IMAGE PROCESSING

        private void pixelCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
                BasicDIP.PixelCopy(loaded, ref processed);
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
                BasicDIP.Greyscale(loaded, ref processed);
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void inversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
                BasicDIP.Inversion(loaded, ref processed);
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void mirrorHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
                BasicDIP.MirrorH(loaded, ref processed);
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void mirrorVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
                BasicDIP.MirrorV(loaded, ref processed);
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
                BasicDIP.Sepia(loaded, ref processed);
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
                BasicDIP.Histogram(loaded, ref processed);
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void brightnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                PopupForm popup = new PopupForm();
                popup.PopupTitle = "Brightness";
                System.Windows.Forms.TrackBar trackBar = new System.Windows.Forms.TrackBar
                {
                    Height = 45,
                    Width = 392,
                    Minimum = -50,
                    Maximum = 50,
                    Value = 0
                };
                popup.SetInputControl(trackBar);
                if (popup.ShowDialog() == DialogResult.OK)
                {
                    BasicDIP.Brightness(loaded, ref processed, trackBar.Value);
                }
                popup.Update();
                textBox1.Text += popup.ifPanelNull();
            }
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }
        
        private void contrastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                PopupForm popup = new PopupForm();
                popup.PopupTitle = "Contrast";
                System.Windows.Forms.TrackBar trackBar = new System.Windows.Forms.TrackBar
                {
                    Height = 45,
                    Width = 392,
                    Minimum = -50,
                    Maximum = 50,
                    Value = 0
                };
                popup.SetInputControl(trackBar);
                if (popup.ShowDialog() == DialogResult.OK)
                {
                    BasicDIP.Equalisation(loaded, ref processed, trackBar.Value / 100);
                }
                popup.Update();
                textBox1.Text += popup.ifPanelNull();
            }
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void rotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                PopupForm popup = new PopupForm();
                popup.PopupTitle = "Rotation";
                System.Windows.Forms.TrackBar trackBar = new System.Windows.Forms.TrackBar
                {
                    Height = 45,
                    Width = 392,
                    Minimum = -180,
                    Maximum = 180,
                    Value = 0
                };
                popup.SetInputControl(trackBar);
                if (popup.ShowDialog() == DialogResult.OK)
                {
                    BasicDIP.Rotate(loaded, ref processed, trackBar.Value);
                }
                popup.Update();
                textBox1.Text += popup.ifPanelNull();
            }
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void to100pxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
                BasicDIP.Scale(loaded, ref processed, 100, 100);
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void to1000pxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
                BasicDIP.Scale(loaded, ref processed, 1000, 1000);
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (loaded != null)
                BasicDIP.Subtraction(loaded, bgloaded, ref processed);
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;
        }

        private void binaryThresholdingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded != null)
            {
                PopupForm popup = new PopupForm();
                popup.PopupTitle = "Binary Thresholding";
                System.Windows.Forms.NumericUpDown numericUpDown = new System.Windows.Forms.NumericUpDown
                {
                    Height = 45,
                    Width = 392,
                    Minimum = 0,
                    Maximum = 255,
                    Value = 180,
                    DecimalPlaces = 0,
                    Name = "Weight"
                };
                popup.SetInputControl(numericUpDown);
                if (popup.ShowDialog() == DialogResult.OK)
                {
                    BasicDIP.BinaryThresholding(loaded, ref processed, (int)numericUpDown.Value);
                }
                popup.Update();
                textBox1.Text += popup.ifPanelNull();
            }
            else
                textBox1.Text += "No Loaded Bitmap\r\n";
            pictureBox3.Image = processed;


        }

        // VIDEO

        private void webcamOnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //wdevices[0].ShowWindow(pictureBox1);
        }

        // Toggle video
        private void onToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isVideoOn && videoSource.IsRunning)
            {
                stopVideo();
            }
            else
            {
                label4.Text = "Loaded (Video)";
                dIPToolStripMenuItem2.Enabled = false;
                videoSource = new VideoCaptureDevice(videoDevices[toolStripComboBox1.SelectedIndex].MonikerString);
                videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);
                videoSource.NewFrame += new NewFrameEventHandler(videoSource_filter);

                videoSource.Start();
                isVideoOn = true;
                pictureBox2.Enabled = false;
            }
        }

        // Stop video
        private void stopVideo()
        {
            label4.Text = "Loaded";
            if (videoSource != null)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
                textBox1.Text += ("Video Stopped");
            }            
            isVideoOn = false;
            pictureBox1.Image = loaded;
            pictureBox2.Enabled = true;
        }

        // Update picturebox1
        private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap video = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = video;
        }

        // Update picturebox2
        private void videoSource_filter(object sender, NewFrameEventArgs eventArgs)
        {
            framecount++;
            if (framecount % frameskip == 0 && isVideoOn)
            {
                Bitmap bm = (Bitmap)eventArgs.Frame.Clone();
                applyFilter(bm, videoFilter);
            }
        }

        private void applyFilter(Bitmap bm, string videoFilter)
        {
            if (!isVideoOn)
            {
                textBox1.Text += "Applying Filter\r\n";
                processed = (Bitmap)loaded.Clone();
                bm = processed;
                if (bm == null)
                {
                    textBox1.Text += "Loaded is null";
                    return;
                }
            }
            switch (videoFilter)
            {
                case "greyscale":
                    BitmapFilter.GrayScale(bm);
                    break;
                case "inversion":
                    BitmapFilter.Invert(bm);
                    break;
                case "mirrorH":
                    BitmapFilter.Flip(bm, true, false);
                    break;
                case "mirrorV":
                    BitmapFilter.Flip(bm, false, true);
                    break;
                case "mirrorHV":
                    BitmapFilter.Flip(bm, true, true);
                    break;
                case "color":
                    BitmapFilter.Color(bm, colorRGB[0], colorRGB[1], colorRGB[2]);
                    break;
                case "brightness":
                    BitmapFilter.Brightness(bm, brightness);
                    break;
                case "contrast":
                    BitmapFilter.Contrast(bm, contrast);
                    break;
                case "gamma":
                    BitmapFilter.Gamma(bm, gammaRGB[0] / 10, gammaRGB[1] / 10, gammaRGB[2] / 10);
                    break;
                case "resize":
                    BitmapFilter.Resize(bm, resizeValues[0], resizeValues[1], resizeIfBilinear);
                    break;
                case "smooth":
                    BitmapFilter.Smooth(bm, smoothWeight);
                    break;
                case "gaussianBlur":
                    BitmapFilter.GaussianBlur(bm, gaussianBlurWeight);
                    break;
                case "meanRemoval":
                    BitmapFilter.MeanRemoval(bm, meanRemovalWeight);
                    break;
                case "sharpen":
                    BitmapFilter.Sharpen(bm, sharpenWeight);
                    break;
                case "embossLaplacian":
                    BitmapFilter.EmbossLaplacian(bm);
                    break;
                case "edgeDetectQuick":
                    BitmapFilter.EdgeDetectQuick(bm);
                    break;
                case "edgeDetectConvolution":
                    BitmapFilter.EdgeDetectConvolution(bm, edgeDetectConvolutionType, edgeEnhanceThreshold);
                    break;
                case "edgeEnhance":
                    BitmapFilter.EdgeEnhance(bm, edgeEnhanceThreshold);
                    break;
                case "edgeDetectH":
                    BitmapFilter.EdgeDetectHorizontal(bm);
                    break;
                case "edgeDetectV":
                    BitmapFilter.EdgeDetectVertical(bm);
                    break;
                case "edgeDetectHomogenity":
                    BitmapFilter.EdgeDetectHomogenity(bm, edgeDetectHomogenityThreshold);
                    break;
                case "edgeDetectDifference":
                    BitmapFilter.EdgeDetectDifference(bm, edgeDetectDifferenceThreshold);
                    break;
                case "randomJitter":
                    BitmapFilter.RandomJitter(bm, jitterDegree);
                    break;
                case "swirl":
                    BitmapFilter.Swirl(bm, swirlDegree, swirlSmoothing);
                    break;
                case "sphere":
                    BitmapFilter.Sphere(bm, sphereSmoothing);
                    break;
                case "timeWarp":
                    BitmapFilter.TimeWarp(bm, timeWarpFactor, timeWarpSmoothing);
                    break;
                case "moire":
                    BitmapFilter.Moire(bm, moireDegree);
                    break;
                case "water":
                    BitmapFilter.Water(bm, waterWave, waterSmoothing);
                    break;
                case "pixelate":
                    BitmapFilter.Pixelate(bm, pixelatePixel, pixelateGrid);
                    break;
                default:
                    break;
            }
            pictureBox3.Image = bm;
            
        }

        private void pixelCopyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            videoFilter = "";
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void greyscaleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            videoFilter = "greyscale";
            // timer1.Enabled = true;
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void inversionToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            videoFilter = "inversion";
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            videoFilter = "mirrorH";
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            videoFilter = "mirrorV";
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void bothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            videoFilter = "mirrorHV";
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void gammaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Gamma RGB Values";
            System.Windows.Forms.TrackBar trackBarR = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = 2,
                Maximum = 50,
                Value = 2
            };
            System.Windows.Forms.TrackBar trackBarG = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = 2,
                Maximum = 50,
                Value = 2
            };
            System.Windows.Forms.TrackBar trackBarB = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = 2,
                Maximum = 50,
                Value = 2
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(trackBarR);
            panel.Controls.Add(trackBarG);
            panel.Controls.Add(trackBarB);
            panel.Width = trackBarR.Width;
            panel.Height = trackBarB.Height + trackBarG.Height + trackBarB.Height + (int)(trackBarR.Margin.Bottom + trackBarG.Margin.Bottom + trackBarB.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                gammaRGB[0] = trackBarR.Value;
                gammaRGB[1] = trackBarG.Value;
                gammaRGB[2] = trackBarB.Value;
                videoFilter = "gamma";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void brightnessToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Brightness";
            System.Windows.Forms.TrackBar trackBar = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = -50,
                Maximum = 50,
                Value = 0
            };
            popup.SetInputControl(trackBar);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                brightness = trackBar.Value;
                videoFilter = "brightness";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void contrastToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Contrast";
            System.Windows.Forms.TrackBar trackBar = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = 1,
                Maximum = 100,
                Value = 1
            };
            popup.SetInputControl(trackBar);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                contrast = (sbyte)trackBar.Value;
                videoFilter = "contrast";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void scaleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Resize Values";
            System.Windows.Forms.NumericUpDown numericUpDownX = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = 1,
                Maximum = 10000,
                Value = 800,
                DecimalPlaces = 0,
                Name = "X"
            };
            System.Windows.Forms.NumericUpDown numericUpDownY = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = 1,
                Maximum = 10000,
                Value = 600,
                DecimalPlaces = 0,
                Name = "Y"
            };
            System.Windows.Forms.CheckBox cb = new System.Windows.Forms.CheckBox
            {
                Height = 20,
                Width = 392,
                Checked = false,
                Text = "useBilinear"
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(numericUpDownX);
            panel.Controls.Add(numericUpDownY);
            panel.Controls.Add(cb);
            panel.Width = Math.Max(numericUpDownX.Width, cb.Width);
            panel.Height = 16 + numericUpDownX.Height + numericUpDownY.Height + cb.Height + (int)(numericUpDownX.Margin.Bottom + numericUpDownY.Margin.Bottom + cb.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                resizeValues[0] = (int)numericUpDownX.Value;
                resizeValues[1] = (int)numericUpDownY.Value;
                resizeIfBilinear = cb.Checked;
                videoFilter = "resize";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Color RGB Values";
            System.Windows.Forms.TrackBar trackBarR = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = -255,
                Maximum = 255,
                Value = 0,
                Name = "R"
            };
            System.Windows.Forms.TrackBar trackBarG = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = -255,
                Maximum = 255,
                Value = 0,
                Name = "G"
            };
            System.Windows.Forms.TrackBar trackBarB = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = -255,
                Maximum = 255,
                Value = 0,
                Name = "B"
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(trackBarR);
            panel.Controls.Add(trackBarG);
            panel.Controls.Add(trackBarB);
            panel.Width = trackBarR.Width;
            panel.Height = trackBarB.Height + trackBarG.Height + trackBarB.Height + (int)(trackBarR.Margin.Bottom + trackBarG.Margin.Bottom + trackBarB.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                colorRGB[0] = trackBarR.Value;
                colorRGB[1] = trackBarG.Value;
                colorRGB[2] = trackBarB.Value;
                videoFilter = "color";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void smoothToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Smooth";
            System.Windows.Forms.NumericUpDown numericUpDown = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = -10,
                Maximum = 20,
                Value = 1,
                DecimalPlaces = 0,
                Name = "Weight"
            };
            popup.SetInputControl(numericUpDown);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                smoothWeight = (int)numericUpDown.Value;
                videoFilter = "smooth";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void gaussianBlurToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Gaussian Blur";
            System.Windows.Forms.NumericUpDown numericUpDown = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = -10,
                Maximum = 255,
                Value = 4,
                DecimalPlaces = 0,
                Name = "Weight"
            };
            popup.SetInputControl(numericUpDown);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                gaussianBlurWeight = (int)numericUpDown.Value;
                videoFilter = "gaussianBlur";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void meanRemovalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Mean Removal";
            System.Windows.Forms.NumericUpDown numericUpDown = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = -10,
                Maximum = 20,
                Value = 9,
                DecimalPlaces = 0,
                Name = "Weight"
            };
            popup.SetInputControl(numericUpDown);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                meanRemovalWeight = (int)numericUpDown.Value;
                videoFilter = "meanRemoval";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void sharpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Sharpen Weight";
            System.Windows.Forms.NumericUpDown numericUpDown = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = -10,
                Maximum = 20,
                Value = 11,
                DecimalPlaces = 0
            };
            popup.SetInputControl(numericUpDown);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                smoothWeight = (int)numericUpDown.Value;
                videoFilter = "sharpen";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void embossLaplacianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            videoFilter = "embossLaplacian";
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void edgeDetectQuickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            videoFilter = "edgeDetectQuick";
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void edgeDetectConvolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Edge Detect Convolution Type and Threshold";
            System.Windows.Forms.RadioButton radio1 = new System.Windows.Forms.RadioButton
            {
                Height = 20,
                Width = 392,
                Checked = true,
                Text = "EDGE_DETECT_KIRSH"
            };
            System.Windows.Forms.RadioButton radio2 = new System.Windows.Forms.RadioButton
            {
                Height = 20,
                Width = 392,
                Text = "EDGE_DETECT_PREWITT"
            };
            System.Windows.Forms.RadioButton radio3 = new System.Windows.Forms.RadioButton
            {
                Height = 20,
                Width = 392,
                Text = "EDGE_DETECT_SOBEL"
            };
            System.Windows.Forms.TrackBar trackBar = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = 0,
                Maximum = 255,
                Value = 0,
                Text = "Threshold"
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(radio1);
            panel.Controls.Add(radio2);
            panel.Controls.Add(radio3);
            panel.Controls.Add(trackBar);
            panel.Width = Math.Max(radio1.Width, trackBar.Width);
            panel.Height = 16 + radio1.Height + radio2.Height + radio3.Height + trackBar.Height + (int)(radio1.Margin.Bottom + radio2.Margin.Bottom + radio3.Margin.Bottom + trackBar.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                if (radio1.Checked)
                    edgeDetectConvolutionType = 1;
                else if (radio2.Checked)
                    edgeDetectConvolutionType = 2;
                else if (radio3.Checked)
                    edgeDetectConvolutionType = 3;
                edgeDetectConvolusionThreshold = (byte)trackBar.Value;
                videoFilter = "edgeDetectConvolution";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void horizontalToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            videoFilter = "edgeDetectH";
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void verticalToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            videoFilter = "edgeDetectV";
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void homogenityToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Edge Detect Homogenity Threshold";
            System.Windows.Forms.TrackBar trackBar = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = 0,
                Maximum = 255,
                Value = 0,
                Text = "Threshold"
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(trackBar);
            panel.Width = trackBar.Width;
            panel.Height = trackBar.Height + (int)(trackBar.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                edgeEnhanceThreshold = (byte)trackBar.Value;
                videoFilter = "edgeDetectHomogenity";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void differenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Edge Detect Difference Threshold";
            System.Windows.Forms.TrackBar trackBar = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = 0,
                Maximum = 255,
                Value = 0,
                Text = "Threshold"
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(trackBar);
            panel.Width = trackBar.Width;
            panel.Height = trackBar.Height + (int)(trackBar.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                edgeEnhanceThreshold = (byte)trackBar.Value;
                videoFilter = "edgeDetectDifference";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void edgeEnhanceToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Edge Enhance Threshold";
            System.Windows.Forms.TrackBar trackBar = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = 0,
                Maximum = 255,
                Value = 0,
                Text = "Threshold"
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(trackBar);
            panel.Width = trackBar.Width;
            panel.Height = trackBar.Height + (int)(trackBar.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                edgeEnhanceThreshold = (byte)trackBar.Value;
                videoFilter = "edgeEnhance";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void randomJitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Random Jitter Degree Value";
            System.Windows.Forms.NumericUpDown numericUpDown = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = 1,
                Maximum = 100,
                Value = 1,
                DecimalPlaces = 0
            };
            popup.SetInputControl(numericUpDown);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                jitterDegree = (short)numericUpDown.Value;
                videoFilter = "randomJitter";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void swirlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Swirl Degree Value 0.001-10";
            System.Windows.Forms.NumericUpDown numericUpDownDegree = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = 0.001M,
                Maximum = 10,
                Value = 0.05M,
                Increment = 0.001M,
                DecimalPlaces = 3
            };
            System.Windows.Forms.CheckBox cbSmoothing = new System.Windows.Forms.CheckBox
            {
                Height = 45,
                Width = 392,
                Checked = false,
                Text = "Smoothing"
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(numericUpDownDegree);
            panel.Controls.Add(cbSmoothing);
            panel.Width = numericUpDownDegree.Width;
            panel.Height = numericUpDownDegree.Height + cbSmoothing.Height + (int)(numericUpDownDegree.Margin.Bottom + cbSmoothing.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                swirlDegree = (double)numericUpDownDegree.Value;
                swirlSmoothing = cbSmoothing.Checked;
                videoFilter = "swirl";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void sphereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Sphere";
            System.Windows.Forms.CheckBox cbSmoothing = new System.Windows.Forms.CheckBox
            {
                Height = 45,
                Width = 392,
                Checked = false,
                Text = "Smoothing"
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(cbSmoothing);
            panel.Width = cbSmoothing.Width;
            panel.Height = cbSmoothing.Height + (int)(cbSmoothing.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                sphereSmoothing = cbSmoothing.Checked;
                videoFilter = "sphere";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void timeWarpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Time Warp Factor";
            System.Windows.Forms.NumericUpDown numericUpDownDegree = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = 1,
                Maximum = 20,
                Value = 1,
                DecimalPlaces = 0
            };
            System.Windows.Forms.CheckBox cbSmoothing = new System.Windows.Forms.CheckBox
            {
                Height = 45,
                Width = 392,
                Checked = false,
                Text = "Smoothing"
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(numericUpDownDegree);
            panel.Controls.Add(cbSmoothing);
            panel.Width = numericUpDownDegree.Width;
            panel.Height = numericUpDownDegree.Height + cbSmoothing.Height + (int)(numericUpDownDegree.Margin.Bottom + cbSmoothing.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                timeWarpFactor = (byte)numericUpDownDegree.Value;
                timeWarpSmoothing = cbSmoothing.Checked;
                videoFilter = "timeWarp";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void moireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Moire Degree";
            System.Windows.Forms.NumericUpDown numericUpDownDegree = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = -1000,
                Maximum = 1000,
                Value = 0.00001M,
                Increment = 0.00001M,
                DecimalPlaces = 5
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(numericUpDownDegree);
            panel.Width = numericUpDownDegree.Width;
            panel.Height = numericUpDownDegree.Height + (int)(numericUpDownDegree.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                moireDegree = (double)numericUpDownDegree.Value;
                videoFilter = "moire";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void waterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Water waves";
            System.Windows.Forms.NumericUpDown numericUpDownDegree = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = 0,
                Maximum = 100,
                Value = 1,
                Increment = 5,
                DecimalPlaces = 0
            };
            System.Windows.Forms.CheckBox cbSmoothing = new System.Windows.Forms.CheckBox
            {
                Height = 45,
                Width = 392,
                Checked = false,
                Text = "Smoothing"
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(numericUpDownDegree);
            panel.Controls.Add(cbSmoothing);
            panel.Width = numericUpDownDegree.Width;
            panel.Height = numericUpDownDegree.Height + cbSmoothing.Height + (int)(numericUpDownDegree.Margin.Bottom + cbSmoothing.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                waterWave = (short)numericUpDownDegree.Value;
                waterSmoothing = cbSmoothing.Checked;
                videoFilter = "water";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void pixelateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Pixelate Pixelation";
            System.Windows.Forms.NumericUpDown numericUpDownDegree = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = 1,
                Maximum = 100,
                Value = 1,
                DecimalPlaces = 0
            };
            System.Windows.Forms.CheckBox cbSmoothing = new System.Windows.Forms.CheckBox
            {
                Height = 45,
                Width = 392,
                Checked = false,
                Text = "Grid"
            };
            System.Windows.Forms.FlowLayoutPanel panel = new System.Windows.Forms.FlowLayoutPanel();
            panel.Controls.Add(numericUpDownDegree);
            panel.Controls.Add(cbSmoothing);
            panel.Width = numericUpDownDegree.Width;
            panel.Height = numericUpDownDegree.Height + cbSmoothing.Height + (int)(numericUpDownDegree.Margin.Bottom + cbSmoothing.Margin.Bottom);

            popup.SetInputControl(panel);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                pixelatePixel = (byte)numericUpDownDegree.Value;
                pixelateGrid = cbSmoothing.Checked;
                videoFilter = "pixelate";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
        }

        private void tweakValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Gaussian Blur";
            System.Windows.Forms.NumericUpDown gbNUD = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = -10,
                Maximum = 20,
                Value = 4,
                DecimalPlaces = 0,
                Text = "Gaussian Blur Weight"
            };
            System.Windows.Forms.NumericUpDown sNUD = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = -10,
                Maximum = 20,
                Value = 1,
                DecimalPlaces = 0,
                Text = "Smooth Weight"
            };
            System.Windows.Forms.TrackBar eetNUD = new System.Windows.Forms.TrackBar
            {
                Height = 45,
                Width = 392,
                Minimum = 0,
                Maximum = 255,
                Value = 0,
                Text = "Threshold"
            };
            popup.SetInputControl(gbNUD);
            popup.SetInputControl(sNUD);
            popup.SetInputControl(eetNUD);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                ccGaussianBlurWeight = (int)gbNUD.Value;
                ccSmoothWeight = (int)sNUD.Value;
                ccEdgeEnhanceThreshold = (byte)eetNUD.Value;
                videoFilter = "coinDetect";
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
        }

        private void applyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = (Bitmap)loaded.Clone();
            Bitmap bm = (Bitmap)processed.Clone();
            
            if (debug && useSteps)
            {
                switch (steps)
                {
                    case 0:
                        BitmapFilter.GrayScale(bm);
                        steps = 1;
                        break;
                    case 1:
                        //BitmapFilter.GaussianBlur(bm, 4);
                        //BitmapFilter.Smooth(bm, 1);
                        GaussianBlur blurFilter = new GaussianBlur(ccGaussianBlurWeight, 7);
                        bm = blurFilter.Apply(bm);
                        steps = 2;
                        break;
                    case 2:
                        //BasicDIP.BinaryThresholding(bm, ref bm, 200);
                        BitmapFilter.EdgeDetectConvolution(bm, BitmapFilter.EDGE_DETECT_SOBEL, (byte)10);
                        steps = 3;
                        break;
                    case 3:
                        List<Rectangle> ellipses = new List<Rectangle>();
                        bm = findCoin(bm, loaded, ref ellipses, fcThres);
                        steps = 4;
                        break;
                    default:
                        steps = 0;
                        return;
                }
                processed = bm;
                pictureBox3.Image = processed;

                loaded = (Bitmap)processed.Clone();
                pictureBox1.Image = loaded;
            }
            else
            {
                List<Rectangle> ellipses = new List<Rectangle>();

                BitmapFilter.GrayScale(bm);
                GaussianBlur blurFilter = new GaussianBlur(ccGaussianBlurWeight, 7);
                bm = (Bitmap)blurFilter.Apply(bm).Clone();
                BitmapFilter.EdgeDetectConvolution(bm, BitmapFilter.EDGE_DETECT_SOBEL, (byte)10);
                BasicDIP.BinaryThresholding(bm, ref bm, btThres);

                bm = (Bitmap)findCoin(bm, loaded, ref ellipses, fcThres).Clone();
                textBox1.Text += $"Object Count: {ellipses.Count}" + Environment.NewLine;
                int index = 1;
                int diaSum = 0;

                int[] coinCount = new int[5];

                foreach (Rectangle ell in ellipses)
                {
                    int xy = (ell.Width + ell.Height)/2;
                    //Console.WriteLine($"({index}), Pos: {ell.X}, {ell.Y}, Dia: {Math.Max(ell.Width, ell.Height)}");
                    //index++;
                    if (xy < coinDiaThres[0])
                        coinCount[0]++;
                    else if (xy < coinDiaThres[1])
                        coinCount[1]++;
                    else if (xy < coinDiaThres[2])
                        coinCount[2]++;
                    else if (xy < coinDiaThres[3])
                        coinCount[3]++;
                    else
                        coinCount[4]++;
                    diaSum += Math.Max(ell.Width, ell.Height);
                }
                Double money = (coinCount[0] * .05) + (coinCount[1] * .10) + (coinCount[2] * .25) + (coinCount[3] * 1) + (coinCount[4] * 5);
                textBox1.Text += $"There are: " + Environment.NewLine + 
                    $"[{coinCount[0]}] 5 cent coins, " + Environment.NewLine +
                    $"[{coinCount[1]}] 10 cent coins, " + Environment.NewLine +
                    $"[{coinCount[2]}] 25 cent coins, " + Environment.NewLine +
                    $"[{coinCount[3]}] 1 peso coins, " + Environment.NewLine +
                    $"[{coinCount[4]}] 5 peso coins" + Environment.NewLine +
                    $"For a total of : {money} pesos";


                processed = bm;
                pictureBox3.Image = processed;
                pictureBox1.Image = loaded;
            }

        }

        private void applyAForgeFiltersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            processed = (Bitmap)loaded.Clone();
            Bitmap bm = (Bitmap)processed.Clone();

            List<Rectangle> ellipses = new List<Rectangle>();

            BitmapFilter.GrayScale(bm);
            GaussianBlur blurFilter = new GaussianBlur(ccGaussianBlurWeight, 7);
            bm = (Bitmap)blurFilter.Apply(bm).Clone();
            BitmapFilter.EdgeDetectConvolution(bm, BitmapFilter.EDGE_DETECT_SOBEL, (byte)10);

            bm = (Bitmap)findCoin(bm, loaded, ref ellipses, fcThres).Clone();
            textBox1.Text += $"Ellipse Count: {ellipses.Count}";
            int index = 1;
            int diaSum = 0;
            foreach (Rectangle ell in ellipses)
            {
                int xy = ell.Width * ell.Height;
                //Console.WriteLine($"({index}), Pos: {ell.X}, {ell.Y}, Dia: {Math.Max(ell.Width, ell.Height)}");
                //index++;
                Console.WriteLine($"xy");
                diaSum += Math.Max(ell.Width, ell.Height);
            }
            Console.WriteLine($"Average Coin size: {diaSum / ellipses.Count}");

            processed = bm;
            pictureBox3.Image = processed;
            pictureBox1.Image = loaded;
        }

        // From sample image
        //White Circles:
        //[18, 15][25, 22] 8 * 8,
        //[24, 43][26, 45] 3 * 3
        //[33, 28][36, 31] 4 * 4,
        //[42, 22][42, 22] 1 * 1,
        //[52, 9][56, 13] 5 * 5,
        //[53, 24][62, 33] 12 * 12,
        //Grey Circles:
        //[34, 11][35, 12] 2 * 2,
        //[68, 20][70, 22] 3 * 3,
        //[26, 34][26, 34] 1 * 1,
        //[11, 39][15, 43] 5 * 5,
        //[46, 47][] 4 * 4,
        //Dark Grey Circles:
        //[22, 6][26, 10] 5 * 5,
        //[34, 42][37, 45] 4 * 4
        //Dark Circles:
        //[64, 8][67, 11] 4 * 4,
        //[12, 26][15, 29] 4 * 4,
        //[42, 30][47, 35] 6 * 6,
        //[58, 43][60, 45] 3 * 3,
        //[66, 48][66, 48] 1 * 1

        // original coin image - total : 64
            //5 cent : 7
            //10 cent : 11
            //25 cent : 28
            //1 peso : 13
            //5 peso : 5


        private Bitmap findCoin(Bitmap bm, Bitmap lbm, ref List<Rectangle> rectList, int threshold)
        {
            bool[,] visited = new bool[bm.Width, bm.Height];
            //u, d, l, r
            int[] dx = new int[] { 0, 0, -1, 1 };
            int[] dy = new int[] { 1, -1, 0, 0 };


            Bitmap newbm = (Bitmap)lbm.Clone();
            using (Graphics g = Graphics.FromImage(newbm))
            {
                // list of rectangles encapsulating coins
                rectList = new List<Rectangle>();
                // traverse image
                for (int x = 1; x < bm.Width-5; x++)
                {
                    for (int y = 1; y < bm.Height-5; y++)
                    {
                        Color pixel = bm.GetPixel(x, y);

                        // check if white enough
                        if (pixel.R >= threshold && pixel.R <= 255 && !visited[x, y])
                        {
                            if (debug)
                            {
                                Console.WriteLine($"coin pixel found in {x}, {y}");
                            }
                            List<Point> connectedPixels = new List<Point>();
                            BFS(x, y, connectedPixels);

                            // get rect edges
                            int minX = x, maxX = x;
                            int minY = y, maxY = y;
                            foreach (Point p in connectedPixels)
                            {
                                minX = Math.Min(minX, p.X);
                                minY = Math.Min(minY, p.Y);
                                maxX = Math.Max(maxX, p.X);
                                maxY = Math.Max(maxY, p.Y);
                            }

                            if (connectedPixels.Count > 0)
                            {
                                if (maxX - minX < sizeThres && maxY - minY < sizeThres)
                                {
                                    continue;
                                }
                                Rectangle rect = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
                                if (debug)
                                {
                                    Console.WriteLine($"Rectangle in [{minX},{minY}], [{maxX},{maxY}]\n");
                                }
                                rectList.Add(rect);
                                for (int rx = rect.Left; rx <= rect.Right; rx++)
                                {
                                    for (int ry = rect.Top; ry <= rect.Bottom; ry++)
                                    {
                                        visited[rx, ry] = true;
                                    }
                                }
                                using (Pen pen = new Pen(shapeOutlineColor, circleThickness))
                                {
                                    //g.FillEllipse(brush, rect); 
                                    g.DrawEllipse(pen, rect);
                                }
                            }
                        }
                    }
                }
            }
            
            bool InBounds(int x, int y)
            {
                return x >= 1 && x < bm.Width-5 && y >= 1 && y < bm.Height-5;
            }

            // BFS
            void BFS(int startX, int startY, List<Point> connectedPixels)
            {
                Queue<Point> queue = new Queue<Point>();
                queue.Enqueue(new Point(startX, startY));
                visited[startX, startY] = true;

                while (queue.Count > 0)
                {
                    Point p = queue.Dequeue();
                    connectedPixels.Add(p);

                    for (int i = 0; i < 4; i++)
                    {
                        int newX = p.X + dx[i];
                        int newY = p.Y + dy[i];

                        // if in bounds, is not visited and is within thresholds
                        if (InBounds(newX, newY) && !visited[newX, newY])
                        {
                            Color currPix = bm.GetPixel(newX, newY);
                            if (currPix.R >= threshold && currPix.R <= 255)
                            {
                                visited[newX, newY] = true;
                                queue.Enqueue(new Point(newX, newY));
                            }
                        }
                    }
                }
            }
            return newbm;
        }

        private void setFrameskipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PopupForm popup = new PopupForm();
            popup.PopupTitle = "Frameskip";
            System.Windows.Forms.NumericUpDown numericUpDown = new System.Windows.Forms.NumericUpDown
            {
                Height = 45,
                Width = 392,
                Minimum = 1,
                Maximum = 60,
                Value = 1,
                DecimalPlaces = 0
            };
            popup.SetInputControl(numericUpDown);
            if (popup.ShowDialog() == DialogResult.OK)
            {
                frameskip = (int)numericUpDown.Value;
            }
            popup.Update();
            textBox1.Text += popup.ifPanelNull();
            if (!isVideoOn)
            {
                applyFilter(processed, videoFilter);
            }
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

        
    }
}
