using System;

/// <summary>
/// Bit-flags for classifying collision messages.
/// </summary>
[Flags]
public enum CollisionFlags : byte
{
    None = 0,        // no special action
    Damage = 1 << 0, // apply damage
    Buff = 1 << 1,   // grant a buff
    Pickup = 1 << 2,
}