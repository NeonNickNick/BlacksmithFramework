// ===== main.js =====
function parseSkill(text) {
    let name = "iron", param = 0;
    if (text) {
        const p = text.split(' ', 2);
        name = p[0];
        if (p.length > 1) param = parseInt(p[1]) || 0;
    }
    return { name, param };
}

// Get DOM elements explicitly
const startBtn = document.getElementById('startBtn');
const strategy = document.getElementById('strategy');
const skillInput = document.getElementById('skill');
const eskill = document.getElementById('eskill');
const declareBtn = document.getElementById('declareBtn');
const prevBtn = document.getElementById('prevBtn');
const nextBtn = document.getElementById('nextBtn');

startBtn.onclick = async () => {
    State.turns = [];
    State.currentTurn = -1;

    const mode = parseInt(strategy.value);
    const json = await startGame(mode);

    State.isManual = json.manual;
    updateEnemyInputVisibility();

    alert(`Game started | AI=${json.ai}`);
};

declareBtn.onclick = async () => {
    const skill = parseSkill(skillInput.value);
    const esk = parseSkill(eskill.value);

    const payload = {
        skillName: skill.name,
        param: skill.param,
        esn: esk.name,
        ep: esk.param
    };

    const json = await declareAPI(payload);
    if (!json.ok) return alert(json.message);

    State.turns.push({
        action: json.message,
        log: json.log,
        player: json.player,
        enemy: json.enemy
    });

    State.currentTurn = State.turns.length - 1;
    renderTurn();
};

prevBtn.onclick = () => {
    if (State.currentTurn > 0) {
        State.currentTurn--;
        renderTurn();
    }
};

nextBtn.onclick = () => {
    if (State.currentTurn < State.turns.length - 1) {
        State.currentTurn++;
        renderTurn();
    }
};

(async function init() {
    const list = await loadStrategies();
    strategy.innerHTML = '';
    list.forEach(s => {
        const opt = document.createElement('option');
        opt.value = s.id;
        opt.textContent = `${s.id} - ${s.name}`;
        strategy.appendChild(opt);
    });
    // Ensure enemy input visibility is correct on load
    updateEnemyInputVisibility();
})();
