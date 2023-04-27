using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutoPostingVK;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using PuppeteerSharp;
using SeleniumExtras.WaitHelpers;

public class ComPoster
{
    private readonly string? _baseUrl = "https://vk.com";
    private readonly string? urlBeforeGoON = "https://vk.com/friends?act=find";
    private readonly string? _contentPath = @"C:\Users\VKauto\net6.0-windows\content";
    private string? _profileId;
    private RemoteWebDriver? _driver;
    private readonly string? _profileName = "";
    private readonly string? _logFileName = "";
    private readonly string? _groupName = "";


    public ComPoster(RemoteWebDriver driver, string profileId, string? logFileName, string profileName, string? groupName)
    {
        _driver = driver;
        _profileId = profileId;
        _logFileName = logFileName;
        _profileName = profileName;
        _groupName = groupName;
    }

    public async Task Posting()
    {
        bool isLoadedContent = false;

        WebDriverWait? wait = new(_driver, TimeSpan.FromSeconds(60));
        IJavaScriptExecutor? executor = (IJavaScriptExecutor)_driver;

        wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

        await Processes.CheckRunningChromeAsync();

        // Проверяем Url на предмет блокировки
        if (_driver.Url.Contains("blocked"))
        {
            Console.WriteLine($"Этот аккаунт заблокирован: {_profileId}");
            _driver.Dispose();
            return;
        }

        // Проверяем страницу на предмет popup с предложением о красивом имени
        try
        {
            IWebElement element = _driver.FindElement(By.XPath("//div[@class='box_layout' and @onclick='boxQueue.skip=true;']"));
            if (element != null)
            {
                IWebElement closeButton = _driver.FindElement(By.XPath("//div[@class='box_x_button']"));
                closeButton.Click();
            }
        }
        catch (Exception)
        {
            // Обработка ошибки
        }


        // Ожидаем загрузки DOM
        //wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        try
        {
            // Проверяем есть ли этот элемент на странице, если да, то мы на главной
            var vkLightBox = _driver.FindElement(By.CssSelector(".vkuiPopoutRoot__modal"));
        }
        catch (Exception)
        {
            // Если нет, то переключаемся на главну
            //await Task.Run(() => _driver.Navigate().GoToUrl(_baseUrl));
            //_driver.Navigate().GoToUrl(_baseUrl);

            //var myPageLink = _driver.FindElement(By.XPath("//a[contains(span/text(),'Моя страница')]"));
            //var actions = new Actions(_driver);
            //actions.MoveToElement(myPageLink).Click().Perform();

            await GoToMainPageAsync();
        }

        // Ожидаем загрузки DOM
        wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

        //await Processes.CheckRunningChromeAsync();
        // Продолжаем работу если на правильной странице
        try
        {
            // Ожидаем загрузки элемента при клике на который появляется input
            IWebElement? postField = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("post_field")));
            // Кликаем на элемент
            executor?.ExecuteScript("arguments[0].focus();", postField);

            // Получаем список файлов JSON из директории
            var jsonFiles = Directory.GetFiles(_contentPath, "*.json");

