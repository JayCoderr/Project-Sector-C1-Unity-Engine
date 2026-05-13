[System.Flags]
public enum DamageCause
{
    Player = 1,
    Bot = 2,
    Explosion = 4,
    FallDamage = 8,
    Self = 16,
    Map = 32,
    Fire = 64,
    Vehicle = 128,
    Collision = 256,
    GameMode = 512,
    Weapon = 1024,
}