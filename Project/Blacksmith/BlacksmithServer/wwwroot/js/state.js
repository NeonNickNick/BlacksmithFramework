const TOKEN_KEY = 'blacksmithServerToken';
const USERNAME_KEY = 'blacksmithServerUsername';

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
    lastBanner: 'Register or log in to connect to the arena.'
};
