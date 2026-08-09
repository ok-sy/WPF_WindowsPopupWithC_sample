using Popup.Dtos;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Popup.Service
{
    /*
     * WPF 프로그램과 Java Spring Boot API 사이의
     * HTTP 통신을 담당한다.
     *
     * 화면 생성이나 팝업 표시를 직접 처리하지 않고,
     * 서버에 요청을 보내고 DTO를 반환하는 역할만 한다.
     */
    public class PopupApiService
    {
        /*
         * Java API 기본 주소
         *
         * Java 서버가 8080 포트에서 실행된다는 기준이다.
         */
        private const string BaseUrl =
            "http://localhost:8080";

        /*
         * HttpClient는 요청마다 새로 만들지 않고
         * 애플리케이션 전체에서 재사용한다.
         */
        private static readonly HttpClient HttpClient =
            new HttpClient
            {
                Timeout =
                    TimeSpan.FromSeconds(10)
            };

        /*
         * Java 서버 JSON과 C# 속성 이름을 연결하기 위한 설정
         *
         * Java:
         * popupId
         *
         * C#:
         * PopupId
         */
        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions
            {
                /*
                 * JSON 속성 이름의 대소문자를 구분하지 않는다.
                 */
                PropertyNameCaseInsensitive =
                    true,

                /*
                 * C# 객체를 JSON으로 보낼 때
                 * camelCase 형식으로 변환한다.
                 *
                 * UserId
                 * → userId
                 *
                 * HideDays
                 * → hideDays
                 */
                PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase
            };

        /*
         * 현재 사용자에게 노출 가능한 팝업 목록을 조회한다.
         *
         * Java API:
         * GET /api/popups?userId=WPF_TEST_USER
         */
        public async Task<List<PopupResponseDto>>
            GetAvailablePopupsAsync(
                string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "사용자 ID가 필요합니다.",
                    nameof(userId));
            }

            string requestUrl =
                $"{BaseUrl}/api/popups" +
                $"?userId={Uri.EscapeDataString(userId)}";

            /*
             * Java API에 GET 요청을 보내고
             * 응답 상태 코드가 성공인지 확인한다.
             */
            using HttpResponseMessage response =
                await HttpClient.GetAsync(
                    requestUrl);

            response.EnsureSuccessStatusCode();

            /*
             * JSON 배열을
             * PopupResponseDto 목록으로 역직렬화한다.
             */
            List<PopupResponseDto>? popups =
                await response.Content
                    .ReadFromJsonAsync<
                        List<PopupResponseDto>>(
                            JsonOptions);

            return popups
                ?? new List<PopupResponseDto>();
        }

        /*
         * 특정 사용자가 선택한 팝업을
         * 일정 기간 보이지 않도록 서버에 저장한다.
         *
         * Java API:
         * POST /api/popups/{popupId}/hide
         */
        public async Task<PopupHideResponseDto>
            HidePopupAsync(
                string popupId,
                string userId,
                int hideDays = 30)
        {
            if (string.IsNullOrWhiteSpace(popupId))
            {
                throw new ArgumentException(
                    "팝업 ID가 필요합니다.",
                    nameof(popupId));
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "사용자 ID가 필요합니다.",
                    nameof(userId));
            }

            if (hideDays <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hideDays),
                    "숨김 기간은 1일 이상이어야 합니다.");
            }

            PopupHideRequestDto request =
                new PopupHideRequestDto
                {
                    UserId =
                        userId,

                    HideDays =
                        hideDays
                };

            string requestUrl =
                $"{BaseUrl}/api/popups/" +
                $"{Uri.EscapeDataString(popupId)}/hide";

            /*
             * 요청 객체를 camelCase JSON으로 변환하여
             * Java API에 POST 요청을 보낸다.
             */
            using HttpResponseMessage response =
                await HttpClient.PostAsJsonAsync(
                    requestUrl,
                    request,
                    JsonOptions);

            response.EnsureSuccessStatusCode();

            /*
             * 서버가 반환한 숨김 결과를
             * 응답 DTO로 변환한다.
             */
            PopupHideResponseDto? result =
                await response.Content
                    .ReadFromJsonAsync<
                        PopupHideResponseDto>(
                            JsonOptions);

            if (result == null)
            {
                throw new InvalidOperationException(
                    "팝업 숨김 응답을 읽을 수 없습니다.");
            }

            return result;
        }
    }
}