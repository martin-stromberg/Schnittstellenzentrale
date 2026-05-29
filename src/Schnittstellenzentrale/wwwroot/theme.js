const STORAGE_KEY = 'colorScheme';

export function getStoredTheme() {
    return localStorage.getItem(STORAGE_KEY);
}

export function setStoredTheme(scheme) {
    localStorage.setItem(STORAGE_KEY, scheme);
}

export function applyTheme(scheme) {
    document.documentElement.setAttribute('data-theme', scheme.toLowerCase());
}

export function getAndApplyStoredTheme() {
    const stored = getStoredTheme();
    applyTheme(stored ?? 'light');
    return stored;
}
