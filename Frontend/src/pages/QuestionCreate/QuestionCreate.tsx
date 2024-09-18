import React, { ChangeEvent, ChangeEventHandler, MouseEvent, FunctionComponent, useContext, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useParams } from 'react-router-dom';
import { CreateQuestionBody, GetCategoriesParams, UpdateQuestionBody, categoriesApiDeclaration, questionsApiDeclaration } from '../../apiDeclarations';
import { Loader } from '../../components/Loader/Loader';
import { IconNames } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { CodeEditorLang, Question, QuestionAnswer, QuestionType } from '../../types/question';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { AuthContext } from '../../context/AuthContext';
import { checkAdmin } from '../../utils/checkAdmin';
import { Category } from '../../types/category';
import { Gap } from '../../components/Gap/Gap';
import { Icon } from '../Room/components/Icon/Icon';
import { CodeEditor } from '../../components/CodeEditor/CodeEditor';
import { Button } from '../../components/Button/Button';
import { ModalWithProgressWarning } from '../../components/ModalWithProgressWarning/ModalWithProgressWarning';
import { ModalFooter } from '../../components/ModalFooter/ModalFooter';
import { Typography } from '../../components/Typography/Typography';
import { QuestionCreateField } from './components/QuestionCreateField/QuestionCreateField';

import './QuestionCreate.css';

const valueFieldName = 'qestionText';

interface QuestionCreateProps {
  editQuestionId: string | null;
  open: boolean;
  onClose: () => void;
}

interface QuestionAnswerFrontend extends QuestionAnswer {
  new?: boolean;
}

