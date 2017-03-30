


    export interface GetDynamicFormResponseModel {
	    dynamicFormData?: DynamicFormData;
    }

    
        export interface DynamicFormData { 
            errorMessage?: string;
            buttonText?: string;
            dynamicControlData?: DynamicControlData[];
        }
        export interface DynamicControlData { 
            name?: string;
            tagName?: string;
            order?: number;
            displayName?: string;
            properties?: { [key: string]: string; };
            validatorData?: ValidatorData[];
            asyncValidatorData?: ValidatorData;
        }
        export interface ValidatorData { 
            name?: string;
            errorMessage?: string;
            options?: { [key: string]: string; };
        }

        

