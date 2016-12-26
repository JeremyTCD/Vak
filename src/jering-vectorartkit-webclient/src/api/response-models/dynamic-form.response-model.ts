import {DynamicControlResponseModel} from './dynamic-control.response-model'; 

export interface DynamicFormResponseModel {
	errorMessage?: string;
	buttonText?: string;
	dynamicControlResponseModels?: DynamicControlResponseModel[];
}