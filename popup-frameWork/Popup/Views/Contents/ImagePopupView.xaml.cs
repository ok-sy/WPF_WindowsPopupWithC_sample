using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Popup.Views.Contents
{
    public partial class ImagePopupView : UserControl
    {
        private readonly string _imagePath;
        private readonly string _linkUrl;

        /*
         * IMAGE 팝업은 이미지와 선택적인 클릭 링크만 사용한다.
         * 기존 제목/설명/이미지 크기 옵션은 표시하지 않는다.
         */
        public ImagePopupView(
            string imagePath,
            string linkUrl = "")
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                throw new ArgumentException(
                    "이미지 경로가 비어 있습니다.",
                    nameof(imagePath));
            }

            _imagePath = imagePath;
            _linkUrl = linkUrl?.Trim() ?? string.Empty;

            PopupImage.Cursor = string.IsNullOrWhiteSpace(_linkUrl)
                ? Cursors.Arrow
                : Cursors.Hand;

            Loaded += ImagePopupView_Loaded;
        }

        private void ImagePopupView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Loaded -= ImagePopupView_Loaded;
            LoadImage(_imagePath);
        }

        private void LoadImage(string imagePath)
        {
            try
            {
                PopupImage.Source = imagePath.StartsWith(
                    "data:image/",
                    StringComparison.OrdinalIgnoreCase)
                    ? LoadDataImage(imagePath)
                    : LoadUriImage(imagePath);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"이미지를 불러오지 못했습니다.\n{exception.Message}",
                    "이미지 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private static BitmapImage LoadUriImage(string imagePath)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmapImage.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        private static BitmapImage LoadDataImage(string dataImage)
        {
            int commaIndex = dataImage.IndexOf(',');
            if (commaIndex < 0 || commaIndex == dataImage.Length - 1)
            {
                throw new FormatException("Base64 이미지 형식이 올바르지 않습니다.");
            }

            byte[] bytes = Convert.FromBase64String(
                dataImage.Substring(commaIndex + 1));

            using MemoryStream stream = new MemoryStream(bytes);

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        private void PopupImage_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_linkUrl))
            {
                return;
            }

            if (!Uri.TryCreate(_linkUrl, UriKind.Absolute, out Uri? uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                MessageBox.Show(
                    "클릭 연결 URL은 http 또는 https 주소만 사용할 수 있습니다.",
                    "링크 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true
            });

            e.Handled = true;
        }
    }
}
