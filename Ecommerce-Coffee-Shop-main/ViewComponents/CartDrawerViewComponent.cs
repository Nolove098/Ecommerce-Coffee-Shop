using Microsoft.AspNetCore.Mvc;

namespace SaleStore.ViewComponents;

public class CartDrawerViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}
