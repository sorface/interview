import { ApiContractGet, ApiContractPatch, ApiContractPost, ApiContractPut } from './types/apiContracts';
import { Question, QuestionType } from './types/question';
import { Tag } from './types/tag';
import { Reaction } from './types/reaction';
import { Room, RoomAccessType, RoomInvite, RoomQuestion, RoomQuestionState, RoomStateAdditionalStatefulPayload, RoomStatus } from './types/room';
import { User, UserType } from './types/user';
import { Category } from './types/category';

export interface PaginationUrlParams {
  PageSize: number;
  PageNumber: number;
}

export interface CreateRoomBody {
  name: string;
  questions: Array<{ id: Question['id']; order: number; }>;
  experts: Array<User['id']>;
  examinees: Array<User['id']>;
  tags: Array<Tag['id']>;
  accessType: RoomAccessType;
  scheduleStartTime: string;
  duration: number;
}

export interface SendEventBody {
  roomId: Room['id'];
  type: string;
  additionalData?: RoomStateAdditionalStatefulPayload;
}

export interface GetRoomPageParams extends PaginationUrlParams {
  Name: string;
  Participants: string[];
  Statuses: RoomStatus[];
  StartValue?: string;
  EndValue?: string;
}

export interface GetRoomCalendarParams {
  StartDateTime: string;
  EndDateTime: string;
  RoomStatus: RoomStatus[];
  TimeZoneOffset: number;
}

export interface GetRoomParticipantParams {
  RoomId: Room['id'];
  UserId: User['id'];
}

export interface RoomIdParam {
  roomId: Room['id'];
}

export interface RoomEditBody {
  id: string;
  name: string;
  questions: Array<Omit<RoomQuestion, 'state'>>;
  scheduleStartTime: Room['scheduledStartTime'];
  durationSec?: number;
}

const eventsSearchLimit = 50;

export const roomsApiDeclaration = {
  getPage: (params: GetRoomPageParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/rooms',
    urlParams: params,
  }),
  calendar: (params: GetRoomCalendarParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/rooms/calendar',
    urlParams: params,
  }),
  getById: (id: Room['id']): ApiContractGet => ({
    method: 'GET',
    baseUrl: `/rooms/${id}`,
  }),
  getState: (id: Room['id']): ApiContractGet => ({
    method: 'GET',
    baseUrl: `/rooms/${id}/state`,
  }),
  analytics: (id: Room['id']): ApiContractGet => ({
    method: 'GET',
    baseUrl: `/rooms/${id}/analytics`,
  }),
  analyticsSummary: (id: Room['id']): ApiContractGet => ({
    method: 'GET',
    baseUrl: `/rooms/${id}/analytics/summary`,
  }),
  create: (body: CreateRoomBody): ApiContractPost => ({
    method: 'POST',
    baseUrl: '/rooms',
    body,
  }),
  sendEvent: (body: SendEventBody): ApiContractPost => ({
    method: 'POST',
    baseUrl: '/rooms/event',
    body,
  }),
  close: (id: Room['id']): ApiContractPatch => ({
    method: 'PATCH',
    baseUrl: `/rooms/${id}/close`,
    body: {},
  }),
  startReview: (id: Room['id']): ApiContractPatch => ({
    method: 'PATCH',
    baseUrl: `/rooms/${id}/startReview`,
    body: {},
  }),
  getParticipant: (params: GetRoomParticipantParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/room-participants',
    urlParams: params,
  }),
  eventsSearch: (params: RoomIdParam): ApiContractPost => ({
    method: 'POST',
    baseUrl: `/rooms/${params.roomId}/transcription/search`,
    body: {
      ChatMessage: {
        responseName: 'ChatMessage',
        last: eventsSearchLimit,
      },
      VoiceRecognition: {
        responseName: 'VoiceRecognition',
        last: eventsSearchLimit,
      },
    },
  }),
  edit: (body: RoomEditBody): ApiContractPatch => ({
    method: 'PATCH',
    baseUrl: `/rooms/${body.id}`,
    body,
  }),
};

export interface ChangeActiveQuestionBody {
  roomId: Room['id'];
  questionId: Question['id'];
}

export interface GetRoomQuestionsBody {
  RoomId: Room['id'];
  States: RoomQuestionState[];
}

