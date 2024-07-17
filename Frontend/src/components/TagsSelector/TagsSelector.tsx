import React, { ChangeEventHandler, FunctionComponent, MouseEventHandler, useEffect, useState } from 'react';
import { Tag } from '../../types/tag';
import { OpenIcon } from '../OpenIcon/OpenIcon';
import { TagsView } from '../TagsView/TagsView';
import { LocalizationKey } from '../../localization';
import { LocalizationCaption } from '../LocalizationCaption/LocalizationCaption';
import { Button } from '../Button/Button';

import './TagsSelector.css';

export interface TagsSelectorProps {
  loading: boolean;
  tags: Tag[];
  selectedTags: Tag[];
  placeHolder: string;
  onSelect: (tag: Tag) => void;
  onUnselect: (tag: Tag) => void;
  onSearch: (value: string) => void;
  onCreate?: (tag: Omit<Tag, 'id'>) => void;
}

export const TagsSelector: FunctionComponent<TagsSelectorProps> = ({
  loading,
  tags,
  selectedTags,
  placeHolder,
  onSelect,
  onUnselect,
  onSearch,
  onCreate,
}) => {
  const [showMenu, setShowMenu] = useState(false);
  const [searchValue, setSearchValue] = useState("");
  const [color, setColor] = useState("#b9221c");

  const checkIsSelected = (tag: Tag) => {
    return !!selectedTags.find(tg => tg.id === tag.id);
  }

  useEffect(() => {
    onSearch(searchValue);
  }, [searchValue, onSearch]);

  const handleInputClick: MouseEventHandler<HTMLDivElement> = () => {
    setShowMenu(!showMenu);
  };

  const handleUnselectClick = (tag: Tag): MouseEventHandler<HTMLElement> => (event) => {
    event.stopPropagation();
    onUnselect(tag);
  }

  const handleSearch: ChangeEventHandler<HTMLInputElement> = (e) => {
    setSearchValue(e.target.value);
  };

  const handleColorChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    setColor(e.target.value);
  };

  const handleCreate: React.MouseEventHandler<HTMLButtonElement> = (event) => {
    event.preventDefault();
    if (!onCreate) {
      return;
    }
    onCreate({
      value: searchValue,
      hexValue: color.replace('#', ''),
    });
    setSearchValue('');
  };

  const options = tags.filter(
    (tag) => !checkIsSelected(tag) && tag.value.toLowerCase().indexOf(searchValue.toLowerCase()) >= 0
  );

  return (
    <div className="tagsSelector-container">
      <div onClick={handleInputClick} className="tagsSelector-input">
        <TagsView tags={selectedTags} placeHolder={placeHolder} onClick={handleUnselectClick} />
        <div className="tagsSelector-tools">
          <div className="tagsSelector-tool">
            <OpenIcon sizeRem={1} />
          </div>
        </div>
      </div>
      {showMenu && (
        <div className="tagsSelector-menu">
          <div className="search-box">
            <input type="text" className='tag-value' onChange={handleSearch} value={searchValue} />
            {onCreate && (
              <>
                <input type="color" className="color-select" value={color} onChange={handleColorChange} />
                <Button onClick={handleCreate}>{<LocalizationCaption captionKey={LocalizationKey.Create} />}</Button>
              </>
            )}
          </div>
          <div className="tagsSelector-items">
          {options.map((option) => (
            <div
              onClick={() => onSelect(option)}
              key={option.id}
              className="tagsSelector-item"
              style={{ borderColor: `#${option.hexValue}` }}
            >
              {option.value}
            </div>
          ))}
          </div>
          {options.length === 0 && (
            <div>{<LocalizationCaption captionKey={LocalizationKey.NoTags} />}</div>
          )}
          {loading && (
            <div><LocalizationCaption captionKey={LocalizationKey.TagsLoading} />...</div>
          )}
        </div>
      )}
    </div>
  );
};
