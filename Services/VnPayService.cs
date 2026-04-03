using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SaleStore.Models;
using Microsoft.AspNetCore.Http;

namespace SaleStore.Services;

public class VnPayService : IVnPayService
{
    private readonly IConfiguration _config;

    public VnPayService(IConfiguration config)
    {
        _config = config;
    }

    public string CreatePaymentUrl(HttpContext context, Order order)
    {
        var vnp_TmnCode = _config["VNPAY:TmnCode"];
        var vnp_HashSecret = _config["VNPAY:HashSecret"];
        var vnp_Url = _config["VNPAY:BaseUrl"];
        var vnp_Returnurl = _config["VNPAY:ReturnUrl"];

        VnPayLibrary vnpay = new VnPayLibrary();

        vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
        vnpay.AddRequestData("vnp_Amount", ((long)(order.TotalAmount * 100)).ToString()); 
        vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + order.Id);
        vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
        vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
        vnpay.AddRequestData("vnp_TxnRef", order.Id.ToString());

        string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

        return paymentUrl;
    }

    public PaymentResponseModel PaymentExecute(IQueryCollection collections)
    {
        var vnpay = new VnPayLibrary();
        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(key, value.ToString());
            }
        }

        var vnp_orderId = vnpay.GetResponseData("vnp_TxnRef");
        var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
        var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        var vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
        var vnp_AmountRaw = vnpay.GetResponseData("vnp_Amount");
        var vnp_SecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;

        bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _config["VNPAY:HashSecret"]);
        
        decimal amount = 0;
        if (decimal.TryParse(vnp_AmountRaw, out var amt)) amount = amt / 100;

        return new PaymentResponseModel
        {
            Success = checkSignature && vnp_ResponseCode == "00" && vnp_TransactionStatus == "00",
            PaymentMethod = "VNPAY",
            TransactionId = vnp_TransactionId,
            OrderId = vnp_orderId,
            VnPayResponseCode = vnp_ResponseCode,
            TransactionStatus = vnp_TransactionStatus,
            Amount = amount
        };
    }
}

// Helper Class
public class VnPayLibrary
{
    public const string VERSION = "2.1.0";
    private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value)) { _requestData.Add(key, value); }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value)) { _responseData.Add(key, value); }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var retValue) ? retValue : "";
    }

    public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
    {
        StringBuilder data = new StringBuilder();
        foreach (KeyValuePair<string, string> kv in _requestData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }
        string queryString = data.ToString();

        baseUrl += "?" + queryString;
        string signData = queryString;
        if (signData.Length > 0)
        {
            signData = signData.Remove(signData.Length - 1);
        }

        string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);
        baseUrl += "vnp_SecureHash=" + vnp_SecureHash;

        return baseUrl;
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        string rspRaw = GetResponseRaw();
        string myChecksum = Utils.HmacSHA512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseRaw()
    {
        StringBuilder data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }
        if (_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }
        foreach (KeyValuePair<string, string> kv in _responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }
        //remove last '&'
        if (data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }
        return data.ToString();
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}

public class Utils
{
    public static string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            byte[] hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }
        return hash.ToString();
    }

    public static string GetIpAddress(HttpContext context)
    {
        var ipAddress = string.Empty;
        try {
            ipAddress = context.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1") {
                ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress)) {
                ipAddress = context.Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            }
        } catch { ipAddress = "127.0.0.1"; }
        return ipAddress ?? "127.0.0.1";
    }
}
