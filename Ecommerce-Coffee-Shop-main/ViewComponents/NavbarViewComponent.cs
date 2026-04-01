using Microsoft.AspNetCore.Mvc;
using SaleStore.Models.ViewModels;

namespace SaleStore.ViewComponents;

public class NavbarViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var model = new NavbarViewModel
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            UserName = User.Identity?.Name ?? string.Empty,
            IsAdmin = User.IsInRole(Models.AppRoles.Admin),
            IsStaff = User.IsInRole(Models.AppRoles.Staff)
        };

        return View(model);
    }
}
