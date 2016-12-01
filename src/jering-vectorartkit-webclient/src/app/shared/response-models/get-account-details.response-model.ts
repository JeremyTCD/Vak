 

export interface GetAccountDetailsResponseModel {
	durationSinceLastPasswordChange?: string;
	displayName?: string;
	twoFactorEnabled?: boolean;
	email?: string;
	emailVerified?: boolean;
	alternativeEmail?: string;
	alternativeEmailVerified?: boolean;
	modelState?: { [key: string]: any; };
	expectedError?: boolean;
	errorMessage?: string;
}