export const pathnames = {
  home: '/:redirect?',
  rooms: '/rooms',
  roomsCreate: '/rooms/create',
  roomsParticipants: '/rooms/participants/:id',
  room: '/room/:id',
  roomAnalyticsSummary: '/rooms/:id/analytics/summary',
  questions: '/questions',
  questionsCreate: '/questions/create',
  questionsEdit: '/questions/edit/:id',
  session: '/session',
  terms: '/terms',
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
  TV = 'tv',
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

