const TOKEN_KEY = 'blacksmithServerToken';
const USERNAME_KEY = 'blacksmithServerUsername';

const ROUND_TIMEOUT_MS = 30000;
const QUEUE_TIMEOUT_MS = 30000;
const ROUND_TIMEOUT_SEC = ROUND_TIMEOUT_MS / 1000;
const QUEUE_TIMEOUT_SEC = QUEUE_TIMEOUT_MS / 1000;
const TIMEOUT_LOSS_THRESHOLD = 3;

const State = {
    token: localStorage.getItem(TOKEN_KEY) || '',
    username: localStorage.getItem(USERNAME_KEY) || '',
    authenticated: false,
    busy: false,
    socket: null,
    socketCloseExpected: false,
    reconnectTimer: null,
    connectionState: 'Disconnected',
    snapshot: null,
    turns: [],
    currentTurn: -1,
    heroCollapsed: false,
    lastBanner: 'Register or log in to connect to the arena.',
    playerSummonIndex: -1,
    enemySummonIndex: -1
};
