

export interface ResetPasswordRequestModel {
	email?: string;
	token?: string;
	newPassword?: string;
	confirmPassword?: string;
}

export const resetPasswordRequestModelName: string = `ResetPasswordRequestModel`;
