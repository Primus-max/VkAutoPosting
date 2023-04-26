using Microsoft.Net.Http.Headers;
using PuppeteerSharp;



namespace PupeTest
{

    public static class CookiesHelper
    {
        public static async Task SetCookiesAsync(Page page, List<CookieParam> cookies)
        {
            foreach (var cookie in cookies)
            {
                await page.SetCookieAsync(new PuppeteerSharp.CookieParam
                {
                    Name = cookie.Name,
                    Value = cookie.Value,
                    Domain = cookie.Domain,
                    Path = cookie.Path,
                    HttpOnly = cookie.HttpOnly,
                    Secure = cookie.Secure,
                    Expires = cookie.Expires,
                    SameSite = (SameSite?)cookie.SameSite
                });
            }
        }
    }

    public static class HeadersHelper
    {
        public static async Task SetHeadersAsync(Page page, Dictionary<string, string> headers)
        {
            await page.SetExtraHttpHeadersAsync(headers);
        }
    }

    public class CookieParam
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }
        public bool HttpOnly { get; set; }
        public bool Secure { get; set; }
        public Double? Expires { get; set; }
        public SameSiteMode? SameSite { get; set; }
    }

}
