import { DynamicControlData } from '../dynamic-control/dynamic-control-data';

export interface DynamicFormData {
    dynamicControlDatas: DynamicControlData[];
    errorMessage: string;
}