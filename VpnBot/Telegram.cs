using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static CalloriesBot.DBModels;

namespace CalloriesBot
{
    public class TG
    {   
        private CancellationTokenSource cts;
        private TelegramBotClient bot;
        private DB dB;
        private readonly Dictionary<long,UserSession> userStates;

        public enum UserState
        {
            None,
            WaitingForName
        }
        public class UserSession
        {
            public UserState State { get; set; } = UserState.None;
            public long TgId { get; set; }
        }
        public TG(string token, DB dB)
        {
            this.dB = dB;
            userStates = new Dictionary<long,UserSession>();

            cts = new CancellationTokenSource();
            bot = new TelegramBotClient(token, cancellationToken: cts.Token);
            bot.OnError += OnError;
            bot.OnMessage += OnMessage;
            //bot.OnUpdate += OnUpdate;
            Console.WriteLine($"Бот работает");
            Console.ReadLine();
            cts.Cancel();
        }

        async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
        }

        async Task OnMessage(Message msg, UpdateType type)
        {
            string text = msg.Text;

            if (text is null) return;

            long tgId = msg.From.Id;

            if (!userStates.TryGetValue(tgId, out var session))
            {
                session = new UserSession { TgId = tgId };
                userStates[tgId] = session;
            }
            if (text == "/start")
            {
                DBModels.User user = await dB.GetUserByTgId(tgId);
                if (user == null)
                {
                    await bot.SendMessage(msg.Chat, "О, с вами я ещё не знаком, как вас величать?");
                    session.State = UserState.WaitingForName;
                }
                else
                {
                    session.State = UserState.None;
                    await bot.SendMessage(msg.Chat, $"Чем я могу угодить вам сегодня, {user.Name}?");
                }
            }
            if (!text.StartsWith("/")) //Отграничил ввод текста от команд
            {
                if(session.State == UserState.WaitingForName)
                {
                    await dB.AddUser(text, tgId);
                    await bot.SendMessage(msg.Chat, $"Отныне я буду прислуживать вам, {text}");
                    session.State = UserState.None;
                }
                else
                {
                    ModelRequest request = new();
                    request.model = "google/gemma-3-4b";
                    request.system_prompt = "Speak in Russian";
                    request.input = msg.Text;
                    ModelResponse response = AiConnect.Post("/api/v1/chat", request).GetAwaiter().GetResult();
                    await bot.SendMessage(msg.Chat, response.OutPut[0].Content);
                }
            }
        }

        // method that handle other types of updates received by the bot:
        //async Task OnUpdate(Update update)
        //{
        //    if (update is { CallbackQuery: { } query }) // non-null CallbackQuery
        //    {
        //        await bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
        //        await bot.SendMessage(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");
        //    }
        //}
    }
}
