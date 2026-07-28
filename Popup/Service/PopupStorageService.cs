using System;
using System.Collections.Generic;

namespace Popup.Services
{
    /*
     * 사용자가 특정 팝업을 보지 않기로 설정한 정보를
     * 저장하고 조회하는 서비스
     *
     * 현재 단계에서는 메모리에만 저장함
     *
     * 프로그램이 종료되면 저장된 정보도 사라짐
     *
     * 이후 JSON 파일 또는 Registry 저장 방식으로
     * 변경할 예정
     */
    public class PopupStorageService
    {
        /*
         * 프로그램 전체에서 공통으로 사용할
         * PopupStorageService 단일 객체
         *
         * Instance를 통해서만 저장소에 접근한다.
         *
         * 예:
         * PopupStorageService.Instance.HideUntil(...);
         * PopupStorageService.Instance.IsHidden(...);
         */
        public static PopupStorageService Instance { get; } =
            new PopupStorageService();

        /*
         * 생성자를 private으로 막는다.
         *
         * 외부에서 다음과 같이 새로운 저장소를
         * 만들 수 없게 한다.
         *
         * new PopupStorageService();
         *
         * 이렇게 해야 프로그램 전체가
         * Instance 하나만 사용하게 된다.
         */
        private PopupStorageService()
        {
        }

        /*
         * 팝업 ID별 숨김 종료 일시 저장
         *
         * Key
         * → 팝업 고유 ID
         *
         * Value
         * → 해당 팝업을 다시 표시할 수 있는 일시
         */
        private readonly Dictionary<string, DateTime>
            _hiddenUntilByPopupId = new();

        /*
         * 특정 팝업을 지정된 기간 동안 숨김 처리
         *
         * popupId
         * → 숨길 팝업의 고유 ID
         *
         * hiddenUntil
         * → 이 일시까지 팝업을 표시하지 않음
         */
        public void HideUntil(
            string popupId,
            DateTime hiddenUntil)
        {
            /*
             * 팝업 ID가 없으면
             * 어떤 팝업의 정보인지 알 수 없으므로 저장하지 않는다.
             */
            if (string.IsNullOrWhiteSpace(popupId))
            {
                return;
            }

            /*
             * 같은 PopupId가 이미 저장되어 있으면
             * 기존 종료 일시를 새로운 종료 일시로 덮어쓴다.
             */
            _hiddenUntilByPopupId[popupId] =
                hiddenUntil;
        }

        /*
         * 현재 해당 팝업이 숨김 상태인지 확인
         *
         * true
         * → 아직 숨김 기간이 남아 있음
         *
         * false
         * → 숨김 정보가 없거나 숨김 기간이 끝남
         */
        public bool IsHidden(string popupId)
        {
            /*
             * 팝업 ID가 없으면
             * 숨김 여부를 판단할 수 없으므로
             * 표시 가능한 것으로 처리한다.
             */
            if (string.IsNullOrWhiteSpace(popupId))
            {
                return false;
            }

            /*
             * Dictionary에서 PopupId에 해당하는
             * 숨김 종료 일시를 찾는다.
             *
             * 저장된 정보가 없으면 숨김 상태가 아니다.
             */
            if (!_hiddenUntilByPopupId.TryGetValue(
                    popupId,
                    out DateTime hiddenUntil))
            {
                return false;
            }

            /*
             * 현재 시간이 숨김 종료 일시보다 이전이면
             * 아직 숨김 기간이 남아 있다.
             */
            if (DateTime.Now < hiddenUntil)
            {
                return true;
            }

            /*
             * 숨김 기간이 끝난 정보는
             * 더 이상 필요하지 않으므로 저장소에서 제거한다.
             */
            _hiddenUntilByPopupId.Remove(popupId);

            return false;
        }
    }
}