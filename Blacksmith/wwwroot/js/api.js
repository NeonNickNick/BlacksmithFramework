
// ===== api.js =====
async function loadStrategies() {
    const res = await fetch('/api/strategies');
    return await res.json();
}

async function startGame(mode) {
    const res = await fetch('/api/start', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mode })
    });
    return await res.json();
}

async function declareAPI(payload) {
    const res = await fetch('/api/declare', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });
    return await res.json();
}