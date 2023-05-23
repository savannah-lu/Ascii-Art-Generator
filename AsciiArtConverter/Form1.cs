using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text;



namespace AsciiArtConverter
{
    public partial class Form1 : Form
    {  
        public Form1()
        {
            InitializeComponent();
        }

        public void button1_Click(object sender, EventArgs e)
        {
            label3.Text = "";  //erase instructions text

            //attempt to select and display an image
            //using open-file dialog and try-catch block
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

                    pictureBox1.Image = new Bitmap(openFileDialog1.FileName);
                    
                    Bitmap b = new Bitmap(openFileDialog1.FileName);
  
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    
                }
            }
        }

        public void button2_Click(object sender, EventArgs e)
        {
            //add user validation to ensure user selects valid image and kernel size
            if(pictureBox1.Image == null)
            {
                MessageBox.Show("Please select an image!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if(numericUpDown1.Value == 0 || numericUpDown2.Value == 0)
            {
                MessageBox.Show("Please select a kernel size.");
            }
            else
            {
                Bitmap b = new Bitmap(pictureBox1.Image);

                if(b.Width > 1500 || b.Height > 1500) //if image is too large, resize it to 
                {                                    //reduce run time
                    if(b.Width >= 1000 && b.Height >= 1000)
                    {
                       b = ResizeImage(b, 1000, 1000);
                    }
                }

                //create new instance of class
                BitmapAscii bitmapAscii = new BitmapAscii((int)numericUpDown1.Value, (int)numericUpDown2.Value);

                string str = "";

                str = bitmapAscii.Asciitize(b); //send bitmap to turn to ascii art

                richTextBox1.Text = str; //display ascii art

            }

            /// <summary>
            /// Resize the image to the specified width and height.
            /// </summary>
            /// <param name="image">The image to resize.</param>
            /// <param name="width">The width to resize to.</param>
            /// <param name="height">The height to resize to.</param>
            /// <returns>The resized image.</returns>
            static Bitmap ResizeImage(Image image, int width, int height)
            {
                var destRect = new Rectangle(0, 0, width, height);
                var destImage = new Bitmap(width, height);

                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                return destImage;
            }
         
        }

       
    }


    public class BitmapAscii
    {
        //declare variables
        private int kernelW;
        private int kernelH;

        //import values that user selected
        public BitmapAscii(int kernelW, int kernelH)
        {
            this.kernelW = kernelW;
            this.kernelH = kernelH;
        }

        public string Asciitize(Bitmap b)
        {
            if (kernelW == 1 && kernelH == 1)
            {
                double pixColor = 0;
                string ascii = "";
                StringBuilder sb = new StringBuilder();
                Color c = new Color();

                //use nested loops to traverse through each pixel
                for (int i = 0; i < b.Height; i++)
                {
                    for (int j = 0; j < b.Width; j++)
                    {
                        c = b.GetPixel(j, i);             //get pixel                                   
                        pixColor = AveragePixel(c);      //get grayscale (normalized) value
                        ascii = GrayToString(pixColor); //get ascii char based on grayscale value

                        sb.Append(ascii);
                       
                    }

                    sb.Append("\r\n");
                    
                }

                return sb.ToString(); //return string containing ascii version of pic
            }
            else
            {
                //declare variables
                Color c = new Color();
                double pixColor = 0;
                string ascii = "";
                StringBuilder sb = new StringBuilder();
                List<Color> colors = new List<Color>();
                int w = 0;
                int h = 0;
                bool go = true;

               
             

                //while loop runs until we get to the last line
                while(go)  
                {                               

                    //loop through kernel area
                    for(int j = h; j < h + kernelH; j++)
                    {
                        for(int k = w; k < w + kernelW; k++)
                        {
                            if(w + kernelW < b.Width && h + kernelH < b.Height) //if we're not out of bounds
                            {
                               c = b.GetPixel(k, j); //get pixel at spot and add to list
                               colors.Add(c);
                            }
                           
                        }
                    }

                    //once we have collected all kernels
                    if (colors.Count == (kernelW * kernelH))
                    {
                        pixColor = AverageColor(colors);  //get normalized value from list of colors
                        ascii = GrayToString(pixColor);  //get ascii char
                        sb.Append(ascii);               //add to char to string and clear list
                        colors.Clear();
                    }

                    
                    if(w + kernelW < b.Width)  //if we haven't reached the end of the row
                    {
                        w += kernelW;
                    }
                    else
                    {
                        if(h + kernelH < b.Height) //if we haven't reached the bottom of the pic
                        {
                            h += kernelH;  //go down x rows, reset width
                            w = 0;
                            sb.Append("\r\n");
                        }
                        else
                        {
                            go = false; //end while loop
                        }
                    }
                }


                return sb.ToString(); //return string containing entire ascii pic
            }
        }
      

        //method that accepts three int values(presumably RBG values) and returns a grayscale value
        public double AveragePixel(int r, int b, int g)
        {
            double avg = (r + b + g) / 3;

            avg = avg / 255;

            return avg;
        }

        //OVERLOADED METHOD: accepts a Color value instead of three ints
        public double AveragePixel(Color c)
        {
            double avg = (c.R + c.G + c.B) / 3;

            avg = avg / 255;

            return avg;
        }

        //create method that accepts a List of color values and returns an average normalized value (double)
        public double AverageColor(List <Color> colors)
        {
            double avg = 0;
            double color = 0;

            //loop through list of colors and get each normalized value
            for(int i = 0; i < colors.Count; i++)
            {
                color = (colors[i].R + colors[i].G + colors[i].B) / 3;

                //store normalized value in var
                avg += color / 255;
            }

            //find average normalized value
            avg = avg / colors.Count;

            return avg;
        }

        // " .:-=+*#%@"

        //{ "#", "#", "@", "%", "=", "+", "*", ":", "-", ".", " " };

        //create method that accepts a normalized value
        //and returns a string containing an ascii character
        public string GrayToString(double d)
        {
            //round number down for optimal results 
            d = Math.Round(d, 2);
            string str = "";

            if(d >= 0.0 && d <= 0.10)
            {
                str = "@";
            }
            else if (d >= 0.11 && d <= 0.20)
            {
                str = "%";
            }
            else if (d >= 0.21 && d <= 0.30)
            {
                str = "#";
            }
            else if (d >= 0.31 && d <= 0.40)
            {
                str = "*";
            }
            else if (d >= 0.41 && d <= 0.50)
            {
                str = "+";
            }
            else if (d >= 0.51 && d <= 0.60)
            {
                str = "=";
            }
            else if (d >= 0.61 && d <= 0.70)
            {
                str = "-";
            }
            else if (d >= 0.71 && d <= 0.80)
            {
                str = ":";
            }
            else if (d >= 0.81 && d <= 0.90)
            {
                str = ".";
            }
            else if (d >= 0.91 && d <= 1)
            {
                str = " ";
            }
            else
            {
                str = "?";
            }

            return str;
        }
    }
}