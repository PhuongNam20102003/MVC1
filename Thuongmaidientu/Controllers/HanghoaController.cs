using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Thuongmaidientu.Data;
using Thuongmaidientu.ViewModels;

namespace Thuongmaidientu.Controllers
{
    public class HanghoaController : Controller
    {
        private readonly MyeStoreContext _db;

        public HanghoaController(MyeStoreContext context)
        {
            _db = context;
        }

        // Hiển thị danh sách sản phẩm
        public IActionResult Index(int? loai)
        {
            var hangHoas = _db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .AsQueryable();

            if (loai.HasValue)
            {
                hangHoas = hangHoas.Where(p => p.MaLoai == loai.Value);
            }
    
            var result = hangHoas.Select(p => new HanghoaVM
            {
                MaHh = p.MaHh,
                TenHh = p.TenHh,
                DonGia = p.DonGia ?? 0,
                Hinh = p.Hinh ?? string.Empty,
                MoTa = p.MoTaDonVi ?? string.Empty,
                TenLoai = p.MaLoaiNavigation.TenLoai,
                Slug = ToSlug(p.TenHh)  // ✅ Tạo slug tại đây
            }).ToList();

            return View(result);
        }

        // Chi tiết sản phẩm (SEO friendly)
        public IActionResult Detail(int id, string? slug)
        {
            var data = _db.HangHoas
                .Include(p => p.MaLoaiNavigation)
                .SingleOrDefault(p => p.MaHh == id);

            if (data == null)
            {
                TempData["Message"] = $"Không tìm thấy sản phẩm có ID {id}";
                return Redirect("/404");
            }

            var result = new ChiTietHanghoaVM
            {
                MaHh = data.MaHh,
                TenHh = data.TenHh,
                DonGia = data.DonGia ?? 0,
                ChiTiet = data.MoTa ?? string.Empty,
                MoTa = data.MoTaDonVi ?? string.Empty,
                DiemDanhGia = 5,
                Hinh = data.Hinh ?? string.Empty,
                TenLoai = data.MaLoaiNavigation?.TenLoai ?? string.Empty,
                SoLuongTon = 10
            };

            return View(result);
        }

        // Hàm tạo slug cho SEO
        public static string ToSlug(string input)
        {
            return input.ToLower()
                        .Replace(" ", "-")
                        .Replace(",", "")
                        .Replace(".", "")
                        .Replace(":", "")
                        .Replace(";", "");
        }
    }
}
