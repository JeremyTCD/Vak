

export interface TwoFactorLogInRequestModel {
	code?: string;
	isPersistent?: string;
}

export const twoFactorLogInRequestModelName: string = `TwoFactorLogInRequestModel`;
