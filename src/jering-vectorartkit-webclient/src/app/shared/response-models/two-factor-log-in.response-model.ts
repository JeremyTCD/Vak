 

export interface TwoFactorLogInResponseModel {
	username?: string;
	isPersistent?: boolean;
	expiredToken?: boolean;
	expiredCredentials?: boolean;
	modelState?: { [key: string]: any; };
	expectedError?: boolean;
	errorMessage?: string;
}