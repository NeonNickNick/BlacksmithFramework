function safeText(value, fallback = '--') {
    return value === null || value === undefined || value === '' ? fallback : String(value);
}

function setHealthBar(elementId, current, max) {
    const element = document.getElementById(elementId);
    if (!element) return;
    const ratio = max > 0 ? Math.max(0, Math.min(100, (current / max) * 100)) : 0;
    element.style.width = `${ratio}%`;
}

function resultLabel(result) {
    switch (result) {
        case 'win': return 'Victory';
        case 'lose': return 'Defeat';
        case 'draw': return 'Draw';
        case 'next': return 'In progress';
        default: return 'Awaiting game';
    }
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
    const hp = document.getElementById(prefix === 'player' ? 'pHP' : 'eHP');
    const name = document.getElementById(prefix === 'player' ? 'playerName' : 'enemyName');
    const profession = document.getElementById(prefix === 'player' ? 'playerProfession' : 'enemyProfession');
    const bodyNameEl = document.getElementById(prefix === 'player' ? 'playerBodyName' : 'enemyBodyName');
    const summonNav = document.getElementById(prefix === 'player' ? 'playerSummonNav' : 'enemySummonNav');
    const summonLabel = document.getElementById(prefix === 'player' ? 'playerSummonLabel' : 'enemySummonLabel');

    const summonIndex = prefix === 'player' ? State.playerSummonIndex : State.enemySummonIndex;
    const summons = Array.isArray(actor?.summons) ? actor.summons : [];

    var effectiveActor = actor;
    if (actor && summonIndex >= 0 && summonIndex < summons.length) {
        effectiveActor = summons[summonIndex];
    }

    if (summonNav) {
        summonNav.classList.toggle('is-hidden', !actor || summons.length === 0);
    }

    if (!effectiveActor) {
        if (name) name.textContent = prefix === 'player' ? 'You' : 'Opponent';
        if (profession) profession.textContent = 'None';
        if (bodyNameEl) bodyNameEl.textContent = '--';
        if (hp) hp.textContent = 'HP --/--';
        setHealthBar(prefix === 'player' ? 'playerHealthFill' : 'enemyHealthFill', 0, 1);
        renderTokenGrid(`${prefix}Resources`, [], () => '', 'No data yet.');
        renderTokenGrid(`${prefix}Defenses`, [], () => '', 'No active defenses.');
        renderTokenGrid(`${prefix}Skills`, [], () => '', 'No data yet.');
        renderTokenGrid(`${prefix}FutureAttacks`, [], () => '', 'No pending attacks.');
        renderTokenGrid(`${prefix}FutureDefenses`, [], () => '', 'No pending defenses.');
        return;
    }

    if (name) name.textContent = prefix === 'player' ? 'You' : 'Opponent';
    if (bodyNameEl) bodyNameEl.textContent = effectiveActor.bodyName || '--';
    if (profession) {
        const profs = Array.isArray(effectiveActor.professions) ? effectiveActor.professions : [];
        profession.textContent = profs.length > 0 ? profs.join(', ') : 'None';
    }
    if (hp) hp.textContent = `HP ${effectiveActor.hp}/${effectiveActor.maxHP}`;
    setHealthBar(prefix === 'player' ? 'playerHealthFill' : 'enemyHealthFill', effectiveActor.hp, effectiveActor.maxHP);

    if (summonLabel && summons.length > 0) {
        if (summonIndex < 0) {
            summonLabel.textContent = 'Main Body';
        } else {
            summonLabel.textContent = effectiveActor.bodyName || 'Summon ' + (summonIndex + 1);
        }
    }

    var prevBtn = document.getElementById(prefix === 'player' ? 'playerSummonPrev' : 'enemySummonPrev');
    var nextBtn = document.getElementById(prefix === 'player' ? 'playerSummonNext' : 'enemySummonNext');
    if (prevBtn) {
        prevBtn.disabled = summonIndex < 0;
        prevBtn.onclick = function () {
            if (prefix === 'player') {
                State.playerSummonIndex = Math.max(-1, State.playerSummonIndex - 1);
            } else {
                State.enemySummonIndex = Math.max(-1, State.enemySummonIndex - 1);
            }
            renderSnapshot(State.snapshot);
        };
    }
    if (nextBtn) {
        nextBtn.disabled = summons.length === 0 || summonIndex >= summons.length - 1;
        nextBtn.onclick = function () {
            if (prefix === 'player') {
                State.playerSummonIndex = Math.min(summons.length - 1, State.playerSummonIndex + 1);
            } else {
                State.enemySummonIndex = Math.min(summons.length - 1, State.enemySummonIndex + 1);
            }
            renderSnapshot(State.snapshot);
        };
    }

    const resources = (effectiveActor.resources || []).filter(function (item) { return item.quantity !== 0; });

    renderTokenGrid(
        `${prefix}Resources`,
        resources,
        item => `<div class="token"><strong>${item.name}</strong><div>${item.quantity}</div></div>`,
        'No data yet.'
    );

    renderTokenGrid(
        `${prefix}Defenses`,
        effectiveActor.defenses || [],
        item => `<div class="token"><strong>${item.name}</strong><div>Power ${item.power}</div></div>`,
        'No active defenses.'
    );

    renderTokenGrid(
        `${prefix}Skills`,
        effectiveActor.availableSkills || [],
        item => `<div class="token"><strong>${item}</strong></div>`,
        'No data yet.'
    );

    renderTokenGrid(
        `${prefix}FutureAttacks`,
        effectiveActor.futureAttacks || [],
        item => `<div class="token"><strong>${item.name}</strong><div>In ${item.delayRounds} turn(s)</div><div>Power ${item.power}</div></div>`,
        'No pending attacks.'
    );

    renderTokenGrid(
        `${prefix}FutureDefenses`,
        effectiveActor.futureDefenses || [],
        item => `<div class="token"><strong>${item.name}</strong><div>In ${item.delayRounds} turn(s)</div><div>Power ${item.power}</div></div>`,
        'No pending defenses.'
    );
}

