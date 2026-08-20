import handleError from '@/lib/handle-error';
import { useApi } from '@/provider';
import type {
  AdminPopupDetail,
  PopupDateValue,
  PopupDisplayMode,
  PopupSizeMode,
  PopupTargetCondition,
  PopupTargetConditionType,
  PopupTargetGroup,
  PopupType,
} from '@local/domain';
import AddIcon from '@mui/icons-material/Add';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import PreviewIcon from '@mui/icons-material/Preview';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControlLabel,
  IconButton,
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
      showContentHeader: true,
      plainText: '',
      showPlainText: true,
      leftSectionTitle: '',
      leftSectionBody: '',
      showLeftSection: false,
      highlightText: '',
      showHighlight: false,
      rightSectionTitle: '',
      rightSectionBody: '',
      additionalDescription: '',
      showRightSection: false,
      bottomDescription: '',
      showBottomDescription: false,
      markdownMode: false,
      markdownContent: '',
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
  const [targetGroups, setTargetGroups] = useState<PopupTargetGroup[]>([]);
  const [previewOpen, setPreviewOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const editing = popupId != null;

  useEffect(() => {
    if (!open) return;
    setActive(initialActive);

    if (popupId == null) {
      setPopup(createDefaultPopup());
      setTargetGroups([]);
      return;
    }

    let canceled = false;
    setLoading(true);
    api.popupAdmin
      .info({ popupId })
      .then(({ body }) => {
        if (!canceled) {
          setPopup(body.popup);
          setTargetGroups(body.targetGroups ?? []);
        }
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

  const updateFooter = (showFooter: boolean) => {
    setPopup((previous) => ({
      ...previous,
      showFooter,
      showDoNotShowAgain: showFooter ? previous.showDoNotShowAgain : false,
    }));
  };

  const addTargetGroup = () => {
    setTargetGroups((groups) => [
      ...groups,
      {
        targetName: `대상 그룹 ${groups.length + 1}`,
        targetDescription: '',
        conditions: [
          { conditionType: 'EMPLOYEE', conditionOperator: '=', value: '', includeChild: false },
        ],
      },
    ]);
  };

  const removeTargetGroup = (groupIndex: number) => {
    setTargetGroups((groups) => groups.filter((_, index) => index !== groupIndex));
  };

  const updateTargetGroup = (groupIndex: number, patch: Partial<PopupTargetGroup>) => {
    setTargetGroups((groups) =>
      groups.map((group, index) => index === groupIndex ? { ...group, ...patch } : group),
    );
  };

  const addTargetCondition = (groupIndex: number) => {
    const condition: PopupTargetCondition = {
      conditionType: 'EMPLOYEE', conditionOperator: '=', value: '', includeChild: false,
    };
    setTargetGroups((groups) => groups.map((group, index) =>
      index === groupIndex
        ? { ...group, conditions: [...group.conditions, condition] }
        : group,
    ));
  };

  const updateTargetCondition = (
    groupIndex: number,
    conditionIndex: number,
    patch: Partial<PopupTargetCondition>,
  ) => {
    setTargetGroups((groups) => groups.map((group, index) =>
      index === groupIndex
        ? {
            ...group,
            conditions: group.conditions.map((condition, currentIndex) =>
              currentIndex === conditionIndex ? { ...condition, ...patch } : condition,
            ),
          }
        : group,
    ));
  };

  const removeTargetCondition = (groupIndex: number, conditionIndex: number) => {
    setTargetGroups((groups) => groups.map((group, index) =>
      index === groupIndex
        ? { ...group, conditions: group.conditions.filter((_, i) => i !== conditionIndex) }
        : group,
    ));
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
      const hasInvalidTarget = targetGroups.some(
        (group) => group.conditions.length === 0
          || group.conditions.some((condition) => !condition.value.trim()),
      );
      if (active && targetGroups.length === 0) {
        toast.warn('활성 팝업은 대상 조건 그룹을 한 개 이상 추가해 주세요.');
        return;
      }
      if (hasInvalidTarget) {
        toast.warn('대상 그룹의 모든 조건 값을 입력해 주세요.');
        return;
      }
      const { body } = await api.popupAdmin.save({
        popup: requestPopup,
        active,
        targetGroups,
      });
      toast.success('팝업을 저장했습니다.');
      onSaved(body.popup.popupId);
    } catch (error) {
      handleError(error);
    } finally {
      setLoading(false);
    }
  };

  const previewDialogSize = () => {
    const availableWidth = window.innerWidth;
    const availableHeight = window.innerHeight;
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
    return {
      width: Math.round(Math.max(320, Math.min(availableWidth * 0.96, width))),
      height: Math.round(Math.max(260, Math.min(availableHeight * 0.96, height))),
    };
  };

  const titleKey = contentTitleKey(popup.popupType);
  const isMedia = popup.popupType === 'IMAGE' || popup.popupType === 'VIDEO';
  const isSurvey = popup.popupType === 'SURVEY' || popup.popupType === 'QUIZ';
  const showTextRightSection =
    popup.content.showRightSection == null
      ? Boolean(
          contentValue(popup, 'rightSectionTitle') ||
            contentValue(popup, 'rightSectionBody') ||
            contentValue(popup, 'additionalDescription'),
        )
      : popup.content.showRightSection === true;
  const showTextHighlight =
    popup.content.showHighlight == null
      ? Boolean(contentValue(popup, 'highlightText'))
      : popup.content.showHighlight === true;
  const showTextContentHeader = popup.content.showContentHeader !== false;
  const showTextPlainText = popup.content.showPlainText !== false;
  const showTextLeftSection =
    popup.content.showLeftSection == null
      ? Boolean(contentValue(popup, 'leftSectionTitle') || contentValue(popup, 'leftSectionBody'))
      : popup.content.showLeftSection === true;
  const showTextBottomDescription =
    popup.content.showBottomDescription == null
      ? Boolean(contentValue(popup, 'bottomDescription'))
      : popup.content.showBottomDescription === true;
  const markdownMode = popup.content.markdownMode === true;
  const modalPreviewSize = previewOpen && typeof window !== 'undefined'
    ? previewDialogSize()
    : { width: popup.width, height: popup.height };

  return (
    <Dialog open={open} onClose={loading ? undefined : onClose} fullWidth maxWidth="xl">
      <DialogTitle>{editing ? '팝업 수정' : '팝업 신규 등록'}</DialogTitle>
      {loading && <LinearProgress />}
      <DialogContent dividers sx={{ p: 0, overflow: 'hidden' }}>
        <Box sx={{ display: 'grid', gridTemplateColumns: 'minmax(520px, 1fr) minmax(520px, 1fr)', height: 'calc(100vh - 150px)' }}>
          <Box sx={{ overflowY: 'auto', p: 3 }}>
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
            노출 대상
          </Typography>
          <Typography variant="caption" color="text.secondary">
            같은 그룹의 조건은 모두 충족(AND), 그룹 사이는 하나만 충족(OR)하면 노출됩니다.
          </Typography>
          {targetGroups.map((group, groupIndex) => (
            <Box
              key={`target-group-${groupIndex}`}
              sx={{ border: '1px solid', borderColor: 'divider', borderRadius: 1, p: 2 }}
            >
              <Stack spacing={1.5}>
                <Stack direction="row" spacing={1} alignItems="center">
                  <TextField
                    size="small"
                    label={`그룹 ${groupIndex + 1} 이름`}
                    value={group.targetName}
                    onChange={(event) =>
                      updateTargetGroup(groupIndex, { targetName: event.target.value })
                    }
                    sx={{ flex: 1 }}
                  />
                  <TextField
                    size="small"
                    label="설명"
                    value={group.targetDescription}
                    onChange={(event) =>
                      updateTargetGroup(groupIndex, { targetDescription: event.target.value })
                    }
                    sx={{ flex: 2 }}
                  />
                  <IconButton
                    aria-label="대상 그룹 삭제"
                    onClick={() => removeTargetGroup(groupIndex)}
                  >
                    <DeleteOutlineIcon />
                  </IconButton>
                </Stack>

                {group.conditions.map((condition, conditionIndex) => {
                  const dateCondition = condition.conditionType === 'HIRE_DATE';
                  return (
                    <Stack
                      key={`target-condition-${groupIndex}-${conditionIndex}`}
                      direction="row"
                      spacing={1}
                      alignItems="center"
                    >
                      <TextField
                        select
                        size="small"
                        label="조건 유형"
                        value={condition.conditionType}
                        onChange={(event) => {
                          const conditionType = event.target.value as PopupTargetConditionType;
                          updateTargetCondition(groupIndex, conditionIndex, {
                            conditionType,
                            conditionOperator: '=',
                            value: '',
                            includeChild: false,
                          });
                        }}
                        sx={{ width: 150 }}
                      >
                        <MenuItem value="DEPARTMENT">부서</MenuItem>
                        <MenuItem value="POSITION">직급</MenuItem>
                        <MenuItem value="EMPLOYEE">사번</MenuItem>
                        <MenuItem value="HIRE_DATE">입사일</MenuItem>
                      </TextField>
                      <TextField
                        select
                        size="small"
                        label="비교"
                        value={condition.conditionOperator}
                        onChange={(event) => updateTargetCondition(groupIndex, conditionIndex, {
                          conditionOperator: event.target.value as PopupTargetCondition['conditionOperator'],
                        })}
                        sx={{ width: 100 }}
                      >
                        <MenuItem value="=">같음</MenuItem>
                        <MenuItem value="!=">같지 않음</MenuItem>
                        {dateCondition && <MenuItem value="<">이전</MenuItem>}
                        {dateCondition && <MenuItem value="<=">이전 또는 당일</MenuItem>}
                        {dateCondition && <MenuItem value=">">이후</MenuItem>}
                        {dateCondition && <MenuItem value=">=">이후 또는 당일</MenuItem>}
                      </TextField>
                      <TextField
                        size="small"
                        type={dateCondition ? 'date' : 'text'}
                        label={dateCondition ? '기준 입사일' : '조건 값'}
                        value={condition.value}
                        InputLabelProps={dateCondition ? { shrink: true } : undefined}
                        placeholder={
                          condition.conditionType === 'DEPARTMENT' ? '부서 ID'
                            : condition.conditionType === 'POSITION' ? '직급 ID'
                              : condition.conditionType === 'EMPLOYEE' ? 'E1002'
                                : undefined
                        }
                        onChange={(event) => updateTargetCondition(groupIndex, conditionIndex, {
                          value: event.target.value,
                        })}
                        sx={{ flex: 1 }}
                      />
                      {condition.conditionType === 'DEPARTMENT' && (
                        <FormControlLabel
                          control={(
                            <Switch
                              size="small"
                              checked={condition.includeChild}
                              onChange={(_, value) => updateTargetCondition(
                                groupIndex, conditionIndex, { includeChild: value },
                              )}
                            />
                          )}
                          label="하위 포함"
                        />
                      )}
                      <IconButton
                        aria-label="대상 조건 삭제"
                        onClick={() => removeTargetCondition(groupIndex, conditionIndex)}
                      >
                        <DeleteOutlineIcon />
                      </IconButton>
                    </Stack>
                  );
                })}

                <Button
                  size="small"
                  startIcon={<AddIcon />}
                  onClick={() => addTargetCondition(groupIndex)}
                  sx={{ alignSelf: 'flex-start' }}
                >
                  AND 조건 추가
                </Button>
              </Stack>
            </Box>
          ))}
          <Button
            variant="outlined"
            startIcon={<AddIcon />}
            onClick={addTargetGroup}
            sx={{ alignSelf: 'flex-start' }}
          >
            OR 대상 그룹 추가
          </Button>

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
              label={
                <Box>
                  <Typography variant="body2">팝업 활성화</Typography>
                  <Typography variant="caption" color="text.secondary">
                    켜면 사용자 팝업 조회 대상에 포함됩니다.
                  </Typography>
                </Box>
              }
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
                  onChange={(_, value) => updateFooter(value)}
                />
              }
              label="푸터 표시"
            />
            <FormControlLabel
              control={
                <Switch
                  checked={popup.showDoNotShowAgain}
                  disabled={!popup.showFooter}
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
            disabled={popup.popupType === 'TEXT' && !showTextContentHeader}
            value={contentValue(popup, titleKey)}
            onChange={(event) => updateContent(titleKey, event.target.value)}
          />
          <TextField
            label="설명"
            disabled={popup.popupType === 'TEXT' && !showTextContentHeader}
            value={contentValue(popup, 'description')}
            multiline
            minRows={2}
            onChange={(event) => updateContent('description', event.target.value)}
          />
          {popup.popupType === 'TEXT' && (
            <Stack spacing={2}>
              <Stack direction="row" flexWrap="wrap" gap={1}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={showTextContentHeader}
                      onChange={(_, value) => updateContent('showContentHeader', value)}
                    />
                  }
                  label="콘텐츠 제목·설명"
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={markdownMode}
                      onChange={(_, value) => updateContent('markdownMode', value)}
                    />
                  }
                  label="Markdown 모드"
                />
                {!markdownMode && <FormControlLabel
                  control={
                    <Switch
                      checked={showTextPlainText}
                      onChange={(_, value) => updateContent('showPlainText', value)}
                    />
                  }
                  label="일반 텍스트"
                />}
                {!markdownMode && <FormControlLabel
                  control={
                    <Switch
                      checked={showTextLeftSection}
                      onChange={(_, value) => updateContent('showLeftSection', value)}
                    />
                  }
                  label="왼쪽 카드"
                />}
                {!markdownMode && (
                <FormControlLabel
                  control={
                    <Switch
                      checked={showTextRightSection}
                      onChange={(_, value) => updateContent('showRightSection', value)}
                    />
                  }
                  label="오른쪽 카드 사용"
                />
                )}
                {!markdownMode && (
                <FormControlLabel
                  control={
                    <Switch
                      checked={showTextHighlight}
                      onChange={(_, value) => updateContent('showHighlight', value)}
                    />
                  }
                  label="강조 문구 사용"
                />
                )}
                {!markdownMode && <FormControlLabel
                  control={
                    <Switch
                      checked={showTextBottomDescription}
                      onChange={(_, value) => updateContent('showBottomDescription', value)}
                    />
                  }
                  label="하단 설명"
                />}
              </Stack>
              {markdownMode ? (
                <TextField
                  label="Markdown 내용"
                  value={contentValue(popup, 'markdownContent')}
                  multiline
                  minRows={14}
                  placeholder={'# 제목\n\n일반 문장과 **강조 문장**\n\n- 목록 1\n- 목록 2'}
                  onChange={(event) => updateContent('markdownContent', event.target.value)}
                />
              ) : (
              <Stack spacing={2}>
              <TextField
                label="일반 텍스트"
                disabled={!showTextPlainText}
                value={contentValue(popup, 'plainText')}
                multiline
                minRows={4}
                onChange={(event) => updateContent('plainText', event.target.value)}
              />
              <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
                <TextField
                  label="왼쪽 카드 제목"
                  disabled={!showTextLeftSection}
                  value={contentValue(popup, 'leftSectionTitle')}
                  onChange={(event) => updateContent('leftSectionTitle', event.target.value)}
                />
                <TextField
                  label="오른쪽 카드 제목"
                  disabled={!showTextRightSection}
                  value={contentValue(popup, 'rightSectionTitle')}
                  onChange={(event) => updateContent('rightSectionTitle', event.target.value)}
                />
                <TextField
                  label="왼쪽 카드 본문"
                  disabled={!showTextLeftSection}
                  value={contentValue(popup, 'leftSectionBody')}
                  multiline
                  minRows={4}
                  onChange={(event) => updateContent('leftSectionBody', event.target.value)}
                />
                <TextField
                  label="오른쪽 카드 본문"
                  disabled={!showTextRightSection}
                  value={contentValue(popup, 'rightSectionBody')}
                  multiline
                  minRows={4}
                  onChange={(event) => updateContent('rightSectionBody', event.target.value)}
                />
              </Box>
              <TextField
                label="강조 문구"
                disabled={!showTextHighlight}
                value={contentValue(popup, 'highlightText')}
                onChange={(event) => updateContent('highlightText', event.target.value)}
              />
              <TextField
                label="추가 설명"
                disabled={!showTextRightSection}
                value={contentValue(popup, 'additionalDescription')}
                multiline
                minRows={2}
                onChange={(event) => updateContent('additionalDescription', event.target.value)}
              />
              <TextField
                label="하단 설명"
                disabled={!showTextBottomDescription}
                value={contentValue(popup, 'bottomDescription')}
                multiline
                minRows={2}
                onChange={(event) => updateContent('bottomDescription', event.target.value)}
              />
              </Stack>
              )}
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

            </Stack>
          </Box>
          <Box sx={{ minWidth: 0, overflow: 'hidden', p: 2, bgcolor: '#f3f5f9', borderLeft: '1px solid', borderColor: 'divider' }}>
            <Stack spacing={1.5} sx={{ height: '100%' }}>
              <Stack direction="row" alignItems="center" justifyContent="space-between">
            <Typography variant="subtitle1" fontWeight={700}>
              팝업 미리보기
            </Typography>
              <Button
                size="small"
                variant="outlined"
                startIcon={<PreviewIcon />}
                onClick={() => setPreviewOpen(true)}
              >
                실제 크기로 보기
              </Button>
              </Stack>
              <PopupPreview popup={popup} fitContainer />
            </Stack>
          </Box>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={loading}>
          취소
        </Button>
        <Button variant="contained" onClick={savePopup} disabled={loading}>
          저장
        </Button>
      </DialogActions>
      <Dialog
        open={previewOpen}
        onClose={() => setPreviewOpen(false)}
        fullScreen={popup.sizeMode === 'FULLSCREEN'}
        maxWidth={false}
        PaperProps={
          popup.sizeMode === 'FULLSCREEN'
            ? undefined
            : {
                sx: {
                  width: modalPreviewSize.width,
                  height: modalPreviewSize.height,
                  maxWidth: '96vw',
                  maxHeight: '96vh',
                  m: 1,
                  overflow: 'hidden',
                },
              }
        }
      >
        <DialogContent sx={{ p: 0, overflow: 'hidden' }}>
          <PopupPreview
            popup={popup}
            fitContainer
            onClose={() => setPreviewOpen(false)}
          />
        </DialogContent>
      </Dialog>
    </Dialog>
  );
}
