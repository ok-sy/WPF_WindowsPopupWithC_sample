using Popup.Dtos;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Popup.Services
{
    /*
     * Java Spring Boot 팝업 API와 통신하는 서비스다.
     *
     * 이 클래스는 다음 역할만 담당한다.
     *
     * 1. Java 서버에 HTTP 요청을 보낸다.
     * 2. 서버가 반환한 JSON을 받는다.
     * 3. JSON을 PopupResponseDto 목록으로 변환한다.
     *
     * PopupWindow를 만들거나 화면에 표시하는 역할은
     * PopupService와 PopupManager가 담당한다.
     */
    public class PopupApiService
    {
        /*
         * Java Spring Boot 서버의 기본 주소다.
         *
         * 개발·테스트·운영 환경마다
         * 다른 주소를 사용할 수 있도록
         * 생성자에서 값을 전달받는다.
         */
        private readonly string
            _baseUrl;


        /*
         * 서버에 HTTP 요청을 보내는 객체다.
         *
         * HttpClient를 요청마다 새로 만들면
         * 네트워크 연결이 불필요하게 계속 생성될 수 있다.
         *
         * 따라서 프로그램 전체에서 같은 객체를
         * 재사용할 수 있도록 static으로 선언한다.
         */
        private static readonly HttpClient HttpClient =
            new HttpClient
            {
                /*
                 * 서버가 응답하지 않을 때
                 * 무한정 기다리지 않도록 제한 시간을 설정한다.
                 */
                Timeout =
                    TimeSpan.FromSeconds(10)
            };

        /*
         * 서버 JSON을 C# DTO로 변환할 때 사용하는 옵션이다.
         */
        private readonly JsonSerializerOptions
            _jsonOptions;

        /*
 * PopupApiService 생성자다.
 *
 * baseUrl
 * → Java Spring Boot 팝업 API의 기본 주소
 *
 * 예:
 * http://localhost:8080
 * https://popup-api.company.com
 */
        public PopupApiService(
            string baseUrl)
        {
            /*
             * API 주소가 없으면
             * 서버 요청 주소를 만들 수 없으므로
             * 객체 생성을 중단한다.
             */
            if (string.IsNullOrWhiteSpace(
                    baseUrl))
            {
                throw new ArgumentException(
                    "팝업 API 주소가 필요합니다.",
                    nameof(baseUrl));
            }

            /*
             * 전달받은 API 주소의 앞뒤 공백과
             * 마지막 슬래시를 제거한다.
             *
             * 예:
             * http://localhost:8080/
             *
             * 변경:
             * http://localhost:8080
             *
             * 이후 /api/popups를 붙였을 때
             * //api/popups가 되는 것을 방지한다.
             */
            _baseUrl =
                baseUrl
                    .Trim()
                    .TrimEnd('/');

            /*
             * Java JSON은 camelCase를 사용하고
             * C# DTO 속성은 PascalCase를 사용한다.
             *
             * JSON:
             * popupId
             *
             * C#:
             * PopupId
             */
            _jsonOptions =
                new JsonSerializerOptions
                {
                    /*
                     * Java 서버가 보내는 camelCase JSON과
                     * C#의 PascalCase 속성을 연결한다.
                     */
                    PropertyNameCaseInsensitive =
                        true,

                    /*
                     * C# DTO를 Java 서버로 보낼 때
                     * 속성명을 camelCase로 변환한다.
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
        }

        /*
         * 현재 사용자에게 표시할 수 있는
         * 팝업 목록을 Java 서버에서 조회한다.
         *
         * userId
         * → 팝업을 조회할 사용자 식별값
         *
         * 반환값
         * → 서버에서 받은 PopupResponseDto 목록
         */
        public async Task<List<PopupResponseDto>>
            GetAvailablePopupsAsync(
                string userId)
        {
            /*
             * 사용자 ID가 없으면
             * 서버에서 사용자를 구분할 수 없으므로
             * 요청을 보내지 않고 예외를 발생시킨다.
             */
            if (string.IsNullOrWhiteSpace(
                    userId))
            {
                throw new ArgumentException(
                    "사용자 ID가 필요합니다.",
                    nameof(userId));
            }

            /*
             * userId에 공백이나 특수문자가 들어가도
             * 안전한 URL이 되도록 인코딩한다.
             */
            string encodedUserId =
                Uri.EscapeDataString(
                    userId.Trim());

            /*
             * Java Controller에 선언한
             * 팝업 목록 조회 API 주소를 만든다.
             *
             * 완성되는 주소 예:
             *
             * http://localhost:8080/api/popups?userId=TEST_USER
             */
            string requestUrl =
            $"{_baseUrl}/api/popups" +
            $"?userId={encodedUserId}";

            /*
             * Java 서버에 GET 요청을 보내고
             * 응답을 기다린다.
             */
            using HttpResponseMessage response =
                await HttpClient.GetAsync(
                    requestUrl);

            /*
             * HTTP 상태 코드가 성공 범위가 아니면
             * 예외를 발생시킨다.
             *
             * 예:
             *
             * 404
             * 500
             */
            response.EnsureSuccessStatusCode();

            /*
             * 서버 응답 본문에 들어 있는
             * JSON 문자열을 읽는다.
             */
            string popupJson =
                await response.Content
                    .ReadAsStringAsync();

            /*
             * 서버 JSON 배열을
             * PopupResponseDto 목록으로 변환한다.
             */
            List<PopupResponseDto>? popupDtos =
                JsonSerializer.Deserialize<
                        List<PopupResponseDto>>(
                        popupJson,
                        _jsonOptions);

            /*
             * 서버가 빈 배열을 반환하면 빈 List가 만들어진다.
             *
             * 역직렬화 결과 자체가 null인 경우에도
             * 호출하는 쪽에서 null 검사를 하지 않도록
             * 빈 List를 반환한다.
             */
            return popupDtos
                ?? new List<PopupResponseDto>();
        }

        /*
         * 사용자가 선택한 팝업을
         * 지정한 기간 동안 숨김 처리한다.
         *
         * 호출되는 Java API:
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
            /*
             * PopupId가 없으면 어떤 팝업을 숨길지
             * 결정할 수 없으므로 요청하지 않는다.
             */
            if (string.IsNullOrWhiteSpace(
                    popupId))
            {
                throw new ArgumentException(
                    "팝업 ID가 필요합니다.",
                    nameof(popupId));
            }

            /*
             * 사용자 ID가 없으면 숨김 상태를
             * 어느 사용자에게 저장할지 알 수 없다.
             */
            if (string.IsNullOrWhiteSpace(
                    userId))
            {
                throw new ArgumentException(
                    "사용자 ID가 필요합니다.",
                    nameof(userId));
            }

            /*
             * 숨김 일수는 반드시
             * 1일 이상이어야 한다.
             */
            if (hideDays < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hideDays),
                    "숨김 일수는 1일 이상이어야 합니다.");
            }

            /*
             * PopupId에 공백이나 특수문자가 들어가도
             * 안전한 URL이 되도록 인코딩한다.
             */
            string encodedPopupId =
                Uri.EscapeDataString(
                    popupId.Trim());


            /*
             * Java 서버로 전달할 요청 데이터를 만든다.
             */
            PopupHideRequestDto requestDto =
                new PopupHideRequestDto
                {
                    UserId =
                        userId.Trim(),

                    HideDays =
                        hideDays
                };

            string requestUrl =
                $"{_baseUrl}/api/popups/" +
                $"{encodedPopupId}/hide";

            /*
             * Java 서버에 JSON 형식으로
             * POST 요청을 보낸다.
             *
             * 실제 전송 JSON:
             *
             * {
             *   "userId": "WPF_TEST_USER",
             *   "hideDays": 30
             * }
             */
            using HttpResponseMessage response =
                await HttpClient.PostAsJsonAsync(
                    requestUrl,
                    requestDto,
                    _jsonOptions);

            /*
             * HTTP 상태 코드가 성공 범위가 아니라면
             * HttpRequestException을 발생시킨다.
             */
            response.EnsureSuccessStatusCode();

            /*
             * Java 서버가 반환한 JSON을
             * PopupHideResponseDto로 변환한다.
             */
            PopupHideResponseDto? responseDto =
                await response.Content
                    .ReadFromJsonAsync<PopupHideResponseDto>(
                        _jsonOptions);

            /*
             * 성공 응답인데 응답 본문이 없으면
             * 정상적인 숨김 결과를 확인할 수 없으므로
             * 예외를 발생시킨다.
             */
            return responseDto
                ?? throw new InvalidOperationException(
                    "팝업 숨김 응답이 비어 있습니다.");
        }
    }
}