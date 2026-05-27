namespace CoreGearERP.Common.Domain.ValueObjects;

/// <summary>
/// Measurable quantity with unit of measure. Always two fields together, never a bare decimal.
/// Immutable -- create a new instance to change a value.
/// </summary>
public sealed class Quantity : IEquatable<Quantity>
{
    public decimal Value { get; }

    /// <summary>Unit of measure code. KG, PCS, LTR, MTR.</summary>
    public string UnitCode { get; }

    /// <summary>
    /// Required by EF Core for owned type materialization. Not for application use.
    /// </summary>
    private Quantity()
    {
        UnitCode = string.Empty;
    }

    /// <summary>
    /// Constructor. Validates that value is non-negative and unit code is not empty.
    /// </summary>
    /// <param name="value">Quantity value. Must be non-negative.</param>
    /// <param name="unitCode">Unit of measure code. Must be a non-empty string like "KG", "PCS", "LTR".</param>
    public Quantity(decimal value, string unitCode)
    {
        if (value < 0)
        {
            throw new ArgumentException("Quantity value cannot be negative.", nameof(value));
        }

        if (string.IsNullOrWhiteSpace(unitCode))
        {
            throw new ArgumentException("Unit code cannot be empty.", nameof(unitCode));
        }

        Value = Math.Round(value, 4);
        UnitCode = unitCode.ToUpperInvariant();
    }

    /// <summary>Adds two quantities. Units must match.</summary>
    public Quantity Add(Quantity other)
    {
        GuardSameUnit(other);
        return new Quantity(Value + other.Value, UnitCode);
    }

    /// <summary>Subtracts two quantities. Units must match.</summary>
    public Quantity Subtract(Quantity other)
    {
        GuardSameUnit(other);
        return new Quantity(Value - other.Value, UnitCode);
    }

    /// <summary>Returns true if this quantity is sufficient to cover the required amount.</summary>
    public bool IsSufficientFor(Quantity required)
    {
        GuardSameUnit(required);
        return Value >= required.Value;
    }

    /// <summary>Returns true if this quantity is greater than the other.</summary>
    public bool IsGreaterThan(Quantity other)
    {
        GuardSameUnit(other);
        return Value > other.Value;
    }

    /// <summary>
    /// Returns a zero quantity for the given unit. Useful for initializing accumulators.
    /// </summary>
    public static Quantity Zero(string unitCode) => new(0, unitCode);

    public bool Equals(Quantity? other)
    {
        if (other is null)
        {
            return false;
        }

        return Value == other.Value && UnitCode == other.UnitCode;
    }

    public override bool Equals(object? obj) => Equals(obj as Quantity);
    public override int GetHashCode() => HashCode.Combine(Value, UnitCode);
    public override string ToString() => $"{Value:F4} {UnitCode}";

    private void GuardSameUnit(Quantity other)
    {
        if (UnitCode != other.UnitCode)
        {
            throw new InvalidOperationException(
                $"Cannot operate on different units: {UnitCode} and {other.UnitCode}.");
        }
    }
}