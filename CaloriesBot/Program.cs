using System.Threading.Tasks;

namespace CaloriesBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //ModelRequest request = new();
            //request.model = "google/gemma-3-4b";
            //request.system_prompt = "Speak in Russian";
            //request.input = Console.ReadLine();
            //ModelResponse response = AiConnect.Post("/api/v1/chat", request).GetAwaiter().GetResult();
            //Console.WriteLine(response.OutPut[0].Content);


            DB dB = new DB();
            TG tg = new TG("8525115282:AAGiZYynhI9P5WwMsX_UJrCYjMInLzgbwW4", dB);
            //List<DBModels.User> users = await dB.GetUsers();
            //foreach (DBModels.User user in users)
            //{
            //    Console.WriteLine($"Name:{user.Name}");
            //}
        }
    }
}