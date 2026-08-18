package com.oksy.popup.domain;

import java.util.List;

/** 팝업 한 건에 속한 대상 그룹들을 묶는 도메인 객체다. */
public record PopupTargetEntity(
        String popupId,
        List<PopupTargetGroupEntity> groups
) {
    public PopupTargetEntity {
        groups = groups == null ? List.of() : List.copyOf(groups);
    }
}
