using Microsoft.AspNetCore.Mvc;

namespace SaleStore.ViewComponents
{
    public class UserAvatarViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(string? userName = null, string? cssClass = "w-10 h-10")
        {
            var name = userName ?? User.Identity?.Name ?? "User";
            var viewModel = new UserAvatarViewModel { UserName = name, CssClass = cssClass };
            return View(viewModel);
        }
    }

    public class UserAvatarViewModel
    {
        public string? UserName { get; set; }
        public string? CssClass { get; set; }
    }
}
