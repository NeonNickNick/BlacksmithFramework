const heroPanel = document.getElementById('heroPanel');
const usernameInput = document.getElementById('usernameInput');
const passwordInput = document.getElementById('passwordInput');
const registerBtn = document.getElementById('registerBtn');
const loginBtn = document.getElementById('loginBtn');
const queueBtn = document.getElementById('queueBtn');
const cancelQueueBtn = document.getElementById('cancelQueueBtn');
const logoutBtn = document.getElementById('logoutBtn');
const skillInput = document.getElementById('skill');
const declareBtn = document.getElementById('declareBtn');
const prevBtn = document.getElementById('prevBtn');
const nextBtn = document.getElementById('nextBtn');

registerBtn?.addEventListener('click', () => withBusy(async () => {
    const response = await registerAccount(usernameInput?.value || '', passwordInput?.value || '');
    if (!response.ok) {
        throw new Error(response.message || 'Registration failed.');
    }

    persistSession(response.token, response.username);
    State.authenticated = true;
    State.connectionState = 'Connecting';
    State.lastBanner = response.message || 'Registration successful.';
    renderConnectionBits();
    renderHeroCopy();
    renderAuthPanels();
    connectSocket();
}));

loginBtn?.addEventListener('click', () => withBusy(async () => {
    const response = await loginAccount(usernameInput?.value || '', passwordInput?.value || '');
    if (!response.ok) {
        throw new Error(response.message || 'Login failed.');
    }

    persistSession(response.token, response.username);
    State.authenticated = true;
    State.connectionState = 'Connecting';
    State.lastBanner = response.message || 'Login successful.';
    renderConnectionBits();
    renderHeroCopy();
    renderAuthPanels();
    connectSocket();
}));

queueBtn?.addEventListener('click', () => {
    try {
        sendSocketMessage({ type: 'queue' });
    } catch (error) {
        State.lastBanner = error instanceof Error ? error.message : 'Unable to queue.';
        renderHeroCopy();
    }
});

cancelQueueBtn?.addEventListener('click', () => {
    try {
        sendSocketMessage({ type: 'cancelQueue' });
    } catch (error) {
        State.lastBanner = error instanceof Error ? error.message : 'Unable to cancel queue.';
        renderHeroCopy();
    }
});

logoutBtn?.addEventListener('click', () => withBusy(async () => {
    closeSocket({ expected: true });
    await logoutAccount().catch(() => null);
    clearSession();
    renderLoggedOutState();
}));

declareBtn?.addEventListener('click', () => {
    try {
        const skill = parseSkill(skillInput?.value || '');
        sendSocketMessage({
            type: 'submitTurn',
            skillName: skill.name,
            param: skill.param
        });
    } catch (error) {
        State.lastBanner = error instanceof Error ? error.message : 'Unable to submit turn.';
        renderHeroCopy();
    }
});

prevBtn?.addEventListener('click', () => {
    if (State.currentTurn > 0) {
        State.currentTurn -= 1;
        renderTurn();
    }
});

nextBtn?.addEventListener('click', () => {
    if (State.currentTurn < State.turns.length - 1) {
        State.currentTurn += 1;
        renderTurn();
    }
});

heroPanel?.addEventListener('toggle', () => {
    State.heroCollapsed = !heroPanel.open;
    updateHeroVisibility();
});

setInterval(() => {
    updateCountdowns();
}, 250);

(async function init() {
    renderLoggedOutState();

    if (!State.token) {
        return;
    }

    await withBusy(async () => {
        const status = await loadAuthStatus();
        if (!status.ok) {
            clearSession();
            renderLoggedOutState();
            return;
        }

        persistSession(status.token, status.username);
        State.authenticated = true;
        State.connectionState = 'Connecting';
        State.lastBanner = status.message || 'Authenticated.';
        renderConnectionBits();
        renderHeroCopy();
        renderAuthPanels();
        connectSocket();
    });
})();
