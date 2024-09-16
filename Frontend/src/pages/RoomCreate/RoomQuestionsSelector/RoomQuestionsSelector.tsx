import { ChangeEventHandler, Fragment, FunctionComponent, useEffect, useState } from 'react';
import { useApiMethod } from '../../../hooks/useApiMethod';
import { Category } from '../../../types/category';
import { GetCategoriesParams, GetQuestionsParams, categoriesApiDeclaration, questionsApiDeclaration } from '../../../apiDeclarations';
import { useLocalizationCaptions } from '../../../hooks/useLocalizationCaptions';
import { LocalizationKey } from '../../../localization';
import { ModalFooter } from '../../../components/ModalFooter/ModalFooter';
import { Question } from '../../../types/question';
import { ItemsGrid } from '../../../components/ItemsGrid/ItemsGrid';
import { QuestionItem } from '../../../components/QuestionItem/QuestionItem';
import { Gap } from '../../../components/Gap/Gap';
import { RoomQuestionListItem } from '../../../types/room';
import { RoomQuestionsSelectorPreview } from '../RoomQuestionsSelectorPreview/RoomQuestionsSelectorPreview';
import { Button } from '../../../components/Button/Button';
import { RoomCreateField } from '../RoomCreateField/RoomCreateField';
import { Typography } from '../../../components/Typography/Typography';
import { Checkbox } from '../../../components/Checkbox/Checkbox';

interface RoomQuestionsSelectorProps {
  preSelected: RoomQuestionListItem[];
  onCancel: () => void;
  onSave: (questions: RoomQuestionListItem[]) => void;
}

const pageSize = 10;
const initialPageNumber = 1;

