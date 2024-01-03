namespace JWTAPI.Models
{
    public class JWTSettings
    {
        public static JWTSettings Instance { get; protected set; } = new JWTSettings();
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int LifetimeInDays { get; set; }
    }
}
