using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebCamLib;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Diagnostics;
using HNUDIP;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;
using System.Runtime.CompilerServices;
using ImageProcess2;

namespace ImageProcessing_2
{
    public partial class Form1 : Form
    {
        Bitmap loadImage;
        Bitmap imageB;
        Bitmap imageA;
        Bitmap colorgreen;
        Bitmap resultImage;

        bool isGray = false;
        bool isInvert = false;
        bool isSepia = false;

        Boolean camActive = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            loadImage = new Bitmap(openFileDialog1.FileName);
            pictureBox1.Image = loadImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            openFileDialog2.ShowDialog();
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            imageB = new Bitmap(openFileDialog2.FileName);
            pictureBox3B.Image = imageB;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            openFileDialog3.ShowDialog();
        }

        private void openFileDialog3_FileOk(object sender, CancelEventArgs e)
        {
            imageA = new Bitmap(openFileDialog3.FileName);
            pictureBox3A.Image = imageA;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            resultImage.Save(saveFileDialog1.FileName);
        }

        private void button2_Click(object sender, EventArgs e) //COPY
        {
            resultImage = new Bitmap(loadImage.Width, loadImage.Height);
            for (int i = 0; i < resultImage.Width; i++)
            {
                for (int j = 0; j < resultImage.Height; j++)
                {
                    Color pixel = loadImage.GetPixel(i, j);
                    resultImage.SetPixel(i, j, pixel);
                }
                pictureBox2.Image = resultImage;
            }
        }

        private void button3_Click(object sender, EventArgs e) //GRAYSCALE
        {
            resultImage = new Bitmap(loadImage.Width, loadImage.Height);
            for (int i = 0; i < resultImage.Width; i++)
            {
                for (int j = 0; j < resultImage.Height; j++)
                {
                    Color pixel = loadImage.GetPixel(i, j);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    resultImage.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
                pictureBox2.Image = resultImage;
            }
        }

        private void button4_Click(object sender, EventArgs e) //INVERSION
        {
            resultImage = new Bitmap(loadImage.Width, loadImage.Height);
            for (int i = 0; i < resultImage.Width; i++)
            {
                for (int j = 0; j < resultImage.Height; j++)
                {
                    Color pixel = loadImage.GetPixel(i, j);
                    resultImage.SetPixel(i, j, Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B));
                }
                pictureBox2.Image = resultImage;
            }
        }

        private void button5_Click(object sender, EventArgs e) //SEPIA
        {
            resultImage = new Bitmap(loadImage.Width, loadImage.Height);
            for (int i = 0; i < resultImage.Width; i++)
            {
                for (int j = 0; j < resultImage.Height; j++)
                {
                    Color pixel = loadImage.GetPixel(i, j);

                    int sepiaR = (int)(0.393 * pixel.R + 0.769 * pixel.G + 0.189 * pixel.B);
                    int sepiaG = (int)(0.349 * pixel.R + 0.686 * pixel.G + 0.168 * pixel.B);
                    int sepiaB = (int)(0.272 * pixel.R + 0.534 * pixel.G + 0.131 * pixel.B);

                    resultImage.SetPixel(i, j, Color.FromArgb(Math.Min(255, sepiaR), Math.Min(255, sepiaG), Math.Min(255, sepiaB)));
                }
            }
            pictureBox2.Image = resultImage;
        }