export interface GetAnswerParams {
  roomId: Room['id'];
  questionId: Question['id'];
}

export const roomQuestionApiDeclaration = {
  changeActiveQuestion: (body: ChangeActiveQuestionBody): ApiContractPut => ({
    method: 'PUT',
    baseUrl: '/room-questions/active-question',
    body,
  }),
  getRoomQuestions: (params: GetRoomQuestionsBody): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/room-questions',
    urlParams: params,
  }),
  getAnswer: (params: GetAnswerParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/room-questions/answer',
    urlParams: params,
  }),
};

export interface GetRoomQuestionEvaluationParams {
  roomId: Room['id'];
  questionId: Question['id'];
}

export interface MergeRoomQuestionEvaluationBody {
  roomId: Room['id'];
  questionId: Question['id'];
  mark: number | null;
  review: string;
}

export const roomQuestionEvaluationApiDeclaration = {
  get: (urlParams: GetRoomQuestionEvaluationParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/room-evaluations',
    urlParams,
  }),
  getMy: (roomId: string): ApiContractGet => ({
    method: 'GET',
    baseUrl: `/room-evaluations/${roomId}/my`,
  }),
  merge: (body: MergeRoomQuestionEvaluationBody): ApiContractPost => ({
    method: 'POST',
    baseUrl: '/room-evaluations/merge',
    body,
  }),
};

export interface CreateQuestionBody {
  value: string;
  tags: Array<Tag['id']>
  type: QuestionType;
  categoryId: Category['id'];
  codeEditor: Question['codeEditor'] | null;
  answers: Array<{
    title: string;
    content: string;
    codeEditor: boolean;
  }>;
}

export interface UpdateQuestionBody extends CreateQuestionBody {
  id: string;
}

export interface GetQuestionsParams extends PaginationUrlParams {
  tags: Array<Tag['id']>;
  value: string;
  categoryId: Category['id'];
}

export const questionsApiDeclaration = {
  getPage: (params: GetQuestionsParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/questions',
    urlParams: {
      'Page.PageSize': params.PageSize,
      'Page.PageNumber': params.PageNumber,
      Tags: params.tags,
      Value: params.value,
      CategoryId: params.categoryId,
    },
  }),
  getPageArchived: (params: PaginationUrlParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/questions/archived',
    urlParams: {
      'PageSize': params.PageSize,
      'PageNumber': params.PageNumber,
    },
  }),
  get: (id: Question['id']): ApiContractGet => ({
    method: 'GET',
    baseUrl: `/questions/${id}`,
  }),
  create: (question: CreateQuestionBody): ApiContractPost => ({
    method: 'POST',
    baseUrl: '/questions',
    body: question,
  }),
  update: (question: UpdateQuestionBody): ApiContractPut => ({
    method: 'PUT',
    baseUrl: `/questions/${question.id}`,
    body: question,
  }),
  archive: (id: Question['id']): ApiContractPatch => ({
    method: 'PATCH',
    baseUrl: `/questions/${id}/archive`,
    body: {},
  }),
  unarchive: (id: Question['id']): ApiContractPatch => ({
    method: 'PATCH',
    baseUrl: `/questions/${id}/unarchive`,
    body: {},
  }),
};

export interface GetTagsParams extends PaginationUrlParams {
  value: string;
}

export interface CreateTagBody {
  value: string;
  hexValue: string;
}

export const tagsApiDeclaration = {
  getPage: (params: GetTagsParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/tags/tag',
    urlParams: params,
  }),
  createTag: (body: CreateTagBody): ApiContractPost => ({
    method: 'POST',
    baseUrl: '/tags/tag',
    body,
  }),
};

export const usersApiDeclaration = {
  getPage: (pagination: PaginationUrlParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/users',
    urlParams: pagination,
  }),
};

export const reactionsApiDeclaration = {
  getPage: (pagination: PaginationUrlParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/reactions',
    urlParams: pagination,
  }),
};

export interface SendReactionBody {
  reactionId: Reaction['id'];
  roomId: Room['id'];
  payload: string;
}

export const roomReactionApiDeclaration = {
  send: (body: SendReactionBody): ApiContractPost => ({
    method: 'POST',
    baseUrl: '/room-reactions',
    body,
  }),
};

