 

export interface LogInResponseModel {
	twoFactorRequired?: boolean;
	username?: string;
	isPersistent?: boolean;
	modelState?: { [key: string]: any; };
	expectedError?: boolean;
	errorMessage?: string;
}