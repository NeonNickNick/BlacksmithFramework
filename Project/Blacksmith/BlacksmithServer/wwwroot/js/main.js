function withBusy(task) {
    if (State.busy) return;

    State.busy = true;
    updateBusyState();

    try {
        task();
    } catch (error) {
        const message = error instanceof Error ? error.message : 'Unexpected error';
        State.lastResult = message;
        renderTurn();
        alert(message);
    } finally {
        State.busy = false;
        updateBusyState();
    }
}

WS.on('queued', (msg) => {
    State.inQueue = msg.position >= 0;
    State.queuePosition = msg.position;
    Matchmaking.renderQueueUI();
});

WS.on('matched', (msg) => {
    State.inQueue = false;
    State.roomId = msg.roomId;
    State.playerNumber = msg.playerNumber;
    Matchmaking.renderQueueUI();
});

WS.on('game_start', (msg) => {
    State.snapshot = msg.snapshot;
    State.turns = msg.snapshot?.turns || [];
    State.gameStarted = true;
    State.currentTurn = msg.snapshot?.turns?.length > 0
        ? msg.snapshot.turns.length - 1
        : -1;
    State.lastResult = resultLabel(msg.snapshot?.result);
    State.heroCollapsed = true;
    State.gameOver = false;
    renderSnapshot(State.snapshot, { autoFocusLatest: true });
    updateBusyState();
    Matchmaking.renderQueueUI();
});

WS.on('snapshot', (msg) => {
    State.waitingForOpponent = false;
    stopTurnTimer();
    State.snapshot = msg.snapshot;
    State.turns = msg.snapshot?.turns || [];
    State.lastResult = resultLabel(msg.snapshot?.result);
    renderSnapshot(State.snapshot, { autoFocusLatest: true });
    updateBusyState();
    renderWaitingState();
});

WS.on('turn_timer_start', (msg) => {
    startTurnTimer(msg.secondsRemaining);
});

WS.on('waiting', () => {
    State.waitingForOpponent = true;
    renderWaitingState();
});

WS.on('game_over', (msg) => {
    State.gameOver = true;
    State.gameStarted = false;
    State.inQueue = false;
    stopTurnTimer();
    State.snapshot = msg.snapshot;
    State.turns = msg.snapshot?.turns || [];
    State.lastResult = resultLabel(msg.result);
    renderSnapshot(State.snapshot, { autoFocusLatest: true });
    updateBusyState();
    renderWaitingState();
});

WS.on('opponent_disconnected', (msg) => {
    State.gameOver = true;
    State.gameStarted = false;
    State.inQueue = false;
    stopTurnTimer();
    State.lastResult = 'Victory (opponent left)';
    renderTurn();
    updateBusyState();
    alert(msg.message || 'Opponent disconnected.');
});

WS.on('error', (msg) => {
    console.error('[Server]', msg.message);
    State.lastResult = 'Error';
    const actionText = document.getElementById('actionText');
    const resultBadge = document.getElementById('resultBadge');
    if (actionText) actionText.textContent = msg.message || 'Server error';
    if (resultBadge) resultBadge.textContent = 'Error';
    updateBusyState();
});

WS.on('pong', () => {});

const queueBtn = document.getElementById('queueBtn');
const skillInput = document.getElementById('skill');
const declareBtn = document.getElementById('declareBtn');
const prevBtn = document.getElementById('prevBtn');
const nextBtn = document.getElementById('nextBtn');
const heroPanel = document.getElementById('heroPanel');

queueBtn?.addEventListener('click', () => {
    if (State.inQueue) {
        Matchmaking.leaveQueue();
    } else {
        Matchmaking.joinQueue();
    }
});

declareBtn?.addEventListener('click', () => withBusy(() => {
    const { name, param } = parseSkill(skillInput?.value || '');
    WS.send('declare', { skillName: name, param });
}));

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

WS.connect();
updateBusyState();
Matchmaking.renderQueueUI();
