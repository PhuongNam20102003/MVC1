using AutoMapper;
using Thuongmaidientu.Data;
using Thuongmaidientu.ViewModels;
namespace Thuongmaidientu.Helpers
{
    public class AutoMapperProFile : Profile
    {
        public AutoMapperProFile()
        {
            CreateMap<RegisterVM, KhachHang>();
                //.ForMember(kh => kh.HoTen, option => option.MapFrom(RegisterVM => RegisterVM.HoTen))
                //.ReverseMap();
        }
    }
}
