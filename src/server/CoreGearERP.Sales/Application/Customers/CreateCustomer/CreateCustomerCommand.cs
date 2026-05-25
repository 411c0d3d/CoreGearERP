using CoreGearERP.Common.Application.Interfaces;

namespace CoreGearERP.Sales.Application.Customers.CreateCustomer;

/// <summary>
/// Command to create a new customer.
/// </summary>
public record CreateCustomerCommand(
    string Code,
    string Name,
    string ContactEmail,
    string ContactPhone = "",
    string Address = "") : ICommand<Guid>;