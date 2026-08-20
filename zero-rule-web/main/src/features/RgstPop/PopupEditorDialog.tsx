import handleError from '@/lib/handle-error';
import { useApi } from '@/provider';
import type {
  AdminPopupDetail,
  PopupDateValue,
  PopupDisplayMode,
  PopupSizeMode,
  PopupType,
} from '@local/domain';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControlLabel,
  LinearProgress,
  MenuItem,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import { useEffect, useState } from 'react';
import { toast } from 'react-toastify';

interface PopupEditorDialogProps {
  open: boolean;
  popupId: string | null;
  initialActive: boolean;
  onClose: () => void;
  onSaved: (popupId: string) => void;
}

const popupTypes: Array<{ value: PopupType; label: string }> = [
  { value: 'TEXT', label: '텍스트' },
  { value: 'IMAGE', label: '이미지' },
  { value: 'VIDEO', label: '영상' },
  { value: 'SURVEY', label: '설문' },
  { value: 'QUIZ', label: '퀴즈' },
];

function createDefaultPopup(): AdminPopupDetail {
  const startAt = new Date();
  const endAt = new Date(startAt);
  endAt.setMonth(endAt.getMonth() + 1);

  return {
    popupId: '',
    popupType: 'TEXT',
    title: '',
    displayStartAt: startAt.toISOString(),
    displayEndAt: endAt.toISOString(),
    displayMode: 'SEQUENTIAL',
    sizeMode: 'FIXED',
    width: 560,
    height: 420,
    widthRatio: 0.7,
    heightRatio: 0.75,
    minimumWidth: 480,
    minimumHeight: 320,
    maximumWidth: 1200,
    maximumHeight: 900,
    showHeader: true,
    showCloseButton: true,
    showFooter: true,
    showDoNotShowAgain: false,
    questionTemplateId: null,
    periodMode: 'FIXED',
    repeatInterval: null,
    repeatDayOfWeek: null,
    repeatDayOfMonth: null,
    hideDays: null,
    completionRatio: null,
    passingScore: null,
    allowCloseBeforeComplete: true,
    questions: [],
    content: {
      contentTitle: '',
      description: '',
      leftSectionBody: '',
    },
  };
}

function dateFromApi(value: PopupDateValue): Date {
  if (typeof value === 'number') {
    return new Date(value < 1_000_000_000_000 ? value * 1000 : value);
  }
  return new Date(value);
}

function toDateTimeLocal(value: PopupDateValue): string {
  const date = dateFromApi(value);
  if (Number.isNaN(date.getTime())) return '';
  const localTime = new Date(date.getTime() - date.getTimezoneOffset() * 60_000);
  return localTime.toISOString().slice(0, 16);
}

function toApiDate(value: string): string {
  return new Date(value).toISOString();
}

function contentTitleKey(popupType: PopupType): string {
  if (popupType === 'IMAGE') return 'imageTitle';
  if (popupType === 'VIDEO') return 'videoTitle';
  if (popupType === 'SURVEY' || popupType === 'QUIZ') return 'surveyTitle';
  return 'contentTitle';
}

function contentValue(popup: AdminPopupDetail, key: string): string {
  const value = popup.content[key];
  return value == null ? '' : String(value);
}

