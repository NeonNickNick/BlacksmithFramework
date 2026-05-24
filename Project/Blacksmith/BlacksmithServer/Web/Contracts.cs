namespace BlacksmithServer.Web
{
    public sealed class AuthRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public sealed class AuthResponse
    {
        public bool Ok { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? Username { get; set; }
    }

    public sealed class ServerEnvelope
    {
        public required string Type { get; set; }
        public string? Message { get; set; }
        public BattleSnapshot? Snapshot { get; set; }
    }

    public sealed class BattleSnapshot
    {
        public required bool Authenticated { get; set; }
        public required string Username { get; set; }
        public required string Status { get; set; }
        public required bool Started { get; set; }
        public required bool ManualMode { get; set; }
        public required string ModeName { get; set; }
        public required string Result { get; set; }
        public ResultDetailDto? ResultDetail { get; set; }
        public string? OpponentName { get; set; }
        public required bool Queued { get; set; }
        public string? QueueExpiresAtUtc { get; set; }
        public string? RoundDeadlineUtc { get; set; }
        public required bool HasSubmittedTurn { get; set; }
        public required int PlayerTimeouts { get; set; }
        public required int EnemyTimeouts { get; set; }
        public required string StatusMessage { get; set; }
        public ActorSnapshot? Player { get; set; }
        public ActorSnapshot? Enemy { get; set; }
        public List<TurnSnapshot> Turns { get; set; } = new();
    }

    public sealed class ResultDetailDto
    {
        public required string Title { get; set; }
        public required string ReasonCode { get; set; }
        public required string Summary { get; set; }
    }

    public sealed class ActorSnapshot
    {
        public string BodyName { get; set; } = string.Empty;
        public List<string> Professions { get; set; } = new();
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public List<NamedValueSnapshot> Defenses { get; set; } = new();
        public List<ResourceSnapshot> Resources { get; set; } = new();
        public List<FutureValueSnapshot> FutureAttacks { get; set; } = new();
        public List<FutureValueSnapshot> FutureDefenses { get; set; } = new();
        public List<string> AvailableSkills { get; set; } = new();
        public List<ActorSnapshot> Summons { get; set; } = new();
    }

    public sealed class NamedValueSnapshot
    {
        public required string Name { get; set; }
        public required int Power { get; set; }
    }

    public sealed class ResourceSnapshot
    {
        public required string Name { get; set; }
        public required float Quantity { get; set; }
    }

    public sealed class FutureValueSnapshot
    {
        public required string Name { get; set; }
        public required int DelayRounds { get; set; }
        public required int Power { get; set; }
    }

    public sealed class TurnSnapshot
    {
        public required int Index { get; set; }
        public required string Result { get; set; }
        public required string PlayerSkill { get; set; }
        public required int PlayerParam { get; set; }
        public required string PlayerStringParam { get; set; }
        public required string EnemySkill { get; set; }
        public required int EnemyParam { get; set; }
        public required string EnemyStringParam { get; set; }
        public required bool PlayerTimedOut { get; set; }
        public required bool EnemyTimedOut { get; set; }
        public string? Note { get; set; }
    }
}
