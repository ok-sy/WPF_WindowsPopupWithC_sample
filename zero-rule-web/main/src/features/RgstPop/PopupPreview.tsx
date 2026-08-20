import type { AdminPopupDetail } from '@local/domain';
import CloseIcon from '@mui/icons-material/Close';
import PlayCircleOutlineIcon from '@mui/icons-material/PlayCircleOutline';
import {
  Box,
  Button,
  Checkbox,
  Divider,
  FormControlLabel,
  IconButton,
  Paper,
  Radio,
  Stack,
  Typography,
} from '@mui/material';

interface PopupPreviewProps {
  popup: AdminPopupDetail;
  standalone?: boolean;
  fitContainer?: boolean;
  onClose?: () => void;
}

function text(value: unknown, fallback: string): string {
  return value == null || String(value).trim() === '' ? fallback : String(value);
}

function titleKey(type: AdminPopupDetail['popupType']): string {
  if (type === 'IMAGE') return 'imageTitle';
  if (type === 'VIDEO') return 'videoTitle';
  if (type === 'SURVEY' || type === 'QUIZ') return 'surveyTitle';
  return 'contentTitle';
}

function previewSize(popup: AdminPopupDetail) {
  if (popup.sizeMode === 'FULLSCREEN') return { width: '100%', height: 520 };
  if (popup.sizeMode === 'RATIO') {
    return {
      width: `${Math.max(25, Math.min(100, popup.widthRatio * 100))}%`,
      height: Math.max(280, Math.min(520, popup.heightRatio * 520)),
    };
  }

  const scale = Math.min(1, 760 / Math.max(popup.width, 1), 520 / Math.max(popup.height, 1));
  return {
    width: Math.max(320, popup.width * scale),
    height: Math.max(260, popup.height * scale),
  };
}

function PopupBody({ popup }: PopupPreviewProps) {
  const content = popup.content;
  const description = text(content.description, '팝업 설명이 여기에 표시됩니다.');

  if (popup.popupType === 'IMAGE') {
    const imageUrl = text(content.imageUrl, '');
    const showDescription = content.showDescription !== false;
    const imageWidth = Number(content.imageWidth) || undefined;
    const imageHeight = Number(content.imageHeight) || undefined;
    return (
      <Stack spacing={1.5} alignItems="center" sx={{ height: '100%' }}>
        {showDescription && <Typography color="text.secondary">{description}</Typography>}
        {imageUrl ? (
          <Box
            component="img"
            src={imageUrl}
            alt="팝업 이미지 미리보기"
            sx={{
              width: '100%',
              maxWidth: imageWidth,
              maxHeight: imageHeight,
              flex: 1,
              minHeight: 150,
              borderRadius: 1,
              border: '1px solid',
              borderColor: 'divider',
              objectFit: 'contain',
              bgcolor: '#f4f6fa',
            }}
          />
        ) : (
          <Box
            sx={{
              width: '100%',
              flex: 1,
              minHeight: 150,
              borderRadius: 1,
              border: '1px dashed',
              borderColor: 'divider',
              bgcolor: '#f4f6fa',
              display: 'grid',
              placeItems: 'center',
            }}
          >
            이미지 URL을 입력하면 여기에 표시됩니다.
          </Box>
        )}
      </Stack>
    );
  }

  if (popup.popupType === 'VIDEO') {
    const showDescription = content.showDescription !== false;
    const showControls = content.showControls !== false;
    const defaultVolume = Number(content.defaultVolume);
    return (
      <Stack spacing={1.5} sx={{ height: '100%' }}>
        {showDescription && <Typography color="text.secondary">{description}</Typography>}
        <Box
          sx={{
            flex: 1,
            minHeight: 170,
            borderRadius: 1,
            bgcolor: '#101521',
            color: 'white',
            display: 'grid',
            placeItems: 'center',
          }}
        >
          <Stack alignItems="center" spacing={1}>
            <PlayCircleOutlineIcon sx={{ fontSize: 58 }} />
            <Typography variant="caption">영상 재생 영역</Typography>
          </Stack>
        </Box>
        {showControls && (
          <Stack direction="row" justifyContent="space-between" color="text.secondary">
            <Typography variant="caption">▶ 00:00 / 00:00</Typography>
            <Typography variant="caption">
              {content.allowPlaybackRateChange !== false ? '1.0× · ' : ''}
              {content.allowFullScreen !== false ? '전체화면' : '전체화면 제한'}
            </Typography>
          </Stack>
        )}
        <Typography variant="caption" color="text.secondary">
          완료 기준 {Math.round((popup.completionRatio ?? 0.8) * 100)}% · 기본 음량{' '}
          {Math.round((Number.isFinite(defaultVolume) ? defaultVolume : 0.7) * 100)}%
        </Typography>
      </Stack>
    );
  }

  if (popup.popupType === 'SURVEY' || popup.popupType === 'QUIZ') {
    return (
      <Stack spacing={1.5}>
        <Typography color="text.secondary">{description}</Typography>
        {(popup.questions.length > 0 ? popup.questions : [null]).map((question, index) => (
          <Paper variant="outlined" sx={{ p: 2 }} key={question?.questionId ?? 'sample'}>
            <Typography fontWeight={700}>
              {index + 1}. {question?.title ?? '샘플 문항입니다.'}
              {question?.isRequired && <Typography component="span" color="error"> *</Typography>}
            </Typography>
            {question?.description && (
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                {question.description}
              </Typography>
            )}
            <Stack sx={{ mt: 1 }}>
              {question?.questionType === 'TEXT' ? (
                <Box sx={{ minHeight: 62, border: '1px solid', borderColor: 'divider', borderRadius: 1 }} />
              ) : (
                (question?.options.length ? question.options : [{ optionId: 0, text: '보기 1' }]).map(
                  (option) => (
                    <FormControlLabel
                      key={option.optionId}
                      control={<Radio size="small" />}
                      label={option.text}
                    />
                  ),
                )
              )}
            </Stack>
          </Paper>
        ))}
        <Button variant="contained" sx={{ alignSelf: 'flex-end' }}>
          제출
        </Button>
      </Stack>
    );
  }

  const showRightSection =
    content.showRightSection == null
      ? Boolean(content.rightSectionTitle || content.rightSectionBody || content.additionalDescription)
      : content.showRightSection === true;
  const showHighlight =
    content.showHighlight == null ? Boolean(content.highlightText) : content.showHighlight === true;

  return (
    <Stack spacing={2}>
      <Typography color="text.secondary">{description}</Typography>
      <Box sx={{ display: 'grid', gridTemplateColumns: showRightSection ? 'repeat(2, minmax(0, 1fr))' : '1fr', gap: 2 }}>
        <Paper variant="outlined" sx={{ p: 2, bgcolor: '#fff', whiteSpace: 'pre-wrap' }}>
          <Typography fontWeight={700} sx={{ mb: 1 }}>
            {text(content.leftSectionTitle, '왼쪽 카드 제목')}
          </Typography>
          <Typography>{text(content.leftSectionBody, '왼쪽 카드 본문')}</Typography>
          {showHighlight && (
            <Box sx={{ mt: 2, p: 1.5, border: '1px solid #93c5fd', borderRadius: 1, bgcolor: '#eff6ff', color: '#1d4ed8' }}>
              {text(content.highlightText, '강조 문구')}
            </Box>
          )}
        </Paper>
        {showRightSection && <Paper variant="outlined" sx={{ p: 2, bgcolor: '#fff', whiteSpace: 'pre-wrap' }}>
          <Typography fontWeight={700} sx={{ mb: 1 }}>
            {text(content.rightSectionTitle, '오른쪽 카드 제목')}
          </Typography>
          <Typography>{text(content.rightSectionBody, '오른쪽 카드 본문')}</Typography>
          <Divider sx={{ my: 2 }} />
          <Typography color="text.secondary">
            {text(content.additionalDescription, '추가 설명')}
          </Typography>
        </Paper>}
      </Box>
      {Boolean(content.bottomDescription) && (
        <Paper variant="outlined" sx={{ p: 2, bgcolor: '#f8f9fc', whiteSpace: 'pre-wrap' }}>
          {String(content.bottomDescription)}
        </Paper>
      )}
    </Stack>
  );
}

