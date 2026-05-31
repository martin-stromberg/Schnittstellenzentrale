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

const _GLOBAL_KEY = '__activityLogPanelListeners';

// Remove any document-level listeners registered by a previous module instance
// (e.g. after a Blazor circuit reconnect where the module is re-imported).
if (document[_GLOBAL_KEY]) {
    const prev = document[_GLOBAL_KEY];
    document.removeEventListener('mousemove', prev.onMouseMove);
    document.removeEventListener('mouseup', prev.onMouseUp);
    delete document[_GLOBAL_KEY];
}

const _listenerRegistry = new Map();

export function initializePanelResize(handleElement, panelElement) {
    if (!handleElement || !panelElement) return;

    destroy();

    let isResizing = false;
    let startY = 0;
    let startHeight = 0;

    const onMouseDown = (event) => {
        isResizing = true;
        startY = event.clientY;
        startHeight = panelElement.offsetHeight;
        document.body.style.userSelect = 'none';
        event.preventDefault();
    };

    const onMouseMove = (event) => {
        if (!isResizing) return;
        const delta = startY - event.clientY;
        const newHeight = startHeight + delta;
        if (newHeight >= 80 && newHeight <= 800) {
            panelElement.style.height = newHeight + 'px';
        }
    };

    const onMouseUp = () => {
        if (!isResizing) return;
        isResizing = false;
        document.body.style.userSelect = '';
        const height = parseInt(panelElement.style.height, 10);
        if (!isNaN(height)) {
            savePanelHeight(height);
        }
    };

    handleElement.addEventListener('mousedown', onMouseDown);
    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);

    document[_GLOBAL_KEY] = { onMouseMove, onMouseUp };
    _listenerRegistry.set(handleElement, { onMouseDown, onMouseMove, onMouseUp });
}

export function destroy(handleElement) {
    if (!handleElement) {
        _listenerRegistry.forEach((listeners, element) => {
            element.removeEventListener('mousedown', listeners.onMouseDown);
            document.removeEventListener('mousemove', listeners.onMouseMove);
            document.removeEventListener('mouseup', listeners.onMouseUp);
        });
        _listenerRegistry.clear();
        delete document[_GLOBAL_KEY];
        return;
    }
    const listeners = _listenerRegistry.get(handleElement);
    if (listeners) {
        handleElement.removeEventListener('mousedown', listeners.onMouseDown);
        document.removeEventListener('mousemove', listeners.onMouseMove);
        document.removeEventListener('mouseup', listeners.onMouseUp);
        _listenerRegistry.delete(handleElement);
        if (_listenerRegistry.size === 0)
            delete document[_GLOBAL_KEY];
    }
}
