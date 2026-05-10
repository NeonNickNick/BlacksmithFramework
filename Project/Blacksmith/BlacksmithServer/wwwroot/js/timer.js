function startTurnTimer(secondsRemaining) {
    stopTurnTimer();
    State.turnTimeRemaining = secondsRemaining;
    renderTimer();

    State.turnTimerInterval = setInterval(() => {
        State.turnTimeRemaining--;
        renderTimer();
        if (State.turnTimeRemaining <= 0) {
            stopTurnTimer();
        }
    }, 1000);
}

function stopTurnTimer() {
    if (State.turnTimerInterval) {
        clearInterval(State.turnTimerInterval);
        State.turnTimerInterval = null;
    }
    State.turnTimeRemaining = 0;
    renderTimer();
}

function renderTimer() {
    const el = document.getElementById('turnTimer');
    const bar = document.getElementById('turnTimerFill');
    const text = document.getElementById('turnTimerText');

    if (!el || !bar) return;

    if (State.turnTimeRemaining <= 0 || State.gameOver || !State.gameStarted) {
        el.classList.add('is-hidden');
        return;
    }

    el.classList.remove('is-hidden');
    const pct = (State.turnTimeRemaining / 15) * 100;
    bar.style.width = `${pct}%`;

    if (State.turnTimeRemaining <= 5) {
        el.classList.add('timer-urgent');
    } else {
        el.classList.remove('timer-urgent');
    }

    if (text) text.textContent = `${State.turnTimeRemaining}s`;
}
