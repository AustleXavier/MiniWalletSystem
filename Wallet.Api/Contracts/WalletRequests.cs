using System.ComponentModel.DataAnnotations;
namespace Wallet.Api;

public sealed class CreateWalletRequest
{

    [Required, StringLength(200)]
    public string Name { get; init; } = "";
    [Required, EmailAddress, StringLength(320)]
    public string Email { get; init; } = "";
    [Required, StringLength(30)]
    public string MobileNumber { get; init; } = "";
    [Range(typeof(decimal), "0", "9999999999999999")]
    public decimal InitialBalance { get; init; }
}
public sealed class MoneyRequest
{
    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Amount { get; init; }
    [Required, StringLength(100)]
    public string ReferenceId { get; init; } = "";
}
public sealed class TransferRequest
{
    [Required]
    public Guid FromWalletId { get; init; }
    [Required] public Guid ToWalletId { get; init; }
    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Amount { get; init; }
    [Required, StringLength(100)]
    public string ReferenceId { get; init; } = "";
}
