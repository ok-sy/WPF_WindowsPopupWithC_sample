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
    return (
      <Stack spacing={1.5} alignItems="center" sx={{ height: '100%' }}>
        <Typography color="text.secondary">{description}</Typography>
        {imageUrl ? (
          <Box
            component="img"
            src={imageUrl}
            alt="팝업 이미지 미리보기"
            sx={{
              width: '100%',
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
    return (
      <Stack spacing={1.5} sx={{ height: '100%' }}>
        <Typography color="text.secondary">{description}</Typography>
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
        <Typography variant="caption" color="text.secondary">
          완료 기준 {Math.round((popup.completionRatio ?? 0.8) * 100)}%
        </Typography>
      </Stack>
    );
  }

  if (popup.popupType === 'SURVEY' || popup.popupType === 'QUIZ') {
    const questionTitle = popup.questions[0]?.title ?? '샘플 문항입니다.';
    return (
      <Stack spacing={1.5}>
        <Typography color="text.secondary">{description}</Typography>
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Typography fontWeight={700}>{questionTitle}</Typography>
          <Stack sx={{ mt: 1 }}>
            <FormControlLabel control={<Radio size="small" />} label="보기 1" />
            <FormControlLabel control={<Radio size="small" />} label="보기 2" />
          </Stack>
        </Paper>
        <Button variant="contained" sx={{ alignSelf: 'flex-end' }}>
          제출
        </Button>
      </Stack>
    );
  }

  return (
    <Stack spacing={2}>
      <Typography color="text.secondary">{description}</Typography>
      <Paper variant="outlined" sx={{ p: 2, bgcolor: '#f8f9fc', whiteSpace: 'pre-wrap' }}>
        {text(content.leftSectionBody, '텍스트 팝업 본문이 여기에 표시됩니다.')}
      </Paper>
    </Stack>
  );
}

export default function PopupPreview({ popup }: PopupPreviewProps) {
  const size = previewSize(popup);
  const contentTitle = text(contentValue(popup, titleKey(popup.popupType)), '콘텐츠 제목');

  return (
    <Box
      sx={{
        minHeight: 570,
        p: 2,
        overflow: 'auto',
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
          borderRadius: 2,
          bgcolor: 'white',
        }}
      >
        {!popup.showHeader && popup.showCloseButton && (
          <IconButton
            size="small"
            aria-label="닫기 미리보기"
            sx={{ position: 'absolute', top: 10, right: 10, zIndex: 1, bgcolor: 'white' }}
          >
            <CloseIcon fontSize="small" />
          </IconButton>
        )}
        {popup.showHeader && (
          <Stack direction="row" alignItems="center" sx={{ minHeight: 52, px: 2 }}>
            <Typography fontWeight={700} sx={{ flex: 1 }} noWrap>
              {text(popup.title, '팝업 제목')}
            </Typography>
            {popup.showCloseButton && (
              <IconButton size="small" aria-label="닫기 미리보기">
                <CloseIcon fontSize="small" />
              </IconButton>
            )}
          </Stack>
        )}
        {popup.showHeader && <Divider />}
        <Box sx={{ flex: 1, minHeight: 0, overflow: 'auto', p: 3 }}>
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
              <Button variant="contained" color="inherit" sx={{ minWidth: 92 }}>
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
