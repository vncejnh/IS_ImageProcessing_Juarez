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

namespace IS_ImageProcessing_Juarez
{

    public partial class Form1 : Form
    {
        Bitmap imageA, imageB, colorgreen, resultImage;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private enum FilterType { None, Greyscale, Sepia, Invert }
        private FilterType currentFilter = FilterType.None;
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
            currentFilter = FilterType.None;

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
            currentFilter = FilterType.Greyscale;
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
            currentFilter = FilterType.Invert;
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
            currentFilter = FilterType.Sepia;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void openWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get all cameras
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("No webcam found!");
                return;
            }

            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += new NewFrameEventHandler(VideoSource_NewFrame);
            videoSource.Start();

            this.FormClosing += (s, ev) =>
            {
                if (videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource.WaitForStop();
                }
            };
        }

        private void closeWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.SignalToStop();
            videoSource.WaitForStop();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void upload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imageA = new Bitmap(ofd.FileName);
                picBox1.Image = imageA;
                picBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
            picBox1.Image = frame;
        }

        //MATRIX
        private Bitmap ApplyConvolutionFilter(Bitmap source, double[,] kernel, double factor = 1.0, double offset = 0.0)
        {
            int width = source.Width;
            int height = source.Height;
            Bitmap result = new Bitmap(width, height);

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double r = 0.0, g = 0.0, b = 0.0;

                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            Color pixel = source.GetPixel(x + kx, y + ky);
                            double kernelValue = kernel[ky + 1, kx + 1];

                            r += pixel.R * kernelValue;
                            g += pixel.G * kernelValue;
                            b += pixel.B * kernelValue;
                        }
                    }

                    int nr = Math.Min(Math.Max((int)(factor * r + offset), 0), 255);
                    int ng = Math.Min(Math.Max((int)(factor * g + offset), 0), 255);
                    int nb = Math.Min(Math.Max((int)(factor * b + offset), 0), 255);

                    result.SetPixel(x, y, Color.FromArgb(nr, ng, nb));
                }
            }
            return result;
        }

        private readonly double[,] SmoothKernel = {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

        private readonly double[,] GaussianBlurKernel = {
            { 1, 2, 1 },
            { 2, 4, 2 },
            { 1, 2, 1 }
        };

        private readonly double[,] SharpenKernel = {
            {  0, -2,  0 },
            { -2, 11, -2 },
            {  0, -2,  0 }
        };

        private readonly double[,] MeanRemovalKernel = {
            { -1, -1, -1 },
            { -1,  9, -1 },
            { -1, -1, -1 }
        };

     
        private readonly double[,] EmbossKernel = {
            {  -1,  0,  -1 },
            {   0,  4,   0 },
            {  -1,  0, -11 }
        };

        
        private readonly double[,] EmbossHVKernel = {
            {  0, -1,  0 },
            { -1,  4, -1 },
            {  0, -1,  0 }
        };

       
        private readonly double[,] EmbossAllKernel = {
            { -1, -1, -1 },
            { -1,  8, -1 },
            { -1, -1, -1 }
        };

        
        private readonly double[,] EmbossLossyKernel = {
            {  1, -2,  1 },
            { -2,  4, -2 },
            { -2,  1, -2 }
        };

       
        private readonly double[,] EmbossHorizontalKernel = {
            {  0,  0,  0 },
            { -1,  2, -1 },
            {  0,  0,  0 }
        };

       
        private readonly double[,] EmbossVerticalKernel = {
            {  0, -1,  0 },
            {  0,  0,  0 },
            {  0,  1,  0 }
        };


        private void smoothToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            resultImage = ApplyConvolutionFilter(imageA, SmoothKernel, 1.0 / 9.0, 0.0);
            picBox3.Image = resultImage;
        }

        private void gaussianBlurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            resultImage = ApplyConvolutionFilter(imageA, GaussianBlurKernel, 1.0 / 16.0, 0.0);
            picBox3.Image = resultImage;
        }

        private void sharpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            resultImage = ApplyConvolutionFilter(imageA, SharpenKernel, 1.0 / 3.0, 0.0);
            picBox3.Image = resultImage;
        }

        private void meanRemovalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            resultImage = ApplyConvolutionFilter(imageA, MeanRemovalKernel, 1.0, 0.0);
            picBox3.Image = resultImage;
        }

        private void embossToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void allDirectionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            resultImage = ApplyConvolutionFilter(imageA, EmbossAllKernel, 1.0, 128.0);
            picBox3.Image = resultImage;
        }

        private void laplascianToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            resultImage = ApplyConvolutionFilter(imageA, EmbossKernel, 1.0, 128.0);
            picBox3.Image = resultImage;
        }

        private void horizontalVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            resultImage = ApplyConvolutionFilter(imageA, EmbossHVKernel, 1.0, 128.0);
            picBox3.Image = resultImage;
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picBox1.Image = null;
            picBox2.Image = null;
            picBox3.Image = null;
        }

        private void lossyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            resultImage = ApplyConvolutionFilter(imageA, EmbossLossyKernel, 1.0, 128.0);
            picBox3.Image = resultImage;
        }

        private void horizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            resultImage = ApplyConvolutionFilter(imageA, EmbossHorizontalKernel, 1.0, 128.0);
            picBox3.Image = resultImage;
        }

        private void verticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imageA == null) return;
            resultImage = ApplyConvolutionFilter(imageA, EmbossVerticalKernel, 1.0, 128.0);
            picBox3.Image = resultImage;
        }

    }
}
