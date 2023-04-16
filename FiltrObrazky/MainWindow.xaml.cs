using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using Color = System.Drawing.Color;

namespace FiltrObrazky
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private List<Action> history = new List<Action>();

        private int blurAmount = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                myImage.Source = bitmap;
                myImage.Effect = null; // odstranění efektu při vkládání nového obrázku

                //načtení změn z historie
                history.Add(() =>
                {
                    myImage.Effect = null;
                });
            }
        }

        private void Button_Click_Rozmazat(object sender, RoutedEventArgs e)
        {
            //BlurEffect blurEffect = new BlurEffect();
            //blurEffect.Radius = 5;
            //myImage.Effect = blurEffect;

            blurAmount += 5;
            BlurEffect blurEffect = new BlurEffect();
            blurEffect.Radius = blurAmount;
            myImage.Effect = blurEffect;

            //načtení změn do historie
            history.Add(() =>
            {
                blurAmount -= 5;
                blurEffect.Radius = blurAmount;
                myImage.Effect = blurAmount > 0 ? blurEffect : null;
            });


        }

        private void Button_Click_Zpet(object sender, RoutedEventArgs e)
        {
            if (history.Count > 0)
            {
                int lastIndex = history.Count - 1;
                Action lastChange = history[lastIndex];
                history.RemoveAt(lastIndex);

                lastChange.Invoke();
            }
        }

        private void Button_Click_Cernobile(object sender, RoutedEventArgs e)
        {
            FormatConvertedBitmap grayBitmap = new FormatConvertedBitmap();
            grayBitmap.BeginInit();
            grayBitmap.DestinationFormat = PixelFormats.Gray8;
            grayBitmap.Source = (BitmapSource)myImage.Source;
            grayBitmap.EndInit();

            myImage.Source = grayBitmap;

            history.Add(() =>
            {
                myImage.Source = grayBitmap.Source;
            });

        }

        private void Button_Click_Zrcadlit(object sender, RoutedEventArgs e)
        {
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1, -1, myImage.ActualWidth / 2, myImage.ActualHeight / 2));
            myImage.RenderTransform = transformGroup;

            history.Add(() =>
            {
                myImage.RenderTransform = null;
            });

        }

        private void Button_Click_Negativ(object sender, RoutedEventArgs e)
        {
            {
                BitmapImage bitmapImage = myImage.Source as BitmapImage;

                if (bitmapImage == null)
                {
                    return;
                }

                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapImage);

                int width = writeableBitmap.PixelWidth;
                int height = writeableBitmap.PixelHeight;

                int[] pixels = new int[width * height];

                writeableBitmap.CopyPixels(pixels, width * 4, 0);

                for (int i = 0; i < pixels.Length; i++)
                {
                    int pixel = pixels[i];

                    byte a = (byte)(pixel >> 24);
                    byte r = (byte)(255 - (pixel >> 16 & 0xff));
                    byte g = (byte)(255 - (pixel >> 8 & 0xff));
                    byte b = (byte)(255 - (pixel & 0xff));

                    pixels[i] = (a << 24) | (r << 16) | (g << 8) | b;
                }

                BitmapSource bitmapSource = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);

                myImage.Source = bitmapSource;

                history.Add(() =>
                {
                    myImage.Source = bitmapImage;
                });

            }
        }

        private void Button_Click_Ulozit(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Image Files (*.png;*.jpeg;*.bmp)|*.png;*.jpeg;*.bmp|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)myImage.Source));
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        private void Button_Click_Odebrat(object sender, RoutedEventArgs e)
        {
            myImage.Source = null;
            myImage.Effect = null;
            myImage.RenderTransform = null;
            history.Clear();
        }

        private void Button_Click_Pruhy(object sender, RoutedEventArgs e)
        {
            BitmapImage bitmapImage = myImage.Source as BitmapImage;

            if (bitmapImage == null)
            {
                return;
            }

            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapImage);

            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;

            int[] pixels = new int[width * height];

            writeableBitmap.CopyPixels(pixels, width * 4, 0);

            for (int i = 0; i < pixels.Length; i++)
            {
                int pixel = pixels[i];

                byte a = (byte)(pixel >> 24);
                byte r = (byte)(pixel >> 16 & 0xff);
                byte g = (byte)(pixel >> 8 & 0xff);
                byte b = (byte)(pixel & 0xff);

                if (i % 2 == 0)
                {
                    pixels[i] = (a << 24) | (r << 16) | (g << 8) | b;
                }
                else
                {
                    pixels[i] = (a << 24) | (0 << 16) | (0 << 8) | 0;
                }
            }

            BitmapSource bitmapSource = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);

            myImage.Source = bitmapSource;

            history.Add(() =>
            {
                myImage.Source = bitmapImage;
            });
        }

        private void Button_Click_Kolecka(object sender, RoutedEventArgs e)
        {
            //pridani nahodnych barevnych kolecek na obrazek
            BitmapImage bitmapImage = myImage.Source as BitmapImage;
            

        }


        //private void Button_Click_Histogram(object sender, RoutedEventArgs e)
        //{
        //    BitmapImage bitmapImage = myImage.Source as BitmapImage;

        //    if (bitmapImage == null)
        //    {
        //        return;
        //    }

        //    WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapImage);

        //    int width = writeableBitmap.PixelWidth;
        //    int height = writeableBitmap.PixelHeight;

        //    int[] pixels = new int[width * height];

        //    writeableBitmap.CopyPixels(pixels, width * 4, 0);

        //    int[] red = new int[256];
        //    int[] green = new int[256];
        //    int[] blue = new int[256];

        //    for (int i = 0; i < pixels.Length; i++)
        //    {
        //        int pixel = pixels[i];

        //        byte r = (byte)(pixel >> 16 & 0xff);
        //        byte g = (byte)(pixel >> 8 & 0xff);
        //        byte b = (byte)(pixel & 0xff);

        //        red[r]++;
        //        green[g]++;
        //        blue[b]++;
        //    }

        //    int max = 0;
        //    for (int i = 0; i < 256; i++)
        //    {
        //        if (red[i] > max)
        //        {
        //            max = red[i];
        //        }
        //        if (green[i] > max)
        //        {
        //            max = green[i];
        //        }
        //        if (blue[i] > max)
        //        {
        //            max = blue[i];
        //        }
        //    }

        //    int[] redScaled = new int[256];
        //    int[] greenScaled = new int[256];
        //    int[] blueScaled = new int[256];

        //    for (int i = 0; i < 256; i++)
        //    {
        //        redScaled[i] = red[i] * 256 / max;
        //        greenScaled[i] = green[i] * 256 / max;
        //        blueScaled[i] = blue[i] * 256 / max;
        //    }

        //    int[] redScaled2 = new int[256];
        //    int[] greenScaled2 = new int[256];
        //    int[] blueScaled2 = new int[256];

        //    for (int i = 0; i < 256; i++)
        //    {
        //        redScaled2[i] = redScaled[i] * 256 / max;
        //        greenScaled2[i] = greenScaled[i] * 256 / max;
        //        blueScaled2[i] = blueScaled[i] * 256 / max;
        //    }

        //    int[] redScaled3 = new int[256];
        //    int[] greenScaled3 = new int[256];
        //    int[] blueScaled3 = new int[256];

        //    Histogram histogramWindow = new Histogram();
        //    HistogramControl histogramControl = new HistogramControl(redScaled3, greenScaled3, blueScaled3);
        //    histogramWindow.Content = histogramControl;
        //    histogramWindow.Title = "Histogram";
        //    histogramWindow.Width = 400;
        //    histogramWindow.Height = 300;

        //    Window histogram = new Window();
        //    histogramWindow.Content = histogramControl;
        //    histogramWindow.ShowDialog();
        //}
    }
}

//spirála, rybí oko, rozpixelování
