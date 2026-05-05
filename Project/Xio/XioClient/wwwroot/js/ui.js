function safeText(value, fallback = '--') {
    return value === null || value === undefined || value === '' ? fallback : String(value);
}

const RankNames = ['Default', 'Golden', 'Diamond', 'BlackHole', 'WanXiang'];
const CycleLength = 22;

function rankLabel(rank) {
    return RankNames[rank] ?? `Rank ${rank}`;
}

function setRankBar(elementId, innerLevel) {
    const element = document.getElementById(elementId);
    if (!element) return;
    const ratio = CycleLength > 0 ? Math.max(0, Math.min(100, (innerLevel / CycleLength) * 100)) : 0;
    element.style.width = `${ratio}%`;
}

function renderTokenGrid(elementId, items, renderer, emptyText) {
    const element = document.getElementById(elementId);
    if (!element) return;

    if (!items || items.length === 0) {
        element.className = 'token-grid empty-state-box';
        element.textContent = emptyText;
        return;
    }

    element.className = 'token-grid';
    element.innerHTML = items.map(renderer).join('');
}

function renderActor(prefix, actor) {
    const rankEl = document.getElementById(prefix === 'player' ? 'pRank' : 'eRank');
    const name = document.getElementById(prefix === 'player' ? 'playerName' : 'enemyName');
    const rankDetail = document.getElementById(prefix === 'player' ? 'playerRankDetail' : 'enemyRankDetail');

    if (!actor) {
        if (name) name.textContent = prefix === 'player' ? 'You' : 'Opponent';
        if (rankEl) rankEl.textContent = 'Rank --';
        if (rankDetail) rankDetail.textContent = 'Inner Level -- / --';
        setRankBar(prefix === 'player' ? 'playerRankFill' : 'enemyRankFill', 0);
        renderTokenGrid(`${prefix}Resources`, [], () => '', 'No data yet.');
        renderTokenGrid(`${prefix}Skills`, [], () => '', 'No data yet.');
        return;
    }

    if (name) name.textContent = prefix === 'player' ? 'You' : 'Opponent';
    if (rankEl) rankEl.textContent = `${rankLabel(actor.rank)}`;
    if (rankDetail) rankDetail.textContent = `Inner Level ${actor.innerLevel} / ${CycleLength}`;
    setRankBar(prefix === 'player' ? 'playerRankFill' : 'enemyRankFill', actor.innerLevel);

    renderTokenGrid(
        `${prefix}Resources`,
        actor.resources || [],
        item => `<div class="token"><strong>${item.name}</strong><div>${item.quantity}</div></div>`,
        'No data yet.'
    );

    renderTokenGrid(
        `${prefix}Skills`,
        actor.availableSkills || [],
        item => `<div class="token skill-${item.usable ? 'available' : 'locked'}"><strong>${item.name}</strong></div>`,
        'No data yet.'
    );
}

function buildTurnSummary(turn) {
    if (!turn) {
        return '<div class="summary-card"><h3>Turn Overview</h3><div class="summary-line">Start a game to see structured turn details.</div></div>';
    }

    return `
        <div class="summary-card">
            <h3>Cultivator Action</h3>
            <div class="summary-line">Skill: ${safeText(turn.playerSkill)}</div>
            <div class="summary-line">Rank: ${rankLabel(turn.playerRank)}</div>
        </div>
        <div class="summary-card">
            <h3>Opponent Action</h3>
            <div class="summary-line">Skill: ${safeText(turn.enemySkill)}</div>
            <div class="summary-line">Rank: ${rankLabel(turn.enemyRank)}</div>
        </div>
    `;
}

function renderHistory() {
    const historyList = document.getElementById('historyList');
    if (!historyList) return;

    if (!State.turns.length) {
        historyList.innerHTML = '<div class="history-empty">No turns yet. Once the battle starts, each round will be archived here.</div>';
        return;
    }

    historyList.innerHTML = State.turns.map((turn, index) => `
        <button class="history-item ${index === State.currentTurn ? 'active' : ''}" data-turn-index="${index}" type="button">
            <div class="history-title">
                <span>Turn ${turn.index}</span>
                <span>${turn.result}</span>
            </div>
            <div>You: ${turn.playerSkill} (${rankLabel(turn.playerRank)}) | Enemy: ${turn.enemySkill} (${rankLabel(turn.enemyRank)})</div>
        </button>
    `).join('');

    historyList.querySelectorAll('[data-turn-index]').forEach(button => {
        button.addEventListener('click', () => {
            State.currentTurn = Number(button.getAttribute('data-turn-index'));
            renderTurn();
        });
    });
}

