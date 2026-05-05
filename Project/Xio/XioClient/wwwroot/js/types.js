// types.js — Lightweight runtime contract for API payloads.

const Types = (() => {
    const is = {
        string: v => typeof v === 'string',
        number: v => typeof v === 'number' && Number.isFinite(v),
        boolean: v => typeof v === 'boolean',
        array: v => Array.isArray(v),
        object: v => v !== null && typeof v === 'object' && !Array.isArray(v),
        maybe: check => v => v == null || check(v),
    };

    const SkillInfoShape = {
        name: is.string,
        usable: is.boolean,
    };

    const ActorShape = {
        rank: is.number,
        rankName: is.string,
        innerLevel: is.number,
        resources: is.array,
        availableSkills: is.array,
    };

    const SnapshotShape = {
        round: is.number,
        innerRound: is.number,
        player: is.maybe(is.object),
        enemy: is.maybe(is.object),
        turns: is.array,
        started: is.boolean,
        manualMode: is.boolean,
        modeName: is.string,
        result: is.string,
    };

    function checkShape(value, shape, path = '') {
        if (value == null) return null;

        const errors = [];
        for (const [key, test] of Object.entries(shape)) {
            const fullPath = path ? `${path}.${key}` : key;
            if (!(key in value)) {
                errors.push(`${fullPath}: missing`);
                continue;
            }
            if (!test(value[key])) {
                const actual = typeof value[key];
                errors.push(`${fullPath}: expected compatible type, got ${actual}`);
            }
        }
        return errors.length > 0 ? errors : null;
    }

    function validateSnapshot(snapshot) {
        const errors = checkShape(snapshot, SnapshotShape);
        if (errors) {
            console.warn('[Types] Snapshot shape mismatch:', errors, snapshot);
        }
        if (snapshot && snapshot.player) {
            const actorErrors = checkShape(snapshot.player, ActorShape, 'player');
            if (actorErrors) console.warn('[Types] Player shape mismatch:', actorErrors, snapshot.player);
            if (Array.isArray(snapshot.player.availableSkills)) {
                snapshot.player.availableSkills.forEach((s, i) => {
                    const errs = checkShape(s, SkillInfoShape, `player.availableSkills[${i}]`);
                    if (errs) console.warn('[Types] Player skill shape mismatch:', errs, s);
                });
            }
        }
        if (snapshot && snapshot.enemy) {
            const actorErrors = checkShape(snapshot.enemy, ActorShape, 'enemy');
            if (actorErrors) console.warn('[Types] Enemy shape mismatch:', actorErrors, snapshot.enemy);
            if (Array.isArray(snapshot.enemy.availableSkills)) {
                snapshot.enemy.availableSkills.forEach((s, i) => {
                    const errs = checkShape(s, SkillInfoShape, `enemy.availableSkills[${i}]`);
                    if (errs) console.warn('[Types] Enemy skill shape mismatch:', errs, s);
                });
            }
        }
    }

    return { validateSnapshot };
})();
