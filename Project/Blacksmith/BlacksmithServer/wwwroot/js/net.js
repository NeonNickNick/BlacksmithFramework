async function readJson(response) {
    const data = await response.json().catch(() => null);
    if (!response.ok) {
        throw new Error(data?.message || `Request failed: ${response.status}`);
    }
    return data;
}

function authHeaders() {
    return State.token
        ? { Authorization: `Bearer ${State.token}` }
        : {};
}

async function registerAccount(username, password) {
    return await readJson(await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
    }));
}

async function loginAccount(username, password) {
    return await readJson(await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
    }));
}

async function logoutAccount() {
    return await readJson(await fetch('/api/auth/logout', {
        method: 'POST',
        headers: authHeaders()
    }));
}

async function loadAuthStatus() {
    return await readJson(await fetch('/api/auth/status', {
        headers: authHeaders()
    }));
}

function persistSession(token, username) {
    State.token = token || '';
    State.username = username || '';
    localStorage.setItem(TOKEN_KEY, State.token);
    localStorage.setItem(USERNAME_KEY, State.username);
}

function clearSession() {
    State.token = '';
    State.username = '';
    State.authenticated = false;
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USERNAME_KEY);
}

function closeSocket(options = {}) {
    const { expected = false } = options;

    if (State.reconnectTimer) {
        clearTimeout(State.reconnectTimer);
        State.reconnectTimer = null;
    }

    if (!State.socket) return;

    State.socketCloseExpected = expected;
    try {
        State.socket.close();
    } catch {
    }
    State.socket = null;
}

function scheduleReconnect() {
    if (!State.authenticated || !State.token || State.reconnectTimer) {
        return;
    }

    State.reconnectTimer = setTimeout(() => {
        State.reconnectTimer = null;
        connectSocket();
    }, 1500);
}

function connectSocket() {
    if (!State.token) return;
    if (State.socket && (State.socket.readyState === WebSocket.OPEN || State.socket.readyState === WebSocket.CONNECTING)) {
        return;
    }

    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    const socket = new WebSocket(`${protocol}//${window.location.host}/ws?token=${encodeURIComponent(State.token)}`);

    State.socketCloseExpected = false;
    State.socket = socket;
    State.connectionState = 'Connecting';
    updateBusyState();
    renderConnectionBits();

    socket.addEventListener('open', () => {
        State.connectionState = 'Connected';
        renderConnectionBits();
        updateBusyState();
        sendSocketMessage({ type: 'requestSnapshot' });
    });

    socket.addEventListener('message', event => {
        let data = null;
        try {
            data = JSON.parse(event.data);
        } catch {
            return;
        }

        if (data.type === 'snapshot' && data.snapshot) {
            renderSnapshot(data.snapshot, { message: data.message, autoFocusLatest: true });
            if (data.message && (data.message.includes('Illegal.') || data.message.includes('Rejected.'))) {
                alert(data.message);
            }
            return;
        }

        if (data.message) {
            State.lastBanner = data.message;
            renderHeroCopy();
        }
    });

    socket.addEventListener('close', () => {
        const expected = State.socketCloseExpected;
        State.socket = null;
        State.socketCloseExpected = false;
        State.connectionState = expected ? 'Disconnected' : 'Reconnecting';
        renderConnectionBits();
        updateBusyState();

        if (!expected) {
            State.lastBanner = 'Socket disconnected. Reconnecting...';
            renderHeroCopy();
            scheduleReconnect();
        }
    });
}

function sendSocketMessage(payload) {
    if (!State.socket || State.socket.readyState !== WebSocket.OPEN) {
        throw new Error('WebSocket is not connected.');
    }

    State.socket.send(JSON.stringify(payload));
}
