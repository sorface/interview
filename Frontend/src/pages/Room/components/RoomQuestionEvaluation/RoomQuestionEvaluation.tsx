import { ChangeEvent, Fragment, FunctionComponent } from 'react';
import { Button } from '../../../../components/Button/Button';
import { Gap } from '../../../../components/Gap/Gap';
import { useThemeClassName } from '../../../../hooks/useThemeClassName';
import { Theme, ThemeInUi } from '../../../../context/ThemeContext';
import { Typography } from '../../../../components/Typography/Typography';
import { useLocalizationCaptions } from '../../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../../localization';

export interface RoomQuestionEvaluationValue {
  mark: number | null;
  review: string;
}

interface RoomQuestionEvaluationPorps {
  value: RoomQuestionEvaluationValue;
  onChange: (newValue: RoomQuestionEvaluationValue) => void;
}

const themeClassNames: Record<ThemeInUi, Record<'active' | 'nonActive', string>> = {
  [Theme.Dark]: {
    active: 'bg-blue-dark',
    nonActive: 'bg-dark',
  },
  [Theme.Light]: {
    active: 'text-white bg-dark',
    nonActive: 'bg-blue-light',
  },
};

export const RoomQuestionEvaluation: FunctionComponent<RoomQuestionEvaluationPorps> = ({
  value,
  onChange,
}) => {
  const commonButtonClassName = 'w-1.75 h-1.75 min-h-unset p-0.375';
  const themeClassName = useThemeClassName(themeClassNames);
  const localizationCaptions = useLocalizationCaptions();
  const markGroups = [
    {
      marks: [1, 2, 3],
      caption: localizationCaptions[LocalizationKey.MarksGroupBad],
    },
    {
      marks: [4, 5],
      caption: localizationCaptions[LocalizationKey.MarksGroupMedium],
    },
    {
      marks: [6, 7, 8],
      caption: localizationCaptions[LocalizationKey.MarksGroupGood],
    },
    {
      marks: [9, 10],
      caption: localizationCaptions[LocalizationKey.MarksGroupPerfect],
    },
  ];

  const handleMarkChange = (mark: number) => () => {
    onChange({
      ...value,
      mark,
    });
  };

  const handleReviewChange = (event: ChangeEvent<HTMLTextAreaElement>) => {
    onChange({
      ...value,
      review: event.target.value,
    });
  };

  return (
    <div>
      <div className='text-left'>
        <Typography size='l' bold>
          {localizationCaptions[LocalizationKey.RoomQuestionEvaluationTitle]}
        </Typography>
      </div>
      <Gap sizeRem={1} />
      <div className='flex'>
        {markGroups.map((markGroup, markGroupIndex) => (
          <Fragment key={`markGroup${markGroupIndex}`}>
            <div>
              <div className={`rounded-l-2 rounded-r-2 overflow-hidden whitespace-nowrap ${themeClassName['nonActive']}`}>
                {markGroup.marks.map((markVal) => (
                  <Button
                    key={markVal}
                    variant='text'
                    className={`${commonButtonClassName} ${themeClassName[markVal === value.mark ? 'active' : 'nonActive']}`}
                    onClick={handleMarkChange(markVal)}
                  >
                    {markVal}
                  </Button>
                ))}
              </div>
              <Typography size='s'>{markGroup.caption}</Typography>
            </div>
            {markGroupIndex !== markGroups.length - 1 && (<Gap sizeRem={0.375} horizontal />)}
          </ Fragment>
        ))}
      </div>
      <Gap sizeRem={1} />
      <div className='flex'>
        <textarea
          className='flex-1 h-6.25'
          value={value.review}
          onInput={handleReviewChange}
        />
      </div>
    </div>
  )
};