export const RoomQuestionsSelector: FunctionComponent<RoomQuestionsSelectorProps> = ({
  preSelected,
  onCancel,
  onSave,
}) => {
  const localizationCaptions = useLocalizationCaptions();

  const { apiMethodState: rootCategoriesState, fetchData: fetchRootCategories } = useApiMethod<Category[], GetCategoriesParams>(categoriesApiDeclaration.getPage);
  const { process: { loading: rootCategoriesLoading, error: rootCategoriesError }, data: rootCategories } = rootCategoriesState;

  const { apiMethodState: subCategoriesState, fetchData: fetchSubCategories } = useApiMethod<Category[], GetCategoriesParams>(categoriesApiDeclaration.getPage);
  const { process: { loading: subCategoriesLoading, error: subCategoriesError }, data: subCategories } = subCategoriesState;

  const { apiMethodState: questionsState, fetchData: fetchQuestios } = useApiMethod<Question[], GetQuestionsParams>(questionsApiDeclaration.getPage);
  const { process: { loading: questionsLoading, error: questionsError }, data: questions } = questionsState;

  const [pageNumber, setPageNumber] = useState(initialPageNumber);
  const [rootCategory, setRootCategory] = useState('');
  const [subCategory, setSubCategory] = useState('');
  const [selectedQuestions, setSelectedQuestions] = useState<RoomQuestionListItem[]>(preSelected);

  const totalLoading = rootCategoriesLoading || subCategoriesLoading || questionsLoading;
  const totalError = rootCategoriesError || subCategoriesError || questionsError;
  const triggerResetAccumData = `${rootCategory}${subCategory}`;

  useEffect(() => {
    fetchRootCategories({
      name: '',
      PageNumber: 1,
      PageSize: 30,
      showOnlyWithoutParent: true,
    });
  }, [fetchRootCategories]);

  useEffect(() => {
    fetchSubCategories({
      name: '',
      PageNumber: 1,
      PageSize: 30,
      parentId: rootCategory,
    });
  }, [rootCategory, fetchSubCategories]);

  useEffect(() => {
    setPageNumber(initialPageNumber);
  }, [triggerResetAccumData]);

  useEffect(() => {
    if (!subCategory) {
      return;
    }
    fetchQuestios({
      PageNumber: pageNumber,
      PageSize: pageSize,
      tags: [],
      value: '',
      categoryId: subCategory,
    });
  }, [pageNumber, subCategory, fetchQuestios]);

  const handleRootCategoryChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setRootCategory(e.target.value);
    setSubCategory('');
  };

  const handleSubCategoryChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setPageNumber(1);
    setSubCategory(e.target.value);
  };

  const handleSave = () => {
    onSave(selectedQuestions);
  };

  const handleNextPage = () => {
    setPageNumber(pageNumber + 1);
  };

  const handleQuestionCheck = (question: RoomQuestionListItem) => {
    const checked = !!selectedQuestions.find(selectedQuestion => selectedQuestion.id === question.id);
    if (!checked) {
      const newSelectedQuestions = [
        ...selectedQuestions,
        {
          ...question,
          order: selectedQuestions.length,
        },
      ].map((selectedQuestion, index) => ({
        ...selectedQuestion,
        order: index,
      }))
      setSelectedQuestions(newSelectedQuestions);
      return;
    }
    const newSelectedQuestions = selectedQuestions.filter(
      selectedQuestion => selectedQuestion.id !== question.id
    );
    setSelectedQuestions(newSelectedQuestions);
  };

  const createQuestionItem = (question: RoomQuestionListItem) => (
    <Fragment key={question.id}>
      <div className='flex'>
        <div>
          <Gap sizeRem={1.25} />
          <Checkbox
            id={question.id}
            checked={!!selectedQuestions.find(selectedQuestion => selectedQuestion.id === question.id)}
            onChange={() => handleQuestionCheck(question)}
            label=''
          />
        </div>
        <Gap sizeRem={0.5} horizontal />
        <div className='flex-1'>
          <QuestionItem
            question={question}
            bgSelected={!!selectedQuestions.find(selectedQuestion => selectedQuestion.id === question.id)}
          />
        </div>
      </div>
      <Gap sizeRem={0.25} />
    </Fragment>
  );

  return (
    <div>
      <div className='flex justify-between'>
        <div className='flex w-full'>
          <RoomCreateField.Wrapper className='w-full max-w-15.75'>
            <RoomCreateField.Label>
              <label htmlFor="rootCategory"><Typography size='m' bold>{localizationCaptions[LocalizationKey.Category]}</Typography></label>
            </RoomCreateField.Label>
            <RoomCreateField.Content>
              <select id="rootCategory" className='w-full' value={rootCategory} onChange={handleRootCategoryChange}>
                <option value=''>{localizationCaptions[LocalizationKey.NotSelected]}</option>
                {rootCategories?.map(rootCategory => (
                  <option key={rootCategory.id} value={rootCategory.id}>{rootCategory.name}</option>
                ))}
              </select>
            </RoomCreateField.Content>
          </RoomCreateField.Wrapper>
          <Gap sizeRem={1} horizontal />
          <RoomCreateField.Wrapper className='w-full max-w-15.75'>
            <RoomCreateField.Label>
              <label htmlFor="subCategory"><Typography size='m' bold>{localizationCaptions[LocalizationKey.Subcategory]}</Typography></label>
            </RoomCreateField.Label>
            <RoomCreateField.Content>
              <select id="subCategory" className='w-full' value={subCategory} disabled={!rootCategory} onChange={handleSubCategoryChange}>
                <option value=''>{localizationCaptions[LocalizationKey.NotSelected]}</option>
                {subCategories?.map(subCategory => (
                  <option key={subCategory.id} value={subCategory.id}>{subCategory.name}</option>
                ))}
              </select>
            </RoomCreateField.Content>
          </RoomCreateField.Wrapper>
          <RoomCreateField.Wrapper className='ml-auto'>
            <RoomCreateField.Label>
              <Gap sizeRem={1.25} />
            </RoomCreateField.Label>
            <RoomCreateField.Content>
              <RoomQuestionsSelectorPreview
                qestions={selectedQuestions}
                onRemove={(qestion) => handleQuestionCheck(qestion)}
              />
            </RoomCreateField.Content>
          </RoomCreateField.Wrapper>
        </div>
      </div>
      <Gap sizeRem={1.5} />
      {!!totalError && (
        <div>
          {localizationCaptions[LocalizationKey.Error]}: {totalError}
        </div>
      )}
      {!!subCategory ? (
        <ItemsGrid
          currentData={questions ? questions.map((q, index) => ({ ...q, order: index })) : null}
          loading={totalLoading}
          error={null}
          triggerResetAccumData={triggerResetAccumData}
          loaderClassName='question-item field-wrap'
          renderItem={createQuestionItem}
          nextPageAvailable={questions?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      ) : (
        <div className='flex flex-col items-center'>
          <Gap sizeRem={5.375} />
          <div className='w-38.375 text-center'>
            <Typography size='xl' secondary>
              {localizationCaptions[LocalizationKey.SelectCategorySubcategory]}
            </Typography>
          </div>
          <Gap sizeRem={5.375} />
        </div>
      )}
      <ModalFooter>
        <Button onClick={onCancel}>{localizationCaptions[LocalizationKey.Cancel]}</Button>
        <Button variant='active' onClick={handleSave}>{localizationCaptions[LocalizationKey.AddRoomQuestions]}</Button>
      </ModalFooter>
    </div>
  )
};
