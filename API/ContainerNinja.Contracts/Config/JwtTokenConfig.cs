namespace ContainerNinja.Contracts.Config
{
    public class JwtTokenConfig
    {
        public string Issuer { get; set; } = "thisismeyouknow";
        public string Audience { get; set; } = "thisismeyouknow";
        public int ExpiryInMinutes { get; set; } = 100;
        public string key { get; set; } = "thiskeyisverylargetobreak_thiskeyisverylargetobreak_thiskeyisverylargetobreak_thiskeyisverylargetobreak_thiskeyisverylargetobreak_thiskeyisverylargetobreak_thiskeyisverylargetobreak_thiskeyisverylargetobreak_thiskeyisverylargetobreak_thiskeyisverylargetobreak";
    }
}