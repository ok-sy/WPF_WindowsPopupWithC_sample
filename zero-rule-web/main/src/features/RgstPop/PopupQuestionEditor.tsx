import { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Checkbox,
  FormControlLabel,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from '@mui/material';
import type {
  AdminPopupQuestion,
  AdminQuestionTemplate,
  PopupQuestion,
  PopupType,
} from '@local/domain';
import { useApi } from '@/provider';
import handleError from '@/lib/handle-error';

interface Props {
  popupType: PopupType;
  entries: AdminPopupQuestion[];
  disabled: boolean;
  onBusy: (busy: boolean) => void;
  onChange: (entries: AdminPopupQuestion[], templateId?: number) => void;
}

export default function PopupQuestionEditor({
  popupType,
  entries,
  disabled,
  onBusy,
  onChange,
}: Props) {
  const api = useApi();
  const [templates, setTemplates] = useState<AdminQuestionTemplate[]>([]);
  const [selected, setSelected] = useState('');
  useEffect(() => {
    let canceled = false;
    api.popupAdmin
      .questionTemplates()
      .then(({ body }) => {
        if (!canceled) setTemplates(body.templates);
      })
      .catch((error) => {
        if (!canceled) handleError(error);
      });
    return () => {
      canceled = true;
    };
  }, [api]);
  const change = (next: AdminPopupQuestion[]) =>
    onChange(
      next.map((entry, i) => ({
        ...entry,
        question: { ...entry.question, sortOrder: i + 1 },
      })),
    );
  const patch = (index: number, values: Partial<PopupQuestion>, correctValues?: string[]) =>
    change(
      entries.map((entry, i) =>
        i === index
          ? {
              question: { ...entry.question, ...values },
              correctValues: correctValues ?? entry.correctValues,
            }
          : entry,
      ),
    );
  const load = async () => {
    onBusy(true);
    try {
      const { body } = await api.popupAdmin.questionTemplate({ templateId: Number(selected) });
      onChange(body.adminQuestions, Number(selected));
    } catch (error) {
      handleError(error);
    } finally {
      onBusy(false);
    }
  };
  return (
    <Box component="fieldset" disabled={disabled} sx={{ border: 0, p: 0, m: 0, minWidth: 0 }}>
      <Stack spacing={2}>
        <Typography fontWeight={700}>문항 편집</Typography>
        <Stack direction="row" spacing={1}>
          <TextField
            select
            fullWidth
            size="small"
            label="기존 템플릿"
            value={selected}
            onChange={(e) => setSelected(e.target.value)}
          >
            <MenuItem value="">선택 안 함</MenuItem>
            {templates
              .filter((t) => t.templateType === popupType)
              .map((t) => (
                <MenuItem key={t.templateId} value={t.templateId}>
                  {t.templateName} ({t.templateId})
                </MenuItem>
              ))}
          </TextField>
          <Button disabled={disabled || !selected} onClick={load}>
            불러오기
          </Button>
        </Stack>
        <Typography variant="caption">
          불러오면 현재 문항이 교체됩니다. 수정한 문항은 저장 시 별도 템플릿으로 보관됩니다.
        </Typography>
        {entries.map(({ question: q, correctValues }, index) => (
          <Stack
            key={index}
            spacing={1}
            sx={{ border: '1px solid', borderColor: 'divider', p: 2, borderRadius: 1 }}
          >
            <Stack direction="row" spacing={1} alignItems="center">
              <Typography sx={{ flex: 1 }}>문항 {index + 1}</Typography>
              <Button
                disabled={disabled || index === 0}
                onClick={() => {
                  const next = [...entries];
                  [next[index - 1], next[index]] = [next[index]!, next[index - 1]!];
                  change(next);
                }}
              >
                위로
              </Button>
              <Button
                disabled={disabled || index === entries.length - 1}
                onClick={() => {
                  const next = [...entries];
                  [next[index], next[index + 1]] = [next[index + 1]!, next[index]!];
                  change(next);
                }}
              >
                아래로
              </Button>
              <Button color="error" onClick={() => change(entries.filter((_, i) => i !== index))}>
                삭제
              </Button>
            </Stack>
            <TextField
              label="문항 제목"
              required
              value={q.title}
              inputProps={{ maxLength: 1000 }}
              onChange={(e) => patch(index, { title: e.target.value })}
            />
            <TextField
              label="설명"
              multiline
              value={q.description ?? ''}
              inputProps={{ maxLength: 2000 }}
              onChange={(e) => patch(index, { description: e.target.value })}
            />
            <TextField
              select
              label="응답 유형"
              value={q.questionType}
              onChange={(e) =>
                patch(
                  index,
                  {
                    questionType: e.target.value,
                    isScored: false,
                    questionScore: null,
                    options: e.target.value === 'TEXT' ? [] : q.options,
                  },
                  [],
                )
              }
            >
              <MenuItem value="TEXT">주관식</MenuItem>
              <MenuItem value="SINGLE_CHOICE">단일 선택</MenuItem>
              <MenuItem value="MULTIPLE_CHOICE">복수 선택</MenuItem>
            </TextField>
            <Stack direction="row" spacing={1}>
              <FormControlLabel
                label="필수 응답"
                control={
                  <Checkbox
                    checked={q.isRequired}
                    onChange={(_, checked) => patch(index, { isRequired: checked })}
                  />
                }
              />
              {q.questionType !== 'TEXT' && (
                <FormControlLabel
                  label="채점"
                  control={
                    <Checkbox
                      checked={q.isScored}
                      onChange={(_, checked) =>
                        patch(index, { isScored: checked, questionScore: checked ? 1 : null }, [])
                      }
                    />
                  }
                />
              )}
              {q.isScored && (
                <TextField
                  type="number"
                  label="배점"
                  value={q.questionScore ?? ''}
                  inputProps={{ min: 0, max: 99999999.99, step: 0.01 }}
                  onChange={(e) =>
                    patch(index, {
                      questionScore: e.target.value === '' ? null : Number(e.target.value),
                    })
                  }
                />
              )}
            </Stack>
            {q.questionType !== 'TEXT' && (
              <>
                {q.options.map((option, oi) => (
                  <Stack key={option.value} direction="row" spacing={1} alignItems="center">
                    {q.isScored && (
                      <FormControlLabel
                        label="정답"
                        control={
                          <Checkbox
                            checked={correctValues.includes(option.value)}
                            onChange={(_, checked) =>
                              patch(
                                index,
                                {},
                                checked
                                  ? q.questionType === 'SINGLE_CHOICE'
                                    ? [option.value]
                                    : [...correctValues, option.value]
                                  : correctValues.filter((v) => v !== option.value),
                              )
                            }
                          />
                        }
                      />
                    )}
                    <TextField
                      fullWidth
                      label={`선택지 ${oi + 1}`}
                      value={option.text}
                      inputProps={{ maxLength: 1000 }}
                      onChange={(e) =>
                        patch(index, {
                          options: q.options.map((o, i) =>
                            i === oi ? { ...o, text: e.target.value } : o,
                          ),
                        })
                      }
                    />
                    <Button
                      color="error"
                      onClick={() =>
                        patch(
                          index,
                          {
                            options: q.options
                              .filter((_, i) => i !== oi)
                              .map((o, i) => ({ ...o, sortOrder: i + 1 })),
                          },
                          correctValues.filter((v) => v !== option.value),
                        )
                      }
                    >
                      삭제
                    </Button>
                  </Stack>
                ))}
                <Button
                  onClick={() => {
                    let value = 1;
                    while (q.options.some((o) => o.value === String(value))) value++;
                    patch(index, {
                      options: [
                        ...q.options,
                        {
                          optionId: 0,
                          value: String(value),
                          text: '',
                          sortOrder: q.options.length + 1,
                        },
                      ],
                    });
                  }}
                >
                  선택지 추가
                </Button>
              </>
            )}
          </Stack>
        ))}
        <Button
          variant="outlined"
          onClick={() =>
            change([
              ...entries,
              {
                question: {
                  questionId: 0,
                  title: '',
                  description: '',
                  questionType: 'SINGLE_CHOICE',
                  isRequired: true,
                  isScored: popupType === 'QUIZ',
                  questionScore: popupType === 'QUIZ' ? 1 : null,
                  sortOrder: entries.length + 1,
                  options: [
                    { optionId: 0, value: '1', text: '', sortOrder: 1 },
                    { optionId: 0, value: '2', text: '', sortOrder: 2 },
                  ],
                },
                correctValues: [],
              },
            ])
          }
        >
          문항 추가
        </Button>
      </Stack>
    </Box>
  );
}
