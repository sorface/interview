import { FunctionComponent, ReactNode } from 'react';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';

import './MessagePage.css';

interface MessagePageProps {
  title: string;
  message: string;
  children: ReactNode;
}

export const MessagePage: FunctionComponent<MessagePageProps> = ({
  title,
  message,
  children,
}) => {
  return (
    <div className='message-page'>
      <div className='message-page-content'>
        <Icon name={IconNames.None} />
        <h3>{title}</h3>
        <div className='message-page-message'>{message}</div>
        <div className='message-page-children'>{children}</div>
      </div>
    </div>
  );
};
