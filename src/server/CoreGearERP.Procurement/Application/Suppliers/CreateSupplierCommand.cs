using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Procurement.Application.Suppliers;

/// <summary>
/// Command to create a new supplier.
/// </summary>
public record CreateSupplierCommand(
    string Code,
    string Name,
    string ContactEmail,
    string ContactPhone = "",
    string Address = "") : ICommand<Guid>;