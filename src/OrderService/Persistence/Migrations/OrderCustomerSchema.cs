using System.ComponentModel.DataAnnotations;

namespace OrderService.Persistence.Migrations;

public sealed class OrderCustomerSchema
{
    [StringLength(50)]
    public string Email { get; set; } = string.Empty;

    [StringLength(80)]
    public string DisplayName { get; set; } = string.Empty;
}
