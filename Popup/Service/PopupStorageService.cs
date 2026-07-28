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
            if (string.IsNullOrWhiteSpace(popupId))
            {
                return;
            }

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
            if (string.IsNullOrWhiteSpace(popupId))
            {
                return false;
            }

            if (!_hiddenUntilByPopupId.TryGetValue(
                    popupId,
                    out DateTime hiddenUntil))
            {
                return false;
            }

            if (DateTime.Now < hiddenUntil)
            {
                return true;
            }

            /*
             * 숨김 기간이 끝난 정보는
             * 저장소에서 제거
             */
            _hiddenUntilByPopupId.Remove(popupId);

            return false;
        }
    }
}