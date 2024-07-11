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
import { RoomQuestionListItem } from '../RoomCreate';
import { RoomQuestionsSelectorPreview } from '../RoomQuestionsSelectorPreview/RoomQuestionsSelectorPreview';

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
  };

  const handleSubCategoryChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setSubCategory(e.target.value);
  };

  const handleSave = () => {
    onSave(selectedQuestions);
  };

  const handleNextPage = () => {
    setPageNumber(pageNumber + 1);
  };

  const handleQuestionCheck = (question: RoomQuestionListItem, checked: boolean) => {
    if (checked) {
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
      <QuestionItem
        question={question}
        checked={!!selectedQuestions.find(selectedQuestion => selectedQuestion.id === question.id)}
        onCheck={(checked) => handleQuestionCheck(question, checked)}
      />
      <Gap sizeRem={0.25} />
    </Fragment>
  );

  return (
    <div>
      <div className='flex justify-between'>
        <div>
          <select id="rootCategory" value={rootCategory} onChange={handleRootCategoryChange}>
            <option value=''>{localizationCaptions[LocalizationKey.NotSelected]}</option>
            {rootCategories?.map(rootCategory => (
              <option key={rootCategory.id} value={rootCategory.id}>{rootCategory.name}</option>
            ))}
          </select>
          <select id="subCategory" value={subCategory} onChange={handleSubCategoryChange}>
            <option value=''>{localizationCaptions[LocalizationKey.NotSelected]}</option>
            {subCategories?.map(subCategory => (
              <option key={subCategory.id} value={subCategory.id}>{subCategory.name}</option>
            ))}
          </select>
        </div>
        <RoomQuestionsSelectorPreview
          qestions={selectedQuestions}
          onRemove={(qestion) => handleQuestionCheck(qestion, false)}
        />
      </div>
      {!!totalError && (
        <div>
          {localizationCaptions[LocalizationKey.Error]}: {totalError}
        </div>
      )}
      {!!subCategory && (
        <ItemsGrid
          currentData={questions ? questions.map((q, index) => ({ ...q, order: index })) : null}
          loading={totalLoading}
          triggerResetAccumData={`${rootCategory}${subCategory}`}
          loaderClassName='question-item field-wrap'
          renderItem={createQuestionItem}
          nextPageAvailable={questions?.length === pageSize}
          handleNextPage={handleNextPage}
        />
      )}
      <ModalFooter>
        <button onClick={onCancel}>{localizationCaptions[LocalizationKey.Cancel]}</button>
        <button className='active' onClick={handleSave}>{localizationCaptions[LocalizationKey.AddRoomQuestions]}</button>
      </ModalFooter>
    </div>
  )
};
