using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageToText
{
    public partial class Form1 : Form
    {
        Bitmap loadedImage;
        int maxQuantizationLevel;
        int currentQuantizationLevel = 16;
        string[] char_text;
        string text;

        public Form1()
        {
            InitializeComponent();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            CalculateMaxQuantizationLevel();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Image Files(*.BMP;*.JPG;*.PNG)|*.BMP;*.JPG;*.PNG|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (openFileDialog.FileName != "")
                    {
                        loadedImage = new Bitmap(openFileDialog.FileName);

                        pictureBox1.Image = loadedImage;
                        button2.Enabled = true;
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
            int ii = loadedImage.Width;
            int jj = loadedImage.Height;

            BitmapData img_data = loadedImage.LockBits(new Rectangle(0, 0, ii, jj), ImageLockMode.ReadOnly, loadedImage.PixelFormat);
            int bytes_per_pixel = Bitmap.GetPixelFormatSize(loadedImage.PixelFormat) / 8;
            bool horizontal = (comboBox2.SelectedIndex == 0);
            int pixel_value = comboBox1.SelectedIndex;
            int size = img_data.Height * img_data.Width;
            char_text = new string[size];

            unsafe
            {
                byte* ptr = (byte * )img_data.Scan0;
                byte* current_byte;

                Parallel.For(0, size, (k) =>
                {
                    if (horizontal)
                        current_byte = ptr + k * bytes_per_pixel;
                    else current_byte = ptr + (k % jj) * ii + (k % ii);
                    char_text[k] = GetPixel(current_byte[0], current_byte[1], current_byte[2], pixel_value);
                });
            }

            loadedImage.UnlockBits(img_data);

            text = String.Join(" ", char_text);
            button3.Enabled = true;
            button4.Enabled = true;
        }

        private void button2222_Click(object sender, EventArgs e)
        {

            int ii = loadedImage.Width;
            int jj = loadedImage.Height;

            int sum = ii * jj;
            char_text = new string[sum];
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
            int quantized_value = (int)Math.Floor(pixel_value / currentQuantizationLevel);
            return quantized_value.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            TextForm text_form = new TextForm(text);
            text_form.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Text|*.txt";
                dialog.Title = "Save an Text File";
                dialog.ShowDialog();
                if (dialog.FileName != "" )
                {
                    System.IO.StreamWriter file = new System.IO.StreamWriter(dialog.FileName);
                    file.WriteLine(text);

                    file.Close();
                }
            }
        }
    }
}
