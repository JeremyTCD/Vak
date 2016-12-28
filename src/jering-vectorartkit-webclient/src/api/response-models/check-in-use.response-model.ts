 

export interface CheckInUseResponseModel {
	inUse?: boolean;
	modelState?: { [key: string]: any; };
	expectedError?: boolean;
	errorMessage?: string;
}