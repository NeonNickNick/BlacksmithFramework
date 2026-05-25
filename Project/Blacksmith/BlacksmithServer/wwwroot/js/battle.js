const heroPanel = document.getElementById('heroPanel');
const queueBtn = document.getElementById('queueBtn');
const cancelQueueBtn = document.getElementById('cancelQueueBtn');
const logoutBtn = document.getElementById('logoutBtn');
const skillInput = document.getElementById('skill');
const declareBtn = document.getElementById('declareBtn');
const prevBtn = document.getElementById('prevBtn');
const nextBtn = document.getElementById('nextBtn');

function goToLogin() {
    window.location.href = '/';
}

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

logoutBtn?.addEventListener('click', () => {
    void withBusy(async () => {
        closeSocket({ expected: true });
        await logoutAccount().catch(() => null);
        clearSession();
        goToLogin();
    });
});

function submitTurn() {
    withBusy(async () => {
        const skill = parseSkill(skillInput?.value || '');
        sendSocketMessage({
            type: 'submitTurn',
            skillName: skill.name,
            param: skill.param,
            stringParam: skill.stringParam
        });
    });
}

declareBtn?.addEventListener('click', () => submitTurn());

skillInput?.addEventListener('keydown', (event) => {
    if (event.key === 'Enter') {
        event.preventDefault();
        submitTurn();
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
        goToLogin();
        return;
    }

    await withBusy(async () => {
        const status = await loadAuthStatus();
        if (!status.ok) {
            clearSession();
            goToLogin();
            return;
        }

        persistSession(status.token, status.username);
        State.authenticated = true;
        State.connectionState = 'Connecting';
        State.lastBanner = status.message || 'Authenticated.';
        renderConnectionBits();
        renderHeroCopy();
        connectSocket();
    });
})();
