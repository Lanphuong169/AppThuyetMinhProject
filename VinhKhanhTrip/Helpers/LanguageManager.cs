using System.Collections.Generic;

namespace VinhKhanhTrip.Helpers
{
    public static class LanguageManager
    {
        public static string CurrentLang { get; set; } = "vi-VN";

        // ĐÃ SỬA THÀNH PUBLIC Ở ĐÂY
        public static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
        {
            ["vi-VN"] = new()
            {
                ["Search"] = "Tìm quán ăn...",
                ["Arrive"] = "Đang đến",
                ["TabMap"] = "Bản đồ",
                ["TabList"] = "Danh sách",
                ["TabSettings"] = "Cài đặt",
                ["ConfirmToEnTitle"] = "Confirm",
                ["ConfirmToEnMsg"] = "Are you sure you want to change to English?",
                ["ConfirmToViTitle"] = "Xác nhận",
                ["ConfirmToViMsg"] = "Bạn có chắc chắn muốn đổi sang Tiếng Việt không?",
                ["Yes"] = "Có",
                ["No"] = "Không",
                ["Change"] = "Thay đổi",
                ["Cancel"] = "Hủy",
                ["DetailDescription"] = "Mô tả",
                ["DetailSpecialDishes"] = "Món đặc sản",
                ["BtnMap"] = "Xem bản đồ"
            },
            ["en-US"] = new()
            {
                ["Search"] = "Search for food...",
                ["Arrive"] = "Arriving at",
                ["TabMap"] = "Maps",
                ["TabList"] = "List",
                ["TabSettings"] = "Settings",
                ["ConfirmToEnTitle"] = "Confirm",
                ["ConfirmToEnMsg"] = "Are you sure you want to change to English?",
                ["ConfirmToViTitle"] = "Xác nhận",
                ["ConfirmToViMsg"] = "Bạn có chắc chắn muốn đổi sang Tiếng Việt không?",
                ["Yes"] = "Yes",
                ["No"] = "No",
                ["Change"] = "Change",
                ["Cancel"] = "Cancel",
                ["DetailDescription"] = "Description",
                ["DetailSpecialDishes"] = "Specialty dishes",
                ["BtnMap"] = "View Map"
            }
        };

        public static string Get(string key) => Translations[CurrentLang].GetValueOrDefault(key, key);

        public static string TranslatePoi(string ten, string moTaVi)
        {
            if (CurrentLang == "en-US") return $"You are approaching {ten}. This is a famous spot on Vinh Khanh street.";
            return $"Bạn đang đến gần {ten}. {moTaVi}";
        }
    }
}