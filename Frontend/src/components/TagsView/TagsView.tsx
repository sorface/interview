import { FunctionComponent, MouseEventHandler } from 'react';
import { Tag } from '../../types/tag';

import './TagsView.css';

interface TagsViewProps {
  tags: Tag[];
  placeHolder: string;
  onClick?: (tag: Tag) => MouseEventHandler<HTMLElement>;
}

export const TagsView: FunctionComponent<TagsViewProps> = ({
  tags,
  placeHolder,
  onClick,
}) => {
  const createItem = (tag: Tag) => {
    const style = { borderColor: `#${tag.hexValue}` };
    if (onClick) {
      return <span className='tag-item' style={style} onClick={onClick(tag)} key={tag.id}>{tag.value} ✖</span>;
    }
    return <span className='tag-item' style={style} key={tag.id}>{tag.value}</span>
  };

  const getDisplay = () => {
    if (tags.length === 0) {
      return placeHolder;
    }
    return tags.map(createItem);
  };

  return (
    <div className="tags-view">{getDisplay()}</div>
  );
};
