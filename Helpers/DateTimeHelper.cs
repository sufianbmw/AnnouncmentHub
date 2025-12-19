using System;
using System.Globalization;

namespace AnnouncmentHub.Helpers
{
    public static class DateTimeHelper
    {
        public static string FormatDateTime(DateTime? dateTime, string format = "yyyy-MM-dd hh:mm tt")
        {
            if (!dateTime.HasValue)
                return "";

            // استخدام ثقافة عربية عشان الـ AM/PM تطلع "ص/م"
            var culture = new CultureInfo("ar-SA");
            return dateTime.Value.ToString(format, culture);
        }

        public static string FormatDate(DateTime? dateTime, string format = "yyyy-MM-dd")
        {
            if (!dateTime.HasValue)
                return "";

            var culture = new CultureInfo("ar-SA");
            return dateTime.Value.ToString(format, culture);
        }
    }
}
