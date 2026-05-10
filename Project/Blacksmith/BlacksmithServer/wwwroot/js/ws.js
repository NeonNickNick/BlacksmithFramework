const WS = (() => {
    let socket = null;
    const messageHandlers = {};

    function connect() {
        const protocol = location.protocol === 'https:' ? 'wss:' : 'ws:';
        socket = new WebSocket(`${protocol}//${location.host}/ws`);

        socket.onopen = () => {
            State.connected = true;
            resetGameState();
            Matchmaking.renderQueueUI();
        };

        socket.onmessage = (event) => {
            const msg = JSON.parse(event.data);
            const handler = messageHandlers[msg.type];
            if (handler) {
                handler(msg);
            } else {
                console.warn('[WS] Unhandled message type:', msg.type, msg);
            }
        };

        socket.onclose = () => {
            State.connected = false;
            State.inQueue = false;
            resetGameState();
            Matchmaking.renderQueueUI();
            setTimeout(connect, 3000);
        };

        socket.onerror = (err) => {
            console.error('[WS] Error:', err);
        };
    }

    function send(type, payload = {}) {
        if (socket && socket.readyState === WebSocket.OPEN) {
            socket.send(JSON.stringify({ type, ...payload }));
        }
    }

    function on(type, handler) {
        messageHandlers[type] = handler;
    }

    setInterval(() => send('ping'), 30000);

    return { connect, send, on };
})();
