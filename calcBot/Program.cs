using calcBot;
using Telegram.Bot;

                    
var token = "8734690712:AAEKNcXiQl868y0Ydxlq9BwPCWkM1eIcmjs";

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var handler = new BotHandler(bot);

bot.OnUpdate += handler.HandleUpdate;
bot.OnError += (ex, source) =>
{
    Console.WriteLine($"[{source}] {ex}");
    return Task.CompletedTask;
};

var me = await bot.GetMe();
Console.WriteLine($"Бот @{me.Username} запущен. Нажмите Enter для остановки.");
Console.ReadLine();
cts.Cancel();