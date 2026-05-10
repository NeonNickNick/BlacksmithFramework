namespace BlacksmithServer.Server
{
    public static class MessageTypes
    {
        public const string JoinQueue = "join_queue";
        public const string LeaveQueue = "leave_queue";
        public const string Declare = "declare";
        public const string Ping = "ping";

        public const string Queued = "queued";
        public const string Matched = "matched";
        public const string GameStart = "game_start";
        public const string TurnTimerStart = "turn_timer_start";
        public const string Waiting = "waiting";
        public const string Snapshot = "snapshot";
        public const string GameOver = "game_over";
        public const string OpponentDisconnected = "opponent_disconnected";
        public const string Error = "error";
        public const string Pong = "pong";
    }

    public class ClientMessage
    {
        public string Type { get; set; } = "";
    }

    public class DeclareMessage
    {
        public string SkillName { get; set; } = "";
        public int Param { get; set; }
    }
}
