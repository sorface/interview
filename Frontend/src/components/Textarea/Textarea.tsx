import { FunctionComponent } from 'react';
import { Typography } from '../Typography/Typography';

interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  value: string;
  showMaxLength?: boolean;
}

export const Textarea: FunctionComponent<TextareaProps> = ({
  showMaxLength,
  ...rest
}) => {
  return (
    <>
      <textarea {...rest} />
      {showMaxLength && rest.maxLength && (
        <div className='text-grey3'>
          <Typography size='s'>
            {rest.value.length} \ {rest.maxLength}
          </Typography>
        </div>
      )}
    </>
  );
};
