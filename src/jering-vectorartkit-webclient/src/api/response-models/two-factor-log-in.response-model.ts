 

export interface TwoFactorLogInResponseModel {
	expiredCredentials?: boolean;
	modelState?: { [key: string]: any; };
	expectedError?: boolean;
	errorMessage?: string;
}