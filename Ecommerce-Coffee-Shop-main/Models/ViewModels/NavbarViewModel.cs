namespace SaleStore.Models.ViewModels;

public class NavbarViewModel
{
    public bool IsAuthenticated { get; set; }
    public string UserName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsStaff { get; set; }
}
