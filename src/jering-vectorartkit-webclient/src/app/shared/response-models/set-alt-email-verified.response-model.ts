 

export interface SetAltEmailVerifiedResponseModel {
	modelState?: { [key: string]: any; };
	expectedError?: boolean;
	invalidToken?: boolean;
	invalidAccountId?: boolean;
	errorMessage?: string;
}