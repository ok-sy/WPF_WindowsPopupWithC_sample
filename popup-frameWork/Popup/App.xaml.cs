using System.ComponentModel;
using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;

namespace Popup
{
    /*
     * WPF 프로그램 전체의 시작과 종료를 관리한다.
     *
     * MainWindow를 직접 시작 화면으로 사용하지 않고,
     * Windows 트레이 아이콘을 프로그램의 기본 진입점으로 사용한다.
     */
    public partial class App : Application
    {
        private MainWindow? _mainWindow;

        private Forms.NotifyIcon? _trayIcon;

        private bool _isExiting;

        /*
         * 관리용 MainWindow는 한 번 표시해 Loaded 이벤트와
         * 팝업 자동 조회를 실행한 뒤 바로 숨긴다.
         */
        protected override void OnStartup(
            StartupEventArgs e)
        {
            base.OnStartup(e);

            _mainWindow =
                new MainWindow();

            MainWindow =
                _mainWindow;

            _mainWindow.Closing +=
                MainWindow_Closing;

            CreateTrayIcon();

            /*
             * Show를 호출해야 MainWindow.Loaded가 발생하여
             * Framework 8의 자동 API 조회가 실행된다.
             */
            _mainWindow.Show();
            _mainWindow.Hide();
        }

        /*
         * Windows 알림 영역에 표시할 트레이 아이콘과 메뉴를 만든다.
         */
        private void CreateTrayIcon()
        {
            Forms.ContextMenuStrip trayMenu =
                new Forms.ContextMenuStrip();

            trayMenu.Items.Add(
                "관리 화면 열기",
                null,
                ShowMainWindowMenuItem_Click);

            trayMenu.Items.Add(
                "팝업 다시 조회",
                null,
                RefreshPopupsMenuItem_Click);

            trayMenu.Items.Add(
                new Forms.ToolStripSeparator());

            trayMenu.Items.Add(
                "종료",
                null,
                ExitMenuItem_Click);

            _trayIcon =
                new Forms.NotifyIcon
                {
                    Icon =
                        SystemIcons.Information,
                    Text =
                        "Popup 알림",
                    ContextMenuStrip =
                        trayMenu,
                    Visible =
                        true
                };

            _trayIcon.DoubleClick +=
                ShowMainWindowMenuItem_Click;
        }

        /*
         * 트레이 아이콘을 더블클릭하거나 메뉴를 선택하면
         * 숨겨 둔 관리 화면을 다시 표시한다.
         */
        private void ShowMainWindowMenuItem_Click(
            object? sender,
            EventArgs e)
        {
            if (_mainWindow == null)
            {
                return;
            }

            _mainWindow.ShowInTaskbar =
                true;

            _mainWindow.Show();

            if (_mainWindow.WindowState
                == WindowState.Minimized)
            {
                _mainWindow.WindowState =
                    WindowState.Normal;
            }

            _mainWindow.Activate();
        }

        /*
         * 관리 화면을 열지 않고도 서버에서 팝업을 다시 조회한다.
         */
        private async void RefreshPopupsMenuItem_Click(
            object? sender,
            EventArgs e)
        {
            if (_mainWindow != null)
            {
                await _mainWindow
                    .RefreshPopupsAsync();
            }
        }

        /*
         * 관리창의 X를 누르면 프로세스를 종료하지 않고
         * 트레이로 다시 숨긴다.
         */
        private void MainWindow_Closing(
            object? sender,
            CancelEventArgs e)
        {
            if (_isExiting
                || _mainWindow == null)
            {
                return;
            }

            e.Cancel =
                true;

            _mainWindow.ShowInTaskbar =
                false;

            _mainWindow.Hide();
        }

        /*
         * 트레이의 종료 메뉴를 선택했을 때만 아이콘과 창을 정리하고
         * WPF 프로세스를 완전히 종료한다.
         */
        private void ExitMenuItem_Click(
            object? sender,
            EventArgs e)
        {
            _isExiting =
                true;

            if (_trayIcon != null)
            {
                _trayIcon.Visible =
                    false;

                _trayIcon.Dispose();
                _trayIcon =
                    null;
            }

            _mainWindow?.Close();
            Shutdown();
        }
    }
}
