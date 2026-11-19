// Theme handling for the static Schnittstellenzentrale presentation page.
// Mirrors the app's theme mechanics: the "dark" class on <html>, persisted
// under the localStorage key "colorScheme" (values "Light"/"Dark").
// Unlike the app, an absent stored value falls back to prefers-color-scheme,
// because page visitors have no app-side default.

const STORAGE_KEY = 'colorScheme';

function getStoredScheme() {
    return localStorage.getItem(STORAGE_KEY);
}

function getInitialScheme() {
    const stored = getStoredScheme();
    if (stored === 'Light' || stored === 'Dark') {
        return stored;
    }
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'Dark' : 'Light';
}

function applyScheme(scheme) {
    document.documentElement.classList.toggle('dark', scheme === 'Dark');
    updateToggleLabel(scheme);
}

function updateToggleLabel(scheme) {
    const toggle = document.getElementById('theme-toggle');
    if (!toggle) {
        return;
    }
    const isDark = scheme === 'Dark';
    toggle.setAttribute('aria-pressed', String(isDark));
    toggle.setAttribute('title', isDark ? 'Zum hellen Modus wechseln' : 'Zum dunklen Modus wechseln');
    toggle.textContent = isDark ? '☀' : '☾';
}

function toggleScheme() {
    const next = document.documentElement.classList.contains('dark') ? 'Light' : 'Dark';
    localStorage.setItem(STORAGE_KEY, next);
    applyScheme(next);
}

document.addEventListener('DOMContentLoaded', () => {
    applyScheme(getInitialScheme());
    const toggle = document.getElementById('theme-toggle');
    if (toggle) {
        toggle.addEventListener('click', toggleScheme);
    }
});
