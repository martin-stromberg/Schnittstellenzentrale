const STORAGE_KEY = 'colorScheme';

export function getStoredTheme() {
    return localStorage.getItem(STORAGE_KEY);
}

export function setStoredTheme(scheme) {
    localStorage.setItem(STORAGE_KEY, scheme);
}

export function applyTheme(scheme) {
    if (scheme.toLowerCase() === 'dark') {
        document.documentElement.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
    }
}

export function getAndApplyStoredTheme() {
    const stored = getStoredTheme();
    applyTheme(stored ?? 'light');
    return stored;
}