function renderTurn() {
    const turnIndex = document.getElementById('turnIndex');
    const roundPill = document.getElementById('roundPill');
    const actionText = document.getElementById('actionText');
    const turnSummary = document.getElementById('turnSummary');
    const resultBadge = document.getElementById('resultBadge');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');

    const turn = State.currentTurn >= 0 && State.currentTurn < State.turns.length
        ? State.turns[State.currentTurn]
        : null;

    if (turnIndex) turnIndex.textContent = turn ? `Turn ${turn.index} / ${State.turns.length}` : `Turn 0 / ${State.turns.length}`;
    if (roundPill) {
        roundPill.textContent = State.snapshot
            ? `母${State.snapshot.round} · 子${State.snapshot.innerRound}`
            : 'Round 0';
    }
    if (actionText) {
        actionText.textContent = turn
            ? `You used ${turn.playerSkill} (${rankLabel(turn.playerRank)}). Enemy used ${turn.enemySkill} (${rankLabel(turn.enemyRank)}).`
            : (State.gameStarted ? 'Realm initialized. Declare the first technique.' : 'No actions recorded yet.');
    }
    if (turnSummary) turnSummary.innerHTML = buildTurnSummary(turn);
    if (resultBadge) resultBadge.textContent = State.lastResult;
    if (prevBtn) prevBtn.disabled = State.currentTurn <= 0;
    if (nextBtn) nextBtn.disabled = State.currentTurn < 0 || State.currentTurn >= State.turns.length - 1;

    renderHistory();
}

function updateHeroVisibility() {
    const heroPanel = document.getElementById('heroPanel');
    if (!heroPanel) return;
    heroPanel.open = !State.heroCollapsed;
}

function renderSnapshot(snapshot, options = {}) {
    const autoFocusLatest = Boolean(options.autoFocusLatest);
    State.snapshot = snapshot;
    State.turns = snapshot?.turns || [];
    State.gameStarted = Boolean(snapshot?.started);
    State.selectedModeName = safeText(snapshot?.modeName, 'Not started');
    State.lastResult = 'In progress';
    State.heroCollapsed = State.gameStarted;

    if (State.turns.length === 0) {
        State.currentTurn = -1;
    } else if (autoFocusLatest || State.currentTurn < 0 || State.currentTurn >= State.turns.length) {
        State.currentTurn = State.turns.length - 1;
    }

    const modeBadge = document.getElementById('modeBadge');
    if (modeBadge) modeBadge.textContent = State.selectedModeName;

    renderActor('player', snapshot?.player || null);
    renderActor('enemy', snapshot?.enemy || null);
    renderTurn();
    updateEnemyInputVisibility();
    updateHeroVisibility();
}

function updateEnemyInputVisibility() {
    const modeHint = document.getElementById('modeHint');
    const actionHint = document.getElementById('actionHint');

    if (modeHint) {
        modeHint.textContent = 'Manual mode lets you enter both sides of the turn.';
    }
    if (actionHint) {
        actionHint.textContent = State.gameStarted
            ? 'Enter technique names. Use rank prefix or type without prefix for rank 0.'
            : 'Start a game to enable technique declaration.';
    }
}

function updateBusyState() {
    const startBtn = document.getElementById('startBtn');
    const restartBtn = document.getElementById('restartBtn');
    const declareBtn = document.getElementById('declareBtn');
    const connectionState = document.getElementById('connectionState');

    if (startBtn) startBtn.disabled = State.busy;
    if (restartBtn) restartBtn.disabled = State.busy || !State.gameStarted;
    if (declareBtn) declareBtn.disabled = State.busy || !State.gameStarted;
    if (connectionState) connectionState.textContent = State.busy ? 'Working' : 'Ready';
}
