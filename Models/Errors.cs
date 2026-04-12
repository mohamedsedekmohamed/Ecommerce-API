namespace EcommerceAPI.Errors
{
    public class ApiException
    {
        public int StatusCode { get; set; }

        public string Message { get; set; }     // الرسالة حسب اللغة
        public string MessageAr { get; set; }   // عربي
        public string MessageEn { get; set; }   // إنجليزي

        public string Details { get; set; }

        public ApiException(int statusCode, string lang = "ar", string details = null)
        {
            StatusCode = statusCode;

            // نجيب الرسائل الافتراضية
            var (ar, en) = GetDefaultMessages(statusCode);

            MessageAr = ar;
            MessageEn = en;

            // تحديد اللغة
            Message = lang.StartsWith("ar") ? ar : en;

            Details = details;
        }

        private (string ar, string en) GetDefaultMessages(int statusCode)
        {
            return statusCode switch
            {
                400 => ("يوجد خطأ في البيانات المرسلة.", "Invalid request data."),
                401 => ("غير مصرح لك، يرجى تسجيل الدخول.", "Unauthorized, please login."),
                403 => ("ليس لديك صلاحية للقيام بهذا الإجراء.", "Forbidden action."),
                404 => ("المورد الذي تبحث عنه غير موجود.", "Resource not found."),
                500 => ("حدث خطأ داخلي في الخادم.", "Internal server error."),
                _   => ("حدث خطأ غير متوقع.", "Unexpected error occurred.")
            };
        }
    }
}