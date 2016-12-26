import {ValidatorResponseModel} from './validator.response-model'; 

export interface DynamicControlResponseModel {
	name?: string;
	tagName?: string;
	order?: number;
	displayName?: string;
	properties?: { [key: string]: string; };
	validatorResponseModels?: ValidatorResponseModel[];
	asyncValidatorResponseModel?: ValidatorResponseModel;
}