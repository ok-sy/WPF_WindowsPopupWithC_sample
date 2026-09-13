package server.service.core.popup;

import com.fasterxml.jackson.databind.ObjectMapper;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import server.domain.popup.*;
import server.repo.core.mapper.popup.PopupMapper;
import java.math.BigDecimal;
import java.time.OffsetDateTime;
import java.util.List;
import java.util.Map;
import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.*;
import static org.mockito.Mockito.*;

class PopupAdminQuestionsTest {
    private PopupMapper mapper;
    private PopupService service;
    private PopupResponseDto popup;
    @BeforeEach void setup() {
        mapper = mock(PopupMapper.class);
        service = spy(new PopupService(mapper, new ObjectMapper()));
        popup = new PopupResponseDto("TEST", "QUIZ", "Quiz", OffsetDateTime.now(),
                OffsetDateTime.now().plusDays(1), "SEQUENTIAL", "FIXED",
                560, 420, .7, .75, 480, 320, 1200, 900,
                true, true, true, false, 10L, "FIXED",
                null, null, null, null, null, null, true, List.of(), Map.of());
        doReturn(popup).when(service).getAdminPopup("TEST");
        when(mapper.upsertAdminPopupNotice(any())).thenReturn(1);
        when(mapper.upsertAdminPopupContent(any())).thenReturn(1);
        when(mapper.insertAdminQuestionTemplate(anyString(), anyString(), anyString(), anyString())).thenReturn(20L);
        when(mapper.insertAdminQuestion(eq(20L), any(), anyString())).thenReturn(30L);
        when(mapper.insertAdminOption(anyLong(), any(), anyString(), anyString())).thenReturn(1);
    }
    private AdminPopupQuestion question(List<String> correct) {
        return new AdminPopupQuestion(new PopupQuestionDto(1L, "Question", null, "SINGLE_CHOICE",
                true, true, new BigDecimal("2.00"), 1,
                List.of(new PopupOptionDto(2L, "a", "A", 1), new PopupOptionDto(3L, "b", "B", 2))), correct);
    }
    @Test void changedQuestionsCreateTemplateAndPersistCorrectAnswer() {
        service.saveAdminPopup(popup, false, List.of(), List.of(question(List.of("b"))), "admin");
        verify(mapper).insertAdminQuestionTemplate(anyString(), eq("Quiz"), eq("QUIZ"), eq("admin"));
        verify(mapper).insertAdminOption(eq(30L), argThat(o -> o.value().equals("a")), eq("N"), eq("admin"));
        verify(mapper).insertAdminOption(eq(30L), argThat(o -> o.value().equals("b")), eq("Y"), eq("admin"));
        verify(mapper).upsertAdminPopupNotice(argThat(c -> c.questionTemplateId().equals(20L)));
    }
    @Test void unchangedQuestionsReuseExistingTemplate() {
        doReturn(List.of(question(List.of("a")))).when(service).getAdminQuestions(10L);
        service.saveAdminPopup(popup, false, List.of(), List.of(question(List.of("a"))), "admin");
        verify(mapper, never()).insertAdminQuestionTemplate(anyString(), anyString(), anyString(), anyString());
        verify(mapper).upsertAdminPopupNotice(argThat(c -> c.questionTemplateId().equals(10L)));
    }
    @Test void invalidCorrectAnswerDoesNotWriteAnything() {
        assertThrows(IllegalArgumentException.class, () ->
                service.saveAdminPopup(popup, false, List.of(), List.of(question(List.of("missing"))), "admin"));
        verify(mapper, never()).upsertAdminPopupNotice(any());
        verify(mapper, never()).insertAdminQuestionTemplate(anyString(), anyString(), anyString(), anyString());
    }
    @Test void singleChoiceRejectsMultipleCorrectAnswers() {
        assertThrows(IllegalArgumentException.class, () ->
                service.saveAdminPopup(popup, false, List.of(), List.of(question(List.of("a", "b"))), "admin"));
    }
    @Test void deletingAllQuestionsClearsTemplateReference() {
        service.saveAdminPopup(popup, false, List.of(), List.of(), "admin");
        verify(mapper).upsertAdminPopupNotice(argThat(c -> c.questionTemplateId() == null));
    }
}
