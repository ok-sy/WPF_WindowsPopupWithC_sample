using Popup.Dtos;
using Popup.Factories;
using Popup.Models;
using System.Collections.Generic;

namespace Popup.Services
{
    /*
     * 서버에서 받은 팝업 DTO를
     * 화면에서 사용할 PopupOptions 목록으로 변환한다.
     */
    public class PopupService
    {
        /*
         * 팝업 목록을 변환한다.
         */
        public List<PopupOptions> CreatePopupOptions(
            IEnumerable<PopupResponseDto> popupDtos)
        {
            List<PopupOptions> popupOptions =
                new();

            foreach (PopupResponseDto popupDto
                     in popupDtos)
            {
                popupOptions.Add(
                    PopupFactory.Create(
                        popupDto));
            }

            return popupOptions;
        }
    }
}