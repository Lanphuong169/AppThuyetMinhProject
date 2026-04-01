using System.Collections.Generic;

namespace VinhKhanhTrip.Helpers
{
    // Lớp tin nhắn dùng chung cho WeakReferenceMessenger
    public class LanguageChangedMessage { }

    public static class LanguageManager
    {
        public static string CurrentLang { get; set; } = "vi-VN";

        public static readonly Dictionary<string, Dictionary<string, string>> Translations = new()
        {
            ["vi-VN"] = new()
            {
                ["Search"] = "Tìm quán ăn...",
                ["Arrive"] = "Đang đến",
                ["TabMap"] = "Bản đồ",
                ["TabList"] = "Danh sách",
                ["TabSettings"] = "Cài đặt",
                ["ConfirmTitle"] = "Xác nhận",
                ["ConfirmMsg"] = "Bạn có chắc chắn muốn đổi sang Tiếng Việt không?",
                ["Yes"] = "Có",
                ["No"] = "Không",
                ["Change"] = "Thay đổi",
                ["Cancel"] = "Hủy",
                ["DetailDescription"] = "Mô tả",
                ["DetailSpecialDishes"] = "Món đặc sản",
                ["BtnMap"] = "Xem bản đồ",
                ["SettingTitle"] = "CÀI ĐẶT HỆ THỐNG",
                ["SettingFeature"] = "Tính năng",
                ["SettingTts"] = "🔊 Thuyết minh tự động",
                ["SettingGeofence"] = "📍 Hiển thị vùng Geofence",
                ["SettingLanguage"] = "Ngôn ngữ",
                ["SettingSpeedTitle"] = "Tốc độ thuyết minh" // <--- THÊM MỚI
            },
            ["en-US"] = new()
            {
                ["Search"] = "Search for food...",
                ["Arrive"] = "Arriving at",
                ["TabMap"] = "Maps",
                ["TabList"] = "List",
                ["TabSettings"] = "Settings",
                ["ConfirmTitle"] = "Confirm",
                ["ConfirmMsg"] = "Are you sure you want to change to English?",
                ["Yes"] = "Yes",
                ["No"] = "No",
                ["Change"] = "Change",
                ["Cancel"] = "Cancel",
                ["DetailDescription"] = "Description",
                ["DetailSpecialDishes"] = "Specialty dishes",
                ["BtnMap"] = "View Map",
                ["SettingTitle"] = "SYSTEM SETTINGS",
                ["SettingFeature"] = "Features",
                ["SettingTts"] = "🔊 Auto Voice Guide",
                ["SettingGeofence"] = "📍 Show Geofence Area",
                ["SettingLanguage"] = "Language",
                ["SettingSpeedTitle"] = "Narration speed" // <--- THÊM MỚI
            },
            ["de-DE"] = new()
            {
                ["Search"] = "Essen suchen...",
                ["Arrive"] = "Ankunft bei",
                ["TabMap"] = "Karte",
                ["TabList"] = "Liste",
                ["TabSettings"] = "Einstellungen",
                ["ConfirmTitle"] = "Bestätigen",
                ["ConfirmMsg"] = "Möchten Sie auf Deutsch umstellen?",
                ["Yes"] = "Ja",
                ["No"] = "Nein",
                ["Change"] = "Ändern",
                ["Cancel"] = "Abbrechen",
                ["DetailDescription"] = "Beschreibung",
                ["DetailSpecialDishes"] = "Spezialitäten",
                ["BtnMap"] = "Karte anzeigen",
                ["SettingTitle"] = "SYSTEMEINSTELLUNGEN",
                ["SettingFeature"] = "Funktionen",
                ["SettingTts"] = "🔊 Automatische Ansage",
                ["SettingGeofence"] = "📍 Geofence-Bereich anzeigen",
                ["SettingLanguage"] = "Sprache",
                ["SettingSpeedTitle"] = "Sprechgeschwindigkeit" // <--- THÊM MỚI
            },
            ["fr-FR"] = new()
            {
                ["Search"] = "Chercher...",
                ["Arrive"] = "Arrivée à",
                ["TabMap"] = "Carte",
                ["TabList"] = "Liste",
                ["TabSettings"] = "Paramètres",
                ["ConfirmTitle"] = "Confirmer",
                ["ConfirmMsg"] = "Voulez-vous passer au français ?",
                ["Yes"] = "Oui",
                ["No"] = "Non",
                ["Change"] = "Changer",
                ["Cancel"] = "Annuler",
                ["DetailDescription"] = "Description",
                ["DetailSpecialDishes"] = "Spécialités",
                ["BtnMap"] = "Voir la carte",
                ["SettingTitle"] = "PARAMÈTRES SYSTÈME",
                ["SettingFeature"] = "Fonctions",
                ["SettingTts"] = "🔊 Guide vocal auto",
                ["SettingGeofence"] = "📍 Afficher la zone Geofence",
                ["SettingLanguage"] = "Langue",
                ["SettingSpeedTitle"] = "Vitesse de narration" // <--- THÊM MỚI
            },
            ["ru-RU"] = new()
            {
                ["Search"] = "Поиск еды...",
                ["Arrive"] = "Прибытие в",
                ["TabMap"] = "Карта",
                ["TabList"] = "Список",
                ["TabSettings"] = "Настройки",
                ["ConfirmTitle"] = "Подтверждение",
                ["ConfirmMsg"] = "Переключиться на русский язык?",
                ["Yes"] = "Да",
                ["No"] = "Нет",
                ["Change"] = "Изменить",
                ["Cancel"] = "Отмена",
                ["DetailDescription"] = "Описание",
                ["DetailSpecialDishes"] = "Фирменные блюда",
                ["BtnMap"] = "Карта",
                ["SettingTitle"] = "СИСТЕМНЫЕ НАСТРОЙКИ",
                ["SettingFeature"] = "Функции",
                ["SettingTts"] = "🔊 Автогид",
                ["SettingGeofence"] = "📍 Показать геозону",
                ["SettingLanguage"] = "Язык",
                ["SettingSpeedTitle"] = "Скорость озвучки" // <--- THÊM MỚI
            },
            ["ja-JP"] = new()
            {
                ["Search"] = "レストランを検索...",
                ["Arrive"] = "到着",
                ["TabMap"] = "マップ",
                ["TabList"] = "リスト",
                ["TabSettings"] = "設定",
                ["ConfirmTitle"] = "確認",
                ["ConfirmMsg"] = "日本語に変更してもよろしいですか？",
                ["Yes"] = "はい",
                ["No"] = "いいえ",
                ["Change"] = "変更",
                ["Cancel"] = "キャンセル",
                ["DetailDescription"] = "説明",
                ["DetailSpecialDishes"] = "名物料理",
                ["BtnMap"] = "地図を見る",
                ["SettingTitle"] = "システム設定",
                ["SettingFeature"] = "機能",
                ["SettingTts"] = "🔊 自動音声ガイド",
                ["SettingGeofence"] = "📍 ジオフェンスエリアを表示",
                ["SettingLanguage"] = "言語",
                ["SettingSpeedTitle"] = "ナレーション速度" // <--- THÊM MỚI
            },
            ["ko-KR"] = new()
            {
                ["Search"] = "식당 검색...",
                ["Arrive"] = "도착",
                ["TabMap"] = "지도",
                ["TabList"] = "목록",
                ["TabSettings"] = "설정",
                ["ConfirmTitle"] = "확인",
                ["ConfirmMsg"] = "한국어로 변경하시겠습니까?",
                ["Yes"] = "예",
                ["No"] = "아니요",
                ["Change"] = "변경",
                ["Cancel"] = "취소",
                ["DetailDescription"] = "설명",
                ["DetailSpecialDishes"] = "특선 요리",
                ["BtnMap"] = "지도 보기",
                ["SettingTitle"] = "시스템 설정",
                ["SettingFeature"] = "기능",
                ["SettingTts"] = "🔊 자동 음성 안내",
                ["SettingGeofence"] = "📍 지오펜스 영역 표시",
                ["SettingLanguage"] = "언어",
                ["SettingSpeedTitle"] = "내레이션 속도" // <--- THÊM MỚI
            },
            ["zh-CN"] = new()
            {
                ["Search"] = "搜索餐厅...",
                ["Arrive"] = "即将到达",
                ["TabMap"] = "地图",
                ["TabList"] = "列表",
                ["TabSettings"] = "设置",
                ["ConfirmTitle"] = "确认",
                ["ConfirmMsg"] = "您确定要更改为中文吗？",
                ["Yes"] = "是",
                ["No"] = "否",
                ["Change"] = "更改",
                ["Cancel"] = "取消",
                ["DetailDescription"] = "描述",
                ["DetailSpecialDishes"] = "特色菜",
                ["BtnMap"] = "查看地图",
                ["SettingTitle"] = "系统设置",
                ["SettingFeature"] = "功能",
                ["SettingTts"] = "🔊 自动语音导览",
                ["SettingGeofence"] = "📍 显示地理围栏区域",
                ["SettingLanguage"] = "语言",
                ["SettingSpeedTitle"] = "解说速度" // <--- THÊM MỚI
            },
            // Bổ sung tiếng Tây Ban Nha vì bị thiếu trong file gốc của bạn
            ["es-ES"] = new()
            {
                ["Search"] = "Buscar comida...",
                ["Arrive"] = "Llegando a",
                ["TabMap"] = "Mapa",
                ["TabList"] = "Lista",
                ["TabSettings"] = "Ajustes",
                ["ConfirmTitle"] = "Confirmar",
                ["ConfirmMsg"] = "¿Estás seguro de que quieres cambiar a español?",
                ["Yes"] = "Sí",
                ["No"] = "No",
                ["Change"] = "Cambiar",
                ["Cancel"] = "Cancelar",
                ["DetailDescription"] = "Descripción",
                ["DetailSpecialDishes"] = "Especialidades",
                ["BtnMap"] = "Ver Mapa",
                ["SettingTitle"] = "AJUSTES DEL SISTEMA",
                ["SettingFeature"] = "Funciones",
                ["SettingTts"] = "🔊 Guía de voz automática",
                ["SettingGeofence"] = "📍 Mostrar área Geofence",
                ["SettingLanguage"] = "Idioma",
                ["SettingSpeedTitle"] = "Velocidad de narración" // <--- THÊM MỚI
            }
        };

        public static string Get(string key) => Translations.ContainsKey(CurrentLang) ? Translations[CurrentLang].GetValueOrDefault(key, key) : key;

        public static string TranslatePoi(string ten)
        {
            return CurrentLang switch
            {
                "en-US" => $"You are approaching {ten}. ",
                "de-DE" => $"Sie nähern sich {ten}. ",
                "fr-FR" => $"Vous approchez de {ten}. ",
                "ru-RU" => $"Вы приближаетесь к {ten}. ",
                "ja-JP" => $"{ten} に近づいています。",
                "ko-KR" => $"{ten}에 접근하고 있습니다. ",
                "zh-CN" => $"您正在靠近 {ten}。",
                "es-ES" => $"Te estás acercando a {ten}. ",
                _ => $"Bạn đang tiến vào khu vực {ten}. "
            };
        }
    }
}