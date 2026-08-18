namespace Popup.Models
{
    /// <summary>
    /// appsettings.json에서 읽은 WPF 팝업 클라이언트 설정이다.
    /// </summary>
    public class PopupClientSettings
    {
        public string BaseUrl { get; set; } =
            string.Empty;

        public string UserId { get; set; } =
            string.Empty;

        public bool AutoLoadOnStartup { get; set; } =
            true;

        public int PollingIntervalSeconds { get; set; } =
            300;
    }
}
