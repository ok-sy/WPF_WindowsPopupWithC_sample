import SubTitleAndIcon from '@/components/SubTitleAndIcon';
import handleError from '@/lib/handle-error';
import { useApi } from '@/provider';
import type { AdminPopupListItem, ApiRequestContext, PopupDateValue } from '@local/domain';
import { Portlet, PortletContent } from '@local/ui';
import AddIcon from '@mui/icons-material/Add';
import RefreshIcon from '@mui/icons-material/Refresh';
import {
  Box,
  Button,
  Chip,
  LinearProgress,
  MenuItem,
  Select,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { useCallback, useEffect, useMemo, useState } from 'react';

const popupTypeLabels: Record<string, string> = {
  TEXT: '텍스트',
  IMAGE: '이미지',
  VIDEO: '영상',
  SURVEY: '설문',
  QUIZ: '퀴즈',
};

function formatPopupDate(value: PopupDateValue): string {
  const numericValue = typeof value === 'number' ? value : Number.NaN;
  const date = Number.isFinite(numericValue)
    ? new Date(numericValue < 1_000_000_000_000 ? numericValue * 1000 : numericValue)
    : new Date(value);

  return Number.isNaN(date.getTime())
    ? '-'
    : new Intl.DateTimeFormat('ko-KR', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
      }).format(date);
}

export default function RgstPop() {
  const api = useApi();
  const [popups, setPopups] = useState<AdminPopupListItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [keyword, setKeyword] = useState('');
  const [popupType, setPopupType] = useState('ALL');
  const [activeFilter, setActiveFilter] = useState('ALL');
  const [selectedPopupId, setSelectedPopupId] = useState<string | null>(null);
  const [refreshToken, setRefreshToken] = useState(0);

  const loadPopups = useCallback(
    async (ctx: ApiRequestContext) => {
      try {
        setLoading(true);
        const { body } = await api.popupAdmin.list({ ctx });
        if (!ctx.canceled) setPopups(body.popups ?? []);
      } catch (error) {
        if (!ctx.canceled) handleError(error);
      } finally {
        if (!ctx.canceled) setLoading(false);
      }
    },
    [api],
  );

  useEffect(() => {
    const ctx = { canceled: false } as ApiRequestContext;
    loadPopups(ctx);
    return () => {
      ctx.canceled = true;
      ctx.cancel?.();
    };
  }, [loadPopups, refreshToken]);

  const filteredPopups = useMemo(() => {
    const normalizedKeyword = keyword.trim().toLowerCase();
    return popups.filter((popup) => {
      const keywordMatched =
        normalizedKeyword.length === 0 ||
        popup.popupId.toLowerCase().includes(normalizedKeyword) ||
        popup.title.toLowerCase().includes(normalizedKeyword);
      const typeMatched = popupType === 'ALL' || popup.popupType === popupType;
      const activeMatched = activeFilter === 'ALL' || popup.activeYn === activeFilter;
      return keywordMatched && typeMatched && activeMatched;
    });
  }, [activeFilter, keyword, popupType, popups]);

  const updateActive = async (popup: AdminPopupListItem, active: boolean) => {
    try {
      setLoading(true);
      await api.popupAdmin.updateActive({ popupId: popup.popupId, active });
      setRefreshToken(Date.now());
    } catch (error) {
      handleError(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box className="RgstPop-root" sx={{ py: 1, pl: 1, pr: 3 }}>
      <Portlet>
        <PortletContent sx={{ p: 2, position: 'relative' }}>
          {loading && <LinearProgress sx={{ position: 'absolute', inset: '0 0 auto 0' }} />}
          <Stack direction="row" alignItems="center" justifyContent="space-between">
            <SubTitleAndIcon labelTitle="팝업 관리" />
            <Stack direction="row" spacing={1}>
              <Button
                variant="outlined"
                size="small"
                startIcon={<RefreshIcon />}
                onClick={() => setRefreshToken(Date.now())}
              >
                새로고침
              </Button>
              <Button variant="contained" size="small" startIcon={<AddIcon />} disabled>
                신규 등록
              </Button>
            </Stack>
          </Stack>

          <Stack direction="row" spacing={1} sx={{ my: 2 }}>
            <TextField
              size="small"
              label="팝업 ID 또는 제목"
              value={keyword}
              onChange={(event) => setKeyword(event.target.value)}
              sx={{ width: 280 }}
            />
            <Select size="small" value={popupType} onChange={(event) => setPopupType(event.target.value)}>
              <MenuItem value="ALL">전체 유형</MenuItem>
              {Object.entries(popupTypeLabels).map(([value, label]) => (
                <MenuItem key={value} value={value}>
                  {label}
                </MenuItem>
              ))}
            </Select>
            <Select
              size="small"
              value={activeFilter}
              onChange={(event) => setActiveFilter(event.target.value)}
            >
              <MenuItem value="ALL">전체 상태</MenuItem>
              <MenuItem value="Y">활성</MenuItem>
              <MenuItem value="N">비활성</MenuItem>
            </Select>
          </Stack>

          <TableContainer sx={{ border: '1px solid #e0e4ee', maxHeight: 'calc(100vh - 280px)' }}>
            <Table stickyHeader size="small">
              <TableHead>
                <TableRow>
                  <TableCell>팝업 ID</TableCell>
                  <TableCell>유형</TableCell>
                  <TableCell>제목</TableCell>
                  <TableCell>노출 시작</TableCell>
                  <TableCell>노출 종료</TableCell>
                  <TableCell>크기</TableCell>
                  <TableCell align="center">활성</TableCell>
                  <TableCell>최종 수정</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredPopups.map((popup) => (
                  <TableRow
                    hover
                    key={popup.popupId}
                    selected={selectedPopupId === popup.popupId}
                    onClick={() => setSelectedPopupId(popup.popupId)}
                    sx={{ cursor: 'pointer' }}
                  >
                    <TableCell>{popup.popupId}</TableCell>
                    <TableCell>
                      <Chip size="small" label={popupTypeLabels[popup.popupType] ?? popup.popupType} />
                    </TableCell>
                    <TableCell>{popup.title}</TableCell>
                    <TableCell>{formatPopupDate(popup.displayStartAt)}</TableCell>
                    <TableCell>{formatPopupDate(popup.displayEndAt)}</TableCell>
                    <TableCell>{popup.sizeMode}</TableCell>
                    <TableCell align="center">
                      <Switch
                        size="small"
                        checked={popup.activeYn === 'Y'}
                        onClick={(event) => event.stopPropagation()}
                        onChange={(_, checked) => updateActive(popup, checked)}
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{formatPopupDate(popup.updatedAt)}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {popup.updatedBy}
                      </Typography>
                    </TableCell>
                  </TableRow>
                ))}
                {!loading && filteredPopups.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={8} align="center" sx={{ py: 5 }}>
                      조회된 팝업이 없습니다.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </PortletContent>
      </Portlet>
    </Box>
  );
}
