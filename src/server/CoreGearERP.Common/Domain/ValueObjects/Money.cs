namespace CoreGearERP.Common.Domain.ValueObjects;

/// <summary>
/// Monetary amount with currency. Always two fields together, never a bare decimal.
/// Immutable -- create a new instance to change a value.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }

    /// <summary>
    /// ISO 4217 currency code. EUR, USD, GBP.
    /// </summary>
    public string CurrencyCode { get; }

    /// <summary>
    /// Parameterless constructor for EF Core. Initializes to zero amount and empty currency code, which should be overridden by the factory method or with property setters in the entity constructor.
    /// </summary>
    public Money()
    {
            Amount = 0;
            CurrencyCode = string.Empty;
    }
    
    /// <summary>
    /// Constructor. Validates that amount is non-negative and currency code is a valid 3-letter ISO code.
    /// </summary>
    /// <param name="amount">Monetary amount. Must be non-negative.</param>
    /// <param name="currencyCode">ISO 4217 currency code. Must be a valid 3-letter code.</param>
    public Money(decimal amount, string currencyCode)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
        {
            throw new ArgumentException("Currency code must be a valid 3-character ISO 4217 code.",
                nameof(currencyCode));
        }

        Amount = Math.Round(amount, 4);
        CurrencyCode = currencyCode.ToUpperInvariant();
    }

    /// <summary>
    /// Adds two Money values. Currencies must match.
    /// </summary>
    public Money Add(Money other)
    {
        GuardSameCurrency(other);
        return new Money(Amount + other.Amount, CurrencyCode);
    }

    /// <summary>
    /// Subtracts two Money values. Currencies must match.
    /// </summary>
    public Money Subtract(Money other)
    {
        GuardSameCurrency(other);
        return new Money(Amount - other.Amount, CurrencyCode);
    }

    /// <summary>
    /// Multiplies amount by a scalar. Used for line total calculations.
    /// </summary>
    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, CurrencyCode);
    }

    /// <summary>
    /// Returns the negative value of this Money instance.
    /// </summary>
    public Money Negate()
    {
        return new Money(Amount * -1, CurrencyCode);
    }

    /// <summary>
    /// Factory method for zero amount in a given currency. Useful for initialization and defaults.
    /// </summary>
    /// <param name="currencyCode">ISO 4217 currency code for the zero amount.</param>
    public static Money Zero(string currencyCode) => new(0, currencyCode);

    public bool Equals(Money? other)
    {
        if (other is null)
        {
            return false;
        }

        return Amount == other.Amount && CurrencyCode == other.CurrencyCode;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);
    public override int GetHashCode() => HashCode.Combine(Amount, CurrencyCode);
    public override string ToString() => $"{Amount:F2} {CurrencyCode}";

    private void GuardSameCurrency(Money other)
    {
        if (CurrencyCode != other.CurrencyCode)
        {
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {CurrencyCode} and {other.CurrencyCode}.");
        }
    }
}