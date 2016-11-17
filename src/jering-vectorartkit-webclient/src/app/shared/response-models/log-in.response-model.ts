 

export interface LogInResponseModel {
	twoFactorRequired?: boolean;
	username?: string;
	isPersistent?: boolean;
}