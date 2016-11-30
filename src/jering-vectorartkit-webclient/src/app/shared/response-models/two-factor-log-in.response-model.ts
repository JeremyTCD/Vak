 

export interface TwoFactorLogInResponseModel {
    username?: string;
    isPersistent?: boolean;
    tokenExpired?: boolean;
    modelState?: { [key: string]: any; };
    expectedError?: boolean;
    errorMessage?: string;
}