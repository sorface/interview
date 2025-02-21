const simpleType = (value: unknown) =>
  typeof value !== 'object' && value !== null;
const compareObjects = (incomingObject: unknown, expectedValue: string) =>
  JSON.stringify(incomingObject) === expectedValue.replace(/'/g, '"');
const normalizeObjectString = (objString: string): string => {
  const addQuotesToKeys = (str: string): string => {
    // преобразование ключей объекта {one} => {"one"}
    return str.replace(/([{,]\s*)([a-zA-Z_][a-zA-Z0-9_]*)\s*:/g, '$1"$2":');
  };

  const removeSpaces = (str: string): string => {
    return str.replace(/\s+/g, '');
  };

  return removeSpaces(addQuotesToKeys(objString));
};

const quotes = {
  singleQuote: "'",
  doubleQoute: '"',
} as const;

const normalizeStringOrNumber = (value: string) => {
  const isExpectedValueString =
    value.at(0) === quotes.singleQuote || value.at(0) === quotes.doubleQoute;

  if (isExpectedValueString) return value.slice(1, -1);

  return parseInt(value);
};

export const compare = (incomingValue: unknown, expectedValue: string) => {
  if (incomingValue === null) {
    console.log(incomingValue, expectedValue === 'null');

    return Object.is(incomingValue, expectedValue);
  }

  if (simpleType(incomingValue)) {
    return Object.is(incomingValue, normalizeStringOrNumber(expectedValue));
  }

  if (!simpleType(incomingValue) && Array.isArray(expectedValue)) {
    return compareObjects(incomingValue, expectedValue);
  }

  if (!simpleType(incomingValue) && !Array.isArray(expectedValue)) {
    return compareObjects(incomingValue, normalizeObjectString(expectedValue));
  }

  return false;
};
