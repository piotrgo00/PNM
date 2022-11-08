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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Set_file_path_Click(object sender, RoutedEventArgs e)
        {
            string fileName = file_input.Text;
            //int width, height;
            byte[] byteImage = GetCharTable(fileName, out width, out height);

            BitmapSource bitmapSource = BitmapSource.Create(width, height, 10, 10, PixelFormats.Indexed8, BitmapPalettes.Gray256, byteImage, width);
            Image.Source = bitmapSource;

            byteImageG = byteImage;
        }

        private byte[] GetCharTable(string fileName, out int width, out int height)
        {
            int iBreak = 0;
            width = 0;
            height = 0;
            int widthMark = 0;
            int heightMark = 0;
            int maxValue = 0;
            string fileType = "";
            string input = File.ReadAllText(fileName).Replace("\r", "");
            string[] StringArray = input.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

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
                    string[] BrokenUp = StringArray[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (BrokenUp.Length == 2 || (BrokenUp.Length != 0 && BrokenUp[2].StartsWith('#')))
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
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (width != 0 || height != 0)
                {
                    string[] BrokenUp = StringArray[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
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

            return data;
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
    }
}