            // Запускаем цикл по файлам
            foreach (string jsonFile in jsonFiles)
            {
                // Десериализуем json в List<Content>
                string json = File.ReadAllText(jsonFile);
                List<Content>? contents = JsonConvert.DeserializeObject<List<Content>>(json);

                // Проверяем, нужно ли нам обрабатывать текущий файл
                bool allViewed = contents.All(content => content.ImageViewed);
                bool allImagesNull = contents.All(content => content.ImagePaths == null);
                bool isForPosting = contents.All(content => content.IsForPosting == false);

                if (allViewed || allImagesNull || isForPosting)
                {
                    continue;
                }

                // Запускаем цикл по списку контента
                foreach (Content content in contents)
                {
                    // Проверяем, подходит ли нам текущий контент
                    if (content.ImageViewed == false && content.ImagePaths != null && content.ImagePaths.Count > 0)
                    {
                        // Ожидаем загрузки элемента для вставки фото и кликаем
                        IWebElement inputFile = wait.Until(ExpectedConditions.ElementExists(By.CssSelector("input[name='photo']")));
                        executor?.ExecuteScript("arguments[0].style.display = 'block';", inputFile);

                        // Получаем элемент div с картинками (если он есть на странице)
                        IWebElement thumbnailsDiv = null;
                        try
                        {
                            thumbnailsDiv = _driver.FindElement(By.CssSelector(".editable_thumbs"));
                        }
                        catch (Exception) { }

                        // Загружаем каждую картинку по очереди
                        int limitImgForPosting = 0;
                        foreach (var imagePath in content.ImagePaths)
                        {
                            if (String.IsNullOrEmpty(imagePath)) continue;
                            if (!File.Exists(imagePath)) continue;

                            // Если уже загружено максимальное количество картинок, то выходим из цикла
                            if (limitImgForPosting >= 4)
                            {
                                break;
                            }

                            // Загружаем картинку
                            try
                            {
                                inputFile.SendKeys(imagePath);
                            }
                            catch (Exception) { }

                            limitImgForPosting++;



                            // Если на странице есть элемент "editable_thumbs", то ждем, пока появится нужное количество картинок
                            if (thumbnailsDiv != null)
                            {
                                int expectedCount = limitImgForPosting;
                                int count = 0;
                                while (true)
                                {
                                    int actualCount = thumbnailsDiv.FindElements(By.CssSelector("div.thumb_photo_wrap")).Count;
                                    if (actualCount == expectedCount)
                                    {
                                        break;
                                    }
                                    await Task.Delay(100);
                                    count++;
                                    if (count >= 300)
                                    {
                                        Console.WriteLine("Не удалось дождаться загрузки всех картинок.");
                                        break;
                                    }
                                }
                            }
                            else // Если элемента "editable_thumbs" еще нет на странице, то ждем его появления
                            {
                                try
                                {
                                    thumbnailsDiv = wait.Until(ExpectedConditions.ElementExists(By.CssSelector(".editable_thumbs")));
                                }
                                catch (TimeoutException)
                                {
                                    Console.WriteLine("Не удалось дождаться появления элемента 'editable_thumbs'.");
                                    break;
                                }
                            }

                            await Task.Delay(500);
                        }

                        if (!string.IsNullOrEmpty(content.Text))
                        {
                            int count = 0;
                            int maxCount = 200;

                            string text = content.Text;
                            string sanitizedText = await FilterTextAsync(text);


                            IWebElement textField = wait.Until(ExpectedConditions.ElementExists(By.Id("post_field")));
                            executor?.ExecuteScript("arguments[0].style.display = 'block';", textField);
                            await Task.Delay(200);
                            textField.SendKeys(sanitizedText);
                            bool isTextInserted = !string.IsNullOrEmpty(textField.Text);
                            while (!isTextInserted)
                            {
                                // Ожидаем если нужный класс не появился
                                await Task.Delay(100);

                                // Увеличиваем счетчик итераций
                                count++;

                                // Проверяем, не превысило ли количество итераций максимальное значение
                                if (count >= maxCount)
                                {
                                    // Выводим сообщение об ошибке и выходим из цикла
                                    Console.WriteLine("Не удалось вставить текст в пост");
                                    break;
                                }
                            }
                        }

                        // Ожидаем загрузки кнопки для отправки поста, фокусируемся на ней и оправляем
                        await Task.Delay(1000);
                        try
                        {
                            IWebElement sendButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id("send_post")));
                            await Task.Delay(1000);
                            executor?.ExecuteScript("arguments[0].focus();", sendButton);
                            await Task.Delay(2000);
                            executor?.ExecuteScript("arguments[0].click();", sendButton);
                            await Task.Delay(3000);

                            // Собираю ссылку для передачи в метод, для отправки накрутки лайков
                            string hrefLinkPost = await GetPostHrefAsync();
                            string baseUrl = _driver.Url;
                            string prefix = "?w=";

                            Uri postUri = new Uri($"{baseUrl}{prefix}{hrefLinkPost}");

                            if(_groupName?.ToLower() == "фулл")
                            {
                                // Отправляю запрос на лайки для поста
                                await OrderLikes.SendRequest(postUri, _profileName, _logFileName);
                            }                            

                            // Записываю логи
                            string message = $"В профиль {_profileName} был загружен пост {content.MessageId}";
                            LogManager.LogMessage(message, _logFileName);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"Не удалось нажать на кнопку загрузки контента в профиле: {_profileId}");
                        }


                        // Обновляем информацию о текущем контенте в списке
                        content.ImageViewed = true;
                        json = JsonConvert.SerializeObject(contents);
                        File.WriteAllText(jsonFile, json);

                        isLoadedContent = true;

                        break;
                    }
                }
                if (isLoadedContent) break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Не получилось разместить пост по этой причине -: {e.Message}");
        }
        finally
        {

            await Task.Delay(500);
            _driver.Navigate().GoToUrl(urlBeforeGoON);
            await Task.Delay(1000);
            _driver.Dispose();

            await Task.CompletedTask;
        }
    }

    public async Task GoToMainPageAsync()
    {
        var myPageLink = await Task.Run(() => new WebDriverWait(_driver, TimeSpan.FromSeconds(10))
            .Until(ExpectedConditions.ElementExists(By.XPath("//a[contains(span/text(),'Моя страница')]"))));

        var actions = new Actions(_driver);
        await Task.Run(() => actions.MoveToElement(myPageLink).Click().Perform());
    }
    public async Task<string> GetPostHrefAsync()
    {
        // найдем все элементы a
        IReadOnlyList<IWebElement> postLinks = await Task.Run(() =>
            new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(
                ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector(".PostHeaderSubtitle__link"))));

        // вернем нужную часть ссылки первого элемента
        return postLinks[0].GetAttribute("href").Split('/').Last();
    }

    // Фльтрация текста для постов
    public static async Task<string> FilterTextAsync(string text)
    {
        string sanitizedText = Regex.Replace(text, @"r/\s*#\S+\s*", "", RegexOptions.IgnoreCase);
        string filteredText = await Task.Run(() => new string(sanitizedText.Where(c => !Char.IsSurrogate(c)).ToArray()));
        return filteredText;
    }

}
