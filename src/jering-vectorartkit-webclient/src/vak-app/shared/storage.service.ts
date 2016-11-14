import { Injectable } from '@angular/core';

/**
 * Provides storage interface
 */
@Injectable()
export class StorageService {
    setItem(key: string, data: string, isPersistent: boolean): void {
        if (isPersistent) {
            localStorage.setItem(key, data);
        } else {
            sessionStorage.setItem(key, data);
        }
    }

    getItem(key: string): string {
        return localStorage.getItem(key) || sessionStorage.getItem(key);
    }

    removeItem(key: string): void {
        localStorage.removeItem(key);
        sessionStorage.removeItem(key);
    }
}