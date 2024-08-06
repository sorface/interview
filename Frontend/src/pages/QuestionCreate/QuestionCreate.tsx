import React, { ChangeEvent, ChangeEventHandler, FormEvent, MouseEvent, FunctionComponent, useCallback, useContext, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useNavigate, useParams } from 'react-router-dom';
import { CreateQuestionBody, GetCategoriesParams, UpdateQuestionBody, categoriesApiDeclaration, questionsApiDeclaration } from '../../apiDeclarations';
import { Field } from '../../components/FieldsBlock/Field';
import { HeaderWithLink } from '../../components/HeaderWithLink/HeaderWithLink';
import { Loader } from '../../components/Loader/Loader';
import { MainContentWrapper } from '../../components/MainContentWrapper/MainContentWrapper';
import { SubmitField } from '../../components/SubmitField/SubmitField';
import { IconNames, pathnames, toastSuccessOptions } from '../../constants';
import { useApiMethod } from '../../hooks/useApiMethod';
import { CodeEditorLang, Question, QuestionType } from '../../types/question';
import { LocalizationKey } from '../../localization';
import { useLocalizationCaptions } from '../../hooks/useLocalizationCaptions';
import { AuthContext } from '../../context/AuthContext';
import { checkAdmin } from '../../utils/checkAdmin';
import { Category } from '../../types/category';
import { Gap } from '../../components/Gap/Gap';
import { Icon } from '../Room/components/Icon/Icon';
import { CodeEditor } from '../../components/CodeEditor/CodeEditor';
import { Button } from '../../components/Button/Button';

import './QuestionCreate.css';

const valueFieldName = 'qestionText';

