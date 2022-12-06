using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

namespace PNM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[]? byteImageG;
        int width;
        int height;
        Bitmap? p3Bitmap;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Set_file_path_Click(object sender, RoutedEventArgs e)
        {
            string fileName = file_input.Text;
            //int width, height;
            byte[] byteImage = GetCharTable(fileName, out width, out height);
            if (p3Bitmap != null)
            {
                Image.Source = Bitmap2BitmapImage(p3Bitmap);
            }
            else
            {
                BitmapSource bitmapSource = BitmapSource.Create(width, height, 10, 10, PixelFormats.Indexed8, BitmapPalettes.Gray256, byteImage, width);
                Image.Source = bitmapSource;
                byteImageG = byteImage;
            }

        }

        private byte[] GetCharTable(string fileName, out int width, out int height)
        {
            int skipAmount = 0;
            int iBreak = 0;
            width = 0;
            height = 0;
            int widthMark = 0;
            int heightMark = 0;
            int maxValue = 0;
            string fileType = "";
            string input = File.ReadAllText(fileName).Replace("\r", "");
            string[] StringArray = input.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string binaryData = string.Empty;

            for (int i = 0; i < StringArray.Length; i++)
            {
                if (StringArray[i].StartsWith("#"))
                {
                    continue;
                }
                else if (fileType == "")
                {
                    string[] BrokenUp = StringArray[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    if (BrokenUp.Length == 1 || (BrokenUp.Length != 0 && BrokenUp[1].StartsWith('#')))
                    {
                        fileType = BrokenUp[0];
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (width == 0 || height == 0)
                {
                    string[] BrokenUp;
                    if (StringArray[i].Contains('#'))
                    {
                        BrokenUp = StringArray[i].Split('#')[0].Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        BrokenUp = StringArray[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    if (BrokenUp.Length == 2)
                    {
                        width = Int32.Parse(BrokenUp[0]);
                        height = Int32.Parse(BrokenUp[1]);
                        if (fileType == "P1")
                        {
                            iBreak = i;
                            break;
                        }
                    }
                    else if (BrokenUp.Length == 1 && width == 0)
                    {
                        width = Int32.Parse(BrokenUp[0]);
                    }
                    else if (BrokenUp.Length == 1 && width != 0)
                    {
                        height = Int32.Parse(BrokenUp[0]);
                        if (fileType == "P1")
                        {
                            iBreak = i;
                            break;
                        }
                    }
                    else if (BrokenUp.Length > 1)
                    {
                        if (width != 0)
                        {
                            height = Int32.Parse(BrokenUp[0]);
                            skipAmount++;
                            i--;
                            if (fileType == "P1")
                            {
                                iBreak = i;
                                break;
                            }
                        }
                        else
                        {
                            width = Int32.Parse(BrokenUp[0]);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (width != 0 || height != 0)
                {
                    if (fileType == "P4")
                    {
                        iBreak = i - 1;
                        break;
                    }

                    string[] BrokenUp;
                    if (StringArray[i].Contains('#'))
                    {
                        BrokenUp = StringArray[i].Split('#')[0].Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    else
                    {
                        BrokenUp = StringArray[i].Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    }
                    if (skipAmount > 0)
                    {
                        maxValue = Int32.Parse(BrokenUp[skipAmount++]);
                        iBreak = i;
                        break;
                    }
                    if (BrokenUp.Length == 1 || (BrokenUp.Length >= 2 && BrokenUp[1].StartsWith('#')))
                    {
                        maxValue = Int32.Parse(BrokenUp[0]);
                        iBreak = i;
                        break;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }

            if (fileType == "" || width == 0 || height == 0)
                throw new NotImplementedException();

            int[,] table = new int[width, height];

            if (fileType == "P1")
            {
                for (int i = iBreak + 1; i < StringArray.Length; i++)
                {
                    foreach (char c in StringArray[i])
                    {
                        if (c == '#')
                            break;
                        if (c == '1' || c == '0')
                            table[widthMark++, heightMark] = c - 48;
                        if (widthMark == width)
                        {
                            widthMark = 0;
                            heightMark++;
                        }
                        if (heightMark == height)
                            break;
                    }
                }
            }

            if (fileType == "P2")
            {
                for (int i = iBreak + 1; i < StringArray.Length; i++)
                {
                    string[] BrokenUp = StringArray[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in BrokenUp)
                    {
                        if (s.StartsWith('#'))
                            break;
                        table[widthMark++, heightMark] = Int32.Parse(s);
                        if (widthMark == width)
                        {
                            widthMark = 0;
                            heightMark++;
                        }
                        if (heightMark == height)
                            break;
                    }
                }
            }

            if (fileType == "P3")
            {
                int skipped = 0;
                int[,,] rgbImage = new int[width, height, 3];
                int rgbValue = 0;
                p3Bitmap = new Bitmap(width, height);
                for (int i = iBreak + 1; i < StringArray.Length; i++)
                {
                    string[] BrokenUp = StringArray[i].Split(new string[] { " ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in BrokenUp)
                    {
                        if (skipped != skipAmount)
                        {
                            skipped++;
                            continue;
                        }
                        if (s.StartsWith('#'))
                            break;
                        if (s.Contains('#'))
                        {
                            rgbImage[widthMark, heightMark, rgbValue++] = Int32.Parse(s.Split('#')[0]) * 255 / maxValue;
                        }
                        else
                        {
                            rgbImage[widthMark, heightMark, rgbValue++] = Int32.Parse(s) * 255 / maxValue;
                        }
                        if (rgbValue == 3)
                        {
                            rgbValue = 0;
                            widthMark++;
                        }
                        if (widthMark == width)
                        {
                            widthMark = 0;
                            heightMark++;
                        }
                        if (heightMark == height)
                            break;
                    }
                }
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        p3Bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(rgbImage[i, j, 0], rgbImage[i, j, 1], rgbImage[i, j, 2]));
                    }
                }
            }
            else
            {
                p3Bitmap = null;
            }

            if (fileType == "P4" || fileType == "P5")
            {
                for (int i = iBreak + 1; i < StringArray.Length; i++)
                {
                    binaryData += StringArray[i];
                }
            }

            byte[] data = new byte[table.GetLength(0) * table.GetLength(1)];

            if (fileType == "P1")
            {
                for (int i = 0; i < table.GetLength(1); i++)
                {
                    for (int j = 0; j < table.GetLength(0); j++)
                    {
                        if (table[j, i] == 1)
                            data[i * table.GetLength(0) + j] = 0;
                        else if (table[j, i] == 0)
                            data[i * table.GetLength(0) + j] = 255;
                        else
                            throw new Exception();
                    }
                }
            }

            if (fileType == "P2")
            {
                for (int i = 0; i < table.GetLength(1); i++)
                {
                    for (int j = 0; j < table.GetLength(0); j++)
                    {
                        data[i * table.GetLength(0) + j] = (byte)(table[j, i] * 255 / maxValue);
                    }
                }
            }

            if (fileType == "P4")
            {
                string c = binaryData;
                var binaryString = StringToBinary(c);

                int counter = 0;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (binaryString[counter] == '1')
                            data[counter] = 0;
                        else if (binaryString[counter] == '0')
                            data[counter] = 255;
                        else
                            throw new Exception();

                        counter++;
                    }
                }
            }

            if (fileType == "P5")
            {
                string c = binaryData;
                var binaryString = StringToBinary(c);

                List<string> strlist = SplitInParts(binaryString, 8).ToList();

                int counter = 0;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        var item = strlist[counter];
                        var number = Convert.ToInt32(item, 2);
                        data[counter] = (byte)(number * 255 / maxValue);

                        counter++;
                    }
                }
            }

            return data;
        }

        public string StringToBinary(string data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in data.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }

        public IEnumerable<string> SplitInParts(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        private async void import_p1_Click(object sender, RoutedEventArgs e)
        {
            string fileName = file_input.Text;
            if (byteImageG == null)
                throw new Exception();

            await WriteToFileP1(byteImageG, fileName, width, height);
        }

        private async void import_p2_Click(object sender, RoutedEventArgs e)
        {
            string fileName = file_input.Text;
            if (byteImageG == null)
                throw new Exception();

            await WriteToFileP2(byteImageG, fileName, width, height);
        }

        private async void import_p3_Click(object sender, RoutedEventArgs e)
        {
            string fileName = file_input.Text;
            if (p3Bitmap == null)
            {
                if (byteImageG == null)
                    throw new Exception();
                else
                    await WriteToFileP3FromByte(byteImageG, fileName, width, height);
            }
            else
                await WriteToFileP3(p3Bitmap, fileName, width, height);
        }

        public static async Task WriteToFileP1(byte[] bits, string filename, int width, int height)
        {
            List<string> text = new List<string>();
            text.Add("P1");
            text.Add(width.ToString() + " " + height.ToString());
            string allBits = "";
            int i = 0;
            foreach (byte b in bits)
            {
                allBits += (Math.Round((double)b / (double)255, 0, MidpointRounding.AwayFromZero) + 1) % 2 + " ";
                i++;
                if (i == width)
                {
                    text.Add(allBits);
                    i = 0;
                    allBits = "";
                }
            }
            text.Add(allBits);
            await File.WriteAllLinesAsync(filename, text);
        }

        public static async Task WriteToFileP2(byte[] bits, string filename, int width, int height)
        {
            List<string> text = new List<string>();
            text.Add("P2");
            text.Add(width.ToString() + " " + height.ToString());
            text.Add("255");
            string allBits = "";
            int i = 0;
            foreach (byte b in bits)
            {
                allBits += b + " ";
                i++;
                if (i == width)
                {
                    text.Add(allBits);
                    i = 0;
                    allBits = "";
                }
            }
            await File.WriteAllLinesAsync(filename, text);
        }

        public static async Task WriteToFileP3(Bitmap bitmap, string filename, int width, int height)
        {
            List<string> text = new List<string>
            {
                "P3",
                width.ToString() + " " + height.ToString(),
                "255"
            };
            string line;
            for (int x = 0; x < height; x++)
            {
                line = "";
                for (int y = 0; y < width; y++)
                {
                    line += bitmap.GetPixel(y, x).R + " " + bitmap.GetPixel(y, x).G + " " + bitmap.GetPixel(y, x).B + " ";
                }
                text.Add(line);
            }
            await File.WriteAllLinesAsync(filename, text);
        }

        public static async Task WriteToFileP3FromByte(byte[] bits, string filename, int width, int height)
        {
            List<string> text = new List<string>();
            text.Add("P3");
            text.Add(width.ToString() + " " + height.ToString());
            text.Add("255");
            string allBits = "";
            int i = 0;
            foreach (byte b in bits)
            {
                allBits += b + " " + b + " " + b + " ";
                i++;
                if (i == width)
                {
                    text.Add(allBits);
                    i = 0;
                    allBits = "";
                }
            }
            await File.WriteAllLinesAsync(filename, text);
        }

        public static BitmapSource Bitmap2BitmapImage(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgra32, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }
    }
}
