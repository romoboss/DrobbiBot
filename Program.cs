using System;
using System.Threading.Tasks;
using System.Text.Json;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Diagnostics;
using System.Net.Http;
using static System.Net.WebRequestMethods;

namespace DrobbiBot
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN", EnvironmentVariableTarget.Machine),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            });

            var slashCommands = discord.UseSlashCommands();
            slashCommands.RegisterCommands<MySlashCommands>();

            discord.Ready += OnReady;

            await discord.ConnectAsync();

            await Task.Delay(-1);
        }

        private static Task OnReady(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Console.WriteLine("Bot is connected and ready!");
            return Task.CompletedTask;
        }
    }

    public class MySlashCommands : ApplicationCommandModule
    {
        [SlashCommand("test", "Tests if the bot is operating.")]
        public async Task TestCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync("The bot is online!");
        }

        [SlashCommand("jiggle", "MaKEs tHE TeXT JIgglE.")]
        public async Task JiggleCommand(InteractionContext ctx, [Option("message", "The message to jiggle")] string message)
        {
            string result = "";

            foreach (char ch in message)
            {
                string rch;
                Random random = new Random();
                if (random.Next(1, 3) == 1)
                {
                    rch = ch.ToString().ToUpper();
                }
                else
                {
                    rch = ch.ToString().ToLower();
                }

                result = result + rch;
            }

            await ctx.CreateResponseAsync(result);
        }

        [SlashCommand("joke", "Gives you a random joke.")]
        public async Task JokeCommand(InteractionContext ctx)
        {
            HttpClient httpc = new HttpClient();
            string apiUrl = "https://official-joke-api.appspot.com/jokes/random";
            HttpResponseMessage response = await httpc.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    string setup = root.GetProperty("setup").GetString();
                    string punchline = root.GetProperty("punchline").GetString();

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Here's a Joke for You!",
                        Description = setup + "\n||" + punchline + "||",
                        Color = DiscordColor.Yellow
                    };

                    await ctx.CreateResponseAsync(embed);
                    return;
                }
            }

            await ctx.CreateResponseAsync("Couldn't fetch a joke at the moment!");
        }

        [SlashCommand("combine", "Combines two words.")]
        public async Task CombineCommand(InteractionContext ctx,
                              [Option("first_word", "The first word to combine.")] string word1,
                              [Option("second_word", "The second word to combine.")] string word2)
        {
            string og1 = word1;
            string og2 = word2;


            StringBuilder result = new StringBuilder();
            Random random = new Random();
            int maxLength = word1.Length + word2.Length;

           
            for (int i = 0; i < maxLength; i++)
            {
               
                if (random.Next(1,3) == 1 && word1.Length > 0)
                {
                    result.Append(word1[0]); 
                    word1 = word1.Substring(1); 
                }
                else if (word2.Length > 0)
                {
                    result.Append(word2[0]); 
                    word2 = word2.Substring(1);
                }
            }
            
            var embed = new DiscordEmbedBuilder
            {
                Title = result.ToString(),
                Description = $"I knew I could mix **{og1}** and **{og2}** into **{result}**!",
                Color = DiscordColor.Turquoise
            };

            await ctx.CreateResponseAsync(embed);
        }

        [SlashCommand("cat", "Gives a random cat picture.")]
        public async Task CatCommand(InteractionContext ctx)
        {
            HttpClient httpc = new HttpClient();
            string apiUrl = "https://cataas.com/cat";
            var response = await httpc.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var imageUrl = "https://cataas.com/cat";

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Random Cat",
                    ImageUrl = imageUrl,
                    Color = DiscordColor.Gray
                };

                await ctx.CreateResponseAsync(embed);
            }
            else
            {
                await ctx.CreateResponseAsync("Couldn't fetch a cat at the moment!");
            }
        }

        [SlashCommand("dog", "Gives a random dog image.")]
        public async Task RandomDogCommand(InteractionContext ctx)
        {
            string apiUrl = "https://dog.ceo/api/breeds/image/random";

            HttpClient httpc = new HttpClient();
            var response = await httpc.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var imageUrl = string.Empty;
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    imageUrl = root.GetProperty("message").GetString();
                }

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Random Dog",
                    ImageUrl = imageUrl,
                    Color = DiscordColor.Green
                };

                await ctx.CreateResponseAsync(embed);
            }
            else
            {
                await ctx.CreateResponseAsync("Couldn't fetch a dog at the moment!");
            }
        }

        [SlashCommand("coinflip", "Flip a coin.")]
        public async Task CoinflipCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder{};
            Random random = new Random();
            if (random.Next(1, 3) == 1)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Heads",
                    Description = "The coin landed on Heads!",
                    Color = DiscordColor.Red
                };
            }
            else
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Tails",
                    Description = "The coin landed on Tails!",
                    Color = DiscordColor.Blue
                };
            }

            await ctx.CreateResponseAsync(embed);
        }

        [SlashCommand("randomnumber", "Gives a random number.")]
        public async Task RandomNumberCommand(InteractionContext ctx,
                              [Option("minimum", "The smallest amount it can give you.")] long minimum,
                              [Option("maximum", "The largest amount it can give you.")] long maximum)
        {
            var embed = new DiscordEmbedBuilder{};

            if (minimum >= int.MinValue && minimum <= int.MaxValue && maximum >= int.MinValue && maximum <= int.MaxValue && minimum < maximum)
            {
                Random random = new Random();
                int randomnumber = random.Next((int)minimum, (int)maximum + 1);

                embed = new DiscordEmbedBuilder
                {
                    Title = randomnumber.ToString(),
                    Description = $"A random number between {minimum} and {maximum} ended up being {randomnumber}!",
                    Color = DiscordColor.Purple
                };
            }  
            else
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Error :(",
                    Description = $"There has been an error while trying to find a random number between {minimum} and {maximum}",
                    Color = DiscordColor.Purple
                };
            }

            await ctx.CreateResponseAsync(embed);
        }
    }
}
