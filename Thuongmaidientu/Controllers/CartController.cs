using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Thuongmadientu.Helpers;
using Thuongmaidientu.Data;
using Thuongmaidientu.Helpers;
using Thuongmaidientu.Services;
using Thuongmaidientu.ViewModels;

namespace Thuongmaidientu.Controllers
{
    public class CartController : Controller
    {
        private readonly PaypalClient _paypalClient;
        private readonly MyeStoreContext db;
        private readonly IVnPayService _vnPayservice;

        public CartController(MyeStoreContext context, PaypalClient paypalClient, IVnPayService vnPayservice)
        {
            _paypalClient = paypalClient;
            db = context;
            _vnPayservice = vnPayservice;
        }

        public List<CartItem> Cart => HttpContext.Session.Get<List<CartItem>>(MySetting.CART_KEY) ?? new List<CartItem>();

        public IActionResult Index()
        {
            return View(Cart);
        }

        public IActionResult AddToCart(int id, int quantity = 1)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item == null)
            {
                var hangHoa = db.HangHoas.SingleOrDefault(p => p.MaHh == id);
                if (hangHoa == null)
                {
                    TempData["Message"] = $"Không tìm thấy hàng hóa có mã {id}";
                    return Redirect("/404");
                }
                item = new CartItem
                {
                    MaHh = hangHoa.MaHh,
                    TenHh = hangHoa.TenHh,
                    DonGia = hangHoa.DonGia ?? 0,
                    Hinh = hangHoa.Hinh ?? string.Empty,
                    SoLuong = quantity
                };
                gioHang.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }

            HttpContext.Session.Set(MySetting.CART_KEY, gioHang);

            return RedirectToAction("Index");
        }

        public IActionResult RemoveCart(int id)
        {
            var gioHang = Cart;
            var item = gioHang.SingleOrDefault(p => p.MaHh == id);
            if (item != null)
            {
                gioHang.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, gioHang);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            if (Cart.Count == 0)
            {
                return Redirect("/");
            }

            ViewBag.PaypalClientdId = _paypalClient.ClientId;
            return View(Cart);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Checkout(CheckoutVM model, string payment = "COD")
        {
            if (ModelState.IsValid)
            {
                if (payment == "Thanh toán VNPay")
                {
                    var vnPayModel = new VnPaymentRequestModel
                    {
                        Amount = Cart.Sum(p => p.TongTien),
                        CreatedDate = DateTime.Now,
                        Description = $"{model.HoTen} {model.DienThoai}",
                        FullName = model.HoTen,
                        OrderId = new Random().Next(1000, 100000)
                    };
                    return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
                }

                var customerId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID).Value;
                var khachHang = new KhachHang();
                if (model.GiongKhachHang) // nếu tick checkbox
                {

                    var dbCustomer = db.KhachHangs.SingleOrDefault(kh => kh.MaKh == customerId);
                    if (dbCustomer == null)
                    {
                        TempData["Message"] = "Không tìm thấy thông tin khách hàng!";
                        return RedirectToAction("Checkout");
                    }

                    model.HoTen = dbCustomer.HoTen;
                    model.DiaChi = dbCustomer.DiaChi;
                    model.DienThoai = dbCustomer.DienThoai;
                }

                // Lấy ID seller ngẫu nhiên từ NhanViens
                string sellerId = db.NhanViens
                    .OrderBy(r => Guid.NewGuid())
                    .Select(nv => nv.MaNv) // nv.MaNv là string
                    .FirstOrDefault();

                var khach = db.KhachHangs.FirstOrDefault(k => k.MaKh == customerId);

                var hoadon = new HoaDon
                {
                    MaKh = khach.MaKh,
                    HoTen = khach.HoTen, // lưu tên khách
                    DiaChi = model.DiaChi,
                    DienThoai = model.DienThoai,
                    NgayDat = DateTime.Now,
                    CachThanhToan = payment,
                    CachVanChuyen = "GRAB",
                    MaTrangThai = 0,
                    GhiChu = model.GhiChu,
                    MaNv = sellerId,
                    MaKhNavigation = khach // gán navigation
                };


                db.Database.BeginTransaction();
                try
                {
                    db.Add(hoadon);
                    db.SaveChanges();

                    var cthds = new List<ChiTietHd>();
                    foreach (var item in Cart)
                    {
                        cthds.Add(new ChiTietHd
                        {
                            MaHd = hoadon.MaHd,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            MaHh = item.MaHh,
                            GiamGia = 0
                        });
                    }

                    db.AddRange(cthds);
                    db.SaveChanges();
                    db.Database.CommitTransaction();

                    // Xóa giỏ hàng sau khi thanh toán thành công
                    HttpContext.Session.Set<List<CartItem>>(MySetting.CART_KEY, new List<CartItem>());

                    return View("Success");
                }
                catch
                {
                    db.Database.RollbackTransaction();
                    TempData["Message"] = "Có lỗi xảy ra khi thanh toán!";
                    return RedirectToAction("Checkout");
                }
            }

            return View(Cart);
        }

        [Authorize]
        public IActionResult PaymentSuccess()
        {
            return View("Success");
        }

        #region Paypal payment
        [Authorize]
        [HttpPost("/Cart/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            // Thông tin đơn hàng gửi qua Paypal
            var tongTien = Cart.Sum(p => p.TongTien).ToString();
            var donViTienTe = "USD";
            var maDonHangThamChieu = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                var response = await _paypalClient.CreateOrder(tongTien, donViTienTe, maDonHangThamChieu);

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [Authorize]
        [HttpPost("/Cart/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);

                // Lưu database đơn hàng của mình

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        #endregion

        [Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }

        [Authorize]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }


            // Lưu đơn hàng vô database

            TempData["Message"] = $"Thanh toán VNPay thành công";
            return RedirectToAction("PaymentSuccess");
        }

        [HttpGet]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = Cart;
            var item = cart.SingleOrDefault(x => x.MaHh == id);
            if (item != null)
            {
                item.SoLuong = quantity < 1 ? 1 : quantity;
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
            }
            var subtotal = cart.Sum(p => p.TongTien);
            var total = subtotal + 30000;
            return Json(new { subtotal, total });
        }

        [HttpGet]
        public IActionResult RemoveCartAjax(int id)
        {
            var cart = Cart;
            var item = cart.SingleOrDefault(x => x.MaHh == id);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.Set(MySetting.CART_KEY, cart);
            }
            var subtotal = cart.Sum(p => p.TongTien);
            var total = subtotal + 30000;
            return Json(new { subtotal, total });
        }

        [Authorize] // Chỉ người đăng nhập mới xem được
        public IActionResult OrderHistory()
        {
            // Lấy user hiện tại
            var userId = HttpContext.User.Claims.SingleOrDefault(p => p.Type == MySetting.CLAIM_CUSTOMERID)?.Value;

            // Lấy thông tin nhân viên
            var nhanVien = db.NhanViens.SingleOrDefault(nv => nv.MaNv == userId);
            if (nhanVien == null)
            {
                return Redirect("/"); // không phải nhân viên (seller), quay về trang chính
            }

            // Lấy tất cả đơn hàng của seller hiện tại
            var orders = db.HoaDons
                .Where(h => h.MaNv == nhanVien.MaNv)
                .OrderByDescending(o => o.NgayDat)
                .ToList();

            return View(orders);
        }



    }

}