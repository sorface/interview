export const inviteParamName = 'invite';

export const pathnames = {
  home: '/:redirect?',
  rooms: '/rooms',
  roomsParticipants: '/rooms/participants/:id',
  room: `/room/:id/:${inviteParamName}?`,
  roomAnalyticsSummary: '/rooms/:id/analytics/summary',
  questions: '/questions/:category',
  questionsCreate: '/questions/create',
  questionsEdit: '/questions/edit/:id',
  session: '/session',
  terms: '/terms',
  categories: '/categories',
  categoriesCreate: '/categories/create',
  categoriesEdit: '/categories/edit/:id',
};

export const enum IconNames {
  None = 'alert-circle',
  MicOn = 'mic',
  MicOff = 'mic-off',
  VideocamOn = 'videocam',
  VideocamOff = 'videocam-off',
  Settings = 'settings',
  RecognitionOn = 'volume-high',
  RecognitionOff = 'volume-mute',
  Chat = 'chatbubble-ellipses',
  Like = 'thumbs-up',
  Dislike = 'thumbs-down',
  CodeEditor = 'code-slash',
  ThemeSwitchLight = 'sunny',
  ThemeSwitchDark = 'moon',
  People = 'people',
  Clipboard = 'clipboard',
  Refresh = 'refresh',
  TV = 'tv',
  Add = 'add',
  Trash = 'trash',
  Close = 'close',
  ChevronForward = 'chevron-forward',
  Link = 'link',
  ChevronDown = 'chevron-down',
  Options = 'options',
  EllipsisVertical = 'ellipsis-vertical',
}

export const enum IconThemePostfix {
  Dark = '-sharp',
  Light = '-outline',
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
