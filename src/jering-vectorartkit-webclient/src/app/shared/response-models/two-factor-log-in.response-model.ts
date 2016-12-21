 

export interface TwoFactorLogInResponseModel {
	username?: string;
	isPersistent?: boolean;
	expiredCredentials?: boolean;
	modelState?: { [key: string]: any; };
	expectedError?: boolean;
	errorMessage?: string;
}