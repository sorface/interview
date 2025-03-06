import { AnyObject } from './anyObject';

export interface ApiContractGet {
  method: 'GET';
  baseUrl: string;
  urlParams?: AnyObject;
}

export interface ApiContractPost {
  method: 'POST';
  baseUrl: string;
  urlParams?: object;
  body?: AnyObject;
}

export interface ApiContractPut {
  method: 'PUT';
  baseUrl: string;
  urlParams?: object;
  body: AnyObject;
}

export interface ApiContractPatch {
  method: 'PATCH';
  baseUrl: string;
  urlParams?: object;
  body: AnyObject;
}

export type ApiContract =
  | ApiContractGet
  | ApiContractPost
  | ApiContractPut
  | ApiContractPatch;