        private void button6_Click(object sender, EventArgs e) //HISTOGRAM
        {
            resultImage = new Bitmap(loadImage.Width, loadImage.Height);
            for (int i = 0; i < loadImage.Width; i++)
                for (int j = 0; j < loadImage.Height; j++)
                {
                    Color pixel = loadImage.GetPixel(i, j);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    resultImage.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            Color sample;
            int[] histData = new int[256];
            for (int i = 0; i < loadImage.Width; i++)
                for (int j = 0; j < loadImage.Height; j++)
                {
                    sample = resultImage.GetPixel(i, j);
                    histData[sample.R] = histData[sample.R] + 1;
                }
            Bitmap myData = new Bitmap(256, 800);
            for (int i = 0; i < 256; i++)
                for (int j = 0; j < 800; j++)
                {
                    myData.SetPixel(i, j, Color.White);
                }
            for (int i = 0; i < 256; i++)
                for (int j = 0; j < Math.Min(histData[i] / 5, 800); j++)
                {
                    myData.SetPixel(i, 799 - j, Color.Black);
                }
            pictureBox2.Image = myData;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to exit?", "Exit Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        // WEBCAM & SUBTRACTION
        private void button10_Click(object sender, EventArgs e) //IMAGE SUBTRACTION
        {
            Color mygreen = Color.FromArgb(0, 0, 255);
            int greygreen = (mygreen.R + mygreen.G + mygreen.B) / 3;
            resultImage = new Bitmap(imageA.Width, imageA.Height);
            int threshold = 5;
            for (int x = 0; x < imageA.Width; x++)
            {
                for (int y = 0; y < imageA.Height; y++)
                {
                    Color pixel = imageA.GetPixel(x, y);
                    Color backpixel = imageB.GetPixel(x, y);
                    int grey = (pixel.R + pixel.G + pixel.B) / 3;
                    int subtractvalue = Math.Abs(grey - greygreen);
                    if (subtractvalue > threshold)
                    {
                        resultImage.SetPixel(x, y, pixel);
                    }
                    else
                    {
                        resultImage.SetPixel(x, y, backpixel);
                    }
                }
            }
            pictureBox2.Image = resultImage;
        }

        Device[] devices = DeviceManager.GetAllDevices();
        Device webcam = DeviceManager.GetDevice(0);

        private void button11_Click(object sender, EventArgs e) //ON CAM
        {
            webcam.ShowWindow(pictureBox3);
            camActive = true;
        }

        private void button12_Click(object sender, EventArgs e) //OFF CAM
        {
            webcam.Stop();
            camActive = false;
        }

        private void button13_Click(object sender, EventArgs e) //SUBTRACTION VID
        {
            if (imageB == null) return;
            if (camActive)
            {
                timer1.Enabled = true;
                timer1.Start();
            }
            else
            {
                Color mygreen = Color.FromArgb(0, 0, 255);
                int greygreen = (mygreen.R + mygreen.G + mygreen.B) / 3;
                resultImage = new Bitmap(imageA.Width, imageA.Height);
                int threshold = 1;

                for (int x = 0; x < imageA.Width; x++)
                {
                    for (int y = 0; y < imageA.Height; y++)
                    {
                        Color pixel = imageA.GetPixel(x, y);
                        Color backpixel = imageB.GetPixel(x, y);
                        int grey = (pixel.R + pixel.G + pixel.B) / 3;

                        resultImage.SetPixel(x, y, Math.Abs(grey - greygreen) > threshold ? pixel : backpixel);
                    }
                }
                pictureBox4.Image = resultImage;
            }
        }

        private void timer1_Tick(object sender, EventArgs e) //SUBTRACTION VID
        {
            IDataObject data;
            Image bmap;
            devices[0].Sendmessage();
            data = Clipboard.GetDataObject();

            int threshold = 100;
            if (data != null)
            {
                bmap = (Image)(data.GetData("System.Drawing.Bitmap", true));
                if (bmap != null)
                {
                    Bitmap b = new Bitmap(bmap);
                    ImageProcess2.BitmapFilter.Subtract(b, imageB, Color.Green, threshold);
                    pictureBox4.Image = b;
                }
            }
        }

        private void button14_Click(object sender, EventArgs e) //GRAYSCALE VID
        {
            isGray = !isGray;
            timer1.Enabled = false;
            timer3.Enabled = false;
            timer2.Enabled = isGray;
            button2.Enabled = false;
        }

        private void timer2_Tick(object sender, EventArgs e) //GRAYSCALE VID
        {
            IDataObject data;
            Image bmap;
            devices[0].Sendmessage();
            data = Clipboard.GetDataObject();

            if (data != null)
            {
                bmap = (Image)(data.GetData("System.Drawing.Bitmap", true));
                if (bmap != null)
                {
                    Bitmap b = new Bitmap(bmap);
                    ImageProcess2.BitmapFilter.GrayScale(b);
                    pictureBox4.Image = b;
                }
            }
        }

        private void button15_Click(object sender, EventArgs e) //INVERSION VID
        {
            isInvert = !isInvert;
            timer1.Enabled = false;
            timer2.Enabled = false;
            timer3.Enabled = isInvert;
            button2.Enabled = false;
        }

        private void timer3_Tick(object sender, EventArgs e) //INVERSION VID
        {
            IDataObject data;
            Image bmap;
            devices[0].Sendmessage();
            data = Clipboard.GetDataObject();

            if (data != null)
            {
                bmap = (Image)(data.GetData("System.Drawing.Bitmap", true));
                if (bmap != null)
                {
                    Bitmap b = new Bitmap(bmap);
                    ImageProcess2.BitmapFilter.Invert(b);
                    pictureBox4.Image = b;
                }
            }
        }

        private void button16_Click(object sender, EventArgs e) //SEPIA VID
        {
            isSepia = !isSepia;
            timer1.Enabled = false;
            timer2.Enabled = false;
            timer3.Enabled = false;
            timer4.Enabled = isSepia;
            button2.Enabled = false;
        }

        private void timer4_Tick(object sender, EventArgs e) //SEPIA VID
        {
            IDataObject data;
            Image bmap;
            devices[0].Sendmessage();
            data = Clipboard.GetDataObject();

            if (data != null)
            {
                bmap = (Image)(data.GetData("System.Drawing.Bitmap", true));
                if (bmap != null)
                {
                    Bitmap b = new Bitmap(bmap);
                    ApplySepiaFilter(b);
                    pictureBox4.Image = b;
                }
            }
        }

        private void ApplySepiaFilter(Bitmap image) //SEPIA FUNCTION
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixel = image.GetPixel(x, y);

                    int r = pixel.R;
                    int g = pixel.G;
                    int b = pixel.B;
                    int grey = (r + g + b) / 3;

                    int newR = (int)Math.Min(0.393 * r + 0.769 * g + 0.189 * b, 255);
                    int newG = (int)Math.Min(0.349 * r + 0.686 * g + 0.168 * b, 255);
                    int newB = (int)Math.Min(0.272 * r + 0.534 * g + 0.131 * b, 255);

                    image.SetPixel(x, y, Color.FromArgb(pixel.A, newR, newG, newB));
                }
            }
        }
    }
}
