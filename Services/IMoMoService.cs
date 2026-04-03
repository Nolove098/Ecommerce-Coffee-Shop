using SaleStore.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SaleStore.Services;

public interface IMoMoService
{
    Task<MoMoPaymentResponseModel> CreatePaymentUrl(Order order);
    MoMoExecuteResponseModel PaymentExecute(IQueryCollection collections);
}

public class MoMoPaymentResponseModel
{
    public string PartnerCode { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public long ResponseTime { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string PayUrl { get; set; } = string.Empty;
    public string Deeplink { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
}

public class MoMoExecuteResponseModel
{
    public string OrderId { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string OrderInfo { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string TransId { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
}
