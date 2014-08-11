using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;

namespace pngToRaw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String ImageSourceFileName;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private void ImageDrop(object sender, DragEventArgs e)
        {
            String infoMsg = ImageDrop_Sub(e);
            if (infoMsg != null)
            {
                TextBox.Text = infoMsg;
            }
        }

        // sanity check
        private string ImageDrop_Sub(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return "Not a file!";

            String[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 1)
                return "Too many files!";

            ImageSourceFileName = files[0];

            if (!File.Exists(ImageSourceFileName))
                return "Not a file!";

            FileStream fs = null;
            try
            {
                fs = File.Open(ImageSourceFileName, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                if (fs != null)
                    fs.Close();
                return "File already in use!";
            }

            Bitmap bitmap = null;
            try
            {
                bitmap = new Bitmap(fs);
            }
            catch (System.Exception /*ex*/)
            {
                bitmap.Dispose();
                return "Not a supported image format!";
            }

            try
            {
                BitmapData bitmapData = bitmap.LockBits(
                    Rectangle.FromLTRB(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);

                int dataByteSize = bitmap.Height * Math.Abs(bitmapData.Stride);
                byte[] data = new byte[dataByteSize];

                if (ComboBoxOrder.Text == ComboBoxItemRGBA.Content.ToString())
                {
                    Marshal.Copy(bitmapData.Scan0, data, 0, dataByteSize);
                }
                else
                {
                    byte[] rgbaData = new byte[dataByteSize];
                    Marshal.Copy(bitmapData.Scan0, rgbaData, 0, dataByteSize);
                    for (int i = 0; i < dataByteSize/4; ++i )
                    {
                        byte r = rgbaData[4 * i + 0];
                        byte g = rgbaData[4 * i + 1];
                        byte b = rgbaData[4 * i + 2];
                        byte a = rgbaData[4 * i + 3];

                        data[4 * i + 0] = b;
                        data[4 * i + 1] = g;
                        data[4 * i + 2] = r;
                        data[4 * i + 3] = a;
                    }
                }

                String fileName = 
                      Path.GetDirectoryName(ImageSourceFileName) + "\\" 
                    + Path.GetFileNameWithoutExtension(ImageSourceFileName) + ".dat";

                File.WriteAllBytes(fileName, data);
            }
            catch (System.Exception /*ex*/)
            {
                return "Error while exporting data!";
            }

            return "Data converted";
        }
    }
   
}
