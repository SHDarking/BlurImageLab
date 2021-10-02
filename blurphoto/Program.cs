using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Threading.Tasks;

namespace blurphoto
{
    class Program
    {
        static void Main(string[] args)
        {
            // getting path to resources directory
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            
            // getting image from file system for using filter
            Console.Write("Enter name of original image file with format: ");
            string imageName = Console.ReadLine();
            Image originalImage = Image.FromFile($"{path}/Resources/InputData/{imageName}");
            Bitmap image = new Bitmap(originalImage);
            
            
            // getting r and k parameters for create coefficient
            Console.Write("Enter r value: ");
            int r = int.Parse(Console.ReadLine());
            Console.Write("Enter k value: ");
            int k = int.Parse(Console.ReadLine());
            Console.Write("Enter number of flows needed: ");
            int flows = int.Parse(Console.ReadLine());
            
            // initializing coefficient`s denominator
            int coef = (2 * r + 1) * (2 * k + 1);

            int stepX = image.Width / flows;
            int startX = 0;
            int endX = 0;
            
            Bitmap newImage = new Bitmap(image.Width, image.Height);
            Dictionary<string, int>[] endpointsFlows = new Dictionary<string, int>[flows];
            Thread[] threads = new Thread[flows];
            var sw = Stopwatch.StartNew();
             
            // cycle passing on all original image

            for (int i = 0; i < flows; i++)
            {
                startX = endX;
                if (endX + 2 * stepX > image.Width)
                {
                    endX = image.Width;
                }
                else
                {
                    endX += stepX;
                }

                endpointsFlows[i] = new Dictionary<string, int> {{"startX", startX}, {"endX", endX}, {"Index", i}};
            }

            foreach (var item in endpointsFlows)
            {
                BlurFilter filter = new BlurFilter(image, item["startX"], item["endX"], r, k, coef,ref newImage);
                threads[item["Index"]] = new Thread(filter.CalculateBlur) {Name = $"Thread {item["Index"]}"};
                threads[item["Index"]].Start();
            }

            foreach (var item in threads)
            {
                item.Join();
            }
            sw.Stop();
            Console.WriteLine("Complete! Time: " + sw.Elapsed.TotalSeconds);
            Console.Write("Enter name for new image and it format: ");
            string newImageName = Console.ReadLine();
            // saving for save new and old image
            newImage.Save($"{path}/Resources/OutputData/{newImageName}",originalImage.RawFormat);
        }
    
        /*public Task CalculateBlur(Bitmap image, int startX, int endX, int r, int k,int coef,ref Bitmap newImage)
        {
            // initializing average RGB values for pixel(0,0)
            double avgR = 0;
            double avgG = 0;
            double avgB = 0;
            
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
            }
            return Task.CompletedTask;
        }*/
    }
}