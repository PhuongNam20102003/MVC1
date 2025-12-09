namespace Thuongmaidientu.ViewModels
{
    public class HanghoaVM
    {
        public int MaHh {  get; set; }
        public string TenHh { get; set; }
        public string Hinh { get; set; }
        public double DonGia {  get; set; }
        public string MoTa {  get; set; }
        public string TenLoai { get; set; }
        // ✅ Thêm thuộc tính Slug để SEO-friendly URL
        public string Slug { get; set; } = string.Empty;
    }
    public class ChiTietHanghoaVM
    {
        public int MaHh { get; set; }
        public string TenHh { get; set; }
        public string Hinh { get; set; }
        public double DonGia { get; set; }
        public string MoTa { get; set; }
        public string TenLoai { get; set; }
        public string ChiTiet { get; set; }
        public int DiemDanhGia { get; set; }
        public int SoLuongTon { get; set; }
    }
}
