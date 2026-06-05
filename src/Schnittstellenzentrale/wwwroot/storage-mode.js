const STORAGE_KEY = 'storageMode';

export function getStoredMode() {
    return localStorage.getItem(STORAGE_KEY);
}

export function setStoredMode(value) {
    localStorage.setItem(STORAGE_KEY, value);
}
