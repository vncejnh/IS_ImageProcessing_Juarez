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

namespace IS_ImageProcessing_Juarez
{

    public partial class Form1 : Form
    {
        Bitmap imageA, imageB, colorgreen, resultImage;
        Device[] devices;
        Device device;
        public Form1()
        {

            InitializeComponent();
            picBox1.SizeMode = PictureBoxSizeMode.Zoom;
            picBox2.SizeMode = PictureBoxSizeMode.Zoom;
            picBox3.SizeMode = PictureBoxSizeMode.Zoom;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imageB = new Bitmap(ofd.FileName);
                picBox2.Image = imageB;
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (imageA == null || imageB == null) return;

            int width = Math.Min(imageB.Width, imageA.Width);
            int height = Math.Min(imageB.Height, imageB.Height);
            resultImage = new Bitmap(width, height);

            Color colorgreen = imageA.GetPixel(50, imageA.Height / 2); 
            int greygreen = (colorgreen.R + colorgreen.G + colorgreen.B) / 3;
            int threshold = 5;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color pixel = imageA.GetPixel(x, y);
                    Color backpixel = imageB.GetPixel(x, y);
                    int grey = (pixel.R + pixel.G + pixel.B) / 3;
                    int subtractvalue = Math.Abs(grey - greygreen);

                    if (subtractvalue <= threshold)
                        resultImage.SetPixel(x, y, backpixel);
                    else
                        resultImage.SetPixel(x, y, pixel);
                }
            }

            picBox3.Image = resultImage;
        }

        private void basicCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap result = new Bitmap(imageA.Width, imageA.Height);
            for (int y = 0; y < imageA.Height; y++)
            {
                for (int x = 0; x < imageA.Width; x++)
                {
                    Color pixelColor = imageA.GetPixel(x, y);
                    result.SetPixel(x, y, pixelColor);
                }
            }
            picBox3.Image = result;

            //if (imageA == null) return;
            //picBox3.Image = picBox1.Image;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (picBox3.Image == null)
            {
                MessageBox.Show("No image to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Save Image";
                sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                    if (sfd.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    else if (sfd.FileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                        format = System.Drawing.Imaging.ImageFormat.Bmp;

                    picBox3.Image.Save(sfd.FileName, format);

                    MessageBox.Show("Image saved successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void greyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            Bitmap resultImage = new Bitmap(imageA.Width, imageA.Height);

            for (int y = 0; y < imageA.Height; y++)
            {
                for (int x = 0; x < imageA.Width; x++)
                {
                    Color pixel = imageA.GetPixel(x, y);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    resultImage.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
            picBox3.Image = resultImage;
            picBox3.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bitmap resultImage = new Bitmap(imageA.Width, imageA.Height);

            for (int y = 0; y < imageA.Height; y++)
            {
                for (int x = 0; x < imageA.Width; x++)
                {
                    Color pixel = imageA.GetPixel(x, y);
                    int invertR = 255 - pixel.R;
                    int invertG = 255 - pixel.G;
                    int invertB = 255 - pixel.B;
                    resultImage.SetPixel(x, y, Color.FromArgb(invertR, invertG, invertB));
                }
            }
            picBox3.Image = resultImage;
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            int[] histogram = new int[256];

            for (int y = 0; y < imageA.Height; y++)
            {
                for (int x = 0; x < imageA.Width; x++)
                {
                    Color pixel = imageA.GetPixel(x, y);

                    int gray = (pixel.R + pixel.G + pixel.B) / 3;

                    histogram[gray]++;
                }
            }

            int histWidth = picBox3.Width;
            int histHeight = picBox3.Height;

            Bitmap histImage = new Bitmap(histWidth, histHeight);

            using (Graphics g = Graphics.FromImage(histImage))
            {
                g.Clear(Color.White);

                int max = histogram.Max();
                double scale = (double)histHeight / max;

                for (int i = 0; i < histogram.Length; i++)
                {
                    int barHeight = (int)(histogram[i] * scale);

                    g.DrawLine(Pens.BlueViolet,
                               new Point(i, histHeight - 1),
                               new Point(i, histHeight - 1 - barHeight));
                }
            }

            picBox3.Image = histImage;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            Bitmap resultImage = new Bitmap(imageA.Width, imageA.Height);
            for (int y = 0; y < imageA.Height; y++)
            {
                for (int x = 0; x < imageA.Width; x++)
                {
                    Color pixel = imageA.GetPixel(x, y);

                    int r = pixel.R;
                    int g = pixel.G;
                    int b = pixel.B;

                    int newR = (int)((r * 0.393) + (g * 0.769) + (b * 0.189));
                    int newG = (int)((r * 0.349) + (g * 0.686) + (b * 0.168));
                    int newB = (int)((r * 0.272) + (g * 0.534) + (b * 0.131));


                    newR = Math.Min(255, newR);
                    newG = Math.Min(255, newG);
                    newB = Math.Min(255, newB);

                    resultImage.SetPixel(x, y, Color.FromArgb(newR, newG, newB));
                }
            }

            picBox3.Image = resultImage;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void openWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            devices = DeviceManager.GetAllDevices();

            if (devices.Length > 0)
            {
                picBox1.CreateControl();
                device = devices[0];
                device.ShowWindow(picBox3);
            }
            else
            {
                MessageBox.Show("No webcam found!");
            }
        }

        private void closeWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (device != null)
            {
                device.Stop();
                device = null;
                picBox1.Image = null;
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void upload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK) { 
                imageA = new Bitmap(ofd.FileName);
                picBox1.Image = imageA;
                picBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }
    }
}
