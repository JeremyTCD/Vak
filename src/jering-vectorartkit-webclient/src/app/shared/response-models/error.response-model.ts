 

export interface ErrorResponseModel {
	expectedError?: boolean;
	modelState?: { [key: string]: any; };
	errorMessage?: string;
}