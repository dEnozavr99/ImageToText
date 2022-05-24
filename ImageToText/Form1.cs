﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ImageToText
{
    public partial class Form1 : Form
    {
        Bitmap loadedImage;
        List<Bitmap> loadedImageList = new List<Bitmap>();
        List<string> textList = new List<string>();
        List<string> textNames = new List<string>();

        int maxQuantizationLevel;
        int currentQuantizationLevel = 16;

        public enum Directions { Horizontal = 'H', Vertical = 'V' };
        public enum ColorScheme { Red = 'R', Green = 'G', Blue = 'B', Monochrome = 'M'};
        public struct Dimensions
        {
            public int Width { get; set; }
            public int Height { get; set; }
        };
        public struct FileNameParsedData
        {
            public string Name { get; set; }
            public Directions Direction { get; set; }
            public ColorScheme ColorScheme { get; set; }
            public Dimensions Dimensions { get; set; }

        };

        string text;

        bool multiple;

        public Form1()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            CalculateMaxQuantizationLevel();
        }
        private void loadImage(string filename)
        {
            loadedImage = new Bitmap(filename);

            pictureBox1.Image = loadedImage;
            button2.Enabled = true;
            label6.Text = Path.GetFileNameWithoutExtension(filename);
        }
        private FileNameParsedData parseFileName(string filename)
        {
            string pattern = @"(?<name>\w+)_(?<direction>H|V)_(?<color>[RGBM]{1})_(?<width>\d+)x{1}(?<height>\d+)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(filename);

            FileNameParsedData parsedData = new FileNameParsedData();
            parsedData.Name = match.Groups["name"].Value;
            parsedData.Direction = (Directions)Convert.ToChar(match.Groups["direction"].Value);
            parsedData.ColorScheme = (ColorScheme)Convert.ToChar(match.Groups["color"].Value);

            Dimensions parsedDimensions = new Dimensions();
            parsedDimensions.Width = Convert.ToInt32(match.Groups["width"].Value);
            parsedDimensions.Height = Convert.ToInt32(match.Groups["height"].Value);
            parsedData.Dimensions = parsedDimensions;

            return parsedData;
        }
        private void loadText(string filename)
        {
            // FileName_H/V_WxH.txt
            // FileName_H/V_R/G/B/M_WxH.txt
            string pattern = @"(?<name>\w+)_(?<direction>H|V)_(?<color>[RGBM]{1})_(?<width>\d+)x{1}(?<height>\d+)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(filename);

            FileNameParsedData fileNameData = parseFileName(filename);

            int[,] pixelValues = new int[fileNameData.Dimensions.Width, fileNameData.Dimensions.Height];
            string[] imageDataLines = File.ReadAllLines(filename);
            int[] pixelValuesFromFile = Array.ConvertAll(imageDataLines[0].Split(' '), str => Convert.ToInt32(str));

            int counter = 0;
            for (int i = 0; i < fileNameData.Dimensions.Width; i++)
            {
                for (int j = 0; j < fileNameData.Dimensions.Height; j++)
                {
                    pixelValues[i, j] = pixelValuesFromFile[counter++];
                }
            }

            Bitmap bitmap = new Bitmap(fileNameData.Dimensions.Width, fileNameData.Dimensions.Height);

            for (int x = 0; x < fileNameData.Dimensions.Width; x++)
            {
                for (int y = 0; y < fileNameData.Dimensions.Height; y++)
                {
                    Color color;
                    int value = 
                        (int)Math.Floor((double)(pixelValues[x, y] * maxQuantizationLevel / currentQuantizationLevel));

                    switch (fileNameData.ColorScheme)
                    {
                        case ColorScheme.Red:
                            color = Color.FromArgb(value, 0, 0);
                            break;
                        case ColorScheme.Green:
                            color = Color.FromArgb(0, value, 0);
                            break;
                        case ColorScheme.Blue:
                            color = Color.FromArgb(0, 0, value);
                            break;
                        case ColorScheme.Monochrome:
                            color = Color.FromArgb(value, value, value);
                            break;
                        default:
                            color = Color.FromArgb(value, value, value);
                            break;
                    }

                    bitmap.SetPixel(x, y, color);
                }
            }

            pictureBox1.Image = bitmap;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ClearData();
            multiple = false;
            string imageFilesFilter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
            string textFilesFilter = "Text Files(*.TXT)|*.TXT|ALL files (*.*)|*.*";
            bool isTextFile = false;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = isTextFile ? textFilesFilter : imageFilesFilter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (openFileDialog.FileName != "")
                    {
                        if (isTextFile)
                        {
                            loadText(openFileDialog.FileName);
                        }
                        else
                        {
                            loadImage(openFileDialog.FileName);
                        }
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateMaxQuantizationLevel();
            CalculateCurrentQuantizationLevel();
        }

        private void CalculateMaxQuantizationLevel()
        {
            if (comboBox1.SelectedIndex == 1)
                maxQuantizationLevel = 256;
            else
            {
                maxQuantizationLevel = 256;
            }
            label4.Text = "Max quantization level = " + maxQuantizationLevel.ToString();

        }

        private void CalculateCurrentQuantizationLevel()
        {
            int quantization_amount;
            if (int.TryParse(textBox1.Text, out quantization_amount))
            {
                currentQuantizationLevel = (int)Math.Round(Math.Pow(2, quantization_amount));
                if (currentQuantizationLevel > maxQuantizationLevel) currentQuantizationLevel = maxQuantizationLevel;
                label3.Text = "Current quantization level = " + currentQuantizationLevel.ToString();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            CalculateCurrentQuantizationLevel();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (multiple)
            {
                foreach (Bitmap image in loadedImageList)
                {
                    textList.Add(String.Join(" ", ReadImage(image)));
                }
            }
            else
            {
                text = String.Join(" ", ReadImage(loadedImage));
                button3.Enabled = true;
            }

            button4.Enabled = true;
        }

        string[] ReadImage(Bitmap loadedImage)
        {
            int ii = loadedImage.Width;
            int jj = loadedImage.Height;
            int size = ii * jj;
            var char_text = new string[size];
            int counter = 0;

            for (int i = 0; i < ii; i++)
            {
                for (int j = 0; j < jj; j++)
                {
                    Color color = loadedImage.GetPixel(i, j);
                    //MessageBox.Show(color.R.ToString() + " " + color.G.ToString() + " " + color.B.ToString());

                    char_text[counter++] = GetPixel(color.R, color.G, color.B, comboBox1.SelectedIndex);
                }
            }

            Console.WriteLine(counter.ToString());
            return char_text;
        }

        string[] ReadImage2(Bitmap loadedImage)
        {
            int ii = loadedImage.Width;
            int jj = loadedImage.Height;

            BitmapData img_data = loadedImage.LockBits(new Rectangle(0, 0, ii, jj), ImageLockMode.ReadOnly, loadedImage.PixelFormat);
            int bytes_per_pixel = Bitmap.GetPixelFormatSize(loadedImage.PixelFormat) / 8;
            bool horizontal = (comboBox2.SelectedIndex == 0);
            int pixel_value = comboBox1.SelectedIndex;
            int size = img_data.Height * img_data.Width;
            var char_text = new string[size];

            unsafe
            {
                byte* ptr = (byte*)img_data.Scan0;
                byte* current_byte;

                //Parallel.For(0, size, (k) =>
                for (int k = 0; k < size; k++)
                {
                    if (horizontal)
                        current_byte = ptr + k * bytes_per_pixel;
                    else current_byte = ptr + (k % jj) * ii + (k % ii);
                    MessageBox.Show("R: " + current_byte[0].ToString() + "G: " + current_byte[1].ToString() + "B :" + current_byte[2]);
                    char_text[k] = GetPixel(current_byte[0], current_byte[1], current_byte[2], pixel_value);
                };
            }

            loadedImage.UnlockBits(img_data);
            return char_text;
        }

        private void button2222_Click(object sender, EventArgs e)
        {

            int ii = loadedImage.Width;
            int jj = loadedImage.Height;

            int sum = ii * jj;
            string[] char_text = new string[sum];
            int i = 0, j = 0;

            bool horizontal = (comboBox2.SelectedIndex == 0);
            int pixel_value = comboBox1.SelectedIndex;

            Parallel.For(0, sum, (k) =>
            {
                if (horizontal)
                {
                    i = k % ii;
                    if (i == 0) j++;
                }
                else
                {
                    j = k % jj;
                    if (j == 0) i++;
                }
                //char_text[k] = GetPixel(i, j, loadedImage, pixel_value);
            });

            //if (comboBox2.SelectedIndex == 0)
            //{
            //    for (int i = 0; i < ii; i++)
            //    {
            //        for (int j = 0; j < jj; j++)
            //        {
            //            text += GetPixel(i,j);
            //        }
            //    }
            //}
            //else
            //{

            //    for (int j = 0; j < jj; j++) 
            //    {
            //        for (int i = 0; i < ii; i++)
            //        {
            //            text += GetPixel(i, j);
            //        }
            //    }
            //}
            button3.Enabled = true;
        }

        private string GetPixel(byte r, byte g, byte b, int value)
        {
            double pixel_value = 0d;
            switch(value)
            {
                case 0:
                    {
                        pixel_value = r;
                        break;
                    }
                case 1:
                    {
                        pixel_value = g;
                        break;
                    }
                case 2:
                    {
                        pixel_value = b;
                        break;
                    }
                case 3:
                    {
                        pixel_value = r * 0.2126d + g * 0.7152d + b * 0.0722d;
                        break;
                    }
            }
            int quantized_value = (int)Math.Floor(pixel_value * currentQuantizationLevel / maxQuantizationLevel);

            return quantized_value.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TextForm text_form = new TextForm(text);
            text_form.Show();
        }
        private string colorSchemeComboBoxParser(int activeIndex)
        {
            string colorSchema = "";

            switch (activeIndex)
            {
                case 0:
                    colorSchema = "R";
                    break;
                case 1:
                    colorSchema = "G";
                    break;
                case 2:
                    colorSchema = "B";
                    break;
                default:
                    colorSchema = "M";
                    break;
            }

            return colorSchema;
        }
        private string buildFileName(string filename)
        {
            // FileName_H/V_WxH.txt
            // FileName_H/V_R/G/B/M_WxH.txt
            bool isHorizontal = comboBox2.SelectedIndex == 0;
            string directionString = isHorizontal ? "H" : "V";
            string dimensionsString = isHorizontal 
                ? $"{loadedImage.Width}x{loadedImage.Height}" 
                : $"{loadedImage.Height}x{loadedImage.Width}";
            string colorSchema = colorSchemeComboBoxParser(comboBox1.SelectedIndex);

            return $"{filename}_{directionString}_{colorSchema}_{dimensionsString}.txt";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!multiple)
            {
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "Text|*.txt";
                    dialog.Title = "Save an Text File";
                    dialog.FileName = buildFileName(label6.Text);
                    dialog.ShowDialog();

                    if (dialog.FileName != "")
                    {
                        System.IO.StreamWriter file = new System.IO.StreamWriter(dialog.FileName);
                        file.WriteLine(text);

                        file.Close();
                    }
                }
            }
            else
            {
                using (FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog())
                {
                    int i = 0;
                    if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                    {
                        var folder = folderBrowserDialog1.SelectedPath;
                        foreach (string current_text in textList)
                        {
                            System.IO.StreamWriter file = new System.IO.StreamWriter(folder + "/" + textNames[i] + ".txt");
                            file.WriteLine(current_text);

                            file.Close();
                            i++;
                        }

                    }
                }
            }


        }

        private void button5_Click(object sender, EventArgs e)
        {
            ClearData();
            multiple = true;
            using (FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog())
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    var folder = folderBrowserDialog1.SelectedPath;

                    foreach (var file in Directory.GetFiles(folder))
                    {
                        textNames.Add(Path.GetFileNameWithoutExtension(file));
                        if (Path.GetExtension(file) == ".jpg" || Path.GetExtension(file) == ".bmp" || Path.GetExtension(file) == ".png")
                        {
                            loadedImageList.Add(new Bitmap(file));
                        }
                    }
                }
            }
            button2.Enabled = true;
            button3.Enabled = false;
        }

        void ClearData()
        {
            loadedImageList.Clear();
            loadedImage = null;
            pictureBox1.Image = null;
            text = "";
            textList.Clear();
            textNames.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ClearData();
            multiple = false;
            string imageFilesFilter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
            string textFilesFilter = "Text Files(*.TXT)|*.TXT|ALL files (*.*)|*.*";
            bool isTextFile = true;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = isTextFile ? textFilesFilter : imageFilesFilter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (openFileDialog.FileName != "")
                    {
                        if (isTextFile)
                        {
                            loadText(openFileDialog.FileName);
                        }
                        else
                        {
                            loadImage(openFileDialog.FileName);
                        }
                    }
                }
            }
        }
    }
}
