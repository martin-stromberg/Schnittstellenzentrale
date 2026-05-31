const SIDEBAR_STORAGE_KEY = 'sidebarWidth';

export function clickElement(id) {
    document.getElementById(id)?.click();
}

let keydownHandler = null;
let beforeUnloadHandler = null;

export function registerSaveShortcut(dotnetHelper) {
    unregisterSaveShortcut();
    keydownHandler = (event) => {
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 's') {
            event.preventDefault();
            dotnetHelper.invokeMethodAsync('OnSaveShortcut');
        }
    };
    document.addEventListener('keydown', keydownHandler);
}

export function unregisterSaveShortcut() {
    if (keydownHandler) {
        document.removeEventListener('keydown', keydownHandler);
        keydownHandler = null;
    }
}

export function enableBeforeUnloadGuard() {
    if (beforeUnloadHandler) return;
    beforeUnloadHandler = (event) => {
        event.preventDefault();
        event.returnValue = '';
        return '';
    };
    window.addEventListener('beforeunload', beforeUnloadHandler);
}

export function disableBeforeUnloadGuard() {
    if (beforeUnloadHandler) {
        window.removeEventListener('beforeunload', beforeUnloadHandler);
        beforeUnloadHandler = null;
    }
}

export function getStoredSidebarWidth() {
    return localStorage.getItem(SIDEBAR_STORAGE_KEY);
}

export function setStoredSidebarWidth(value) {
    localStorage.setItem(SIDEBAR_STORAGE_KEY, value);
}

export function initializeSidebarResize(handleElement, sidebarElement) {
    if (!handleElement || !sidebarElement) return;
    let isResizing = false;

    handleElement.addEventListener('mousedown', (event) => {
        isResizing = true;
        document.body.style.userSelect = 'none';
        event.preventDefault();
    });

    document.addEventListener('pointermove', (event) => {
        if (!isResizing) return;
        const rect = sidebarElement.getBoundingClientRect();
        const newWidth = event.clientX - rect.left;
        if (newWidth >= 150 && newWidth <= 800) {
            sidebarElement.style.setProperty('--sidebar-width', newWidth + 'px');
            sidebarElement.style.width = newWidth + 'px';
            if (sidebarElement.parentElement)
                sidebarElement.parentElement.style.width = newWidth + 'px';
        }
    });

    document.addEventListener('mouseup', () => {
        if (!isResizing) return;
        isResizing = false;
        document.body.style.userSelect = '';
        const width = sidebarElement.parentElement?.style.width || sidebarElement.style.width;
        if (width) {
            setStoredSidebarWidth(parseInt(width, 10));
        }
    });
}

export function applyStoredSidebarWidth(sidebarElement) {
    if (!sidebarElement) return;
    const stored = getStoredSidebarWidth();
    if (stored) {
        sidebarElement.style.setProperty('--sidebar-width', stored + 'px');
        sidebarElement.style.width = stored + 'px';
        if (sidebarElement.parentElement)
            sidebarElement.parentElement.style.width = stored + 'px';
    }
}
