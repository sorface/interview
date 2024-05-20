import { ChangeEventHandler, FunctionComponent, useEffect, useState } from 'react';
import { Tag } from '../../types/tag';
import { useApiMethod } from '../../hooks/useApiMethod';
import { GetTagsParams, tagsApiDeclaration } from '../../apiDeclarations';
import { TagsSelector } from '../TagsSelector/TagsSelector';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { LocalizationCaption } from '../LocalizationCaption/LocalizationCaption';

import './QustionsSearch.css'

const pageSize = 30;
const pageNumber = 1;

interface QustionsSearchProps {
  onSearchChange: (value: string) => void;
  onTagsChange: (tags: Tag[]) => void;
}

export const QustionsSearch: FunctionComponent<QustionsSearchProps> = ({
  onSearchChange,
  onTagsChange,
}) => {
  const [tagsSearchValue, setTagsSearchValue] = useState('');
  const [selectedTags, setSelectedTags] = useState<Tag[]>([]);
  const [searchValue, setSearchValue] = useState('');
  const localizationCaptions = useLocalizationCaptions();

  const {
    apiMethodState: tagsState,
    fetchData: fetchTags,
  } = useApiMethod<Tag[], GetTagsParams>(tagsApiDeclaration.getPage);
  const { process: { loading: tagsLoading, error: tagsError }, data: tags } = tagsState;

  useEffect(
    () => onSearchChange(searchValue),
    [searchValue, onSearchChange]
  );

  useEffect(
    () => onTagsChange(selectedTags),
    [selectedTags, onTagsChange]
  );

  useEffect(() => {
    fetchTags({
      PageNumber: pageNumber,
      PageSize: pageSize,
      value: tagsSearchValue,
    });
  }, [tagsSearchValue, fetchTags]);

  const handleSelect = (tag: Tag) => {
    setSelectedTags([...selectedTags, tag]);
  };

  const handleUnselect = (tag: Tag) => {
    const newSelectedTags = selectedTags.filter(tg => tg.id !== tag.id);
    setSelectedTags(newSelectedTags);
  };

  const handleTagSearch = (value: string) => {
    setTagsSearchValue(value);
  };

  const handleSearchChange: ChangeEventHandler<HTMLInputElement> = (e) => {
    setSearchValue(e.target.value);
  };

  return (
    <div className="qustions-search">
      <input
        type="text"
        className="qustions-search-value"
        placeholder={localizationCaptions[LocalizationKey.SearchByValue]}
        value={searchValue}
        onChange={handleSearchChange}
      />
      {tagsError ? (
        <div><LocalizationCaption captionKey={LocalizationKey.Error} />: {tagsError}</div>
      ) : (
        <TagsSelector
          placeHolder={localizationCaptions[LocalizationKey.SearchByTags]}
          loading={tagsLoading}
          tags={tags || []}
          selectedTags={selectedTags}
          onSelect={handleSelect}
          onUnselect={handleUnselect}
          onSearch={handleTagSearch}
        />
      )}
    </div>
  );
};
