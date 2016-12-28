 

export interface ValidateValueResponseModel {
	valid?: boolean;
	modelState?: { [key: string]: any; };
	expectedError?: boolean;
	errorMessage?: string;
}