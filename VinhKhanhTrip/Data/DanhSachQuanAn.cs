using System.Collections.Generic;
using VinhKhanhTrip.Models;

namespace VinhKhanhTrip.Data
{
    public static class DanhSachQuanAn
    {
        public static List<QuanAn> dsQuan = new List<QuanAn>
        {
            new QuanAn { Ten = "Ốc Oanh", Loai = "Oc", MoTa = "Chuyên các món ốc hương hoàng kim và hải sản tươi ngon bậc nhất Quận 4.", Lat = 10.762800, Lng = 106.701900,
                DiaChi = "534 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "093 111 2222", Email = "ocoanh534@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1599084993091-1cb5c0721cc6?auto=format&fit=crop&w=800&q=80",
                Rating = 4.8, OpeningHours = "16:00 - 23:30", PriceRange = "125K - 300K VND",
                MenuItems = new List<string>{"Ốc hương trứng muối", "Càng ghẹ rang muối", "Nghêu hấp xả", "Sò lông mỡ hành"} },

            new QuanAn { Ten = "Ốc Vũ", Loai = "Oc", MoTa = "Không gian rộng rãi, thoáng mát, hải sản bình dân phù hợp sinh viên.", Lat = 10.762920, Lng = 106.701950,
                DiaChi = "37 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "090 333 4444", Email = "ocvu.vinhkhanh@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1615599066661-d00db6e35591?auto=format&fit=crop&w=800&q=80",
                Rating = 4.5, OpeningHours = "15:00 - 23:00", PriceRange = "80K - 200K VND",
                MenuItems = new List<string>{"Lẩu hải sản", "Hàu nướng phô mai", "Tôm tít rang me"} },

            new QuanAn { Ten = "Ốc Thảo", Loai = "Oc", MoTa = "Nổi tiếng with nước sốt xào me chua ngọt và nướng mỡ hành đậm đà.", Lat = 10.763040, Lng = 106.702000,
                DiaChi = "383 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "098 765 4321", Email = "octhao383@yahoo.com",
                ImageUrl = "https://images.unsplash.com/photo-1577717903610-184cf0b5368a?auto=format&fit=crop&w=800&q=80",
                Rating = 4.6, OpeningHours = "16:00 - 23:00", PriceRange = "100K - 250K VND",
                MenuItems = new List<string>{"Ốc móng tay rau muống", "Ốc mỡ xào me", "Sò điệp nướng mọi"} },

            new QuanAn { Ten = "Lẩu Bò Khu Ba", Loai = "Lau", MoTa = "Quán lẩu bò truyền thống lâu đời với nước dùng thanh ngọt hầm từ xương 10 tiếng.", Lat = 10.761500, Lng = 106.702500,
                DiaChi = "181 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "091 234 5678", Email = "laubokhuba@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1605296830714-7caca9216091?auto=format&fit=crop&w=800&q=80",
                Rating = 4.7, OpeningHours = "11:00 - 22:00", PriceRange = "150K - 350K VND",
                MenuItems = new List<string>{"Lẩu bò thập cẩm", "Bò nhúng giấm", "Bò nướng ngói"} },

            new QuanAn { Ten = "Lẩu Bò Tí Chuột", Loai = "Lau", MoTa = "Thịt bò tươi mềm, nướng hay nấu lẩu đều xuất sắc. Chỗ ngồi rộng rãi.", Lat = 10.761530, Lng = 106.702520,
                DiaChi = "1/2 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "094 567 8901", Email = "tichuot.laubo@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1549488344-1f9b8d2bd1f3?auto=format&fit=crop&w=800&q=80",
                Rating = 4.6, OpeningHours = "11:00 - 22:30", PriceRange = "150K - 350K VND",
                MenuItems = new List<string>{"Lẩu đuôi bò", "Thịt bò nướng tảng", "Bò xào lăn"} },

            new QuanAn { Ten = "Bánh Tráng Đà Lạt", Loai = "Khac", MoTa = "Bánh tráng nướng Đà Lạt full topping giòn rụm, vừa thổi vừa ăn.", Lat = 10.760000, Lng = 106.703500,
                DiaChi = "200 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "092 345 6789", Email = "banhtrangnuong200@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1625938146369-adc83368bda2?auto=format&fit=crop&w=800&q=80",
                Rating = 4.8, OpeningHours = "16:00 - 22:30", PriceRange = "30K - 60K VND",
                MenuItems = new List<string>{"Bánh tráng nướng thập cẩm", "Bánh tráng phô mai bơ", "Bánh tráng hải sản"} },

            new QuanAn { Ten = "Trà Dâu Cô Giang", Loai = "Khac", MoTa = "Nước uống giải khát chua ngọt, thanh mát, cực hợp để ăn kèm đồ nướng.", Lat = 10.759970, Lng = 106.703520,
                DiaChi = "202 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "096 789 0123", Email = "tradaucogiang@yahoo.com",
                ImageUrl = "https://images.unsplash.com/photo-1534353473418-4cfa6c56fd38?auto=format&fit=crop&w=800&q=80",
                Rating = 4.7, OpeningHours = "16:00 - 22:30", PriceRange = "20K - 35K VND",
                MenuItems = new List<string>{"Trà dâu tây tươi", "Trà dâu tắc", "Nước ép thơm", "Dừa tắc"} },

            new QuanAn { Ten = "Phá Lấu Dì Nũi", Loai = "Khac", MoTa = "Phá lấu nước cốt dừa thơm béo, ăn kèm bánh mì nóng giòn cực kỳ bắt miệng.", Lat = 10.763500, Lng = 106.701500,
                DiaChi = "243/30 Tôn Đản, Phường 15, Quận 4, TP.HCM", SoDienThoai = "097 890 1234", Email = "phalau.dinui@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d?auto=format&fit=crop&w=800&q=80",
                Rating = 4.9, OpeningHours = "14:00 - 22:00", PriceRange = "35K - 55K VND",
                MenuItems = new List<string>{"Phá lấu lòng bò", "Bánh mì phá lấu", "Mì gói phá lấu"} },

            new QuanAn { Ten = "Súp Cua Hạnh", Loai = "Khac", MoTa = "Súp cua đặc ruột, óc heo béo ngậy, topping ngập tràn ăn no nê.", Lat = 10.763300, Lng = 106.701700,
                DiaChi = "277 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "099 012 3456", Email = "supcuahanh277@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1548943487-a2e4e43b4859?auto=format&fit=crop&w=800&q=80",
                Rating = 4.7, OpeningHours = "16:00 - 23:00", PriceRange = "40K - 80K VND",
                MenuItems = new List<string>{"Súp cua thập cẩm", "Súp óc heo", "Cháo cua"} },

            new QuanAn { Ten = "Ốc Đào", Loai = "Oc", MoTa = "Thương hiệu ốc cực kỳ lâu đời với phong cách xào bơ tỏi đặc trưng không đụng hàng.", Lat = 10.762500, Lng = 106.702000,
                DiaChi = "C122 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "090 112 2334", Email = "ocdao.saigon@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1565557623262-b51c2513a641?auto=format&fit=crop&w=800&q=80",
                Rating = 4.6, OpeningHours = "11:00 - 23:00", PriceRange = "120K - 280K VND",
                MenuItems = new List<string>{"Ốc Đào xào bơ", "Tu hài nướng mỡ hành", "Mực trứng chiên"} },

            new QuanAn { Ten = "Sushi Viên", Loai = "Khac", MoTa = "Các loại sushi đa dạng màu sắc, giá sinh viên, mua mang đi rất tiện lợi.", Lat = 10.762300, Lng = 106.702100,
                DiaChi = "415 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "093 223 3445", Email = "sushivien415@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?auto=format&fit=crop&w=800&q=80",
                Rating = 4.4, OpeningHours = "16:00 - 22:00", PriceRange = "10K - 20K VND/Viên",
                MenuItems = new List<string>{"Sushi cá hồi", "Sushi lươn", "Maki cuộn", "Gunkan trứng cá"} },

            new QuanAn { Ten = "Mì Cay Sasin", Loai = "Lau", MoTa = "Thử thách sức chịu đựng với mì cay 7 cấp độ chuẩn vị Hàn Quốc.", Lat = 10.762100, Lng = 106.702200,
                DiaChi = "312 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "098 334 4556", Email = "micaysasin312@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1552611052-33e04de081de?auto=format&fit=crop&w=800&q=80",
                Rating = 4.3, OpeningHours = "11:00 - 22:00", PriceRange = "60K - 120K VND",
                MenuItems = new List<string>{"Mì cay hải sản", "Mì cay bò Mỹ", "Takoyaki", "Trà sữa"} },

            new QuanAn { Ten = "Chè Mỹ 2", Loai = "Khac", MoTa = "Thế giới chè giải nhiệt siêu ngon, nổi bật nhất là chè Thái sầu riêng.", Lat = 10.761900, Lng = 106.702300,
                DiaChi = "591 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "091 445 5667", Email = "chemy2.vk@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1551024601-bec78aea704b?auto=format&fit=crop&w=800&q=80",
                Rating = 4.7, OpeningHours = "10:00 - 22:30", PriceRange = "25K - 50K VND",
                MenuItems = new List<string>{"Chè Thái sầu riêng", "Chè khúc bạch", "Chè đậu xanh phô mai", "Sâm bổ lượng"} },

            new QuanAn { Ten = "Bạch Tuộc Nướng", Loai = "Oc", MoTa = "Bạch tuộc nướng sa tế cay xé lưỡi, nhâm nhi cùng bia lạnh là chuẩn bài.", Lat = 10.761200, Lng = 106.702700,
                DiaChi = "120 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "094 556 6778", Email = "bachtuocnuong120@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1599458252573-56ae36120de1?auto=format&fit=crop&w=800&q=80",
                Rating = 4.8, OpeningHours = "16:30 - 23:30", PriceRange = "100K - 200K VND",
                MenuItems = new List<string>{"Bạch tuộc nướng đá", "Mực lá nướng sa tế", "Răng mực xào bơ tỏi"} },

            new QuanAn { Ten = "Bò Lá Lốt Thanh Vy", Loai = "Lau", MoTa = "Bò nướng lá lốt mỡ chài thơm lừng, cuốn bánh tráng rau rừng chấm mắm nêm đậm đà.", Lat = 10.760900, Lng = 106.702900,
                DiaChi = "267 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "092 667 7889", Email = "bolalot.thanhvy@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?auto=format&fit=crop&w=800&q=80",
                Rating = 4.8, OpeningHours = "16:00 - 22:30", PriceRange = "60K - 100K VND",
                MenuItems = new List<string>{"Bò nướng lá lốt", "Bò mỡ chài", "Nem nướng", "Thịt xiên nướng"} },

            new QuanAn { Ten = "Dimsum Sinh Viên", Loai = "Khac", MoTa = "Há cảo, xíu mại nóng hổi bốc khói trong xửng tre, nước chấm chua ngọt xuất sắc.", Lat = 10.760600, Lng = 106.703100,
                DiaChi = "315 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "096 778 8990", Email = "dimsum315@yahoo.com",
                ImageUrl = "https://images.unsplash.com/photo-1496116218417-1a781b1c416c?auto=format&fit=crop&w=800&q=80",
                Rating = 4.5, OpeningHours = "15:00 - 22:00", PriceRange = "25K - 60K VND",
                MenuItems = new List<string>{"Há cảo tôm", "Xíu mại nấm đông cô", "Bánh cuốn tôm", "Chân gà tàu xì"} },

            new QuanAn { Ten = "Gà Nướng Muối Ớt", Loai = "Lau", MoTa = "Gà quay lu nguyên con ướp muối ớt, da giòn rụm, thịt bên trong mọng nước.", Lat = 10.760300, Lng = 106.703300,
                DiaChi = "482 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "097 889 9001", Email = "ganuongmuoiot@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1598514982205-f36b96d1e8d4?auto=format&fit=crop&w=800&q=80",
                Rating = 4.7, OpeningHours = "16:00 - 23:00", PriceRange = "180K - 300K VND/Con",
                MenuItems = new List<string>{"Gà nướng muối ớt", "Gà luộc lá chanh", "Xôi chiên", "Mề gà cháy tỏi"} },

            new QuanAn { Ten = "Trái Cây Tô", Loai = "Khac", MoTa = "Trái cây tươi xắt miếng lớn, trộn cùng sữa chua và siro đá bào lạnh buốt.", Lat = 10.759500, Lng = 106.703800,
                DiaChi = "501 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "099 990 0112", Email = "traicayto501@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1490474418585-ba9f52c10995?auto=format&fit=crop&w=800&q=80",
                Rating = 4.6, OpeningHours = "10:00 - 22:30", PriceRange = "25K - 45K VND",
                MenuItems = new List<string>{"Trái cây tô đặc biệt", "Sữa chua mít", "Tàu hủ đá phô mai"} },

            new QuanAn { Ten = "Hột Vịt Lộn Rang Me", Loai = "Oc", MoTa = "Hột vịt lộn úp mề xào với cốt me đặc, ăn với rau răm và đậu phộng rang.", Lat = 10.759200, Lng = 106.704000,
                DiaChi = "18A Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "090 001 1223", Email = "hotvitlon18a@gmail.com",
                ImageUrl = "https://images.unsplash.com/photo-1564834724105-918b73d1b9e0?auto=format&fit=crop&w=800&q=80",
                Rating = 4.5, OpeningHours = "16:00 - 22:30", PriceRange = "15K - 30K VND",
                MenuItems = new List<string>{"Hột vịt lộn luộc", "Vịt lộn rang me", "Trứng cút lộn xào me", "Ốc len xào dừa"} },

            new QuanAn { Ten = "Bún Đậu Ti Tí", Loai = "Khac", MoTa = "Mẹt bún đậu đầy đặn với nem chua rán xịn, chả cốm dẻo và mắm tôm đánh bọt.", Lat = 10.758900, Lng = 106.704200,
                DiaChi = "99 Vĩnh Khánh, Phường 8, Quận 4, TP.HCM", SoDienThoai = "093 112 2334", Email = "bundautiti@yahoo.com",
                ImageUrl = "https://images.unsplash.com/photo-1585032226651-759b368d7246?auto=format&fit=crop&w=800&q=80",
                Rating = 4.8, OpeningHours = "11:00 - 22:00", PriceRange = "60K - 120K VND/Mẹt",
                MenuItems = new List<string>{"Bún đậu mẹt lớn", "Bún đậu thịt luộc", "Chả cốm chiên", "Nem chua rán"} }
        };
    }
}