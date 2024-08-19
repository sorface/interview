import { FunctionComponent, MouseEvent } from 'react';
import { RoomQuestionListItem } from '../RoomCreate';
import { useLocalizationCaptions } from '../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../localization';
import { Dropdown } from '../../../components/Dropdown/Dropdown';
import { Icon } from '../../Room/components/Icon/Icon';
import { IconNames } from '../../../constants';
import { Typography } from '../../../components/Typography/Typography';
import { Gap } from '../../../components/Gap/Gap';

const sortQestions = (qestion1: RoomQuestionListItem, qestion2: RoomQuestionListItem) =>
  qestion1.order - qestion2.order;

interface RoomQuestionsSelectorPreviewProps {
  qestions: RoomQuestionListItem[];
  onRemove: (question: RoomQuestionListItem) => void;
}

export const RoomQuestionsSelectorPreview: FunctionComponent<RoomQuestionsSelectorPreviewProps> = ({
  qestions,
  onRemove,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  const handleRemoveClick = (qestion: RoomQuestionListItem) => (e: MouseEvent<HTMLSpanElement>) => {
    e.stopPropagation();
    onRemove(qestion);
  };

  return (
    <div>
      <Dropdown
        useButton
        buttonVariant='invertedActive'
        toggleContent={
          <div className='flex'>
            <Typography size='m'>
              {localizationCaptions[LocalizationKey.RoomSelectedQuestions]}:
            </Typography>
            <Gap sizeRem={0.25} horizontal />
            <Typography size='m'>
              {qestions.length}
            </Typography>
          </div>
        }
        toggleClassName='w-13.75 flex justify-between'
        toggleIcon
        contentClassName='translate-x--4.25-y-0.25'
      >
        <div className='w-18.125 rounded-0.75 bg-wrap shadow'>
          {qestions.sort(sortQestions).map(qestion => (
            <div
              key={qestion.id}
              className='flex items-center justify-between h-2.125 px-1 cursor-pointer hover:bg-grey-active'
            >
              <Typography size='s'>{qestion.value}</Typography>
              <span onClick={handleRemoveClick(qestion)} className='text-grey2'>
                <Icon size='s' name={IconNames.Trash} />
              </span>
            </div>
          ))}
        </div>
      </Dropdown>
    </div>
  );
};
