using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.AnonymizeUa;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerSharp;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Automation;


Stopwatch stopwatch = new Stopwatch(); // Экземпляр для замера времени выполнения программы
stopwatch.Start(); // Начинало работы

// Проверяю работает запущен инкогнитон или нет
string processName = "Incogniton";
Process[] processes = Process.GetProcessesByName(processName);
if (processes.Length == 0)
{
    try
    {
        Process.Start(@"C:\Users\Администратор\AppData\Local\Programs\incogniton\Incogniton.exe");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Не удалось запустить Incongniton: {ex.Message}");
    }

}

string targetTitle = @"Incogniton - Version 3.2.7.5";
bool isWindowOpen = false;
int tryCount = 0;
string? logFileName = DateTime.Now.ToString("HH-mm-ss") + ".txt"; // Имя для файла логов на одну сессию программы

/// Ожидаем загрузки окна инкогнитона
while (!isWindowOpen)
{
    // Проверяем открытое окно с заданным заголовком
    foreach (Process p in Process.GetProcesses())
    {
        if (p.MainWindowTitle.Contains(targetTitle))
        {
            isWindowOpen = true;
            break;
        }
    }
    tryCount++;
}

if (isWindowOpen)
{
    RemoteWebDriver connectedDriver = null;
    int profileCounter = 0;
    List<ProfileInfo>? profilesInfo = await ProfileManager.GetProfileInfoAsync();
    foreach (var profileInfo in profilesInfo)
    {
        if (profileInfo == null) continue;

        try
        {
            connectedDriver = await BrowserManager.ConnectDriver(profileInfo.ProfileId);

            ComPoster poster = new ComPoster(connectedDriver, profileInfo.ProfileId, logFileName, profileInfo?.Name);
            await poster.Posting();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при запуске профиля {profileInfo.ProfileId}: {ex.Message}");
        }
        profileCounter++;


        connectedDriver?.Dispose();

        //Processes.CheckRunningChromeDriver();
    }


    string message1 = $"Запущено {profileCounter} из {profilesInfo.Count} профилей";
    LogManager.LogMessage(message1, logFileName);
    stopwatch.Stop(); // Завершение программы

    int h = stopwatch.Elapsed.Hours;
    int m = stopwatch.Elapsed.Minutes;
    int s = stopwatch.Elapsed.Seconds;

    string message2 = $"Время выполнения программы составило: {h} часов {m} минут {s} секунд";
    LogManager.LogMessage(message2, logFileName);
}
