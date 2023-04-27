using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AutoPostingVK
{
    public static class OrderLikes
    {
        public static async Task SendRequest(Uri url, string profileName, string fileName)
        {
            string API_KEY = "mT6pcdAr61jl9xmajngjr34M8Z4jjY2hpfHWyWHVu8p8x9ERg9lp9rQF6401";
            string service = "88";
            Uri link = url;
            Random random = new Random();
            string quantity = random.Next(20, 31).ToString();

            string fullUrl = $"https://soc-proof.su/api/v2?action=add&service={service}&link={link}&quantity={quantity}&key={API_KEY}";

            try
            {
                using (var client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(fullUrl);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                string message = $"Не получилось сделать заказ лайков на пост {url} в профиле {profileName}." +
                    $"Причина: {ex.Message}";
                LogManager.LogMessage(message, fileName);
            }
        }
    }
}
