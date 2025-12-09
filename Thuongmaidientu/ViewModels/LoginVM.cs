using System.ComponentModel.DataAnnotations;

namespace Thuongmaidientu.ViewModels
{
    public class LoginVM
    {
        [Display(Name ="Tên Đăng Nhập") ]
        [Required(ErrorMessage="Chưa nhập têm đăng nhập")]
        [MaxLength(20, ErrorMessage = "Tối đa 20 kí tự")]
        public string UserName { get; set; }

        [Display(Name = "Mật Khẩu")]
        [Required(ErrorMessage = "Chưa nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
