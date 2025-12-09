using Microsoft.AspNetCore.Mvc;
using Thuongmaidientu.Data;
using Thuongmaidientu.ViewModels;

namespace Thuongmaidientu.ViewComponents
{
    public class MenuLoaiViewComponent : ViewComponent 
    {
        private readonly MyeStoreContext db;
        public MenuLoaiViewComponent(MyeStoreContext context) => db = context;

        public IViewComponentResult Invoke()
        {
            var data = db.Loais.Select(lo => new MenuLoaiVM
            {
                MaLoai = lo.MaLoai,
                TenLoai = lo.TenLoai,
                SoLuong = lo.HangHoas.Count
            }).OrderBy(p => p.TenLoai);

            return View(data);
        }
    }
}