export const QuestionCreate: FunctionComponent<{ edit: boolean; }> = ({ edit }) => {
  const auth = useContext(AuthContext);
  const admin = checkAdmin(auth);
  const localizationCaptions = useLocalizationCaptions();
  const {
    id,
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

  const navigate = useNavigate();
  const [questionValue, setQuestionValue] = useState('');
  const [type, setType] = useState<QuestionType>(QuestionType.Private);
  const [rootCategory, setRootCategory] = useState(rootCategoryParam || '');
  const [subCategory, setSubCategory] = useState(subCategoryParam || '');
  const [codeEditor, setCodeEditor] = useState<Question['codeEditor'] | null>(null);
  const [answers, setAnswers] = useState<Question['answers']>([]);

  const totalLoading = loading || updatingLoading || questionLoading || rootCategoriesLoading || subCategoriesLoading;
  const totalError = error || questionError || updatingError || rootCategoriesError || subCategoriesError;

  useEffect(() => {
    if (!edit) {
      return;
    }
    if (!id) {
      throw new Error('Question id not found');
    }
    fetchQuestion(id);
  }, [edit, id, fetchQuestion]);

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
    toast.success(localizationCaptions[LocalizationKey.QuestionCreatedSuccessfully], toastSuccessOptions);
    navigate(pathnames.questions);
  }, [createdQuestionId, localizationCaptions, navigate]);

  useEffect(() => {
    if (!updatedQuestionId) {
      return;
    }
    toast.success(localizationCaptions[LocalizationKey.QuestionUpdatedSuccessfully], toastSuccessOptions);
    navigate(pathnames.questions);
  }, [updatedQuestionId, localizationCaptions, navigate]);

  const handleQuestionValueChange = (event: ChangeEvent<HTMLInputElement>) => {
    setQuestionValue(event.target.value);
  };

  const handleSubmitCreate = useCallback(async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    fetchCreateQuestion({
      value: questionValue,
      tags: [],
      type,
      categoryId: subCategory,
      answers,
      codeEditor,
    });
  }, [questionValue, type, subCategory, answers, codeEditor, fetchCreateQuestion]);

  const handleSubmitEdit = useCallback(async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!question) {
      return;
    }
    fetchUpdateQuestion({
      id: question.id,
      value: questionValue,
      tags: [],
      type,
      categoryId: subCategory,
      answers,
      codeEditor,
    });

  }, [question, questionValue, type, subCategory, answers, codeEditor, fetchUpdateQuestion]);

  const handleTypeChange = (e: ChangeEvent<HTMLSelectElement>) => {
    setType(e.target.value as QuestionType);
  };

  const handleRootCategoryChange: ChangeEventHandler<HTMLSelectElement> = (e) => {
    setRootCategory(e.target.value);
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
        <Field>
          <div>{localizationCaptions[LocalizationKey.Error]}: {totalError}</div>
        </Field>
      );
    }
    if (totalLoading) {
      return (
        <Field>
          <Loader />
        </Field>
      );
    }
    return <></>;
  };

  return (
    <MainContentWrapper className="question-create">
      <HeaderWithLink
        title={localizationCaptions[LocalizationKey.CreateQuestion]}
        linkVisible={true}
        path={
          pathnames.questions
            .replace(':rootCategory', rootCategory || '')
            .replace(':subCategory', subCategory || '')
        }
        linkCaption="<"
        linkFloat="left"
      />
      {renderStatus()}
      <form onSubmit={edit ? handleSubmitEdit : handleSubmitCreate}>
        <Field>
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
        </Field>
        <Field>
          <div><label htmlFor="qestionText">{localizationCaptions[LocalizationKey.QuestionText]}:</label></div>
          <input id="qestionText" name={valueFieldName} type="text" value={questionValue} onChange={handleQuestionValueChange} />
          <Gap sizeRem={0.75} />
          <div className='question-code-editor-controls'>
            <Button style={{ ...(codeEditor && { display: 'none' }) }} onClick={handleAddCodeEditor}>
              <Icon name={IconNames.Add} />
              {localizationCaptions[LocalizationKey.QuestionAddCodeEditor]}
            </Button>
            <div style={{ ...(!codeEditor && { display: 'none' }) }}>
              {localizationCaptions[LocalizationKey.QuestionCodeEditor]}
            </div>
            <Button style={{ ...(!codeEditor && { display: 'none' }) }} onClick={handleRemoveCodeEditor}>
              <Icon name={IconNames.Trash} />
              {localizationCaptions[LocalizationKey.QuestionRemoveCodeEditor]}
            </Button>
          </div>
          {codeEditor && (
            <CodeEditor
              languages={Object.values(CodeEditorLang)}
              language={codeEditor.lang}
              value={codeEditor.content}
              onLanguageChange={handleLanguageChange}
              onChange={handleCodeEditorChange}
            />
          )}
        </Field>
        <Field>
          <div>{localizationCaptions[LocalizationKey.QuestionAnswerOptions]}</div>
          <Gap sizeRem={0.75} />
          {answers.map(answer => (
            <div key={answer.id}>
              <div className='question-answer-controls'>
                <input
                  className='question-answer-name'
                  placeholder={localizationCaptions[LocalizationKey.QuestionAnswerOptionName]}
                  value={answer.title}
                  onChange={handleAnswerTitleChange(answer.id)}
                />
                <Button onClick={handleAnswerDelete(answer.id)}>
                  <Icon name={IconNames.Trash} />
                  {localizationCaptions[LocalizationKey.QuestionDeleteAnswerOption]}
                </Button>
              </div>
              <CodeEditor
                value={answer.content}
                language={(answer.codeEditor && codeEditor?.lang) ? codeEditor.lang : CodeEditorLang.Plaintext}
                languages={codeEditor ? [CodeEditorLang.Plaintext, codeEditor.lang] : [CodeEditorLang.Plaintext]}
                onLanguageChange={handleAnswerLanguageChange(answer.id)}
                onChange={handleAnswerContentChange(answer.id)}
              />
            </div>
          ))}
          <Gap sizeRem={0.75} />
          <Button onClick={handleAddQuestionAnswer}>
            <Icon name={IconNames.Add} />
            {localizationCaptions[LocalizationKey.QuestionAddAnswerOption]}
          </Button>
        </Field>
        {admin && (
          <Field>
            <div><label htmlFor="qestionType">{localizationCaptions[LocalizationKey.QuestionType]}:</label></div>
            <select id="qestionType" value={type} onChange={handleTypeChange}>
              <option value={QuestionType.Private}>{localizationCaptions[LocalizationKey.QuestionTypePrivate]}</option>
              <option value={QuestionType.Public}>{localizationCaptions[LocalizationKey.QuestionTypePublic]}</option>
            </select>
          </Field>
        )}
        <SubmitField caption={localizationCaptions[LocalizationKey.Create]} />
      </form>
    </MainContentWrapper>
  );
};
