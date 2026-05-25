function safeText(value, fallback = '--') {
    return value === null || value === undefined || value === '' ? fallback : String(value);
}

function parseSkill(text) {
    const raw = (text || '').trim();
    if (!raw) return { name: 'iron', param: 0, stringParam: '' };

    const tokens = raw.split(/\s+/);
    const name = tokens[0] || 'iron';
    let param = 0;
    let stringParam = '';

    for (let i = 1; i < tokens.length; i++) {
        if (tokens[i] === '-p' && i + 1 < tokens.length) {
            const parsed = Number.parseInt(tokens[i + 1], 10);
            if (Number.isFinite(parsed) && parsed >= 0) {
                param = parsed;
                i++;
            }
        } else if (tokens[i] === '-s' && i + 1 < tokens.length) {
            stringParam = tokens[i + 1];
            i++;
        }
    }

    return { name, param, stringParam };
}

function resultLabel(result) {
    switch (result) {
        case 'win': return 'Win';
        case 'lose': return 'Lose';
        case 'draw': return 'Draw';
        case 'next': return 'In progress';
        default: return 'Ready';
    }
}

function stateLabel(snapshot) {
    if (!snapshot) return 'Offline';

    switch (snapshot.status) {
        case 'queueing': return 'Queueing';
        case 'playing': return 'Playing';
        case 'finished': return 'Finished';
        case 'lobby': return 'Lobby';
        default: return 'Offline';
    }
}

