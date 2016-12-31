


    export interface LogInResponseModel {
	    authenticationError?: boolean;
	    antiForgeryError?: boolean;
	    twoFactorRequired?: boolean;
	    modelState?: { [key: string]: any; };
	    expectedError?: boolean;
	    errorMessage?: string;
    }

    

        

