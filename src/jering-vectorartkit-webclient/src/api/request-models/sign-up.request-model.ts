

export interface SignUpRequestModel {
	email?: string;
	password?: string;
	confirmPassword?: string;
}

export const signUpRequestModelName: string = `SignUpRequestModel`;
