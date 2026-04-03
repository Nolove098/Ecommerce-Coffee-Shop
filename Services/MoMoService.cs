using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using SaleStore.Models;

namespace SaleStore.Services;

public class MoMoService : IMoMoService
{
    private readonly IConfiguration _config;

    public MoMoService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<MoMoPaymentResponseModel> CreatePaymentUrl(Order order)
    {
        var partnerCode = _config["Momo:PartnerCode"] ?? "";
        var requestId = Guid.NewGuid().ToString();
        var orderId = order.Id.ToString() + "_" + DateTime.Now.Ticks.ToString();
        var orderInfo = "Thanh toán cho cửa hàng Coffee Shop: " + order.Id;
        var redirectUrl = _config["Momo:ReturnUrl"] ?? "";
        var ipnUrl = _config["Momo:NotifyUrl"] ?? "";
        var amount = ((long)order.TotalAmount).ToString();
        var extraData = "";
        var requestType = "captureWallet";

        // Signature: accessKey=$accessKey&amount=$amount&extraData=$extraData&ipnUrl=$ipnUrl&orderId=$orderId&orderInfo=$orderInfo&partnerCode=$partnerCode&redirectUrl=$redirectUrl&requestId=$requestId&requestType=$requestType
        string rawHash = "accessKey=" + _config["Momo:AccessKey"] +
            "&amount=" + amount +
            "&extraData=" + extraData +
            "&ipnUrl=" + ipnUrl +
            "&orderId=" + orderId +
            "&orderInfo=" + orderInfo +
            "&partnerCode=" + partnerCode +
            "&redirectUrl=" + redirectUrl +
            "&requestId=" + requestId +
            "&requestType=" + requestType;

        var signature = HmacSHA256(rawHash, _config["Momo:SecretKey"] ?? "");

        var requestData = new
        {
            partnerCode,
            partnerName = _config["Momo:PartnerName"] ?? "CoffeeShop",
            storeId = "CoffeeShop",
            requestId,
            amount = long.Parse(amount),
            orderId,
            orderInfo,
            redirectUrl,
            ipnUrl,
            lang = "vi",
            extraData,
            requestType,
            signature
        };

        using (var client = new HttpClient())
        {
            var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_config["Momo:Endpoint"], content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<MoMoPaymentResponseModel>(responseContent) ?? new MoMoPaymentResponseModel();
        }
    }

    public MoMoExecuteResponseModel PaymentExecute(IQueryCollection collections)
    {
        var amount = collections.First(s => s.Key == "amount").Value.ToString();
        var orderInfo = collections.First(s => s.Key == "orderInfo").Value.ToString();
        var orderId = collections.First(s => s.Key == "orderId").Value.ToString();
        var resultCode = int.Parse(collections.First(s => s.Key == "resultCode").Value.ToString());
        var message = collections.First(s => s.Key == "message").Value.ToString();
        var transId = collections.First(s => s.Key == "transId").Value.ToString();

        return new MoMoExecuteResponseModel()
        {
            Amount = amount,
            OrderId = orderId,
            OrderInfo = orderInfo,
            ResultCode = resultCode,
            Message = message,
            TransId = transId
        };
    }

    private string HmacSHA256(string inputData, string key)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA256(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        }
    }
}
