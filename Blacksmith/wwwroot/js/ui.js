// ===== ui.js =====
function renderTurn() {
    const { currentTurn, turns } = State;
    if (currentTurn < 0 || currentTurn >= turns.length) return;

    const t = turns[currentTurn];

    const turnIndex = document.getElementById('turnIndex');
    const pHP = document.getElementById('pHP');
    const eHP = document.getElementById('eHP');
    const actionText = document.getElementById('actionText');
    const detailLog = document.getElementById('detailLog');

    if (turnIndex) turnIndex.innerText = `Turn ${currentTurn + 1} / ${turns.length}`;
    if (pHP) pHP.innerText = `HP: ${t.player.hp}/${t.player.mhp}`;
    if (eHP) eHP.innerText = `HP: ${t.enemy.hp}/${t.enemy.mhp}`;
    if (actionText) actionText.innerText = t.action;
    if (detailLog) detailLog.innerText = t.log;
}

function updateEnemyInputVisibility() {
    const eskill = document.getElementById('eskill');
    if (eskill) eskill.style.display = State.isManual ? 'block' : 'none';
}
