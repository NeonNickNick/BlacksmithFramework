const Matchmaking = (() => {
    function joinQueue() {
        if (State.inQueue || State.gameStarted) return;
        if (!State.connected) {
            alert('Not connected to server. Please wait for reconnection.');
            return;
        }
        WS.send('join_queue');
    }

    function leaveQueue() {
        if (!State.inQueue) return;
        WS.send('leave_queue');
    }

    function renderQueueUI() {
        const queueBtn = document.getElementById('queueBtn');
        const queueStatus = document.getElementById('queueStatus');
        const playerBadge = document.getElementById('playerBadge');

        if (State.inQueue) {
            if (queueBtn) queueBtn.textContent = 'Cancel';
            if (queueBtn) queueBtn.className = 'primary-btn queue-leave-btn';
            if (queueStatus) {
                queueStatus.textContent = 'Searching for opponent...';
                queueStatus.className = 'queue-status searching';
            }
        } else {
            if (queueBtn) queueBtn.textContent = 'Join Queue';
            if (queueBtn) queueBtn.className = 'primary-btn';
            if (queueStatus) {
                queueStatus.textContent = State.connected ? 'Ready' : 'Disconnected';
                queueStatus.className = 'queue-status';
            }
        }

        if (State.playerNumber > 0 && playerBadge) {
            playerBadge.textContent = `You are Player ${State.playerNumber}`;
            playerBadge.classList.remove('is-hidden');
        } else if (playerBadge) {
            playerBadge.classList.add('is-hidden');
        }
    }

    return { joinQueue, leaveQueue, renderQueueUI };
})();
