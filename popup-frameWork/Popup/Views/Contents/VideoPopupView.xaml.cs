using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Popup.Views.Contents
{
    public partial class VideoPopupView : UserControl
    {
        /*
         * 전달받은 원본 영상 경로다.
         */
        private readonly string _videoPath;

        /*
         * 현재 영상이 YouTube 영상인지 나타낸다.
         */
        private readonly bool _useWebPlayer;

        /*
         * MediaElement가 영상을 정상적으로 열었는지 나타낸다.
         */
        private bool _isMediaOpened;

        /*
         * 영상 재생 위치와 UI를 동기화하는 타이머다.
         */
        private readonly DispatcherTimer _progressTimer;
        /*
         * 일정 시간 동안 마우스 움직임이 없으면
         * 영상 컨트롤바를 숨기는 타이머다.
         */
        private readonly DispatcherTimer _controlHideTimer;
        /*
         * 현재 영상이 재생 중인지 나타낸다.
         */
        private bool _isPlaying;

        /*
         * 사용자가 진행바를 조작 중인지 나타낸다.
         */
        private bool _isSeeking;

        /*
         * 사용자가 음량 Slider를
         * 드래그 중인지 나타낸다.
         */
        private bool _isChangingVolume;

        /*
         * 진행바를 드래그하기 전
         * 영상이 재생 중이었는지 저장한다.
         *
         * true
         * → 드래그 종료 후 다시 재생
         *
         * false
         * → 드래그 종료 후 일시정지 유지
         */
        private bool _wasPlayingBeforeSeeking;
        /*
         * 현재 음소거 상태인지 나타낸다.
         */
        private bool _isMuted;

        /*
         * 음소거 해제 시 복원할 이전 음량이다.
         */
        private double _volumeBeforeMute = 0.7;

        /*
 * 전체화면 영상을 표시하는 별도 Window다.
 */
        private Window? _fullScreenWindow;

        /*
         * 전체화면 전 VideoContainer가 들어 있던 부모다.
         */
        private Panel? _originalVideoParent;

        /*
         * 부모 컨테이너 내부의 원래 배치 순서다.
         */
        private int _originalVideoIndex;

        /*
         * VideoContainer의 원래 크기와 레이아웃 값이다.
         */
        private double _originalVideoWidth;
        private double _originalVideoHeight;

        private Thickness _originalVideoMargin;

        private HorizontalAlignment
            _originalHorizontalAlignment;

        private VerticalAlignment
            _originalVerticalAlignment;

        private CornerRadius
            _originalCornerRadius;

        private Thickness
            _originalBorderThickness;

        public VideoPopupView(
            string videoTitle,
            string videoPath,
            string videoDescription,
            bool showDescription = true)
        {
            InitializeComponent();

            _progressTimer =
            new DispatcherTimer
            {
                Interval =
                    TimeSpan.FromMilliseconds(250)
            };

            _progressTimer.Tick +=
                ProgressTimer_Tick;
            if (string.IsNullOrWhiteSpace(videoPath))
            {
                throw new ArgumentException(
                    "영상 경로는 필수입니다.",
                    nameof(videoPath));
            }

            _controlHideTimer =
            new DispatcherTimer
            {
                Interval =
                    TimeSpan.FromSeconds(2)
            };

            _controlHideTimer.Tick +=
                ControlHideTimer_Tick;

            _videoPath = videoPath.Trim();

            /*
             * 생성 시점에 YouTube 주소 여부를 판별한다.
             */
            _useWebPlayer =
                TryGetYouTubeVideoId(
                    _videoPath,
                    out _);

            TitleTextBlock.Text =
                videoTitle ?? string.Empty;

            DescriptionTextBlock.Text =
                videoDescription ?? string.Empty;

            DescriptionTextBlock.Visibility =
                showDescription
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            /*
             * UserControl이 실제 화면에 로드된 뒤
             * 영상 컨트롤 초기화를 시작한다.
             */
            Loaded += VideoPopupView_Loaded;
        }

        private async void VideoPopupView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            /*
             * Loaded 이벤트가 중복 실행되지 않도록 제거한다.
             */
            Loaded -= VideoPopupView_Loaded;

            if (_useWebPlayer)
            {
                await LoadYouTubeVideoAsync();
            }
            else
            {
                LoadMediaElementVideo();
            }
        }

        /*
         * 로컬 파일 또는 직접 영상 URL을
         * MediaElement로 재생한다.
         */
        private void LoadMediaElementVideo()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[VIDEO] Load 시작: {_videoPath}");

                _isMediaOpened = false;


                PopupVideo.Visibility = Visibility.Visible;
                VideoWebView.Visibility = Visibility.Collapsed;
                LocalVideoControlArea.Visibility = Visibility.Visible;

                
                ShowLoadingMessage("영상을 불러오는 중입니다.");

                if (Uri.TryCreate(
                        _videoPath,
                        UriKind.Absolute,
                        out Uri? absoluteUri)
                    && (absoluteUri.Scheme == Uri.UriSchemeHttp
                        || absoluteUri.Scheme == Uri.UriSchemeHttps))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[VIDEO] HTTP URL 인식: {absoluteUri}");

                    PopupVideo.Source = absoluteUri;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        System.Diagnostics.Debug.WriteLine(
                            "[VIDEO] Play 호출");

                        PopupVideo.Play();
                    }));

                    return;
                }

                string resolvedPath =
                    Path.IsPathRooted(_videoPath)
                        ? _videoPath
                        : Path.GetFullPath(
                            Path.Combine(
                                AppContext.BaseDirectory,
                                _videoPath));

                System.Diagnostics.Debug.WriteLine(
                    $"[VIDEO] 로컬 경로: {resolvedPath}");

                if (!File.Exists(resolvedPath))
                {
                    throw new FileNotFoundException(
                        "영상 파일을 찾을 수 없습니다.",
                        resolvedPath);
                }

                PopupVideo.Source =
                    new Uri(resolvedPath, UriKind.Absolute);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    PopupVideo.Play();
                }));
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[VIDEO] 예외: {exception}");

                ShowVideoError(
                    $"영상을 불러올 수 없습니다.\n{exception.Message}");
            }
        }

        /*
         * YouTube 주소를 WebView2에 임베드한다.
         */
        private async System.Threading.Tasks.Task LoadYouTubeVideoAsync()
        {
            try
            {
                if (!TryGetYouTubeVideoId(
                        _videoPath,
                        out string? videoId))
                {
                    throw new InvalidOperationException(
                        "올바른 YouTube 주소가 아닙니다.");
                }

                PopupVideo.Visibility =
                    Visibility.Collapsed;

                VideoWebView.Visibility =
                    Visibility.Visible;

                /*
                 * YouTube는 자체 플레이어 UI를 사용하므로
                 * MediaElement 전용 버튼을 숨긴다.
                 */
                LocalVideoControlArea.Visibility =
                    Visibility.Collapsed;


                ShowLoadingMessage(
                    "YouTube 영상을 불러오는 중입니다.");

                /*
                 * WebView2 초기화를 명시적으로 수행한다.
                 */
                await VideoWebView.EnsureCoreWebView2Async();

                /*
                 * 새 창 열기 동작을 현재 WebView 안에서 처리한다.
                 */
                VideoWebView.CoreWebView2.NewWindowRequested +=
                    CoreWebView2_NewWindowRequested;

                /*
                 * 기본 컨텍스트 메뉴와 개발자 도구를 제한한다.
                 * 필요하다면 이후 옵션으로 분리할 수 있다.
                 */
                VideoWebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled =
                    false;

                VideoWebView.CoreWebView2.Settings.AreDevToolsEnabled =
                    false;

                string embedUrl =
                    $"https://www.youtube.com/embed/{videoId}" +
                    "?autoplay=1" +
                    "&controls=1" +
                    "&rel=0" +
                    "&playsinline=1";

                VideoWebView.Source =
                    new Uri(embedUrl);
            }
            catch (Exception exception)
            {
                ShowVideoError(
                    $"YouTube 영상을 불러올 수 없습니다.\n{exception.Message}");
            }
        }

        /*
         * YouTube 공유 주소, 일반 주소, Shorts 주소,
         * embed 주소에서 영상 ID를 추출한다.
         */
        private static bool TryGetYouTubeVideoId(
            string url,
            out string? videoId)
        {
            videoId = null;

            if (!Uri.TryCreate(
                    url,
                    UriKind.Absolute,
                    out Uri? uri))
            {
                return false;
            }

            string host =
                uri.Host.ToLowerInvariant();

            /*
             * youtu.be/VIDEO_ID
             */
            if (host == "youtu.be"
                || host == "www.youtu.be")
            {
                videoId =
                    uri.AbsolutePath.Trim('/');

                return !string.IsNullOrWhiteSpace(videoId);
            }

            if (host != "youtube.com"
                && host != "www.youtube.com"
                && host != "m.youtube.com")
            {
                return false;
            }

            /*
             * youtube.com/watch?v=VIDEO_ID
             */
            if (uri.AbsolutePath.Equals(
                    "/watch",
                    StringComparison.OrdinalIgnoreCase))
            {
                /*var query =
                    HttpUtility.ParseQueryString(
                        uri.Query);
                videoId =
                    videoId =
                    query["v"];
                */
                videoId = GetQueryParameter(
                    uri,
                    "v");

                return !string.IsNullOrWhiteSpace(videoId);
            }

            /*
             * youtube.com/embed/VIDEO_ID
             * youtube.com/shorts/VIDEO_ID
             */
            string[] pathSegments =
                uri.AbsolutePath
                    .Trim('/')
                    .Split(
                        '/',
                        StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length >= 2
                && (pathSegments[0].Equals(
                        "embed",
                        StringComparison.OrdinalIgnoreCase)
                    || pathSegments[0].Equals(
                        "shorts",
                        StringComparison.OrdinalIgnoreCase)))
            {
                videoId =
                    pathSegments[1];

                return !string.IsNullOrWhiteSpace(videoId);
            }

            return false;
        }

        private void PopupVideo_MediaOpened(
         object sender,
         RoutedEventArgs e)
        {
            _isMediaOpened = true;

            VideoMessageArea.Visibility =
                Visibility.Collapsed;

            LocalVideoControlArea.Visibility =
                Visibility.Visible;

            PopupVideo.Volume =
                VolumeSlider.Value;

            if (PopupVideo.NaturalDuration.HasTimeSpan)
            {
                TimeSpan duration =
                    PopupVideo.NaturalDuration.TimeSpan;

                ProgressSlider.Minimum =
                    0;

                ProgressSlider.Maximum =
                    duration.TotalSeconds;

                DurationTimeText.Text =
                    FormatTime(duration);
            }

            CurrentTimeText.Text =
                "00:00";

            ProgressSlider.Value =
                0;

            PlayVideo();
        }

        private void ProgressTimer_Tick(
        object? sender,
        EventArgs e)
        {
            if (!_isMediaOpened
                || _isSeeking)
            {
                return;
            }

            TimeSpan currentPosition =
                PopupVideo.Position;

            CurrentTimeText.Text =
                FormatTime(currentPosition);

            ProgressSlider.Value =
                currentPosition.TotalSeconds;
        }

        private void ProgressSlider_PreviewMouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
        {
            if (!_isMediaOpened)
            {
                return;
            }

            /*
             * 드래그 시작 전 재생 상태를 저장한다.
             *
             * 영상이 끝난 상태라면
             * _isPlaying은 false이므로
             * 드래그 후에도 일시정지 상태가 유지된다.
             */
            _wasPlayingBeforeSeeking =
                _isPlaying;

            _isSeeking = true;

            _controlHideTimer.Stop();

            /*
             * 드래그 중에는 영상이 계속 흘러가지 않도록
             * 일시정지한다.
             */
            PopupVideo.Pause();

            ProgressSlider.CaptureMouse();

            MoveProgressSliderByMouse(e);

            e.Handled = true;
        }
        /*
         * 사용자가 진행바를 누른 상태로 마우스를 이동하면
         * 마우스 위치 비율에 맞춰 재생 위치를 계속 변경한다.
         */
        private void ProgressSlider_PreviewMouseMove(
            object sender,
            MouseEventArgs e)
        {
            /*
             * 영상이 아직 열리지 않았거나
             * 드래그 중이 아니라면 아무 작업도 하지 않는다.
             */
            if (!_isMediaOpened ||
                !_isSeeking ||
                e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            MoveProgressSliderByMouse(e);
        }
        /*
  * 진행바 위의 마우스 X좌표를 비율로 계산하여
  * Slider 값과 화면의 시간 표시만 변경한다.
  *
  * 실제 영상 위치는 마우스를 놓았을 때만 변경한다.
  */
        private void MoveProgressSliderByMouse(
            MouseEventArgs e)
        {
            double sliderWidth =
                ProgressSlider.ActualWidth;

            if (sliderWidth <= 0)
            {
                return;
            }

            Point mousePosition =
                e.GetPosition(ProgressSlider);

            double positionRatio =
                mousePosition.X / sliderWidth;

            positionRatio = Math.Clamp(
                positionRatio,
                0,
                1);

            double targetSeconds =
                ProgressSlider.Minimum +
                (
                    ProgressSlider.Maximum -
                    ProgressSlider.Minimum
                ) * positionRatio;

            /*
             * 드래그 중에는 진행바 모양만 이동시킨다.
             */
            ProgressSlider.Value =
                targetSeconds;

            /*
             * 실제 영상 위치는 건드리지 않고
             * 사용자가 이동하려는 예상 시간만 표시한다.
             */
            CurrentTimeText.Text =
                FormatTime(
                    TimeSpan.FromSeconds(
                        targetSeconds));
        }
        private void ProgressSlider_PreviewMouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
        {
            if (!_isMediaOpened)
            {
                _isSeeking = false;

                ProgressSlider.ReleaseMouseCapture();

                return;
            }

            /*
             * 마우스를 놓은 최종 위치로
             * 실제 영상 시간을 한 번만 이동한다.
             */
            PopupVideo.Position =
                TimeSpan.FromSeconds(
                    ProgressSlider.Value);

            CurrentTimeText.Text =
                FormatTime(
                    PopupVideo.Position);

            _isSeeking = false;

            ProgressSlider.ReleaseMouseCapture();

            /*
             * 드래그 전 영상이 재생 중이었던 경우에만
             * 다시 재생한다.
             *
             * 영상이 끝난 뒤 드래그했거나
             * 원래 일시정지 상태였다면
             * 그대로 일시정지를 유지한다.
             */
            if (_wasPlayingBeforeSeeking)
            {
                PlayVideo();
            }
            else
            {
                PopupVideo.Pause();

                _isPlaying = false;

                /*
                 * 재생 버튼 아이콘을
                 * 재생 모양으로 되돌린다.
                 */
                PlayPauseIcon.Text =
                    "\uE768";

                PlayPauseButton.ToolTip =
                    "재생";
            }

            if (!VideoContainer.IsMouseOver)
            {
                LocalVideoControlArea.Visibility =
                    Visibility.Collapsed;

                ShowVideoControls();
            }

            e.Handled = true;
        }
        /*
         * 영상 영역에 마우스가 들어오면
         * 컨트롤바를 표시하고 숨김 시간을 다시 계산한다.
         */
        private void VideoContainer_MouseEnter(
            object sender,
            MouseEventArgs e)
        {
            ShowVideoControls();
        }

        /*
         * 일반 팝업 화면에서 영상 영역을 완전히 벗어나면
         * 컨트롤바를 즉시 숨긴다.
         *
         * 전체화면에서는 컨테이너 밖으로 나갈 수 없으므로
         * 이 이벤트보다 MouseMove 타이머가 핵심이다.
         */
        private void VideoContainer_MouseLeave(
            object sender,
            MouseEventArgs e)
        {
            if (_fullScreenWindow != null)
            {
                return;
            }

            if (_isSeeking
                || LocalVideoControlArea.IsMouseOver)
            {
                return;
            }

            HideVideoControls();
        }

        /*
         * 영상 위에서 마우스가 움직일 때마다
         * 컨트롤바를 표시하고 자동 숨김 시간을 초기화한다.
         */
        private void VideoContainer_MouseMove(
            object sender,
            MouseEventArgs e)
        {
            ShowVideoControls();
        }

        private void ShowVideoControls()
        {
            if (!_isMediaOpened
                || _useWebPlayer)
            {
                return;
            }

            LocalVideoControlArea.Visibility =
                Visibility.Visible;

            /*
             * 마우스가 움직일 때마다 기존 시간을 취소하고
             * 다시 2초를 센다.
             */
            _controlHideTimer.Stop();

            /*
             * 일시정지 중에는 컨트롤을 계속 표시한다.
             */
            if (!_isPlaying)
            {
                return;
            }

            _controlHideTimer.Start();
        }

        


        private void HideVideoControls()
        {
            /*
             * 사용자가 컨트롤을 조작 중이거나
             * 영상이 일시정지 상태라면 숨기지 않는다.
             */
            if (_isSeeking
                || LocalVideoControlArea.IsMouseOver
                || !_isPlaying)
            {
                return;
            }

            LocalVideoControlArea.Visibility =
                Visibility.Collapsed;

            _controlHideTimer.Stop();
        }

        private void ControlHideTimer_Tick(
        object? sender,
        EventArgs e)
        {
            _controlHideTimer.Stop();

            HideVideoControls();
        }

        private void LocalVideoControlArea_MouseEnter(
        object sender,
        MouseEventArgs e)
        {
            _controlHideTimer.Stop();
        }

        private void LocalVideoControlArea_MouseLeave(
            object sender,
            MouseEventArgs e)
        {
            if (!_isPlaying)
            {
                return;
            }

            _controlHideTimer.Stop();
            _controlHideTimer.Start();
        }

        private void VolumeSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
        {
            /*
             * InitializeComponent 실행 중에는
             * PopupVideo가 아직 생성되지 않았을 수 있다.
             */
            if (PopupVideo == null)
            {
                return;
            }

            PopupVideo.Volume =
                e.NewValue;

            _isMuted =
                e.NewValue <= 0;

            UpdateVolumeIcon();
        }
        /*
 * 음량 바를 클릭했을 때
 * 클릭 위치 비율에 맞춰 음량을 변경한다.
 */
        private void VolumeSlider_PreviewMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            _isChangingVolume = true;

            VolumeSlider.CaptureMouse();

            MoveVolumeSliderByMouse(e);

            /*
             * 기본 Slider의 +/- 이동 동작을 막는다.
             */
            e.Handled = true;
        }

        /*
         * 음량 바를 누른 상태로 이동하면
         * 마우스 위치에 맞춰 음량을 계속 변경한다.
         */
        private void VolumeSlider_PreviewMouseMove(
            object sender,
            MouseEventArgs e)
        {
            if (!_isChangingVolume ||
                e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            MoveVolumeSliderByMouse(e);

            e.Handled = true;
        }

        /*
         * 음량 드래그가 끝나면
         * 마우스 캡처를 해제한다.
         */
        private void VolumeSlider_PreviewMouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            _isChangingVolume = false;

            VolumeSlider.ReleaseMouseCapture();

            e.Handled = true;
        }

        /*
         * 마우스 X좌표를 음량 Slider 전체 폭의 비율로 계산한다.
         *
         * 왼쪽 끝
         * → 음량 0
         *
         * 가운데
         * → 음량 0.5
         *
         * 오른쪽 끝
         * → 음량 1
         */
        private void MoveVolumeSliderByMouse(
            MouseEventArgs e)
        {
            double sliderWidth =
                VolumeSlider.ActualWidth;

            if (sliderWidth <= 0)
            {
                return;
            }

            Point mousePosition =
                e.GetPosition(VolumeSlider);

            double volumeRatio =
                mousePosition.X / sliderWidth;

            volumeRatio = Math.Clamp(
                volumeRatio,
                0,
                1);

            VolumeSlider.Value =
                VolumeSlider.Minimum +
                (
                    VolumeSlider.Maximum -
                    VolumeSlider.Minimum
                ) * volumeRatio;
        }

        private void MuteButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_isMediaOpened)
            {
                return;
            }

            if (_isMuted)
            {
                double restoredVolume =
                    _volumeBeforeMute > 0
                        ? _volumeBeforeMute
                        : 0.7;

                VolumeSlider.Value =
                    restoredVolume;

                PopupVideo.Volume =
                    restoredVolume;

                _isMuted = false;
            }
            else
            {
                if (VolumeSlider.Value > 0)
                {
                    _volumeBeforeMute =
                        VolumeSlider.Value;
                }

                VolumeSlider.Value =
                    0;

                PopupVideo.Volume =
                    0;

                _isMuted = true;
            }

            UpdateVolumeIcon();
        }

        private void FullScreenButton_Click(
        object sender,
        RoutedEventArgs e)
        {
            if (_fullScreenWindow == null)
            {
                EnterFullScreen();
            }
            else
            {
                ExitFullScreen();
            }
        }
        private void EnterFullScreen()
        {
            if (_fullScreenWindow != null)
            {
                return;
            }

            /*
             * 현재 VideoContainer가 들어 있는
             * 원래 부모 Grid를 저장한다.
             */
            _originalVideoParent =
                VideoContainer.Parent as Panel;

            if (_originalVideoParent == null)
            {
                return;
            }

            _originalVideoIndex =
                _originalVideoParent.Children.IndexOf(
                    VideoContainer);

            /*
             * 전체화면 종료 시 복원할
             * 기존 레이아웃 정보를 저장한다.
             */
            _originalVideoWidth =
                VideoContainer.Width;

            _originalVideoHeight =
                VideoContainer.Height;

            _originalVideoMargin =
                VideoContainer.Margin;

            _originalHorizontalAlignment =
                VideoContainer.HorizontalAlignment;

            _originalVerticalAlignment =
                VideoContainer.VerticalAlignment;

            _originalCornerRadius =
                VideoContainer.CornerRadius;

            _originalBorderThickness =
                VideoContainer.BorderThickness;

            /*
             * 기존 부모에서 영상 컨테이너를 분리한다.
             */
            _originalVideoParent.Children.Remove(
                VideoContainer);

            /*
             * 전체화면에 맞게 영상 컨테이너를 확장한다.
             */
            VideoContainer.Width =
                double.NaN;

            VideoContainer.Height =
                double.NaN;

            VideoContainer.Margin =
                new Thickness(0);

            VideoContainer.HorizontalAlignment =
                HorizontalAlignment.Stretch;

            VideoContainer.VerticalAlignment =
                VerticalAlignment.Stretch;

            VideoContainer.CornerRadius =
                new CornerRadius(0);

            VideoContainer.BorderThickness =
                new Thickness(0);

            Window? ownerWindow =
                Window.GetWindow(this);

            _fullScreenWindow =
                new Window
                {
                    Owner =
                        ownerWindow,

                    WindowStyle =
                        WindowStyle.None,

                    WindowState =
                        WindowState.Maximized,

                    ResizeMode =
                        ResizeMode.NoResize,

                    Background =
                        System.Windows.Media.Brushes.Black,

                    ShowInTaskbar =
                        false,

                    Content =
                        VideoContainer
                };

            _fullScreenWindow.KeyDown +=
                FullScreenWindow_KeyDown;

            _fullScreenWindow.Closed +=
                FullScreenWindow_Closed;

            FullScreenIcon.Text =
                "⛶";

            FullScreenButton.ToolTip =
                "전체 화면 종료";

            _fullScreenWindow.Show();
            _fullScreenWindow.Activate();
        }

        private void ExitFullScreen()
        {
            if (_fullScreenWindow == null)
            {
                return;
            }

            Window fullScreenWindow =
                _fullScreenWindow;

            /*
             * Closed 이벤트에서 중복 복원되지 않도록
             * 먼저 이벤트를 제거한다.
             */
            fullScreenWindow.KeyDown -=
                FullScreenWindow_KeyDown;

            fullScreenWindow.Closed -=
                FullScreenWindow_Closed;

            RestoreVideoContainer();

            fullScreenWindow.Close();
        }

        private void RestoreVideoContainer()
        {
            if (_fullScreenWindow == null
                || _originalVideoParent == null)
            {
                return;
            }

            /*
             * 전체화면 Window에서 VideoContainer를 분리한다.
             */
            _fullScreenWindow.Content =
                null;

            /*
             * 기존 레이아웃 값을 복구한다.
             */
            VideoContainer.Width =
                _originalVideoWidth;

            VideoContainer.Height =
                _originalVideoHeight;

            VideoContainer.Margin =
                _originalVideoMargin;

            VideoContainer.HorizontalAlignment =
                _originalHorizontalAlignment;

            VideoContainer.VerticalAlignment =
                _originalVerticalAlignment;

            VideoContainer.CornerRadius =
                _originalCornerRadius;

            VideoContainer.BorderThickness =
                _originalBorderThickness;

            /*
             * 원래 부모의 원래 순서에 다시 삽입한다.
             */
            int insertIndex =
                Math.Min(
                    _originalVideoIndex,
                    _originalVideoParent.Children.Count);

            _originalVideoParent.Children.Insert(
                insertIndex,
                VideoContainer);

            _fullScreenWindow =
                null;

            FullScreenIcon.Text =
                "⛶";

            FullScreenButton.ToolTip =
                "전체 화면";

            /*
             * 전체화면 종료 직후 마우스가 영상 위에 없으면
             * 컨트롤바를 숨긴다.
             */
            LocalVideoControlArea.Visibility =
                VideoContainer.IsMouseOver
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        private void FullScreenWindow_KeyDown(
        object sender,
        KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ExitFullScreen();

                e.Handled =
                    true;
            }
        }

        private void FullScreenWindow_Closed(
            object? sender,
            EventArgs e)
        {
            /*
             * Alt+F4 등으로 전체화면 Window가 종료된 경우에도
             * 영상 컨테이너를 원래 위치로 돌려놓는다.
             */
            RestoreVideoContainer();
        }


        private void UpdateVolumeIcon()
        {
            if (VolumeIcon == null)
            {
                return;
            }

            if (_isMuted
                || VolumeSlider.Value <= 0)
            {
                /*
                 * 음소거 아이콘
                 */
                VolumeIcon.Text =
                    "\uE74F";

                MuteButton.ToolTip =
                    "음소거 해제";

                return;
            }

            /*
             * 일반 음량 아이콘
             */
            VolumeIcon.Text =
                "\uE767";

            MuteButton.ToolTip =
                "음소거";
        }

        private static string FormatTime(
            TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return time.ToString(
                    @"hh\:mm\:ss");
            }

            return time.ToString(
                @"mm\:ss");
        }

        private void PopupVideo_MediaEnded(
        object sender,
        RoutedEventArgs e)
        {
            _isPlaying = false;

            _progressTimer.Stop();

            _controlHideTimer.Stop();

            LocalVideoControlArea.Visibility =
                Visibility.Visible;

            PlayPauseIcon.Text =
                "\uE768";

            PlayPauseButton.ToolTip =
                "다시 재생";

       

            if (PopupVideo.NaturalDuration.HasTimeSpan)
            {
                TimeSpan duration =
                    PopupVideo.NaturalDuration.TimeSpan;

                ProgressSlider.Value =
                    duration.TotalSeconds;

                CurrentTimeText.Text =
                    FormatTime(duration);
            }
        }

        private void PopupVideo_MediaFailed(
            object sender,
            ExceptionRoutedEventArgs e)
        {

            _progressTimer.Stop();

            _isPlaying = false;


            string errorMessage =
                e.ErrorException?.Message
                ?? "알 수 없는 영상 재생 오류가 발생했습니다.";

          
            ShowVideoError(
                $"영상 재생에 실패했습니다.\n{errorMessage}");
        }

        private void VideoWebView_NavigationCompleted(
            object sender,
            CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                VideoMessageArea.Visibility =
                    Visibility.Collapsed;

           
            }
            else
            {
                ShowVideoError(
                    $"YouTube 페이지를 불러오지 못했습니다.\n" +
                    $"오류 코드: {e.WebErrorStatus}");
            }
        }

        private void CoreWebView2_NewWindowRequested(
            object? sender,
            CoreWebView2NewWindowRequestedEventArgs e)
        {
            /*
             * 링크가 새 창을 요청하더라도
             * 별도 브라우저 창을 생성하지 않고
             * 현재 WebView2에서 이동한다.
             */
            e.Handled = true;

            if (Uri.TryCreate(
                    e.Uri,
                    UriKind.Absolute,
                    out Uri? targetUri))
            {
                VideoWebView.Source =
                    targetUri;
            }
        }

        private void PlayPauseButton_Click(
    object sender,
    RoutedEventArgs e)
        {
            if (!_isMediaOpened
                || _useWebPlayer)
            {
                return;
            }

            if (_isPlaying)
            {
                PauseVideo();
            }
            else
            {
                PlayVideo();
            }
        }

        private void PlayVideo()
        {
            if (!_isMediaOpened)
            {
                return;
            }

            /*
             * 영상이 끝난 상태라면 처음부터 다시 재생한다.
             */
            if (PopupVideo.NaturalDuration.HasTimeSpan
                && PopupVideo.Position
                    >= PopupVideo.NaturalDuration.TimeSpan)
            {
                PopupVideo.Position =
                    TimeSpan.Zero;
            }

            PopupVideo.Play();

            _isPlaying = true;

            /*
             * 일시정지 아이콘
             */
            PlayPauseIcon.Text =
                "\uE769";

            PlayPauseButton.ToolTip =
                "일시정지";


            _progressTimer.Start();


        }

        private void PauseVideo()
        {
            if (!_isMediaOpened)
            {
                return;
            }

            PopupVideo.Pause();

            _isPlaying = false;

            /*
             * 재생 아이콘
             */
            PlayPauseIcon.Text =
                "\uE768";

            PlayPauseButton.ToolTip =
                "재생";


            _progressTimer.Stop();

            _controlHideTimer.Stop();

            LocalVideoControlArea.Visibility =
                Visibility.Visible;


        }

        private void ShowLoadingMessage(
            string message)
        {
            VideoMessageText.Text =
                message;

            VideoMessageArea.Visibility =
                Visibility.Visible;
        }

        private void ShowVideoError(
            string message)
        {
            _isMediaOpened = false;

            VideoMessageText.Text =
                message;

            VideoMessageArea.Visibility =
                Visibility.Visible;
        }

        private void VideoPopupView_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (_fullScreenWindow != null)
                {
                    ExitFullScreen();
                }

                _progressTimer.Stop();
                
                _controlHideTimer.Stop();
                /*
                 * 로컬 영상 정리
                 */
                PopupVideo.Stop();

                PopupVideo.Source =
                    null;

                /*
                 * YouTube 영상 정리
                 *
                 * 빈 페이지로 이동시켜 영상과 음성을 중단한다.
                 */
                if (VideoWebView.CoreWebView2 != null)
                {
                    VideoWebView.CoreWebView2.NewWindowRequested -=
                        CoreWebView2_NewWindowRequested;

                    VideoWebView.CoreWebView2.Navigate(
                        "about:blank");
                }

                VideoWebView.Dispose();
            }
            catch
            {
                /*
                 * 팝업 종료 중 발생한 정리 오류는 무시한다.
                 */
            }

            _isMediaOpened = false;
            _isPlaying = false;
            _isSeeking = false;
        }

        private static string? GetQueryParameter(
        Uri uri,
        string parameterName)
            {
                string query =
                    uri.Query.TrimStart('?');

                foreach (string pair in query.Split('&'))
                {
                    string[] parts =
                        pair.Split(
                            '=',
                            2);

                    if (parts.Length == 2
                        && parts[0].Equals(
                            parameterName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return Uri.UnescapeDataString(
                            parts[1]);
                    }
                }

                return null;
            }
    }
}