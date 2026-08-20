import handleError from '@/lib/handle-error';
import { useApi } from '@/provider';
import type {
  AdminPopupDetail,
  PopupDateValue,
  PopupDisplayMode,
  PopupSizeMode,
  PopupType,
} from '@local/domain';
import OpenInNewIcon from '@mui/icons-material/OpenInNew';
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
import PopupPreview from './PopupPreview';

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
      leftSectionTitle: '',
      leftSectionBody: '',
      highlightText: '',
      rightSectionTitle: '',
      rightSectionBody: '',
      additionalDescription: '',
      showDescription: true,
      imageSizeMode: 'FIXED',
      imageWidth: 0,
      imageHeight: 0,
      linkUrl: '',
      showControls: true,
      allowFullScreen: true,
      allowPlaybackRateChange: true,
      autoPlay: false,
      isLoop: false,
      defaultVolume: 0.7,
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
        if (!canceled) handleError(error);
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

  const updateContent = (key: string, value: unknown) => {
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
      handleError(error);
    } finally {
      setLoading(false);
    }
  };

  const openPreviewWindow = () => {
    const storageKey = `popup-admin-preview:${Date.now()}`;
    localStorage.setItem(storageKey, JSON.stringify(popup));

    const availableWidth = window.screen.availWidth;
    const availableHeight = window.screen.availHeight;
    const requestedWidth =
      popup.sizeMode === 'FULLSCREEN'
        ? availableWidth
        : popup.sizeMode === 'RATIO'
          ? availableWidth * popup.widthRatio
          : popup.width;
    const requestedHeight =
      popup.sizeMode === 'FULLSCREEN'
        ? availableHeight
        : popup.sizeMode === 'RATIO'
          ? availableHeight * popup.heightRatio
          : popup.height;
    const width =
      popup.sizeMode === 'FULLSCREEN'
        ? requestedWidth
        : Math.max(popup.minimumWidth, Math.min(popup.maximumWidth, requestedWidth));
    const height =
      popup.sizeMode === 'FULLSCREEN'
        ? requestedHeight
        : Math.max(popup.minimumHeight, Math.min(popup.maximumHeight, requestedHeight));
    const windowWidth = Math.round(Math.max(320, Math.min(availableWidth, width)));
    const windowHeight = Math.round(Math.max(260, Math.min(availableHeight, height)));
    const left = Math.max(0, Math.round((availableWidth - windowWidth) / 2));
    const top = Math.max(0, Math.round((availableHeight - windowHeight) / 2));
    const url = `/popup-preview/?key=${encodeURIComponent(storageKey)}`;
    const previewWindow = window.open(
      url,
      `popup-preview-${popup.popupId || 'new'}`,
      `popup=yes,width=${windowWidth},height=${windowHeight},left=${left},top=${top}`,
    );

    if (previewWindow == null) {
      localStorage.removeItem(storageKey);
      toast.warn('브라우저에서 팝업을 허용해 주세요.');
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
            <Stack spacing={2}>
              <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
                <TextField
                  label="왼쪽 카드 제목"
                  value={contentValue(popup, 'leftSectionTitle')}
                  onChange={(event) => updateContent('leftSectionTitle', event.target.value)}
                />
                <TextField
                  label="오른쪽 카드 제목"
                  value={contentValue(popup, 'rightSectionTitle')}
                  onChange={(event) => updateContent('rightSectionTitle', event.target.value)}
                />
                <TextField
                  label="왼쪽 카드 본문"
                  value={contentValue(popup, 'leftSectionBody')}
                  multiline
                  minRows={4}
                  onChange={(event) => updateContent('leftSectionBody', event.target.value)}
                />
                <TextField
                  label="오른쪽 카드 본문"
                  value={contentValue(popup, 'rightSectionBody')}
                  multiline
                  minRows={4}
                  onChange={(event) => updateContent('rightSectionBody', event.target.value)}
                />
              </Box>
              <TextField
                label="강조 문구"
                value={contentValue(popup, 'highlightText')}
                onChange={(event) => updateContent('highlightText', event.target.value)}
              />
              <TextField
                label="추가 설명"
                value={contentValue(popup, 'additionalDescription')}
                multiline
                minRows={2}
                onChange={(event) => updateContent('additionalDescription', event.target.value)}
              />
            </Stack>
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
          {popup.popupType === 'IMAGE' && (
            <Stack spacing={2}>
              <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 2 }}>
                <TextField
                  select
                  label="이미지 크기 모드"
                  value={contentValue(popup, 'imageSizeMode') || 'FIXED'}
                  onChange={(event) => updateContent('imageSizeMode', event.target.value)}
                >
                  <MenuItem value="FIXED">고정 영역</MenuItem>
                  <MenuItem value="FIT_TO_IMAGE">원본에 맞춤</MenuItem>
                  <MenuItem value="ADAPTIVE">화면에 맞춤</MenuItem>
                </TextField>
                <TextField
                  type="number"
                  label="이미지 너비"
                  value={contentValue(popup, 'imageWidth')}
                  onChange={(event) => updateContent('imageWidth', Number(event.target.value))}
                />
                <TextField
                  type="number"
                  label="이미지 높이"
                  value={contentValue(popup, 'imageHeight')}
                  onChange={(event) => updateContent('imageHeight', Number(event.target.value))}
                />
              </Box>
              <TextField
                label="클릭 연결 URL"
                value={contentValue(popup, 'linkUrl')}
                onChange={(event) => updateContent('linkUrl', event.target.value)}
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={popup.content.showDescription !== false}
                    onChange={(_, value) => updateContent('showDescription', value)}
                  />
                }
                label="이미지 설명 표시"
              />
            </Stack>
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
            <Stack spacing={2}>
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
                <TextField
                  type="number"
                  label="기본 음량"
                  value={contentValue(popup, 'defaultVolume')}
                  inputProps={{ min: 0, max: 1, step: 0.1 }}
                  onChange={(event) => updateContent('defaultVolume', Number(event.target.value))}
                />
              </Box>
              <Stack direction="row" flexWrap="wrap" gap={1}>
                {[
                  ['showDescription', '영상 설명 표시'],
                  ['showControls', '컨트롤 표시'],
                  ['allowFullScreen', '전체화면 허용'],
                  ['allowPlaybackRateChange', '배속 변경 허용'],
                  ['autoPlay', '자동 재생'],
                  ['isLoop', '반복 재생'],
                ].map(([key, label]) => (
                  <FormControlLabel
                    key={key}
                    control={
                      <Switch
                        checked={
                          popup.content[key] == null
                            ? key !== 'autoPlay' && key !== 'isLoop'
                            : popup.content[key] === true
                        }
                        onChange={(_, value) => updateContent(key, value)}
                      />
                    }
                    label={label}
                  />
                ))}
                <FormControlLabel
                  control={
                    <Switch
                      checked={popup.allowCloseBeforeComplete}
                      onChange={(_, value) => updatePopup('allowCloseBeforeComplete', value)}
                    />
                  }
                  label="완료 전 닫기 허용"
                />
              </Stack>
            </Stack>
          )}

          <Divider />
          <Stack direction="row" alignItems="center" justifyContent="space-between">
            <Typography variant="subtitle1" fontWeight={700}>
              팝업 미리보기
            </Typography>
            <Stack direction="row" spacing={1} alignItems="center">
              <Typography variant="caption" color="text.secondary">
                입력값과 표시 옵션이 실시간으로 반영됩니다.
              </Typography>
              <Button size="small" variant="outlined" startIcon={<OpenInNewIcon />} onClick={openPreviewWindow}>
                새 창으로 보기
              </Button>
            </Stack>
          </Stack>
          <PopupPreview popup={popup} />
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
