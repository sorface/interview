export const inviteParamName = 'invite';

export const pathnames = {
  home: '/:redirect?',
  currentRooms: '/rooms/current',
  closedRooms: '/rooms/closed',
  roomsParticipants: '/rooms/participants/:id',
  room: `/room/:id/:${inviteParamName}?`,
  roomReview: '/rooms/:id/review',
  roomAnalyticsSummary: '/rooms/:id/analytics/summary',
  roomAnalytics: '/rooms/:id/analytics',
  questions: '/questions/:rootCategory/:subCategory',
  questionsCreate: '/questions/create/:rootCategory/:subCategory',
  questionsEdit: '/questions/edit/:id',
  session: '/session',
  terms: '/terms',
  categories: '/categories',
  categoriesCreate: '/categories/create',
  categoriesEdit: '/categories/edit/:id',
};

export const enum IconNames {
  None = 'alert-circle-outline',
  MicOn = 'mic-outline',
  MicOff = 'mic-off-outline',
  VideocamOn = 'videocam-outline',
  VideocamOff = 'videocam-off-outline',
  Settings = 'settings-outline',
  RecognitionOn = 'volume-high-outline',
  RecognitionOff = 'volume-mute-outline',
  Chat = 'chatbubble-ellipses-outline',
  Like = 'thumbs-up-outline',
  Dislike = 'thumbs-down-outline',
  CodeEditor = 'code-slash-outline',
  ThemeSwitchLight = 'sunny-outline',
  ThemeSwitchDark = 'moon-outline',
  People = 'people-outline',
  Clipboard = 'clipboard-outline',
  Refresh = 'refresh-outline',
  TV = 'tv-outline',
  Add = 'add-outline',
  Trash = 'trash-outline',
  Close = 'close-outline',
  ChevronForward = 'chevron-forward-outline',
  Link = 'link-outline',
  ChevronDown = 'chevron-down-outline',
  Options = 'options-outline',
  EllipsisVertical = 'ellipsis-vertical-outline',
  Cube = 'cube-outline',
  Golf = 'golf-outline',
  Exit = 'exit-outline',
  Call = 'call-outline',
  ReorderFour = 'reorder-four-outline',
  Stop = 'stop-outline',
  PersonAdd = 'person-add-outline',
  Time = 'time-outline',
  CheckmarkDone = 'checkmark-done-outline',
  Information = 'information-circle-outline',
}

export const reactionIcon: Record<string, IconNames> = {
  Like: IconNames.Like,
  Dislike: IconNames.Dislike,
  CodeEditor: IconNames.CodeEditor,
}

export const toastSuccessOptions = {
  icon: '👌',
};

export const enum EventName {
  CodeEditor = 'CodeEditor',
  CodeEditorLanguage = 'CodeEditorLanguage',
  CodeEditorCursor = 'CodeEditorCursor',
}
