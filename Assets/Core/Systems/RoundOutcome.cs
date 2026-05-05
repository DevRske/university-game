namespace Core.Systems
{
    public enum RoundOutcome
    {
        None,           // Round has not ended yet
        DefendersWin,   // Timer expired with no defuse, or defuse was cancelled after timer expired
        AttackersWin    // Reserved for future use (e.g. bomb defused successfully)
    }
}
