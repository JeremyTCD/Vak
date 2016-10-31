import { DynamicControlValidatorData } from './dynamic-control-validator-data';

export interface DynamicControlData {
    name?: string;
    displayName?: string;
    order?: number;
    tagName?: string;
    validatorData?: DynamicControlValidatorData[];
    asyncValidatorData?: DynamicControlValidatorData;
    properties?: { [key: string]: string };
}