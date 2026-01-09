using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Thuongmaidientu.Data;

[Authorize(Roles = "Seller")]
public class SellerController : Controller
{
    private readonly MyeStoreContext db;

    public SellerController(MyeStoreContext context)
    {
        db = context;
    }

    // Hiển thị toàn bộ hóa đơn
    public IActionResult OrderHistory()
    {
        var orders = db.HoaDons
    .Include(h => h.MaKhNavigation) // khách hàng
    .Include(h => h.ChiTietHds)    // chi tiết hóa đơn
        .ThenInclude(ct => ct.MaHhNavigation) // thông tin sản phẩm
    .OrderByDescending(h => h.NgayDat)
    .ToList();


        return View("OrderHistory", orders);
    }
}
