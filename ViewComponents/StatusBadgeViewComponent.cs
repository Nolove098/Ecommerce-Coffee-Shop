using Microsoft.AspNetCore.Mvc;
using SaleStore.Models;

namespace SaleStore.ViewComponents
{
    public class StatusBadgeViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(OrderStatus status)
        {
            return View(status);
        }
    }
}
