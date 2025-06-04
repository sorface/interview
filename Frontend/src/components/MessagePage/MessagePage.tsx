import React, { FunctionComponent, ReactNode } from 'react';
import { Icon } from '../../pages/Room/components/Icon/Icon';
import { IconNames } from '../../constants';
import { Typography } from '../Typography/Typography';
import { Gap } from '../Gap/Gap';

import './MessagePage.css';

interface MessagePageProps {
  title: string;
  message: string;
  iconName?: IconNames;
  children?: ReactNode;
}

export const MessagePage: FunctionComponent<MessagePageProps> = ({
  title,
  message,
  iconName,
  children,
}) => {
  return (
    <div className="message-page">
      <div className="message-page-content flex flex-col items-center">
        <Gap sizeRem={1} />
        <Icon size="xxl" name={iconName || IconNames.None} />
        <Gap sizeRem={1.75} />
        <Typography size="xl" bold>
          {title}
        </Typography>
        <Gap sizeRem={0.75} />
        <Typography size="m">{message}</Typography>
        {children && <div className="message-page-children">{children}</div>}
      </div>
    </div>
  );
};
