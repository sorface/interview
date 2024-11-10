import { UserType } from '../../../types/user';

export type ParsedWsMessage =
  {
    Type: 'room-code-editor-enabled';
    Value: {
      Enabled: boolean;
    };
  } | {
    Type: 'ChatMessage';
    Id: string;
    CreatedById: string;
    CreatedAt: string;
    Value: {
      Nickname: string;
      Message: string;
    };
  } | {
    Type: 'ChangeCodeEditor';
    CreatedById: string;
    Value: {
      Content: string;
      Source: 'User' | 'System';
    };
  } | {
    Type: 'all users';
    Value: Array<{
      Id: string;
      Nickname: string;
      Avatar: string;
      ParticipantType: UserType;
    }>;
  } | {
    Type: 'user joined';
    Value: {
      From: {
        Id: string;
        Nickname: string;
        Avatar: string;
        ParticipantType: UserType;
      };
      Signal: string;
    };
  } | {
    Type: 'user left';
    Value: {
      Id: string;
    };
  } | {
    Type: 'receiving returned signal';
    Value: {
      From: string;
      Signal: string;
    };
  } | {
    Type: 'Like' | 'Dislike';
    Value: {
      UserId: string;
    };
  } | {
    Type: 'CodeEditorCursor';
    Value: {
      UserId: string;
      AdditionalData: {
        nickname: string;
        cursor: {
          column: number;
          lineNumber: number;
        };
        selection: {
          start: {
            column: number;
            lineNumber: number;
          };
          end: {
            column: number;
            lineNumber: number;
          };
        };
      };
    };
  };

const parseWsPayload = (parsedData: any) => {
  try {
    return JSON.parse(parsedData?.Value);
  } catch {
    return parsedData?.Value;
  }
};

export const parseWsMessage = (message: MessageEvent<any> | null): ParsedWsMessage | null => {
  if (!message) {
    return null;
  }
  try {
    const parsedData = JSON.parse(message.data);
    const parsedPayload = parseWsPayload(parsedData);
    return {
      ...parsedData,
      Value: parsedPayload,
    };
  } catch {
    return null;
  }
};