export interface GetParticipantParams {
  userId: Reaction['id'];
  roomId: Room['id'];
}

export interface ChangeParticipantStatusBody {
  userId: Reaction['id'];
  roomId: Room['id'];
  userType: string;
}

export const roomParticipantApiDeclaration = {
  getRoomParticipant: (params: GetParticipantParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/room-participants',
    urlParams: params,
  }),
  changeParticipantStatus: (body: ChangeParticipantStatusBody): ApiContractPatch => ({
    method: 'PATCH',
    baseUrl: '/room-participants',
    body,
  }),
};

export interface GetRoomReviewsParams {
  'Page.PageSize': number;
  'Page.PageNumber': number;
  'Filter.RoomId': Room['id'],
}

export interface UpsertRoomReviewsBody {
  roomId: Room['id'];
  review: string;
}

export interface CompleteRoomReviewsBody {
  roomId: Room['id'];
}

export const roomReviewApiDeclaration = {
  getPage: (params: GetRoomReviewsParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/room-reviews',
    urlParams: params,
  }),
  getMy: (roomId: Room['id']): ApiContractGet => ({
    method: 'GET',
    baseUrl: `/room-reviews/${roomId}/my`,
  }),
  upsert: (body: UpsertRoomReviewsBody): ApiContractPut => ({
    method: 'PUT',
    baseUrl: '/room-reviews/upsert',
    body,
  }),
  complete: (body: CompleteRoomReviewsBody): ApiContractPost => ({
    method: 'POST',
    baseUrl: '/room-reviews/complete',
    body,
  }),
};

export interface ApplyRoomInviteBody {
  roomId: Room['id'];
  inviteId: RoomInvite['inviteId'];
}

export interface GenerateRoomInviteBody {
  roomId: Room['id'];
  participantType: UserType;
}

export const roomInviteApiDeclaration = {
  get: (params: RoomIdParam): ApiContractGet => ({
    method: 'GET',
    baseUrl: `/rooms/invites/${params.roomId}`,
  }),
  apply: (body: ApplyRoomInviteBody): ApiContractPost => ({
    method: 'POST',
    baseUrl: `/rooms/invites/${body.roomId}/apply`,
    urlParams: { inviteId: body.inviteId },
    body: {},
  }),
  generate: (body: GenerateRoomInviteBody): ApiContractPut => ({
    method: 'PUT',
    baseUrl: '/rooms/invites',
    body,
  }),
  generateAll: (param: RoomIdParam): ApiContractPost => ({
    method: 'POST',
    baseUrl: `/rooms/invites/${param.roomId}`,
    body: {},
  }),
};

export const eventApiDeclaration = {
  get: (params: PaginationUrlParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/event',
    urlParams: params,
  }),
};

export interface CreateCategoryBody {
  name: string;
  parentId: string | null;
}

export interface UpdateCategoryBody extends CreateCategoryBody {
  id: string;
}

export interface GetCategoriesParams extends PaginationUrlParams {
  name: string;
  parentId?: Category['id'] | null;
  showOnlyWithoutParent?: boolean;
}

export const categoriesApiDeclaration = {
  getPage: (params: GetCategoriesParams): ApiContractGet => ({
    method: 'GET',
    baseUrl: '/category',
    urlParams: {
      'Page.PageSize': params.PageSize,
      'Page.PageNumber': params.PageNumber,
      Name: params.name,
      ...(typeof params.showOnlyWithoutParent === 'boolean' && ({ 'Filter.ShowOnlyWithoutParent': params.showOnlyWithoutParent })),
      ...(params.parentId && ({ 'Filter.ParentId': params.parentId })),
    },
  }),
  get: (id: Category['id']): ApiContractGet => ({
    method: 'GET',
    baseUrl: `/category/${id}`,
  }),
  create: (category: CreateCategoryBody): ApiContractPost => ({
    method: 'POST',
    baseUrl: '/category',
    body: category,
  }),
  update: (category: UpdateCategoryBody): ApiContractPut => ({
    method: 'PUT',
    baseUrl: `/category/${category.id}`,
    body: { name: category.name, parentId: category.parentId },
  }),
  archive: (id: Category['id']): ApiContractPost => ({
    method: 'POST',
    baseUrl: `/category/archive/${id}`,
    body: undefined,
  }),
};