async function withBusy(task) {
    if (State.busy) return;

    State.busy = true;
    updateBusyState();

    try {
        await task();
    } catch (error) {
        const message = error instanceof Error ? error.message : 'Unexpected error';
        State.lastBanner = message;
        renderHeroCopy();
    } finally {
        State.busy = false;
        updateBusyState();
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

function setHealthBar(elementId, current, max) {
    const element = document.getElementById(elementId);
    if (!element) return;

    const ratio = max > 0 ? Math.max(0, Math.min(100, (current / max) * 100)) : 0;
    element.style.width = `${ratio}%`;
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

    if (name) name.textContent = prefix === 'player' ? 'You' : (State.snapshot?.opponentName || 'Opponent');
    if (bodyNameEl) bodyNameEl.textContent = safeText(effectiveActor.bodyName, '--');
    if (profession) {
        const professions = Array.isArray(effectiveActor.professions) ? effectiveActor.professions : [];
        profession.textContent = professions.length ? professions.join(', ') : 'None';
    }
    if (hp) hp.textContent = `HP ${effectiveActor.hp}/${effectiveActor.maxHp}`;
    setHealthBar(prefix === 'player' ? 'playerHealthFill' : 'enemyHealthFill', effectiveActor.hp, effectiveActor.maxHp);

    if (summonLabel && summons.length > 0) {
        if (summonIndex < 0) {
            summonLabel.textContent = 'Main Body';
        } else {
            summonLabel.textContent = safeText(effectiveActor.bodyName, `Summon ${summonIndex + 1}`);
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
        item => `<div class="token"><strong>${safeText(item.name)}</strong><div>${safeText(item.quantity, 0)}</div></div>`,
        'No data yet.'
    );

    renderTokenGrid(
        `${prefix}Defenses`,
        effectiveActor.defenses || [],
        item => `<div class="token"><strong>${safeText(item.name)}</strong><div>Power ${safeText(item.power, 0)}</div></div>`,
        'No active defenses.'
    );

    renderTokenGrid(
        `${prefix}Skills`,
        effectiveActor.availableSkills || [],
        item => `<div class="token"><strong>${safeText(item)}</strong></div>`,
        'No data yet.'
    );

    renderTokenGrid(
        `${prefix}FutureAttacks`,
        effectiveActor.futureAttacks || [],
        item => `<div class="token"><strong>${safeText(item.name)}</strong><div>In ${safeText(item.delayRounds, 0)} turn(s)</div><div>Power ${safeText(item.power, 0)}</div></div>`,
        'No pending attacks.'
    );

    renderTokenGrid(
        `${prefix}FutureDefenses`,
        effectiveActor.futureDefenses || [],
        item => `<div class="token"><strong>${safeText(item.name)}</strong><div>In ${safeText(item.delayRounds, 0)} turn(s)</div><div>Power ${safeText(item.power, 0)}</div></div>`,
        'No pending defenses.'
    );
}

function buildTurnSummary(turn) {
    if (!turn) {
        return '<div class="summary-card"><h3>Battle Summary</h3><div class="summary-line">Start a match to see structured turn details.</div></div>';
    }

    const noteHtml = turn.note
        ? `<div class="summary-line think-time">${safeText(turn.note)}</div>`
        : '';

    return `
        <div class="summary-card player-card">
            <h3>Your Action</h3>
            <div class="skill-highlight">${safeText(turn.playerSkill)}</div>
            <div class="summary-line">Param: ${safeText(turn.playerParam, 0)}</div>
            <div class="summary-line">Timed out: ${turn.playerTimedOut ? 'Yes' : 'No'}</div>
        </div>
        <div class="summary-card opponent-card">
            <h3>Opponent Action</h3>
            <div class="skill-highlight">${safeText(turn.enemySkill)}</div>
            <div class="summary-line">Param: ${safeText(turn.enemyParam, 0)}</div>
            <div class="summary-line">Timed out: ${turn.enemyTimedOut ? 'Yes' : 'No'}</div>
            ${noteHtml}
        </div>
    `;
}

function renderHistory() {
    const historyList = document.getElementById('historyList');
    if (!historyList) return;

    if (!State.turns.length) {
        historyList.innerHTML = '<div class="history-empty">No turns yet. Once a match starts, each round will be archived here.</div>';
        return;
    }

    historyList.innerHTML = State.turns.map((turn, index) => {
        const timeoutBadge = turn.playerTimedOut || turn.enemyTimedOut
            ? '<span class="think-badge">Timeout</span>'
            : '';

        return `
            <button class="history-item ${index === State.currentTurn ? 'active' : ''}" data-turn-index="${index}" type="button">
                <div class="history-title">
                    <span>Turn ${turn.index}</span>
                    <span>${timeoutBadge}</span>
                </div>
                <div>You: <strong class="skill-name">${safeText(turn.playerSkill)}</strong> ${safeText(turn.playerParam, 0)} | Opponent: <strong class="skill-name">${safeText(turn.enemySkill)}</strong> ${safeText(turn.enemyParam, 0)}</div>
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
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');

    const turn = State.currentTurn >= 0 && State.currentTurn < State.turns.length
        ? State.turns[State.currentTurn]
        : null;

    if (turnIndex) turnIndex.textContent = turn ? `Turn ${turn.index} / ${State.turns.length}` : `Turn 0 / ${State.turns.length}`;
    if (turnCounterPill) turnCounterPill.textContent = turn ? `Turn ${turn.index}` : `Turn ${State.turns.length}`;
    if (actionText) {
        if (turn) {
            actionText.innerHTML = `You used <strong class="skill-name">${safeText(turn.playerSkill)}</strong> ${safeText(turn.playerParam, 0)}. Opponent used <strong class="skill-name">${safeText(turn.enemySkill)}</strong> ${safeText(turn.enemyParam, 0)}.`;
        } else if (State.snapshot?.resultDetail?.summary) {
            actionText.textContent = State.snapshot.resultDetail.summary;
        } else {
            actionText.textContent = State.snapshot?.statusMessage || 'No actions recorded yet.';
        }
    }
    if (turnSummary) turnSummary.innerHTML = buildTurnSummary(turn);
    if (prevBtn) prevBtn.disabled = State.currentTurn <= 0;
    if (nextBtn) nextBtn.disabled = State.currentTurn < 0 || State.currentTurn >= State.turns.length - 1;

    renderHistory();
}

function renderHeroCopy() {
    const heroCopy = document.getElementById('heroCopy');
    if (heroCopy) {
        heroCopy.textContent = State.lastBanner || State.snapshot?.statusMessage || 'Ready.';
    }
}

function renderConnectionBits() {
    const connectionState = document.getElementById('connectionState');
    const userBadge = document.getElementById('userBadge');
    const stateBadge = document.getElementById('stateBadge');
    const resultBadge = document.getElementById('resultBadge');
    const opponentBadge = document.getElementById('opponentBadge');
    const resultTitle = document.getElementById('resultTitle');
    const resultDetail = document.getElementById('resultDetail');

    if (connectionState) connectionState.textContent = State.connectionState;
    if (userBadge) userBadge.textContent = State.authenticated ? safeText(State.username) : 'Guest';
    if (stateBadge) stateBadge.textContent = stateLabel(State.snapshot);
    if (resultBadge) resultBadge.textContent = resultLabel(State.snapshot?.result);
    if (opponentBadge) opponentBadge.textContent = State.snapshot?.opponentName ? `Opponent: ${State.snapshot.opponentName}` : 'No opponent';

    if (resultTitle) {
        resultTitle.textContent = State.snapshot?.resultDetail?.title || 'No match yet';
    }
    if (resultDetail) {
        resultDetail.textContent = State.snapshot?.resultDetail?.summary || 'Once a match ends, the battle result and cause will appear here.';
    }
}

function formatCountdown(isoString, totalMs) {
    if (!isoString) return { text: '--', urgent: false, percent: 0 };

    const remainingMs = new Date(isoString).getTime() - Date.now();
    const clampedMs = Math.max(0, remainingMs);
    const seconds = Math.max(0, Math.ceil(clampedMs / 1000));
    const percent = totalMs > 0 ? Math.max(0, Math.min(100, (clampedMs / totalMs) * 100)) : 0;

    return {
        text: `${seconds}s`,
        urgent: seconds > 0 && seconds <= 5,
        percent
    };
}

function setTimerBar(elementId, countdown) {
    const element = document.getElementById(elementId);
    if (!element) return;

    element.style.width = `${countdown.percent}%`;
    element.classList.toggle('is-urgent', countdown.urgent);
}

function updateCountdowns() {
    const queueCountdown = document.getElementById('queueCountdown');
    const roundCountdown = document.getElementById('roundCountdown');
    const playerTimeouts = document.getElementById('playerTimeouts');
    const enemyTimeouts = document.getElementById('enemyTimeouts');

    const queue = formatCountdown(State.snapshot?.queueExpiresAtUtc || null, QUEUE_TIMEOUT_MS);
    const round = formatCountdown(State.snapshot?.roundDeadlineUtc || null, ROUND_TIMEOUT_MS);

    if (queueCountdown) {
        queueCountdown.textContent = queue.text;
        queueCountdown.classList.toggle('is-urgent', queue.urgent);
    }
    if (roundCountdown) {
        roundCountdown.textContent = round.text;
        roundCountdown.classList.toggle('is-urgent', round.urgent);
    }
    setTimerBar('queueCountdownBar', queue);
    setTimerBar('roundCountdownBar', round);
    if (playerTimeouts) playerTimeouts.textContent = `${State.snapshot?.playerTimeouts ?? 0} / ${TIMEOUT_LOSS_THRESHOLD}`;
    if (enemyTimeouts) enemyTimeouts.textContent = `${State.snapshot?.enemyTimeouts ?? 0} / ${TIMEOUT_LOSS_THRESHOLD}`;
}

function updateHeroVisibility() {
    const heroPanel = document.getElementById('heroPanel');
    if (!heroPanel) return;
    heroPanel.open = !State.heroCollapsed;
}

function renderSnapshot(snapshot, options = {}) {
    const message = options.message || '';
    const autoFocusLatest = Boolean(options.autoFocusLatest);
    const previousTurns = State.turns.length;

    if (snapshot !== State.snapshot) {
        State.playerSummonIndex = -1;
        State.enemySummonIndex = -1;
    }

    State.snapshot = snapshot;
    State.turns = Array.isArray(snapshot?.turns) ? snapshot.turns : [];
    State.authenticated = Boolean(snapshot?.authenticated) || Boolean(State.token);
    State.username = snapshot?.username || State.username;
    State.lastBanner = message || snapshot?.statusMessage || State.lastBanner;
    State.heroCollapsed = snapshot?.status === 'playing' && snapshot?.result === 'next';

    if (!State.turns.length) {
        State.currentTurn = -1;
    } else if (autoFocusLatest || previousTurns !== State.turns.length || State.currentTurn < 0 || State.currentTurn >= State.turns.length) {
        State.currentTurn = State.turns.length - 1;
    }

    renderConnectionBits();
    renderHeroCopy();
    renderActor('player', snapshot?.player || null);
    renderActor('enemy', snapshot?.enemy || null);
    renderTurn();
    updateCountdowns();
    updateHeroVisibility();
    updateBusyState();
}

function renderLoggedOutState() {
    State.snapshot = null;
    State.turns = [];
    State.currentTurn = -1;
    State.authenticated = false;
    State.connectionState = 'Disconnected';
    State.lastBanner = 'Your session is no longer valid. Please sign in again.';

    renderConnectionBits();
    renderHeroCopy();
    renderActor('player', null);
    renderActor('enemy', null);
    renderTurn();
    updateCountdowns();
    updateBusyState();
}

function updateBusyState() {
    const registerBtn = document.getElementById('registerBtn');
    const loginBtn = document.getElementById('loginBtn');
    const queueBtn = document.getElementById('queueBtn');
    const cancelQueueBtn = document.getElementById('cancelQueueBtn');
    const logoutBtn = document.getElementById('logoutBtn');
    const declareBtn = document.getElementById('declareBtn');
    const usernameInput = document.getElementById('usernameInput');
    const passwordInput = document.getElementById('passwordInput');
    const actionHint = document.getElementById('actionHint');

    const socketReady = State.socket && State.socket.readyState === WebSocket.OPEN;
    const status = State.snapshot?.status || 'offline';
    const inMatch = status === 'playing';
    const queued = status === 'queueing';
    const matchFinished = status === 'finished';

    if (registerBtn) registerBtn.disabled = State.busy;
    if (loginBtn) loginBtn.disabled = State.busy;
    if (usernameInput) usernameInput.disabled = State.busy;
    if (passwordInput) passwordInput.disabled = State.busy;
    if (queueBtn) queueBtn.disabled = State.busy || !State.authenticated || !socketReady || queued || inMatch;
    if (cancelQueueBtn) cancelQueueBtn.disabled = State.busy || !State.authenticated || !socketReady || !queued;
    if (logoutBtn) logoutBtn.disabled = State.busy || !State.authenticated;
    if (declareBtn) declareBtn.disabled = State.busy || !State.authenticated || !socketReady || !inMatch || matchFinished || Boolean(State.snapshot?.hasSubmittedTurn);

    if (actionHint) {
        if (!State.authenticated) {
            actionHint.textContent = 'Log in and connect to a match before submitting a turn.';
        } else if (queued) {
            actionHint.textContent = `Queueing for an opponent. If no match is found in ${QUEUE_TIMEOUT_SEC} seconds, click Find Match again.`;
        } else if (inMatch && State.snapshot?.hasSubmittedTurn) {
            actionHint.textContent = 'Your turn is locked. Waiting for the opponent or the timer.';
        } else if (inMatch) {
            actionHint.textContent = `Submit your action within ${ROUND_TIMEOUT_SEC} seconds or the server will default to iron 0.`;
        } else if (matchFinished) {
            actionHint.textContent = 'This match has ended. You can queue again for a new duel.';
        } else {
            actionHint.textContent = 'Click Find Match to enter the matchmaking queue.';
        }
    }
}