export default function PopupPreview({ popup, standalone = false, fitContainer = false, onClose }: PopupPreviewProps) {
  const size = standalone
    ? { width: '100%', height: '100vh' }
    : fitContainer
      ? { width: '100%', height: '100%' }
      : previewSize(popup);
  const contentTitle = text(contentValue(popup, titleKey(popup.popupType)), '콘텐츠 제목');

  return (
    <Box
      sx={{
        minHeight: standalone ? '100vh' : fitContainer ? 0 : 570,
        height: fitContainer ? '100%' : undefined,
        p: standalone || fitContainer ? 0 : 2,
        overflow: 'hidden',
        borderRadius: 1,
        bgcolor: '#e9edf4',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'flex-start',
      }}
    >
      <Paper
        elevation={8}
        sx={{
          ...size,
          maxWidth: '100%',
          display: 'flex',
          flexDirection: 'column',
          position: 'relative',
          overflow: 'hidden',
          borderRadius: standalone ? 0 : 2,
          bgcolor: 'white',
        }}
      >
        {popup.showHeader && (
          <Stack direction="row" alignItems="center" sx={{ minHeight: 52, px: 2 }}>
            <Typography fontWeight={700} sx={{ flex: 1 }} noWrap>
              {text(popup.title, '팝업 제목')}
            </Typography>
            {popup.showCloseButton && (
              <IconButton size="small" aria-label="닫기 미리보기" onClick={onClose}>
                <CloseIcon fontSize="small" />
              </IconButton>
            )}
          </Stack>
        )}
        {popup.showHeader && <Divider />}
        <Box sx={{ flex: 1, minHeight: 0, overflow: fitContainer ? 'hidden' : 'auto', p: 3 }}>
          <Typography variant="h5" fontWeight={800} sx={{ mb: 2 }}>
            {contentTitle}
          </Typography>
          <PopupBody popup={popup} />
        </Box>
        {popup.showFooter && (
          <>
            <Divider />
            <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ px: 2, py: 1.5 }}>
              {popup.showDoNotShowAgain ? (
                <FormControlLabel control={<Checkbox size="small" />} label="다시 보지 않기" />
              ) : (
                <span />
              )}
              <Button variant="contained" color="inherit" sx={{ minWidth: 92 }} onClick={onClose}>
                닫기
              </Button>
            </Stack>
          </>
        )}
      </Paper>
    </Box>
  );
}

function contentValue(popup: AdminPopupDetail, key: string): unknown {
  return popup.content[key];
}
