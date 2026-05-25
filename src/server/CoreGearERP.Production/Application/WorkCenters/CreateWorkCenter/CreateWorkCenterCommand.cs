using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Production.Application.WorkCenters.CreateWorkCenter;

/// <summary>
/// Command to create a new work center.
/// </summary>
public record CreateWorkCenterCommand(
    string Code,
    string Name,
    decimal CapacityPerHour,
    string Description = "") : ICommand<Guid>;