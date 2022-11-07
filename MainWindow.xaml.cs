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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Set_file_path_Click(object sender, RoutedEventArgs e)
        {
            string fileName = file_input.Text;
            int width, height;
            byte[] byteImage = GetCharTable(fileName, out width, out height);
            
            //BitmapImage image = LoadImage(data);
            BitmapSource bitmapSource = BitmapSource.Create(width, height, 10, 10, PixelFormats.Indexed8, BitmapPalettes.Gray256, byteImage, width);
            Image.Source = bitmapSource;
        }

        private byte[] GetCharTable(string fileName, out int width, out int height)
        {
            int iBreak = 0;
            width = 0;
            height = 0;
            int widthMark = 0;
            int heightMark = 0;
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
                        iBreak = i;
                        break;
                    }
                    else if (BrokenUp.Length == 1 && width == 0)
                    {
                        width = Int32.Parse(BrokenUp[0]);
                    }
                    else if (BrokenUp.Length == 1 && width != 0)
                    {
                        height = Int32.Parse(BrokenUp[0]);
                        iBreak = i;
                        break;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            if(fileType == "" || width == 0 || height == 0)
                throw new NotImplementedException();

            int[,] table = new int[width, height];

            for (int i = iBreak + 1; i < StringArray.Length; i++)
            {
                if (fileType == "P1")
                {
                    foreach (char c in StringArray[i])
                    {
                        if (c == '#')
                            break;
                        if (c == '1' || c == '0')
                            table[widthMark++, heightMark] = c - 48;
                        if(widthMark == width)
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

            if (fileType == "P1")
            {

            }

            return data;
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }
}
