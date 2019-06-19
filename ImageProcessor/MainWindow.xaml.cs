using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageProcessor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly Stack<Bitmap> _redoRecord = new Stack<Bitmap>();
        private readonly Stack<Bitmap> _images = new Stack<Bitmap>();

        private async Task ShowImage(Bitmap bitmap)
        {
            try
            {
                RenderLabel.Visibility = Visibility.Visible;
                BmpImg.Children.Clear();
                await Task.Delay(10);
                var guid = Guid.NewGuid().ToString().Replace("-", "");
                var fileName = Path.GetTempPath() + "/" + guid + ".bmp";
                File.WriteAllBytes(fileName, bitmap.GetBitmapFileData());
                var b = new System.Drawing.Bitmap(fileName);

                var img = new System.Windows.Controls.Image
                {
                    Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(b.GetHbitmap(), IntPtr.Zero,
                        Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()),
                    Visibility = Visibility.Visible
                };
                BmpImg.Children.Add(img);
                b.Dispose();
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                    /* ignored */
                }

                RenderLabel.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ButtonUndo_Click(object sender, RoutedEventArgs e)
        {
            if (_images.Count <= 1)
            {
                MessageBox.Show("不能再撤销了", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _redoRecord.Push(_images.Pop());
            try
            {
                await ShowImage(_images.Peek());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void ButtonRedo_Click(object sender, RoutedEventArgs e)
        {
            if (_redoRecord.Count <= 0)
            {
                MessageBox.Show("不能再重做了", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _images.Push(_redoRecord.Pop());
            try
            {
                await ShowImage(_images.Peek());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool ValidateImg()
        {
            if (_images.Count < 1)
            {
                MessageBox.Show("没有读取图像", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private async void ButtonThreshold_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek().Threshold(Convert.ToByte(Threshold.Value)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private void ButtonShowHist_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                var (R, G, B) = _images.Peek().CalcHist();
                var histWindowR = new Hist(R, new SolidColorBrush(Colors.Red));
                var histWindowG = new Hist(G, new SolidColorBrush(Colors.Green));
                var histWindowB = new Hist(B, new SolidColorBrush(Colors.Blue));
                histWindowR.Show();
                histWindowG.Show();
                histWindowB.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void ButtonEqualizeHist_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek().EqualizeHist());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private void ButtonInfo_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            var img = _images.Peek();
            var info = new StringBuilder();
            info.AppendLine($"文件大小：{img.FileHeader.bfSize} b");
            info.AppendLine($"数据偏移：0x{img.FileHeader.bfOffBits.ToString("x2")}");
            info.AppendLine($"图片位数：{img.InfoHeader.biBitCount}");
            info.AppendLine($"图片高度：{-img.InfoHeader.biHeight}");
            info.AppendLine($"图片宽度：{img.InfoHeader.biWidth}");
            info.AppendLine($"X 向 DPI：{img.InfoHeader.biXPelsPerMeter / 1000 * 25.4}");
            info.AppendLine($"Y 向 DPI：{img.InfoHeader.biYPelsPerMeter / 1000 * 25.4}");
            MessageBox.Show(info.ToString(), "图片信息", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            var dlg = new SaveFileDialog
            {
                DefaultExt = ".bmp",
                OverwritePrompt = true,
                Title = "保存图片",
                CheckPathExists = true,
                AddExtension = true,
                Filter = "Bitmap|*.bmp"
            };
            if (dlg.ShowDialog() ?? false)
            {
                try
                {
                    using (var fileStream = dlg.OpenFile())
                    {
                        var data = _images.Peek().GetBitmapFileData();
                        await fileStream.WriteAsync(data, 0, data.Length);
                    }
                    MessageBox.Show("保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void ButtonMedianFilter_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek().MedianFilter(3));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private async void ButtonAddSaltNoise_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek().AddSaltNoise(int.Parse(Salt.Text)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private async void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                var dlg = new OpenFileDialog
                {
                    DefaultExt = ".bmp",
                    Title = "打开图片",
                    CheckPathExists = true,
                    AddExtension = true,
                    Filter = "Bitmap|*.bmp",
                    CheckFileExists = true
                };
                if (dlg.ShowDialog() ?? false)
                {
                    var img = new Bitmap(dlg.FileName);
                    _images.Push(_images.Peek().Plus(img));
                    img.Dispose();
                    await ShowImage(_images.Peek());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void ButtonMinus_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                var dlg = new OpenFileDialog
                {
                    DefaultExt = ".bmp",
                    Title = "打开图片",
                    CheckPathExists = true,
                    AddExtension = true,
                    Filter = "Bitmap|*.bmp",
                    CheckFileExists = true
                };
                if (dlg.ShowDialog() ?? false)
                {
                    var img = new Bitmap(dlg.FileName);
                    _images.Push(_images.Peek().Minus(img));
                    img.Dispose();
                    await ShowImage(_images.Peek());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void ButtonEmph_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek().ApplyConvolution(new[] { new[] { 1, 2, 1 }, new[] { 0, 0, 0 }, new[] { -1, -2, -1 } }, 3));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }
        private async void ButtonErode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek().Ercode(new OpenCvSharp.Size(3, 3)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private async void ButtonDilate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek().Dilate(new OpenCvSharp.Size(3, 3)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private async void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek()
                    .Ercode(new OpenCvSharp.Size(3, 3))
                    .Dilate(new OpenCvSharp.Size(3, 3)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private async void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek()
                    .Dilate(new OpenCvSharp.Size(3, 3))
                    .Ercode(new OpenCvSharp.Size(3, 3)));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private async void ButtonDajin_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                var img = _images.Peek();
                var threshold = img.GetAutomaticThreshold();
                _images.Push(img.RGB2Gray().Threshold(threshold));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private async void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            _redoRecord.Clear();
            try
            {
                _images.Push(new Bitmap(FileName.Text));
                ImageHeight.Text = (-_images.Peek().InfoHeader.biHeight).ToString();
                ImageWidth.Text = _images.Peek().InfoHeader.biWidth.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private async void ButtonGray_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek().RGB2Gray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private async void ButtonDft_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek().Dft());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private async void ButtonResize_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek().Resize(new OpenCvSharp.Size(int.Parse(ImageWidth.Text), int.Parse(ImageHeight.Text))));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private async void ButtonClip_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                _images.Push(_images.Peek().Clip(new OpenCvSharp.Point(0, 0), new OpenCvSharp.Size(int.Parse(ImageWidth.Text), int.Parse(ImageHeight.Text))));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }

        private async void ButtonSharp_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateImg())
            {
                return;
            }

            _redoRecord.Clear();
            try
            {
                var tmp = _images.Peek()
                    .ApplyConvolution(new[] {new[] {1, 2, 1}, new[] {0, 0, 0}, new[] {-1, -2, -1}}, 3);
                _images.Push(tmp.Plus(_images.Peek(), 1));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await ShowImage(_images.Peek());
        }
    }

    public class Visibility2BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case Visibility v:
                    if (parameter is bool bp)
                    {
                        return bp ? v == Visibility.Hidden : v != Visibility.Hidden;
                    }
                    break;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case bool v:
                    if (parameter is bool bp)
                    {
                        return bp ? v ? Visibility.Hidden : Visibility.Visible : !v ? Visibility.Hidden : Visibility.Visible;
                    }
                    break;
            }
            return value;
        }
    }
}
