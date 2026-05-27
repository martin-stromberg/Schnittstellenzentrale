const DISPLAY_MODE_KEY = 'activityLogDisplayMode';
const PANEL_HEIGHT_KEY = 'activityLogPanelHeight';

export function saveDisplayMode(mode) {
    localStorage.setItem(DISPLAY_MODE_KEY, mode);
}

export function loadDisplayMode() {
    return localStorage.getItem(DISPLAY_MODE_KEY);
}

export function savePanelHeight(height) {
    localStorage.setItem(PANEL_HEIGHT_KEY, height);
}

export function loadPanelHeight() {
    return localStorage.getItem(PANEL_HEIGHT_KEY);
}

export function initializePanelResize(handleElement, panelElement) {
    if (!handleElement || !panelElement) return;
    let isResizing = false;
    let startY = 0;
    let startHeight = 0;

    handleElement.addEventListener('mousedown', (event) => {
        isResizing = true;
        startY = event.clientY;
        startHeight = panelElement.offsetHeight;
        document.body.style.userSelect = 'none';
        event.preventDefault();
    });

    document.addEventListener('mousemove', (event) => {
        if (!isResizing) return;
        const delta = startY - event.clientY;
        const newHeight = startHeight + delta;
        if (newHeight >= 80 && newHeight <= 800) {
            panelElement.style.height = newHeight + 'px';
        }
    });

    document.addEventListener('mouseup', () => {
        if (!isResizing) return;
        isResizing = false;
        document.body.style.userSelect = '';
        const height = parseInt(panelElement.style.height, 10);
        if (!isNaN(height)) {
            savePanelHeight(height);
        }
    });
}
