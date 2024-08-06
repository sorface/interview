import { FunctionComponent } from 'react';
import { RoomQuestionListItem } from '../RoomCreate';
import { useLocalizationCaptions } from '../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../localization';
import { Dropdown } from '../../../components/Dropdown/Dropdown';
import { Icon } from '../../Room/components/Icon/Icon';
import { IconNames } from '../../../constants';

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

  return (
    <div>
      <Dropdown
        useButton
        toggleContent={
          <>
            {localizationCaptions[LocalizationKey.RoomSelectedQuestions]}: {qestions.length}
            <Icon name={IconNames.ChevronForward} size='small' />
          </>
        }
        toggleClassName='w-18.125 flex justify-between'
      >
        <div className='w-18.125 rounded-0.75 bg-wrap shadow'>
          {qestions.sort(sortQestions).map(qestion => (
            <div
              key={qestion.id}
              className='flex justify-between px-1 py-0.5'
            >
              <div>{qestion.value}</div>
              <span onClick={() => onRemove(qestion)} className='cursor-pointer'>
                <Icon name={IconNames.Trash} size='small' />
              </span>
            </div>
          ))}
        </div>
      </Dropdown>
    </div>
  );
};
