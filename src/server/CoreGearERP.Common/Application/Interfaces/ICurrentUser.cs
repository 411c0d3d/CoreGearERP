namespace CoreGearERP.Common.Application.Interfaces;

/// <summary>
/// Provides current authenticated user context throughout the request pipeline.
/// Resolved from JWT claims in the Host layer.
/// Used to populate audit columns on every entity mutation.
/// </summary>
public interface ICurrentUser
{
    Guid UserId { get; }
    string Email { get; }
}