using SaleStore.Models;
using Microsoft.AspNetCore.Http;

namespace SaleStore.Services;

public interface IVnPayService
{
    string CreatePaymentUrl(HttpContext context, Order order);
    PaymentResponseModel PaymentExecute(IQueryCollection collections);
}

public class PaymentResponseModel
{
    public string OrderId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string VnPayResponseCode { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