export default function PopupEditorDialog({
  open,
  popupId,
  initialActive,
  onClose,
  onSaved,
}: PopupEditorDialogProps) {
  const api = useApi();
  const [popup, setPopup] = useState<AdminPopupDetail>(createDefaultPopup);
  const [active, setActive] = useState(true);
  const [loading, setLoading] = useState(false);
  const editing = popupId != null;

  useEffect(() => {
    if (!open) return;
    setActive(initialActive);

    if (popupId == null) {
      setPopup(createDefaultPopup());
      return;
    }

    let canceled = false;
    setLoading(true);
    api.popupAdmin
      .info({ popupId })
      .then(({ body }) => {
        if (!canceled) setPopup(body.popup);
      })
      .catch((error) => {
        if (!canceled) toast.error(handleError(error));
      })
      .finally(() => {
        if (!canceled) setLoading(false);
      });

    return () => {
      canceled = true;
    };
  }, [api, initialActive, open, popupId]);

  const updatePopup = <K extends keyof AdminPopupDetail>(
    key: K,
    value: AdminPopupDetail[K],
  ) => {
    setPopup((previous) => ({ ...previous, [key]: value }));
  };

  const updateContent = (key: string, value: string) => {
    setPopup((previous) => ({
      ...previous,
      content: { ...previous.content, [key]: value },
    }));
  };

  const savePopup = async () => {
    if (!popup.popupId.trim() || !popup.title.trim()) {
      toast.warn('팝업 ID와 제목을 입력해 주세요.');
      return;
    }

    try {
      setLoading(true);
      const requestPopup: AdminPopupDetail = {
        ...popup,
        popupId: popup.popupId.trim(),
        title: popup.title.trim(),
        displayStartAt: toApiDate(toDateTimeLocal(popup.displayStartAt)),
        displayEndAt: toApiDate(toDateTimeLocal(popup.displayEndAt)),
      };
      const { body } = await api.popupAdmin.save({ popup: requestPopup, active });
      toast.success('팝업을 저장했습니다.');
      onSaved(body.popup.popupId);
    } catch (error) {
      toast.error(handleError(error));
    } finally {
      setLoading(false);
    }
  };

  const titleKey = contentTitleKey(popup.popupType);
  const isMedia = popup.popupType === 'IMAGE' || popup.popupType === 'VIDEO';
  const isSurvey = popup.popupType === 'SURVEY' || popup.popupType === 'QUIZ';

  return (
    <Dialog open={open} onClose={loading ? undefined : onClose} fullWidth maxWidth="lg">
      <DialogTitle>{editing ? '팝업 수정' : '팝업 신규 등록'}</DialogTitle>
      {loading && <LinearProgress />}
      <DialogContent dividers>
        <Stack spacing={2}>
          <Typography variant="subtitle1" fontWeight={700}>
            기본 정보
          </Typography>
          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: 2 }}>
            <TextField
              required
              label="팝업 ID"
              value={popup.popupId}
              disabled={editing}
              inputProps={{ maxLength: 50 }}
              onChange={(event) => updatePopup('popupId', event.target.value)}
            />
            <TextField
              required
              label="팝업 제목"
              value={popup.title}
              inputProps={{ maxLength: 200 }}
              onChange={(event) => updatePopup('title', event.target.value)}
            />
            <TextField
              select
              label="팝업 유형"
              value={popup.popupType}
              onChange={(event) => updatePopup('popupType', event.target.value as PopupType)}
            >
              {popupTypes.map((type) => (
                <MenuItem key={type.value} value={type.value}>
                  {type.label}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              select
              label="표시 방식"
              value={popup.displayMode}
              onChange={(event) =>
                updatePopup('displayMode', event.target.value as PopupDisplayMode)
              }
            >
              <MenuItem value="SEQUENTIAL">순차 표시</MenuItem>
              <MenuItem value="SIMULTANEOUS">동시 표시</MenuItem>
            </TextField>
            <TextField
              type="datetime-local"
              label="노출 시작"
              value={toDateTimeLocal(popup.displayStartAt)}
              InputLabelProps={{ shrink: true }}
              onChange={(event) => updatePopup('displayStartAt', event.target.value)}
            />
            <TextField
              type="datetime-local"
              label="노출 종료"
              value={toDateTimeLocal(popup.displayEndAt)}
              InputLabelProps={{ shrink: true }}
              onChange={(event) => updatePopup('displayEndAt', event.target.value)}
            />
          </Box>

          <Divider />
          <Typography variant="subtitle1" fontWeight={700}>
            크기 설정
          </Typography>
          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(3, minmax(0, 1fr))', gap: 2 }}>
            <TextField
              select
              label="크기 모드"
              value={popup.sizeMode}
              onChange={(event) => updatePopup('sizeMode', event.target.value as PopupSizeMode)}
            >
              <MenuItem value="FIXED">고정 크기</MenuItem>
              <MenuItem value="RATIO">화면 비율</MenuItem>
              <MenuItem value="FULLSCREEN">전체 화면</MenuItem>
            </TextField>
            {popup.sizeMode === 'FIXED' && (
              <>
                <TextField
                  type="number"
                  label="너비"
                  value={popup.width}
                  onChange={(event) => updatePopup('width', Number(event.target.value))}
                />
                <TextField
                  type="number"
                  label="높이"
                  value={popup.height}
                  onChange={(event) => updatePopup('height', Number(event.target.value))}
                />
              </>
            )}
            {popup.sizeMode === 'RATIO' && (
              <>
                <TextField
                  type="number"
                  label="너비 비율"
                  value={popup.widthRatio}
                  inputProps={{ min: 0.1, max: 1, step: 0.05 }}
                  onChange={(event) => updatePopup('widthRatio', Number(event.target.value))}
                />
                <TextField
                  type="number"
                  label="높이 비율"
                  value={popup.heightRatio}
                  inputProps={{ min: 0.1, max: 1, step: 0.05 }}
                  onChange={(event) => updatePopup('heightRatio', Number(event.target.value))}
                />
              </>
            )}
          </Box>

          <Stack direction="row" flexWrap="wrap" gap={1}>
            <FormControlLabel
              control={<Switch checked={active} onChange={(_, value) => setActive(value)} />}
              label="활성"
            />
            <FormControlLabel
              control={
                <Switch
                  checked={popup.showHeader}
                  onChange={(_, value) => updatePopup('showHeader', value)}
                />
              }
              label="헤더 표시"
            />
            <FormControlLabel
              control={
                <Switch
                  checked={popup.showCloseButton}
                  onChange={(_, value) => updatePopup('showCloseButton', value)}
                />
              }
              label="닫기 표시"
            />
            <FormControlLabel
              control={
                <Switch
                  checked={popup.showFooter}
                  onChange={(_, value) => updatePopup('showFooter', value)}
                />
              }
              label="푸터 표시"
            />
            <FormControlLabel
              control={
                <Switch
                  checked={popup.showDoNotShowAgain}
                  onChange={(_, value) => updatePopup('showDoNotShowAgain', value)}
                />
              }
              label="다시 보지 않기"
            />
          </Stack>

          <Divider />
          <Typography variant="subtitle1" fontWeight={700}>
            콘텐츠
          </Typography>
          <TextField
            label="콘텐츠 제목"
            value={contentValue(popup, titleKey)}
            onChange={(event) => updateContent(titleKey, event.target.value)}
          />
          <TextField
            label="설명"
            value={contentValue(popup, 'description')}
            multiline
            minRows={2}
            onChange={(event) => updateContent('description', event.target.value)}
          />
          {popup.popupType === 'TEXT' && (
            <TextField
              label="본문"
              value={contentValue(popup, 'leftSectionBody')}
              multiline
              minRows={5}
              onChange={(event) => updateContent('leftSectionBody', event.target.value)}
            />
          )}
          {isMedia && (
            <TextField
              label={popup.popupType === 'IMAGE' ? '이미지 URL' : '영상 URL'}
              value={contentValue(popup, popup.popupType === 'IMAGE' ? 'imageUrl' : 'videoUrl')}
              onChange={(event) =>
                updateContent(
                  popup.popupType === 'IMAGE' ? 'imageUrl' : 'videoUrl',
                  event.target.value,
                )
              }
            />
          )}
          {isSurvey && (
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
              <TextField
                type="number"
                label="문항 템플릿 ID"
                value={popup.questionTemplateId ?? ''}
                onChange={(event) =>
                  updatePopup(
                    'questionTemplateId',
                    event.target.value ? Number(event.target.value) : null,
                  )
                }
              />
              <TextField
                type="number"
                label="통과 점수"
                value={popup.passingScore ?? ''}
                onChange={(event) =>
                  updatePopup(
                    'passingScore',
                    event.target.value ? Number(event.target.value) : null,
                  )
                }
              />
            </Box>
          )}
          {popup.popupType === 'VIDEO' && (
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
              <TextField
                type="number"
                label="완료 비율"
                value={popup.completionRatio ?? ''}
                inputProps={{ min: 0, max: 1, step: 0.05 }}
                onChange={(event) =>
                  updatePopup(
                    'completionRatio',
                    event.target.value ? Number(event.target.value) : null,
                  )
                }
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={popup.allowCloseBeforeComplete}
                    onChange={(_, value) => updatePopup('allowCloseBeforeComplete', value)}
                  />
                }
                label="완료 전 닫기 허용"
              />
            </Box>
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>
          취소
        </Button>
        <Button variant="contained" onClick={savePopup} disabled={loading}>
          저장
        </Button>
      </DialogActions>
    </Dialog>
  );
}
