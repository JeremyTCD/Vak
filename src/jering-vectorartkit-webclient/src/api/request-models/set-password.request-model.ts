

export interface SetPasswordRequestModel {
	currentPassword?: string;
	newPassword?: string;
	confirmNewPassword?: string;
}

export const setPasswordRequestModelName: string = `SetPasswordRequestModel`;
