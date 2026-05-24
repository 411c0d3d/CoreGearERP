namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Represents a void return type for commands that do not return a value.
/// Avoids having two separate handler interfaces for void and non-void commands.
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = new();
}