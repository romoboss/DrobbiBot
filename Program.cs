using System;
using System.Threading.Tasks;
using System.Text.Json;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using DSharpPlus.EventArgs;
using System.Diagnostics;

namespace DrobbiBot
{

    class Program
    {
        public static float Tries = 1;
        public static int TriesFull = 1;

        public static bool AutoRestart = true;

        private static async Task InitializeDiscordClientAsync()
        {
            string token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN", EnvironmentVariableTarget.Machine);

            if (token == string.Empty || token == null)
            {
                token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            }
            
            if (token == string.Empty || token == null)
            {
                Console.WriteLine("Enter Bot Token");
                token = Console.ReadLine();
            }

            var discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                AutoReconnect = false
            });

            discord.ClientErrored += OnClientError;
            discord.SocketErrored += OnSocketError;

            var slashCommands = discord.UseSlashCommands();
            slashCommands.RegisterCommands<MySlashCommands>();
            discord.Ready += OnReady;

            await discord.ConnectAsync();

            var consoleOutput = new StringWriter();
            var originOut = Console.Out;
            while (AutoRestart)
            {
                Console.SetOut(consoleOutput);
                var output = consoleOutput.ToString();
                if (output.Contains("terminated"))
                {
                    await Main();
                    return;

                }
                await Task.Delay(5000);
            }
            await Task.Delay(-1);
        }

        private static async Task OnClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            Console.WriteLine($"Error, Retrying in {(int)MathF.Round(5 * Tries)} seconds! ({TriesFull})");
            await Task.Delay((int)MathF.Round(5000 * Tries));
            TriesFull++;
            Tries *= 1.2f;

            if (TriesFull >= 1) 
            {
               return;
            }


            await InitializeDiscordClientAsync();
            return;
        }

        private static async Task OnSocketError(DiscordClient sender, SocketErrorEventArgs e)
        {
            Console.WriteLine($"Error, Retrying in {(int)MathF.Round(5 * Tries)} seconds! ({TriesFull})");
            await Task.Delay((int)MathF.Round(5000 * Tries));
            TriesFull++;
            Tries *= 1.2f;

            if (TriesFull >= 1)
            {
                return;
            }


            await InitializeDiscordClientAsync();
            return;
        }

        public static async Task Main()
        {
            try
            {
                await InitializeDiscordClientAsync();
            }
            catch
            {
                Console.WriteLine($"Error, Retrying in {(int)MathF.Round(5 * Tries)} seconds! ({TriesFull})");
                await Task.Delay((int)MathF.Round(5000 * Tries));
                TriesFull++;
                Tries *= 1.2f;

                if (TriesFull >= 20)
                {
                    return;
                }


                await Main();
                return;
            }
        }

        private static Task OnReady(DiscordClient sender, ReadyEventArgs e)
        {
            Console.WriteLine("Bot is connected and ready!");
            return Task.CompletedTask;
        }

        public static string StyleSentance(string sectance, string customending)
        {
            string firstchar = sectance[0].ToString();
            sectance = sectance.Remove(0, 1);
            sectance = firstchar.ToUpper() + sectance;

            if (!sectance.EndsWith(".") && !sectance.EndsWith("!") && !sectance.EndsWith("?"))
            {
                if (customending == null)
                    sectance = sectance + "!";
                else 
                    sectance = sectance + customending;
            }
            return sectance;
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
            string apiUrl = "https://cataas.com/cat?json=true";
            var response = await httpc.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                var imageUrl = string.Empty;
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    imageUrl = "https://cataas.com/cat/" + root.GetProperty("_id").GetString();
                }

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

        [SlashCommand("trivia", "Gives a random trivia question.")]
        public async Task TriviaCommand(InteractionContext ctx)
        {
            HttpClient httpc = new HttpClient();
            string apiUrl = "https://opentdb.com/api.php?amount=10&type=multiple";
            var response = await httpc.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    var result = root.GetProperty("results")[0];

                    string category = result.GetProperty("category").GetString();
                    string difficulty = result.GetProperty("difficulty").GetString();
                    var question = HttpUtility.HtmlDecode(result.GetProperty("question").GetString());
                    var correctAnswer = HttpUtility.HtmlDecode(result.GetProperty("correct_answer").GetString());
                    var incorrectAnswers = result.GetProperty("incorrect_answers").EnumerateArray()
                                                    .Select(a => HttpUtility.HtmlDecode(a.GetString())).ToList();

                    List<string> allAnswers = new List<string>(incorrectAnswers) { correctAnswer };
                    allAnswers = allAnswers.OrderBy(a => Guid.NewGuid()).ToList();

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = question,
                        Description = $"{category.ToLower()}, {difficulty.ToLower()}",
                        Color = DiscordColor.Azure
                    };

                    string uniqueId = Guid.NewGuid().ToString();

                    var buttons = new List<DiscordButtonComponent>();
                    foreach (var answer in allAnswers)
                    {
                        buttons.Add(new DiscordButtonComponent(ButtonStyle.Primary, $"{uniqueId}_answer_{HttpUtility.UrlEncode(answer)}", answer));
                    }

                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                    var webhookBuilder = new DiscordWebhookBuilder()
                        .AddEmbed(embed)
                        .AddComponents(buttons);

                    await ctx.EditResponseAsync(webhookBuilder);

                    ctx.Client.ComponentInteractionCreated += async (s, e) =>
                    {
                        if (e.Id.StartsWith(uniqueId) && e.User.Id == ctx.User.Id)
                        {
                            string selectedAnswer = HttpUtility.UrlDecode(e.Id.Substring(uniqueId.Length + 8));
                            var isCorrect = selectedAnswer == correctAnswer;

                            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                new DiscordInteractionResponseBuilder()
                                    .AddEmbed(new DiscordEmbedBuilder
                                    {
                                        Title = isCorrect ? "Correct" : "Incorrect",
                                        Description = $"It was {correctAnswer}",
                                        Color = isCorrect ? DiscordColor.DarkGreen : DiscordColor.DarkRed
                                    }));

                            var disabledButtons = buttons.Select(button =>
                                new DiscordButtonComponent(button.Style, button.CustomId, button.Label, disabled: true)).ToList();

                            var updatedMessage = new DiscordWebhookBuilder()
                                .AddEmbed(embed)
                                .AddComponents(disabledButtons);

                            await ctx.EditResponseAsync(updatedMessage);
                        }
                    };
                }
            }
            else
            {
                await ctx.CreateResponseAsync("Couldn't fetch a question at the moment!");
            }
        }

        [SlashCommand("banana", "If life gives you bananas, you have bananas.")]
        public async Task BananaCommand(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder { };
            Random random = new Random();
            string description = "";

            for (int i = 0; i < random.Next(1,10); i++) 
            {
                description += ":banana:";
            }

            embed = new DiscordEmbedBuilder
            {
                Title = "BANANA",
                Description = description,
                Color = DiscordColor.Yellow
            };

            await ctx.CreateResponseAsync(embed);
        }

        [SlashCommand("riddle", "Gives a random riddle.")]
        public async Task RiddleCommand(InteractionContext ctx)
        {
            HttpClient httpc = new HttpClient();
            string apiUrl = "https://riddles-api.vercel.app/random";
            var response = await httpc.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    var riddle = root.GetProperty("riddle").ToString();
                    var answer = root.GetProperty("answer").ToString();

                    answer = Program.StyleSentance(answer, "!");
                    riddle = Program.StyleSentance(riddle, "?");

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Try to Solve this Riddle!",
                        Description = riddle,
                        Color = DiscordColor.Chartreuse
                    };

                    string uniqueId = Guid.NewGuid().ToString();

                    DiscordButtonComponent button = new DiscordButtonComponent(ButtonStyle.Primary, $"{uniqueId}_riddle_button", "Show the Answer");

                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                    var webhookBuilder = new DiscordWebhookBuilder()
                        .AddEmbed(embed)
                        .AddComponents(button);

                    await ctx.EditResponseAsync(webhookBuilder);

                    ctx.Client.ComponentInteractionCreated += async (s, e) =>
                    {
                        if (e.Id == $"{uniqueId}_riddle_button" && e.User.Id == ctx.User.Id)
                        {
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder()
                                    .AddEmbed(new DiscordEmbedBuilder
                                    {
                                        Title = answer,
                                        Description = new Random().Next(0, 2) == 1 ? "That's what I thought!" : "I didn't see that one comming!",
                                        Color = DiscordColor.Chartreuse
                                    }));

                            webhookBuilder = new DiscordWebhookBuilder()
                            .AddEmbed(embed)
                            .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, $"{uniqueId}_riddle_button", "Show the Answer", true));

                            await ctx.EditResponseAsync(webhookBuilder);
                        }
                    };
                }
            }
            else
            {
                await ctx.CreateResponseAsync("Couldn't fetch a riddle at the moment!");
            }
        }
    }
}
