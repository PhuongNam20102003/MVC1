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
            .Include(h => h.MaKhNavigation)          // Load khách hàng
            .Include(h => h.ChiTietHds)              // Load chi tiết hóa đơn
                .ThenInclude(ct => ct.MaHhNavigation) // Load tên sản phẩm
            .OrderByDescending(h => h.NgayDat)
            .ToList();

        return View(orders);
    }
}
