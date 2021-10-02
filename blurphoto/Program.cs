using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            BitmapData imageData = image.LockBits(rect,ImageLockMode.ReadWrite,image.PixelFormat);
            IntPtr scan0 = imageData.Scan0;
            int scanWidth = Math.Abs(imageData.Stride);
            int bytes = scanWidth * image.Height;
            image.UnlockBits(imageData);
            
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
            
            //Bitmap newImage = new Bitmap(image.Width, image.Height);
            
            Task<BlurFilter>[] tasks = new Task<BlurFilter>[flows];
            Dictionary<string, int>[] endpointsFlows = new Dictionary<string, int>[flows];
            // cycle creating flows for calculation image blur
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < flows; i++)
            {
                
                // division of the flow calculation area
                startX = endX;
                if (endX + 2 * stepX > image.Width) // reassign limits for images with indivisible width
                {
                    endX = image.Width;
                }
                else
                {
                    endX += stepX;
                }

                endpointsFlows[i] = new Dictionary<string, int>();
                endpointsFlows[i].Add("startX",startX);
                endpointsFlows[i].Add("endX",endX);
                endpointsFlows[i].Add("Index",i);
            }

            foreach (var item in endpointsFlows)
            {
                tasks[item["Index"]] = new Task<BlurFilter>(() => new BlurFilter(image, item["startX"], item["endX"], r, k, coef)
                    .CalculateBlur(scanWidth,new byte[scanWidth / flows * image.Height],stepX));
                tasks[item["Index"]].Start();
            }
            
            Task.WaitAll(tasks);
            byte[] rgbValues = new byte[bytes];
            for (int i = 0; i < flows; i++)
            {
                for (int x = tasks[i].Result.startX; x < tasks[i].Result.endX; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        int bitsOfPixel = scanWidth / image.Width;
                        int pointX = x * bitsOfPixel + y * scanWidth;
                        rgbValues[pointX] =
                            tasks[i].Result.rgbValue[bitsOfPixel * (x - tasks[i].Result.startX + stepX * y)];
                        rgbValues[pointX + 1] =
                            tasks[i].Result.rgbValue[bitsOfPixel * (x - tasks[i].Result.startX + stepX * y) + 1];
                        rgbValues[pointX + 2] =
                            tasks[i].Result.rgbValue[bitsOfPixel * (x - tasks[i].Result.startX + stepX * y) + 2];
                        rgbValues[pointX + 3] =
                            tasks[i].Result.rgbValue[bitsOfPixel * (x - tasks[i].Result.startX + stepX * y) + 3];
                        
                    }
                }
            }
            
            sw.Stop();
            Console.WriteLine("Complete! Time: " + sw.Elapsed.TotalMinutes);
            imageData = image.LockBits(rect, ImageLockMode.ReadWrite, image.PixelFormat);
            scan0 = imageData.Scan0;
            Marshal.Copy(rgbValues,0,scan0,bytes);
            image.UnlockBits(imageData);
            
            Console.Write("Enter name for new image and it format: ");
            string newImageName = Console.ReadLine();
            // saving for save new and old image
            image.Save($"{path}/Resources/OutputData/{newImageName}",originalImage.RawFormat);
        }
        
    }
}