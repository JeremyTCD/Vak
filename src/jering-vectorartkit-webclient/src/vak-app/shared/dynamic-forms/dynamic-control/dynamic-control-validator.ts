﻿import { DynamicControl } from './dynamic-control';

/**
 * Function that validates DynamicControls.
 *
 * Returns
 * - Error message if DynamicControl value is not valid
 * - null if DynamicControl value is valid
 */
export interface DynamicControlValidator { (dynamicControl: DynamicControl<any>): string; }


