 

export interface ResetPasswordResponseModel {
	modelState?: { [key: string]: any; };
	linkExpiredOrInvalid?: boolean;
	expectedError?: boolean;
	errorMessage?: string;
	email?: string;
}