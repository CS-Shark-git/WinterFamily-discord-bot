using WinterFamily.Main.Common.Configuration.JsonModels;
using WinterFamily.Main.Common.Configuration;

namespace WinterFamily.Main
{
    internal class Program
    {
        static async Task Main()
        {
            var configService = new ConfigurationService<Token>();
            var token = configService.Build();
            var bot = new Bot(token);

            await bot.RunAsync();
        }
    }
}