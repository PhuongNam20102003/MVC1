using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Thuongmaidientu.Data;
using Thuongmaidientu.Helpers;
using Thuongmaidientu.ViewModels;

namespace Thuongmaidientu.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly MyeStoreContext db;
        private readonly IMapper _mapper;

        public KhachHangController(MyeStoreContext context, IMapper mapper)
        {
            db = context;
            _mapper = mapper;
        }

        #region Register
        [HttpGet]
        public IActionResult Dangky()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Dangky(RegisterVM model, IFormFile Hinh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var KhachHang = _mapper.Map<KhachHang>(model);
                    KhachHang.RandomKey = Util.GenerateRandomKey();
                    KhachHang.MatKhau = model.MatKhau.ToMd5Hash(KhachHang.RandomKey);
                    KhachHang.HieuLuc = true;
                    KhachHang.VaiTro = 0;

                    if (Hinh != null)
                    {
                        KhachHang.Hinh = Util.UploadHinh(Hinh, "KhachHang");
                    }

                    db.Add(KhachHang);
                    db.SaveChanges();

                    // 🔥 Thông báo đăng ký thành công
                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Hãy đăng nhập để tiếp tục.";

                    // Chuyển sang trang đăng nhập
                    return RedirectToAction("DangNhap", "KhachHang");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra, vui lòng thử lại.");
                }
            }

            return View(model);
        }
        #endregion



        #region Login 
        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(LoginVM model, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;

            if (ModelState.IsValid)
            {
                var KhachHang = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == model.UserName);

                if (KhachHang == null)
                {
                    ModelState.AddModelError("", "Không tồn tại tài khoản này.");
                }
                else
                {
                    if (!KhachHang.HieuLuc)
                    {
                        ModelState.AddModelError("", "Tài khoản đã bị khóa.");
                    }
                    else
                    {
                        // 🔥 Sửa LOGIC kiểm tra mật khẩu
                        if (KhachHang.MatKhau != model.Password.ToMd5Hash(KhachHang.RandomKey))
                        {
                            ModelState.AddModelError("", "Sai mật khẩu.");
                        }
                        else
                        {
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Email, KhachHang.Email),
                                new Claim(ClaimTypes.Name, KhachHang.HoTen),
                                new Claim(MySetting.CLAIM_CUSTOMERID, KhachHang.MaKh),
                                new Claim(ClaimTypes.Role, "Customer")
                            };

                            var claimsIdentity = new ClaimsIdentity(claims, "login");
                            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                            await HttpContext.SignInAsync(claimsPrincipal);

                            if (Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                return Redirect("/");
                            }
                        }
                    }
                }
            }

            return View(model);
        }
        #endregion



        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }
        [HttpPost]
        public IActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var khachHang = new KhachHang
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    MatKhau = model.MatKhau, // nhớ hash password nếu muốn bảo mật
                };

                db.KhachHangs.Add(khachHang);
                db.SaveChanges();

                TempData["Message"] = "Đăng ký thành công!";
                return RedirectToAction("Login");
            }

            return View(model);
        }

    }
}