import { FunctionComponent, ReactNode } from "react";

import './Checkbox.css';

interface CheckboxProps extends React.InputHTMLAttributes<HTMLInputElement> {
  id: string;
  label: ReactNode;
}

export const Checkbox: FunctionComponent<CheckboxProps> = ({
  id,
  label,
  ...rest
}) => {
  return (
    <>
      <input type='checkbox' className='checkbox' id={id} {...rest} />
      <label htmlFor={id} className='cursor-pointer'>{label}</label>
    </>
  );
};
