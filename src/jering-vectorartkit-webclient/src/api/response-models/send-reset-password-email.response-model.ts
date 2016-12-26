 

export interface SendResetPasswordEmailResponseModel {
	modelState?: { [key: string]: any; };
	expectedError?: boolean;
	errorMessage?: string;
	invalidEmail?: boolean;
}