export const QuestionCreate: FunctionComponent<QuestionCreateProps> = ({
  editQuestionId,
  open,
  onClose,
}) => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
  const localizationCaptions = useLocalizationCaptions();
  const {
    rootCategory: rootCategoryParam,
    subCategory: subCategoryParam,
  } = useParams();
  const {
    apiMethodState: questionState,
    fetchData: fetchCreateQuestion,
  } = useApiMethod<Question['id'], CreateQuestionBody>(questionsApiDeclaration.create);
  const { process: { loading, error }, data: createdQuestionId } = questionState;

  const { apiMethodState: updatingQuestionState, fetchData: fetchUpdateQuestion } = useApiMethod<Question, UpdateQuestionBody>(questionsApiDeclaration.update);
  const {
    process: { loading: updatingLoading, error: updatingError },
    data: updatedQuestionId,
  } = updatingQuestionState;

  const {
    apiMethodState: getQuestionState,
    fetchData: fetchQuestion,
  } = useApiMethod<Question, Question['id']>(questionsApiDeclaration.get);
  const { process: { loading: questionLoading, error: questionError }, data: question } = getQuestionState;

  const { apiMethodState: rootCategoriesState, fetchData: fetchRootCategories } = useApiMethod<Category[], GetCategoriesParams>(categoriesApiDeclaration.getPage);
  const { process: { loading: rootCategoriesLoading, error: rootCategoriesError }, data: rootCategories } = rootCategoriesState;

  const { apiMethodState: subCategoriesState, fetchData: fetchSubCategories } = useApiMethod<Category[], GetCategoriesParams>(categoriesApiDeclaration.getPage);
  const { process: { loading: subCategoriesLoading, error: subCategoriesError }, data: subCategories } = subCategoriesState;

  const [questionValue, setQuestionValue] = useState('');
  const [type, setType] = useState<QuestionType>(QuestionType.Private);
  const [rootCategory, setRootCategory] = useState(rootCategoryParam || '');
  const [subCategory, setSubCategory] = useState(subCategoryParam || '');
  const [codeEditor, setCodeEditor] = useState<Question['codeEditor'] | null>(null);
  const [answers, setAnswers] = useState<QuestionAnswerFrontend[]>([]);

  const totalLoading = loading || updatingLoading || questionLoading || rootCategoriesLoading || subCategoriesLoading;
  const totalError = error || questionError || updatingError || rootCategoriesError || subCategoriesError;

  useEffect(() => {
    if (!editQuestionId) {
      return;
    }
    fetchQuestion(editQuestionId);
  }, [editQuestionId, fetchQuestion]);

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
    if (!question) {
      return;
    }
    setQuestionValue(question.value);
    setCodeEditor(question.codeEditor);
    setAnswers(question.answers);
  }, [question]);

  useEffect(() => {
    if (!createdQuestionId) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.QuestionCreatedSuccessfully]);
    onClose();
  }, [createdQuestionId, localizationCaptions, onClose]);

  useEffect(() => {
    if (!updatedQuestionId) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.QuestionUpdatedSuccessfully]);
    onClose();
  }, [updatedQuestionId, localizationCaptions, onClose]);

  const handleQuestionValueChange = (event: ChangeEvent<HTMLInputElement>) => {
    setQuestionValue(event.target.value);
  };

  const handleSubmitCreate = () => {
    fetchCreateQuestion({
      value: questionValue,
      tags: [],
      type,
      categoryId: subCategory,
      answers,
      codeEditor,
    });
  };

  const handleSubmitEdit = () => {
    if (!question) {
      return;
    }
    const answersForRequest = answers.map(answer =>
      answer.new ? { ...answer, new: undefined, id: undefined } : answer
    );
    fetchUpdateQuestion({
      id: question.id,
      value: questionValue,
      tags: [],
      type,
      categoryId: subCategory,
      answers: answersForRequest,
      codeEditor,
    });
  };

  const handleTypeChange = (e: ChangeEvent<HTMLSelectElement>) => {
    setType(e.target.value as QuestionType);
  };

  const handleRootCategoryChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setRootCategory(e.target.value);
    setSubCategory('');
  };

  const handleSubCategoryChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setSubCategory(e.target.value);
  };

  const handleAddCodeEditor = (e: MouseEvent) => {
    e.preventDefault();
    setCodeEditor({
      content: '',
      lang: CodeEditorLang.Plaintext,
    });
  };

  const handleRemoveCodeEditor = (e: MouseEvent) => {
    e.preventDefault();
    setCodeEditor(null);
  };

  const handleLanguageChange = (lang: CodeEditorLang) => {
    if (!codeEditor) {
      return;
    }
    setCodeEditor({
      ...codeEditor,
      lang,
    });
  };

  const handleCodeEditorChange = (content: string | undefined) => {
    if (!codeEditor) {
      return;
    }
    setCodeEditor({
      ...codeEditor,
      content: content || '',
    });
  };

  const handleAddQuestionAnswer = (e: MouseEvent) => {
    e.preventDefault();
    setAnswers([
      ...answers,
      {
        id: `${Math.random()}`,
        new: true,
        codeEditor: false,
        title: `${localizationCaptions[LocalizationKey.QuestionAnswerOptionDefaultName]} ${answers.length + 1}`,
        content: '',
      },
    ]);
  };

  const handleAnswerTitleChange = (id: string): ChangeEventHandler<HTMLInputElement> => (e) => {
    const newAnswers = answers.map(answer => {
      if (answer.id !== id) {
        return answer;
      }
      return {
        ...answer,
        title: e.target.value,
      };
    });
    setAnswers(newAnswers);
  };

  const handleAnswerLanguageChange = (id: string) => (lang: CodeEditorLang) => {
    const newAnswers = answers.map(answer => {
      if (answer.id !== id) {
        return answer;
      }
      return {
        ...answer,
        codeEditor: lang !== CodeEditorLang.Plaintext,
      };
    });
    setAnswers(newAnswers);
  };

  const handleAnswerContentChange = (id: string) => (content: string | undefined) => {
    const newAnswers = answers.map(answer => {
      if (answer.id !== id) {
        return answer;
      }
      return {
        ...answer,
        content: content || '',
      };
    });
    setAnswers(newAnswers);
  };

  const handleAnswerDelete = (id: string) => (e: MouseEvent) => {
    e.preventDefault();
    const newAnswers = answers.filter(answer => answer.id !== id);
    setAnswers(newAnswers);
  };

  const renderStatus = () => {
    if (totalError) {
      return (
        <>
          <Typography size='m' error>
            <div className='flex'>
              <Icon name={IconNames.Information} />
              <Gap sizeRem={0.25} horizontal />
              {totalError}
            </div>
          </Typography>
          <Gap sizeRem={0.5} />
        </>
      );
    }
    if (totalLoading) {
      return (
        <>
          <Loader />
          <Gap sizeRem={0.5} />
        </>
      );
    }
    return <></>;
  };

  return (
    <ModalWithProgressWarning
      warningCaption={localizationCaptions[LocalizationKey.CurrentQuestionNotBeSaved]}
      contentLabel={editQuestionId ? localizationCaptions[LocalizationKey.EditQuestion] : localizationCaptions[LocalizationKey.CreateQuestion]}
      open={open}
      wide
      onClose={onClose}
    >
      <div className="question-create text-left">
        {renderStatus()}
        <div className='flex'>
          <QuestionCreateField.Wrapper className='flex-1'>
            <QuestionCreateField.Label>
              <label htmlFor="rootCategory">{localizationCaptions[LocalizationKey.Category]}</label>
            </QuestionCreateField.Label>
            <QuestionCreateField.Content className='flex'>
              <select id="rootCategory" className='flex-1' value={rootCategory} onChange={handleRootCategoryChange}>
                <option value=''>{localizationCaptions[LocalizationKey.NotSelected]}</option>
                {rootCategories?.map(rootCategory => (
                  <option key={rootCategory.id} value={rootCategory.id}>{rootCategory.name}</option>
                ))}
              </select>
            </QuestionCreateField.Content>
          </QuestionCreateField.Wrapper>
          <Gap sizeRem={0.625} horizontal />
          <QuestionCreateField.Wrapper className='flex-1'>
            <QuestionCreateField.Label>
              <label htmlFor="subCategory">{localizationCaptions[LocalizationKey.Subcategory]}</label>
            </QuestionCreateField.Label>
            <QuestionCreateField.Content className='flex'>
              <select id="subCategory" className='flex-1' value={subCategory} onChange={handleSubCategoryChange}>
                <option value=''>{localizationCaptions[LocalizationKey.NotSelected]}</option>
                {subCategories?.map(subCategory => (
                  <option key={subCategory.id} value={subCategory.id}>{subCategory.name}</option>
                ))}
              </select>
            </QuestionCreateField.Content>
          </QuestionCreateField.Wrapper>
        </div>
        <Gap sizeRem={1.5} />
        <QuestionCreateField.Wrapper>
          <QuestionCreateField.Label>
            <label htmlFor="qestionText">{localizationCaptions[LocalizationKey.QuestionText]}:</label>
          </QuestionCreateField.Label>
          <QuestionCreateField.Content className='flex'>
            <input id="qestionText" className='flex-1' name={valueFieldName} type="text" value={questionValue} onChange={handleQuestionValueChange} />
          </QuestionCreateField.Content>
        </QuestionCreateField.Wrapper>
        <Gap sizeRem={1} />
        <div className='question-code-editor-controls'>
          <Button variant='active2' style={{ ...(codeEditor && { display: 'none' }) }} onClick={handleAddCodeEditor}>
            <Icon name={IconNames.Add} />
            {localizationCaptions[LocalizationKey.QuestionAddCodeEditor]}
          </Button>
          <div style={{ ...(!codeEditor && { display: 'none' }) }}>
            <Typography size='m' bold>
              {localizationCaptions[LocalizationKey.QuestionCodeEditor]}
            </Typography>
          </div>
          <div
            className='cursor-pointer hover:text-red text-grey3'
            style={{ ...(!codeEditor && { display: 'none' }) }}
            onClick={handleRemoveCodeEditor}
          >
            <Typography size='m'>
              {localizationCaptions[LocalizationKey.QuestionRemoveCodeEditor]}
            </Typography>
          </div>
        </div>
        {codeEditor && (
          <>
            <Gap sizeRem={0.5} />
            <CodeEditor
              languages={Object.values(CodeEditorLang)}
              language={codeEditor.lang}
              value={codeEditor.content}
              alwaysConsumeMouseWheel={false}
              scrollBeyondLastLine={false}
              onLanguageChange={handleLanguageChange}
              onChange={handleCodeEditorChange}
            />
          </>
        )}
        <Gap sizeRem={1.5} />
        <div>
          <QuestionCreateField.Wrapper>
            <QuestionCreateField.Label>
              {localizationCaptions[LocalizationKey.QuestionAnswerOptions]}
            </QuestionCreateField.Label>
            <QuestionCreateField.Content>
              {answers.map(answer => (
                <div key={answer.id}>
                  <div className='question-answer-controls flex items-center'>
                    <input
                      type='text'
                      className='question-answer-name'
                      placeholder={localizationCaptions[LocalizationKey.QuestionAnswerOptionName]}
                      value={answer.title}
                      onChange={handleAnswerTitleChange(answer.id)}
                    />
                    <div className='cursor-pointer hover:text-red text-grey3' onClick={handleAnswerDelete(answer.id)}>
                      <Typography size='m'>
                        {localizationCaptions[LocalizationKey.QuestionDeleteAnswerOption]}
                      </Typography>
                    </div>
                  </div>
                  <Gap sizeRem={0.5} />
                  <CodeEditor
                    value={answer.content}
                    language={(answer.codeEditor && codeEditor?.lang) ? codeEditor.lang : CodeEditorLang.Plaintext}
                    languages={(codeEditor && codeEditor.lang !== CodeEditorLang.Plaintext) ? [CodeEditorLang.Plaintext, codeEditor.lang] : [CodeEditorLang.Plaintext]}
                    alwaysConsumeMouseWheel={false}
                    scrollBeyondLastLine={false}
                    onLanguageChange={handleAnswerLanguageChange(answer.id)}
                    onChange={handleAnswerContentChange(answer.id)}
                  />
                </div>
              ))}
              <Gap sizeRem={0.75} />
              <Button variant='active2' onClick={handleAddQuestionAnswer}>
                <Icon name={IconNames.Add} />
                {localizationCaptions[LocalizationKey.QuestionAddAnswerOption]}
              </Button>
            </QuestionCreateField.Content>
          </QuestionCreateField.Wrapper>
        </div>
        {admin && (
          <div>
            <Gap sizeRem={1.5} />
            <div><label htmlFor="qestionType">{localizationCaptions[LocalizationKey.QuestionType]}:</label></div>
            <select id="qestionType" value={type} onChange={handleTypeChange}>
              <option value={QuestionType.Private}>{localizationCaptions[LocalizationKey.QuestionTypePrivate]}</option>
              <option value={QuestionType.Public}>{localizationCaptions[LocalizationKey.QuestionTypePublic]}</option>
            </select>
          </div>
        )}
        <ModalFooter>
          <Button onClick={onClose}>{localizationCaptions[LocalizationKey.Cancel]}</Button>
          <Button variant='active' onClick={editQuestionId ? handleSubmitEdit : handleSubmitCreate}>
            {editQuestionId ? localizationCaptions[LocalizationKey.Save] : localizationCaptions[LocalizationKey.Create]}
          </Button>
        </ModalFooter>
      </div>
    </ModalWithProgressWarning>
  );
};
