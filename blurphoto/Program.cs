using System;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;

namespace blurphoto
{
    class Program
    {
        static void Main(string[] args)
        {
            // getting image from file system for using filter
            Bitmap image = new Bitmap(Image.FromFile("/home/dima/Project/PhotoProg/blurphoto/blurphoto/test.jpg"));
            
            // getting r and k parameters for create coefficient
            Console.Write("Enter r value: ");
            int r = int.Parse(Console.ReadLine());
            Console.Write("Enter k value: ");
            int k = int.Parse(Console.ReadLine());
            
            // initializing coefficient`s denominator
            int coef = (2 * r + 1) * (2 * k + 1);
            
            // initializing average RGB values for pixel(0,0)
            double avgR = 0;
            double avgG = 0;
            double avgB = 0;

            Bitmap newImage = new Bitmap(image.Width, image.Height);
            var sw = Stopwatch.StartNew();
            // cycle passing on all original image
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    // cycle passing on blur matrix 
                    for (int i = -k; i <= k; i++)
                    {
                        for (int j = -r; j <= r; j++)
                        {
                            // temp values for i and j parameters for mirroring 
                            int m = i;
                            int n = j;
                            
                            // calculation mirroring 
                            if (y+i < 0 || y+i >= image.Height)
                            {
                                m = -2 * i + i;
                            }
                            if (x+j < 0 || x+j >= image.Width)
                            {
                                n = -2 * j + j;
                            }
                            
                            // compute new color for pixel(x,y)
                            var pixel = image.GetPixel(x + n, y + m);
                            avgR += (double) pixel.R / coef;
                            avgG += (double) pixel.G / coef;
                            avgB += (double) pixel.B / coef;

                        }
                    }
                    // create new pixel in new image file
                    newImage.SetPixel(x,y,Color.FromArgb((int)avgR,(int)avgG,(int)avgB));
                    avgR = 0;
                    avgG = 0;
                    avgB = 0;
                }
                Console.WriteLine(sw.Elapsed.TotalMinutes);
            }
            sw.Stop();
            Console.WriteLine("Complete! Time: " + sw.Elapsed.TotalMinutes);
            
            // saving for save new and old image
            newImage.Save("/home/dima/Project/PhotoProg/blurphoto/blurphoto/blur-test.jpg",ImageFormat.Jpeg);

        }

        public void CalculateBlur(Bitmap image, int startX, int endX, ref Bitmap newImage)
        {
            
        }
    }
}