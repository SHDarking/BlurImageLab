using System;
using System.Drawing;
using System.Threading;

namespace blurphoto
{
    public class BlurFilter
    {
        private readonly Bitmap image;
        private readonly int startX;
        private readonly int endX;
        private readonly int r;
        private readonly int k;
        private readonly int coef;
        private Bitmap newImage;

        public BlurFilter(Bitmap image, int startX, int endX, int r, int k, int coef, ref Bitmap newImage)
        {
            this.image = image;
            this.startX = startX;
            this.endX = endX;
            this.r = r;
            this.k = k;
            this.coef = coef;
            this.newImage = newImage;
        }
        
        public void CalculateBlur()
        {
            // initializing average RGB values for pixel(0,0)
            double avgR = 0;
            double avgG = 0;
            double avgB = 0;
            
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = startX; x < endX; x++)
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

            Console.WriteLine(Thread.CurrentThread.Name + " complete!");
        }
    }
}