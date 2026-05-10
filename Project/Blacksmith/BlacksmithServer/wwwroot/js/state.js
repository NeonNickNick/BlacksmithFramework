const State = {
    turns: [],
    currentTurn: -1,
    gameStarted: false,
    lastResult: 'Awaiting game',
    busy: false,
    snapshot: null,
    heroCollapsed: false,

    playerNumber: 0,
    roomId: null,
    connected: false,
    inQueue: false,
    queuePosition: 0,
    waitingForOpponent: false,
    turnTimeRemaining: 0,
    turnTimerInterval: null,
    gameOver: false
};

function resetGameState() {
    State.turns = [];
    State.currentTurn = -1;
    State.gameStarted = false;
    State.lastResult = 'Awaiting game';
    State.busy = false;
    State.snapshot = null;
    State.waitingForOpponent = false;
    State.gameOver = false;
    stopTurnTimer();
}
