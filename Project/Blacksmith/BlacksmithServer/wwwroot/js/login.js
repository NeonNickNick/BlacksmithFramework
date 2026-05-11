const usernameInput = document.getElementById('usernameInput');
const passwordInput = document.getElementById('passwordInput');
const registerBtn = document.getElementById('registerBtn');
const loginBtn = document.getElementById('loginBtn');
const connectionState = document.getElementById('connectionState');
const authStatusText = document.getElementById('authStatusText');
const timeoutRuleDisplay = document.getElementById('timeoutRuleDisplay');

if (timeoutRuleDisplay) {
    timeoutRuleDisplay.textContent = `${ROUND_TIMEOUT_SEC}s, ${TIMEOUT_LOSS_THRESHOLD} strikes`;
}

function withLoginBusy(task) {
    if (State.busy) return Promise.resolve();

    State.busy = true;
    updateLoginState();

    return Promise.resolve()
        .then(task)
        .catch(error => {
            const message = error instanceof Error ? error.message : 'Unexpected error';
            State.lastBanner = message;
            updateLoginState(message);
            throw error;
        })
        .finally(() => {
            State.busy = false;
            updateLoginState();
        });
}

function updateLoginState(message = '') {
    if (connectionState) {
        connectionState.textContent = State.busy ? 'Working' : 'Disconnected';
    }

    if (usernameInput) usernameInput.disabled = State.busy;
    if (passwordInput) passwordInput.disabled = State.busy;
    if (registerBtn) registerBtn.disabled = State.busy;
    if (loginBtn) loginBtn.disabled = State.busy;
    if (authStatusText) authStatusText.textContent = message || State.lastBanner || 'Register or log in to connect to the arena.';
}

function goToBattle() {
    window.location.href = '/battle.html';
}

function handleAuthSuccess(response, fallbackMessage) {
    persistSession(response.token, response.username);
    State.authenticated = true;
    State.username = response.username || '';
    State.lastBanner = response.message || fallbackMessage;
    updateLoginState(State.lastBanner);
    goToBattle();
}

registerBtn?.addEventListener('click', () => {
    void withLoginBusy(async () => {
        const response = await registerAccount(usernameInput?.value || '', passwordInput?.value || '');
        if (!response.ok) {
            throw new Error(response.message || 'Registration failed.');
        }

        handleAuthSuccess(response, 'Registration successful.');
    });
});

loginBtn?.addEventListener('click', () => {
    void withLoginBusy(async () => {
        const response = await loginAccount(usernameInput?.value || '', passwordInput?.value || '');
        if (!response.ok) {
            throw new Error(response.message || 'Login failed.');
        }

        handleAuthSuccess(response, 'Login successful.');
    });
});

passwordInput?.addEventListener('keydown', event => {
    if (event.key === 'Enter' && !State.busy) {
        loginBtn?.click();
    }
});

(async function init() {
    updateLoginState();

    if (!State.token) {
        return;
    }

    await withLoginBusy(async () => {
        const status = await loadAuthStatus();
        if (!status.ok) {
            clearSession();
            State.lastBanner = 'Previous session expired. Please log in again.';
            updateLoginState(State.lastBanner);
            return;
        }

        persistSession(status.token, status.username);
        State.authenticated = true;
        State.username = status.username || '';
        State.lastBanner = status.message || 'Authenticated.';
        goToBattle();
    });
})();
