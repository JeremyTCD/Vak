


    export interface ResetPasswordResponseModel {
	    authenticationError?: boolean;
	    antiForgeryError?: boolean;
	    modelState?: { [key: string]: any; };
	    invalidToken?: boolean;
	    invalidEmail?: boolean;
	    expectedError?: boolean;
	    errorMessage?: string;
    }

    

        

