 

export interface ResetPasswordResponseModel {
	modelState?: { [key: string]: any; };
	invalidToken?: boolean;
	invalidEmail?: boolean;
	expectedError?: boolean;
	errorMessage?: string;
}