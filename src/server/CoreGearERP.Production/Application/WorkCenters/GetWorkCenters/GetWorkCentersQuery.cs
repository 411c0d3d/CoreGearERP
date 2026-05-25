using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Production.Application.WorkCenters.GetWorkCenters;

/// <summary>
/// Query to retrieve all active work centers for the current tenant.
/// </summary>
public record GetWorkCentersQuery : IQuery<IReadOnlyList<WorkCenterDto>>;

/// <summary>
/// Read model for work center display.
/// </summary>
public record WorkCenterDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    decimal CapacityPerHour,
    string Status);