function parseSkillInput(text) {
    const raw = (text || '').trim();
    if (!raw) return { skillName: 'x', rank: 0 };

    const prefixMap = { hj: 1, zs: 2, hd: 3, wx: 4 };

    const lower = raw.toLowerCase();
    for (const [prefix, rank] of Object.entries(prefixMap)) {
        const prefixDash = prefix + '-';
        if (lower.startsWith(prefixDash)) {
            const skillName = raw.substring(prefixDash.length);
            return { skillName: skillName || 'x', rank };
        }
    }

    return { skillName: raw, rank: 0 };
}

async function withBusy(task) {
    if (State.busy) return;

    State.busy = true;
    updateBusyState();

    try {
        await task();
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

const startBtn = document.getElementById('startBtn');
const restartBtn = document.getElementById('restartBtn');
const strategy = document.getElementById('strategy');
const skillInput = document.getElementById('skill');
const eskill = document.getElementById('eskill');
const declareBtn = document.getElementById('declareBtn');
const prevBtn = document.getElementById('prevBtn');
const nextBtn = document.getElementById('nextBtn');
const heroPanel = document.getElementById('heroPanel');

async function startOrRestartGame() {
    const mode = Number.parseInt(strategy.value, 10);
    const response = await startGame(mode);
    if (!response.ok) {
        throw new Error(response.message || 'Unable to start game.');
    }
    renderSnapshot(response.snapshot, { autoFocusLatest: true });
    updateBusyState();
}

startBtn?.addEventListener('click', () => withBusy(startOrRestartGame));
restartBtn?.addEventListener('click', () => withBusy(startOrRestartGame));

strategy?.addEventListener('change', () => {
    const selectedOption = strategy.options[strategy.selectedIndex];
    State.selectedModeName = selectedOption ? selectedOption.textContent : 'Not started';
    const modeBadge = document.getElementById('modeBadge');
    if (modeBadge) modeBadge.textContent = State.selectedModeName;
});

declareBtn?.addEventListener('click', () => withBusy(async () => {
    const playerInput = skillInput?.value || '';
    const enemyInput = eskill?.value || '';

    const response = await declareAPI({
        skillInput: playerInput,
        enemySkillInput: enemyInput
    });

    if (!response.ok) {
        renderSnapshot(response.snapshot, { autoFocusLatest: true });
        updateBusyState();
        throw new Error(response.message || 'Turn declaration failed.');
    }

    renderSnapshot(response.snapshot, { autoFocusLatest: true });
    updateBusyState();
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

(async function init() {
    await withBusy(async () => {
        const list = await loadStrategies();
        strategy.innerHTML = '';
        list.forEach(item => {
            const option = document.createElement('option');
            option.value = item.id;
            option.textContent = item.name;
            strategy.appendChild(option);
        });

        if (strategy.options.length > 0) {
            strategy.selectedIndex = 0;
            State.selectedModeName = strategy.options[0].textContent;
        }

        const status = await loadStatus();
        if (status.ok) {
            renderSnapshot(status.snapshot, { autoFocusLatest: true });
        } else {
            updateEnemyInputVisibility();
            renderTurn();
        }
        updateBusyState();
    });
})();
