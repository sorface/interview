export interface ApiContractGet {
  method: 'GET';
  baseUrl: string;
  urlParams?: object;
}

export interface ApiContractPost {
  method: 'POST';
  baseUrl: string;
  urlParams?: object;
  body: any;
}

export interface ApiContractPut {
  method: 'PUT';
  baseUrl: string;
  urlParams?: object;
  body: any;
}

export interface ApiContractPatch {
  method: 'PATCH';
  baseUrl: string;
  urlParams?: object;
  body: any;
}

export type ApiContract = ApiContractGet | ApiContractPost | ApiContractPut | ApiContractPatch;
