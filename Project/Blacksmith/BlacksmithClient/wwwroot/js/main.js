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
    const skill = parseSkill(skillInput?.value || '');
    const enemySkill = parseSkill(eskill?.value || '');
    const response = await declareAPI({
        skillName: skill.name,
        param: skill.param,
        esn: enemySkill.name,
        ep: enemySkill.param,
        stringParam: skill.stringParam,
        esp: enemySkill.stringParam
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