function formatThinkTime(ms) {
    if (!ms || ms <= 0) return null;
    if (ms >= 1000) return `${(ms / 1000).toFixed(2)}s`;
    return `${ms.toFixed(0)}ms`;
}

function buildTurnSummary(turn) {
    if (!turn) {
        return '<div class="summary-card"><h3>Battle Summary</h3><div class="summary-line">Start a game to see structured turn details.</div></div>';
    }

    const thinkTime = formatThinkTime(turn.thinkingTimeMs);
    const thinkHtml = thinkTime
        ? `<div class="summary-line think-time">AI thought for ${thinkTime}</div>`
        : '';

    const playerStrParam = turn.playerStringParam ? `<div class="summary-line">StringParam: ${safeText(turn.playerStringParam)}</div>` : '';
    const enemyStrParam = turn.enemyStringParam ? `<div class="summary-line">StringParam: ${safeText(turn.enemyStringParam)}</div>` : '';

    return `
        <div class="summary-card">
            <h3>Player Action</h3>
            <div class="summary-line">Skill: ${safeText(turn.playerSkill)}</div>
            <div class="summary-line">Param: ${safeText(turn.playerParam, 0)}</div>
            ${playerStrParam}
            ${thinkHtml}
        </div>
        <div class="summary-card">
            <h3>Enemy Action</h3>
            <div class="summary-line">Skill: ${safeText(turn.enemySkill)}</div>
            <div class="summary-line">Param: ${safeText(turn.enemyParam, 0)}</div>
            ${enemyStrParam}
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

    historyList.innerHTML = State.turns.map((turn, index) => {
        const thinkTime = formatThinkTime(turn.thinkingTimeMs);
        const thinkBadge = thinkTime
            ? `<span class="think-badge">${thinkTime}</span>`
            : '';

        return `
        <button class="history-item ${index === State.currentTurn ? 'active' : ''}" data-turn-index="${index}" type="button">
            <div class="history-title">
                <span>Turn ${turn.index}</span>
                <span>${resultLabel(turn.result)}${thinkBadge}</span>
            </div>
            <div>You: ${turn.playerSkill} ${turn.playerParam}${turn.playerStringParam ? ' \"' + turn.playerStringParam + '\"' : ''} | Enemy: ${turn.enemySkill} ${turn.enemyParam}${turn.enemyStringParam ? ' \"' + turn.enemyStringParam + '\"' : ''}</div>
        </button>
        `;
    }).join('');

    historyList.querySelectorAll('[data-turn-index]').forEach(button => {
        button.addEventListener('click', () => {
            State.currentTurn = Number(button.getAttribute('data-turn-index'));
            renderTurn();
        });
    });
}

function renderTurn() {
    const turnIndex = document.getElementById('turnIndex');
    const turnCounterPill = document.getElementById('turnCounterPill');
    const actionText = document.getElementById('actionText');
    const turnSummary = document.getElementById('turnSummary');
    const resultBadge = document.getElementById('resultBadge');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');

    const turn = State.currentTurn >= 0 && State.currentTurn < State.turns.length
        ? State.turns[State.currentTurn]
        : null;

    if (turnIndex) turnIndex.textContent = turn ? `Turn ${turn.index} / ${State.turns.length}` : `Turn 0 / ${State.turns.length}`;
    if (turnCounterPill) turnCounterPill.textContent = turn ? `Turn ${turn.index}` : `Turn ${State.turns.length}`;
    if (actionText) {
        actionText.textContent = turn
            ? `You used ${turn.playerSkill} ${turn.playerParam}${turn.playerStringParam ? ' "' + turn.playerStringParam + '"' : ''}. Enemy used ${turn.enemySkill} ${turn.enemyParam}${turn.enemyStringParam ? ' "' + turn.enemyStringParam + '"' : ''}.`
            : (State.gameStarted ? 'Battle initialized. Declare the first turn.' : 'No actions recorded yet.');
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

function renderAiStats() {
    const aiTurns = State.turns.filter(t => t.thinkingTimeMs > 0);
    const bar = document.getElementById('aiStatsBar');
    const lastThink = document.getElementById('lastThinkTime');
    const avgThink = document.getElementById('avgThinkTime');
    const aiCount = document.getElementById('aiTurnCount');

    if (bar) {
        bar.classList.toggle('is-hidden', State.isManual || !State.gameStarted);
    }

    if (!aiTurns.length) {
        if (lastThink) lastThink.textContent = '--';
        if (avgThink) avgThink.textContent = '--';
        if (aiCount) aiCount.textContent = '0';
        return;
    }

    const last = aiTurns[aiTurns.length - 1];
    const total = aiTurns.reduce((sum, t) => sum + t.thinkingTimeMs, 0);
    const avg = total / aiTurns.length;

    if (lastThink) lastThink.textContent = formatThinkTime(last.thinkingTimeMs) || '--';
    if (avgThink) avgThink.textContent = formatThinkTime(avg) || '--';
    if (aiCount) aiCount.textContent = String(aiTurns.length);
}

function renderSnapshot(snapshot, options = {}) {
    const autoFocusLatest = Boolean(options.autoFocusLatest);

    if (snapshot !== State.snapshot) {
        State.playerSummonIndex = -1;
        State.enemySummonIndex = -1;
    }

    State.snapshot = snapshot;
    State.turns = snapshot?.turns || [];
    State.gameStarted = Boolean(snapshot?.started);
    State.isManual = Boolean(snapshot?.manualMode);
    State.selectedModeName = safeText(snapshot?.modeName, 'Not started');
    State.lastResult = resultLabel(snapshot?.result);
    State.heroCollapsed = State.gameStarted && !['Victory', 'Defeat', 'Draw'].includes(State.lastResult);

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
    renderAiStats();
    updateEnemyInputVisibility();
    updateHeroVisibility();
}

function updateEnemyInputVisibility() {
    const enemyField = document.getElementById('enemyField');
    const modeHint = document.getElementById('modeHint');
    const actionHint = document.getElementById('actionHint');

    if (enemyField) enemyField.classList.toggle('is-hidden', !State.isManual);
    if (modeHint) {
        modeHint.textContent = State.isManual
            ? 'Manual mode lets you enter both sides of the turn.'
            : 'AI mode hides enemy input and lets the selected strategy play automatically.';
    }
    if (actionHint) {
        actionHint.textContent = State.gameStarted
            ? (State.isManual ? 'Enter your action and the enemy action for this round.' : 'Enter your action. The enemy move will be chosen by AI.')
            : 'Start a game to enable turn declaration.';
    }
}

function updateBusyState() {
    const startBtn = document.getElementById('startBtn');
    const restartBtn = document.getElementById('restartBtn');
    const declareBtn = document.getElementById('declareBtn');
    const connectionState = document.getElementById('connectionState');

    const isGameOver = State.lastResult === 'Victory' || State.lastResult === 'Defeat' || State.lastResult === 'Draw';

    if (startBtn) startBtn.disabled = State.busy;
    if (restartBtn) restartBtn.disabled = State.busy || !State.gameStarted;
    if (declareBtn) declareBtn.disabled = State.busy || !State.gameStarted || isGameOver;
    if (connectionState) connectionState.textContent = State.busy ? 'Working' : 'Ready';
}
