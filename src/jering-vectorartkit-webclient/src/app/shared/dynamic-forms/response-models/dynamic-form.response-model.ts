import { DynamicControlResponseModel } from './dynamic-control.response-model'; 

export interface DynamicFormResponseModel {
	errorMessage?: string;
	dynamicControlResponseModels?: DynamicControlResponseModel[];
}