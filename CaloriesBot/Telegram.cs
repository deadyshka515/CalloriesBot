using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static CaloriesBot.DBModels;

namespace CaloriesBot
{
    public class TG
    {
        private string MinimumPromt = @"Ты — профессиональный нутрициолог и шеф-повар. Отвечай на русском языке.
            Форматируй ответ с помощью эмоджи, стандартный формат отправки markdown, но в случае неккоретной разметки будет отправлен обычным текстом, поэтому не злоупотребляй им и
                используй большей эмоджи.";


        private string StandartPromt = "Ты — профессиональный нутрициолог и шеф-повар. Отвечай исключительно на русском языке.\r\n\r\n" +
            "Отвечай на ЛЮБОЙ запрос пользователя строго по следующим правилам:\r\n\r\n" +
            "1. Если запрос явно или предположительно касается еды/продукта/блюда — отвечай ТОЛЬКО в этом формате (ровно так, с пустыми строками):" +
            "\r\n\r\nПродукт: Название блюда\r\n\r\nКалорийность: X ккал\r\nБелки: X г\r\nЖиры: X г\r\nУглеводы: X г\r\n\r\n представляй" +
            " калории и бжу на грамовку которую тебе прислали, если неизвестно какую грамовку имел ввиду пользователь, то кратко упомяни в описании, что расчёт шел на 100гр" +
            "Описание: Краткий комментарий\r\n\r\n" +
            "2. Если запрос НЕ про еду, слишком короткий, непонятный или неоднозначный (например: gold, test, привет, hi, как дела и т.д.) — " +
            "отвечай **ровно одной строкой**:\r\n\r\n-1\r\n\r\nПравила (очень важно):\r\n- Никогда не добавляй никакой другой текст, " +
            "приветствия, \"Понял\", \"Готов к работе\", объяснения и т.д.\r\n- Для не-еды — всегда только \"-1\" и ничего больше.\r\n- " +
            "Для еды — строго указанный формат выше, без лишних строк.\r\n- В \"Описание\" пиши коротко и по делу (1-2 предложения,объем учитвается без доп предлоежния про грамовку).\r\n- " +
            "Используй стандартную порцию, если размер не указан." +
            "3. Если в запросе пользователь просит проанализировать пищу съеденную в последнее время то отправить \"2\" и ничего больше.";
        private CancellationTokenSource cts;
        private TelegramBotClient bot;
        private DB dB;
        private readonly Dictionary<long,UserSession> userStates;

        public static string RepairMarkDownV2(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var special = new HashSet<char> { '*','_', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };
            var specialPairs = new HashSet<char> { '_', '*', '~', };
            StringBuilder sB = new StringBuilder(text.Length*2);
            int i = 0;
            while (i < text.Length-1)
            {
                if (special.Contains(text[i]) && !(specialPairs.Contains(text[i]) && specialPairs.Contains(text[i + 1])))
                    i++;
                else
                {
                    sB.Append(text[i]);
                    i++;
                }
            }
            return sB.ToString();
        }

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
            bot.OnUpdate += OnUpdate;
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
            
            if (msg.Text is null) return;
            if (msg.From is null) return;

            string text = msg.Text;

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
            if (text == "/meals")
            {
                string str = "";
                DBModels.Food[] meals = dB.GetMeals(msg.From.Id).Result;
                if (meals is null)
                    str = "Вы ещё не успели внести свои приёмы пищи";
                else
                    foreach (DBModels.Food food in meals)
                    {
                        str += $"Номер:{food.Id}\n{food.Title}\nК:{food.Calories} Б: {food.Proteins} Ж: {food.Fats} У: {food.Carbohydrate}\n\n";
                    }
                await bot.SendMessage(msg.Chat, str);
                    
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
                    request.system_prompt = StandartPromt;
                    request.input = StandartPromt +"Запрос пользователя:"+msg.Text;
                    ModelResponse response = AiConnect.Post("/api/v1/chat", request).GetAwaiter().GetResult();
                    string aiMesText = response.OutPut[0].Content;
                    
                    //string[] aiMesParametrs = aiMesText.Split('|');
                    if (aiMesText == "-1" || aiMesText == "-1\n")
                        await bot.SendMessage(msg.Chat, $"Ошибка, запрос не связан с едой");
                    else if(aiMesText == "2" || aiMesText == "2\n")
                    {
                        DBModels.Food[] meals = dB.GetLastMeals(tgId, -7).Result;
                        request = new();
                        request.model = "google/gemma-3-4b";
                        request.system_prompt = MinimumPromt;
                        request.input =@"Проанализируй рацион пользователя за последние 7 дней:
                            { mealsData}
                            Выдели сильные и слабые стороны, оцени баланс БЖУ и калорийность.Дай конкретные, реалистичные рекомендации по улучшению.
                            В конце предложи примерный сбалансированный рацион на день.";
                        foreach (Food food in meals)
                        {
                            request.input += food.ToString();
                        }
                        response = AiConnect.Post("/api/v1/chat", request).GetAwaiter().GetResult();
                        aiMesText = response.OutPut[0].Content;
                        //aiMesText = aiMesText.Replace("*", "");
                        try
                        {
                            await bot.SendMessage(msg.Chat, RepairMarkDownV2(aiMesText),ParseMode.MarkdownV2);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            await bot.SendMessage(msg.Chat, aiMesText);
                        }


                    }
                    else
                            {
                                await bot.SendMessage(msg.Chat, aiMesText,
                                    replyMarkup: new InlineKeyboardButton[] { "Записать блюдо" });
                            }
                    //    await bot.SendMessage(msg.Chat, $"Продукт: {aiMesParametrs[5]}\n\nКалорийность: {aiMesParametrs[1]}\nБелки: {aiMesParametrs[2]}\nЖиры: {aiMesParametrs[3]}\nУглеводы: {aiMesParametrs[4]}\n\nОписание: {aiMesParametrs[6]}");
                }
            }
        }
        async Task OnUpdate(Update update)
        {

            if (update is { CallbackQuery: { } query })
            {
                DBModels.Food food = new DBModels.Food();
                
                if (query.Message is null) return;
                if (query.Message.Text is null) return;

                if (Parsing.ParseFood(query.Message.Text,out food))
                {
                    food.UserId = query.From.Id;
                    if (dB.AddMeal(food).Result != -1)
                    {
                        await bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
                        await bot.SendMessage(query.Message!.Chat, $"Данные успешно внесены");
                    }
                    else
                    {
                        await bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
                        await bot.SendMessage(query.Message!.Chat, $"Ошибка при внесении данных");
                        throw new Exception("Не удалось добавить прием пищи");
                    }
                }
                else
                {
                    await bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
                    await bot.SendMessage(query.Message!.Chat, $"Ошибка при парсинге еды");
                }



                //await bot.SendMessage(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");

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
