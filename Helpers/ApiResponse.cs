public static class ApiResponse
{
    public static object Success(string ar, string en, string lang, object data = null)
    {
        return new
        {
            message = lang.StartsWith("ar") ? ar : en,
            messageAr = ar,
            messageEn = en,
            data
        };
    }

    public static object Error(string ar, string en, string lang)
    {
        return new
        {
            message = lang.StartsWith("ar") ? ar : en,
            messageAr = ar,
            messageEn = en
        };
    }
}