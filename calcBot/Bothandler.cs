using System.Collections.Concurrent;
using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace calcBot;

public class BotHandler(ITelegramBotClient bot)
{
    private static readonly CultureInfo Ru = new("ru-RU");
    
    private readonly ConcurrentDictionary<long, Session> _sessions = new();
    
    public async Task HandleUpdate(Update update)
    {
        try
        {
            if (update.CallbackQuery is { } cb) await OnCallback(cb);
            else if (update.Message is { Text: { } text } msg) await OnText(msg.Chat.Id, text);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex}");
        }
    }
    
    private async Task OnText(long chatId, string text)
    {
        var s = _sessions.GetOrAdd(chatId, _ => new Session());

        if (text.StartsWith("/start"))
        {
            s.Step = Step.None;
            await AskWall(chatId);
            return;
        }

        switch (s.Step)
        {
            case Step.WaitLength:
                if (!TryNumber(text, out var length))
                {
                    await Send(chatId, "Введите число, например 5,5");
                    return;
                }
                s.Length = length;
                s.Step = Step.WaitHeight;
                await Send(chatId, "Введите высоту стены в метрах:");
                break;

            case Step.WaitHeight:
                if (!TryNumber(text, out var height))
                {
                    await Send(chatId, "Введите число, например 2,7");
                    return;
                }
                s.Step = Step.None;

                var volume = Calc.Volume(s.Wall, s.Length, height);
                var result =
                    $"🧱 <b>Забутовочный кирпич</b>\n" +
                    $"Стена: {s.Length.ToString("0.##", Ru)} × {height.ToString("0.##", Ru)} м\n" +
                    $"Объём кладки: {volume.ToString("0.###", Ru)} м³\n" +
                    $"Кирпича: {Calc.Bricks(volume)} шт.\n" +
                    $"✅ <b>С запасом 5%: {Calc.BricksWithReserve(volume)} шт.</b>";

                await Send(chatId, result, Kb(B("🔁 Посчитать ещё", "again")));
                break;

            default:
                await AskWall(chatId);
                break;
        }
    }
    
    private async Task OnCallback(CallbackQuery cb)
    {
        await bot.AnswerCallbackQuery(cb.Id);
        if (cb.Message is null || cb.Data is null) return;

        var chatId = cb.Message.Chat.Id;
        var s = _sessions.GetOrAdd(chatId, _ => new Session());

        if (cb.Data == "again")
        {
            await AskWall(chatId);
        }
        else if (cb.Data.StartsWith("w:") && Enum.TryParse<Wall>(cb.Data[2..], out var wall))
        {
            s.Wall = wall;
            s.Step = Step.WaitLength;
            await Send(chatId, "Введите длину стены в метрах (например, 12,5):");
        }
    }

    
    private Task AskWall(long chatId) =>
        Send(chatId, "Выберите тип кладки:",
            Kb(B("½ кирпича", "w:Half"), B("1 кирпич", "w:One"), B("1½ кирпича", "w:OneAndHalf")));

    private Task Send(long chatId, string text, InlineKeyboardMarkup? kb = null) =>
        bot.SendMessage(chatId, text, parseMode: ParseMode.Html, replyMarkup: kb);

    
    private static bool TryNumber(string text, out double value) =>
        double.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
        && value > 0 && value < 10_000;
    
    private static InlineKeyboardMarkup Kb(params InlineKeyboardButton[] buttons) => new(buttons);

    private static InlineKeyboardButton B(string text, string data) =>
        InlineKeyboardButton.WithCallbackData(text, data);
}