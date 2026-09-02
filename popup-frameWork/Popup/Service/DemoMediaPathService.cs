using System.IO;

namespace Popup.Services
{
    /// <summary>
    /// 폐쇄망 Demo Mode에서 사용하는 로컬 이미지와 동영상 경로를 관리한다.
    /// 개발 실행과 publish 실행 모두 EXE가 있는 위치를 기준으로 Media 폴더를 찾는다.
    /// </summary>
    public static class DemoMediaPathService
    {
        public const string ImageFileName =
            "demo-image.jpg";

        public const string VideoFileName =
            "demo-video.mp4";

        public static string GetImagePath()
        {
            return GetRequiredMediaPath(
                ImageFileName);
        }

        public static string GetVideoPath()
        {
            return GetRequiredMediaPath(
                VideoFileName);
        }

        private static string GetRequiredMediaPath(
            string fileName)
        {
            string mediaPath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Media",
                    fileName);

            if (!File.Exists(
                    mediaPath))
            {
                throw new FileNotFoundException(
                    "Demo Mode 미디어 파일을 찾을 수 없습니다.\n" +
                    "publish 폴더의 Media 디렉터리에 파일을 넣어주세요.\n\n" +
                    mediaPath,
                    mediaPath);
            }

            return mediaPath;
        }
    }
}
