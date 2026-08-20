using System;

namespace ProjectJ.Data
{
    public static class GameDataId
    {
        public static string Create()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static bool IsValid(string value)
        {
            return Guid.TryParseExact(value, "N", out _);
        }
    }
}
