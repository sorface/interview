const parseWsPayload = (parsedData: any) => {
  try {
    return JSON.parse(parsedData?.Value);
  } catch {
    return parsedData?.Value;
  }
};

export const parseWsMessage = (data: MessageEvent<any>['data']) => {
  try {
    const parsedData = JSON.parse(data);
    const parsedPayload = parseWsPayload(parsedData);
    return {
      ...parsedData,
      Value: parsedPayload,
    };
  } catch {
    return {};
  }
};
