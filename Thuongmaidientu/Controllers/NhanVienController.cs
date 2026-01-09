using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Thuongmaidientu.Data;
using Thuongmaidientu.ViewModels;
using Thuongmaidientu.Helpers;
using System.Threading.Tasks;
using System.Linq;

public class NhanVienController : Controller
{
    private readonly MyeStoreContext db;

    public NhanVienController(MyeStoreContext context)
    {
        db = context;
    }

    // GET: Trang đăng nhập nhân viên
    [HttpGet]
    public IActionResult DangNhap()
    {
        return View();
    }

    // POST: Xử lý đăng nhập nhân viên
    [HttpPost]
    public async Task<IActionResult> DangNhap(LoginVM model)
    {
        if (!ModelState.IsValid)
            return View(model);

        string userName = model.UserName.Trim();
        string password = model.Password.Trim();

        // Tìm nhân viên theo MaNv, ignore case (SQL Server CI collation sẽ tự ignore case)
        var nv = db.NhanViens
                   .FirstOrDefault(n => n.MaNv == userName);

        if (nv == null)
        {
            ModelState.AddModelError("", "Tài khoản nhân viên không tồn tại.");
            return View(model);
        }

        // Kiểm tra mật khẩu
        if (string.IsNullOrEmpty(nv.MatKhau) || nv.MatKhau.Trim() != password)
        {
            ModelState.AddModelError("", "Sai mật khẩu.");
            return View(model);
        }

        // Tạo claim
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, nv.HoTen),
        new Claim(MySetting.CLAIM_CUSTOMERID, nv.MaNv),
        new Claim(ClaimTypes.Role, "Seller") // phân quyền nhân viên
    };

        var identity = new ClaimsIdentity(claims, "login");
        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

        // Chuyển sang trang lịch sử đơn hàng
        return RedirectToAction("OrderHistory", "Seller");
    }




    // GET: Trang profile nhân viên
    [Authorize(Roles = "Seller")]
    public IActionResult Profile()
    {
        var nvId = HttpContext.User.Claims
            .SingleOrDefault(c => c.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

        if (string.IsNullOrEmpty(nvId))
            return RedirectToAction("DangNhap");

        var nv = db.NhanViens
                   .FirstOrDefault(n => n.MaNv.Trim().Equals(nvId.Trim(), System.StringComparison.OrdinalIgnoreCase));

        if (nv == null)
            return RedirectToAction("DangNhap");

        return View(nv);
    }

    // GET: Logout nhân viên
    public async Task<IActionResult> DangXuat()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("DangNhap");
    }
}