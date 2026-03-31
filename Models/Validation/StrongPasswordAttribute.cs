using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SaleStore.Models.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class StrongPasswordAttribute : ValidationAttribute
{
    public StrongPasswordAttribute()
        : base("Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ in hoa, chữ thường, số và ký tự đặc biệt.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not string password)
            return false;

        // Tối thiểu 8 ký tự, 1 chữ hoa, 1 chữ thường, 1 số, 1 ký tự đặc biệt
        return password.Length >= 8
            && Regex.IsMatch(password, @"[A-Z]")
            && Regex.IsMatch(password, @"[a-z]")
            && Regex.IsMatch(password, @"\d")
            && Regex.IsMatch(password, @"[^a-zA-Z\d]");
    }
}
