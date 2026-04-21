namespace Core.Systems
{
    public enum RoundState
    {
        Initialising,   // Before the round begins — setup phase
        Active,         // Round is running, timer is counting down
        Overtime,       // Timer expired while a defuse is in progress — waiting for resolution
        Ended           // Round is over, outcome has been decided
    }
}
