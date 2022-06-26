using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Exceptions;
using Newtonsoft.Json;
using System.Net;
using myApi.Model;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading;
using tgBot.Constant;
namespace tgBot
{
    public class tgbot
    {
        WebClient webClient = new WebClient();
        TelegramBotClient botClient = new TelegramBotClient("5452166004:AAGeonDHUNyHoGLdX1ecWFSs-ft6dn3sY5I");
        private string valcode;
        private string coin;
        private string newExchange;
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
            Console.ReadKey();
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПІ:\n {apiRequestException.ErrorCode}" +
                $"\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandlerMessageAsync(botClient, update.Message);
            }
            if (update?.Type == UpdateType.CallbackQuery)
            {
                await HandlerCallbackQuery(botClient, update.CallbackQuery);
            }
        }
        private async Task HandlerCallbackQuery(ITelegramBotClient botClient, CallbackQuery? callbackQuery)
        {
            if (callbackQuery.Message.Text == "Виберіть валюту:")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new
                  (
                      new[]
                      {
                      new KeyboardButton [] { "Прогноз курса на наступний місяць"},
                      new KeyboardButton[] { "Сьогодні" },
                      new KeyboardButton[] { "Інша дата" }
                      }
                   )
                {
                    ResizeKeyboard = true
                };
                valcode = callbackQuery.Data;
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Виберіть дату за яку бажаєте отримати курс {callbackQuery.Data}",
                replyMarkup: replyKeyboardMarkup);
            }
            else if (callbackQuery.Message.Text == "Виберіть криптовалюту:")
            {
                coin = callbackQuery.Data;
                ReplyKeyboardMarkup replyKeyboardMarkup = new
                  (
                      new[]
                      {
                        new KeyboardButton [] { "Ввести біржу"},
                        new KeyboardButton [] { "Обрані біржі"}
                      }
                  )
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Виберіть біржу на якій бажаєте переглянути курс {callbackQuery.Data}",
                replyMarkup: replyKeyboardMarkup);
            }          
            return;
        }
        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {

            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Відкрийте меню: /menu");
                return;
            }
            else if (message.Text == "/menu")
            {
                ReplyKeyboardMarkup replyKeyboardMarkup = new
                    (
                    new[]
                        {
                        new KeyboardButton [] { "Курс валют"},
                        new KeyboardButton [] { "Курс криптовалют"}
                        }
                    )
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть пункт меню:", replyMarkup: replyKeyboardMarkup);
                return;
            }
            else if (message.Text == "Курс валют")
            {
                InlineKeyboardMarkup keyboardMarkup = new
                       (
                           new[]
                           {
                            new[]
                            {
                            InlineKeyboardButton.WithCallbackData("EUR", $"EUR"),
                            InlineKeyboardButton.WithCallbackData("USD", $"USD"),
                            InlineKeyboardButton.WithCallbackData("PLN", $"PLN")
                            },
                              new[]
                            {
                            InlineKeyboardButton.WithCallbackData("CNY", $"CNY"),
                            InlineKeyboardButton.WithCallbackData("GBP", $"GBP"),
                            InlineKeyboardButton.WithCallbackData("CZK", $"CZK")
                            }

                           }

                       );         
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть валюту:", replyMarkup: keyboardMarkup);
                return;
            }          
            else if (message.Text == "Прогноз курса на наступний місяць")
            {
                string Date = DateTime.Now.ToString("yyyyMMdd");
                string nextMonth = DateTime.Now.AddDays(+30).ToString("yyyyMMdd");
                string pastyearNextMonth = nextMonth.Insert(3, ((Convert.ToInt32($"{Date[3]}")) - 1).ToString()).Remove(4, 1);
                string pastyearToday = Date.Insert(3, ((Convert.ToInt32($"{Date[3]}")) - 1).ToString()).Remove(4, 1);
                string consolenextMonth = $"{nextMonth[6]}" + $"{nextMonth[7]}" + "." + $"{nextMonth[4]}" + $"{nextMonth[5]}" + "." + $"{nextMonth[0]}" + $"{nextMonth[1]}"
                    + $"{nextMonth[2]}" + $"{nextMonth[3]}";

                var json = webClient.DownloadString($"{Constants.address}/ExchangeRate{Date}/{valcode}");
                var result = JsonConvert.DeserializeObject<List<ExchangeDate>>(json);

                var json1 = webClient.DownloadString($"{Constants.address}/ExchangeRate{pastyearNextMonth}/{valcode}");
                var result_pastyearTomorrow = JsonConvert.DeserializeObject<List<ExchangeDate>>(json1);

                var json2 = webClient.DownloadString($"{Constants.address}/ExchangeRate{pastyearToday}/{valcode}");
                var result_pastyearToday = JsonConvert.DeserializeObject<List<ExchangeDate>>(json2);

                await botClient.SendTextMessageAsync(message.Chat.Id, $"Наступного місяця: {consolenextMonth }\n" +
                  $"Курс {result.FirstOrDefault().txt.ToLower()} до гривні становитиме:\n1 {result.FirstOrDefault().cc} = " +
                  $"{(float)(result.FirstOrDefault().rate + ((result_pastyearToday.FirstOrDefault().rate) - (result_pastyearTomorrow.FirstOrDefault().rate)))}({(float)((result_pastyearToday.FirstOrDefault().rate) - (result_pastyearTomorrow.FirstOrDefault().rate))}) UAH",
                 parseMode: ParseMode.Markdown);
                 return;
            }
            else if (message.Text == "Сьогодні")
            {
                string Date = DateTime.Now.ToString("yyyyMMdd");
                var json = webClient.DownloadString($"{Constants.address}/ExchangeRate{Date}/{valcode}");
                var result = JsonConvert.DeserializeObject<List<ExchangeDate>>(json);
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Сьогодні: {result.FirstOrDefault().exchangedate}\n" +
                    $"Курс {result.FirstOrDefault().txt.ToLower()} до гривні становить:\n1 {result.FirstOrDefault().cc} = {result.FirstOrDefault().rate} UAH",
                    parseMode: ParseMode.Markdown);
                return;
            }
            else if (message.Text == "Інша дата")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть дату(dd.MM.yyyy)", replyMarkup: new ForceReplyMarkup { Selective = true });

            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Введіть дату(dd.MM.yyyy)"))
            {
                string[] dates = message.Text.Split('.');
                string Date = "";
                for (int i = dates.Length - 1; i >= 0; i--)
                {
                    Date += dates[i];
                }
                var json = webClient.DownloadString($"{Constants.address}/ExchangeRate{Date}/{valcode}");
                var result = JsonConvert.DeserializeObject<List<ExchangeDate>>(json);
                if (result.FirstOrDefault().exchangedate != null)
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Станом на: {result.FirstOrDefault().exchangedate}\n" +
                        $"Курс {result.FirstOrDefault().txt.ToLower()} до гривні становив:\n1 {result.FirstOrDefault().cc} = {result.FirstOrDefault().rate} UAH",
                        parseMode: ParseMode.Markdown);
                    return;
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Дата записана у неправильному форматі поверніться до /menu та спробуйте ще раз!",
                    parseMode: ParseMode.Markdown);
                    return;
                }
            }
            else if (message.Text == "Курс криптовалют")
            {
                InlineKeyboardMarkup keyboardMarkup = new
                      (
                          new[]
                          {
                            new[]
                            {
                            InlineKeyboardButton.WithCallbackData("BTC", $"bitcoin"),
                            InlineKeyboardButton.WithCallbackData("ETH", $"ethereum"),
                            InlineKeyboardButton.WithCallbackData("Doge", $"dogecoin")
                            },
                             new[]
                             {
                            InlineKeyboardButton.WithCallbackData("SOL", $"solana"),
                            InlineKeyboardButton.WithCallbackData("TRX", $"tron"),
                            InlineKeyboardButton.WithCallbackData("XMR", $"monero")
                             }
                          }
                      );
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть криптовалюту:", replyMarkup: keyboardMarkup);
                return;
            }

            else if (message.Text == "Ввести біржу:")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Введіть біржу", replyMarkup: new ForceReplyMarkup { Selective = true });
            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text.Contains("Введіть біржу"))
            {
                newExchange = message.Text;
                var json = webClient.DownloadString($"{Constants.address}/ExchangeRateCrypt/{coin}/{newExchange}");
                var result = JsonConvert.DeserializeObject<CryptExchangeBurse>(json);
                if (result.tickers.FirstOrDefault().market.identifier == newExchange.ToLower())
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"На біржі {result.tickers.FirstOrDefault().market.name}\n" +
                        $"Курс {result.name} становить:" +
                         $"\n{result.tickers.FirstOrDefault().converted_Last.usd} USD\n" +
                         $"{result.tickers.FirstOrDefault().converted_Last.btc} BTC", parseMode: ParseMode.Markdown);
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Введіть /add якщо бажаєте додати цю біржу до обраних", parseMode: ParseMode.Markdown);
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Введіть /delete якщо бажаєте видалити цю біржу з обраних", parseMode: ParseMode.Markdown);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Біржа введена не коректно або не існує в даній програмі певерніться до /menu та спробуйте ще раз!", parseMode: ParseMode.Markdown);
                }
            }
            else if (message.Text == "/add")
            {
                string newExchangeBd = "";
                var json1 = webClient.DownloadString($"{Constants.address}/CryptBD{message.From.Id}");
                var result1 = JsonConvert.DeserializeObject<favmarket>(json1);
                if (result1 != null)
                {
                    string[] favorites = result1.Exchange.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    List<string> fav = new List<string>();
                    foreach (var i in favorites)
                    {
                        fav.Add(i);
                    }
                    fav.Add(newExchange.ToLower());
                    var res = fav.Distinct();

                    foreach (var i in res)
                    {
                        newExchangeBd += " " + i;
                    }
                }
                else
                {
                    newExchangeBd = newExchange;
                }
                var data = new CryptExchangeDb
                {
                    ID = message.From.Id.ToString(),
                    Exchange = newExchangeBd

                };
                var json2 = JsonConvert.SerializeObject(data);
                var Data = new StringContent(json2, Encoding.UTF8, "application/json");
                var url = $"{Constants.address}/CryptBD";
                using var client = new HttpClient();
                await client.PostAsync(url, Data);
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Біржу {newExchange} додано до обраних!", parseMode: ParseMode.Markdown);
            }
            else if (message.Text == "Обрані біржі")
            {
                var json1 = webClient.DownloadString($"{Constants.address}/CryptBD{message.From.Id}");
                var result1 = JsonConvert.DeserializeObject<favmarket>(json1);
                if (result1 != null)
                {
                    string[] favorites = result1.Exchange.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in favorites)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"{item}", parseMode: ParseMode.Markdown);
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"У вас немає обраних бірж поверніться до /menu та спробуйте ще раз!", parseMode: ParseMode.Markdown);
                }
            }
            else if (message.Text == "/deleteall")
            {
                var result = $"{Constants.address}/CryptBD{message.From.Id}";
                using var client = new HttpClient();
                await client.DeleteAsync(result);
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Видалено ваші обрані біржі!", parseMode: ParseMode.Markdown);
            }
            else if (message.Text == "/delete")
            {
                string deleteExchange = newExchange;
                var json = webClient.DownloadString($"{Constants.address}/CryptBD{message.From.Id}");
                var result = JsonConvert.DeserializeObject<favmarket>(json);
                using var client = new HttpClient();
                if (result != null && newExchange != null && result.Exchange.Contains(newExchange) == true)
                {
                    newExchange = result.Exchange.Replace(newExchange, null);
                    string[] exchanges = newExchange.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (exchanges.Length == 0)
                    {
                        var result1 = $"{Constants.address}/CryptBD{message.From.Id}";

                        await client.DeleteAsync(result1);
                        return;
                    }
                    string resultExchange = null;
                    foreach (string s in exchanges)
                    {
                        resultExchange += " " + s;
                    }
                    resultExchange = resultExchange.Remove(0, 1);

                    var data = new CryptExchangeDb
                    {
                        ID = message.From.Id.ToString(),
                        Exchange = resultExchange

                    };
                    var json2 = JsonConvert.SerializeObject(data);
                    var Data = new StringContent(json2, Encoding.UTF8, "application/json");
                    var url = $"{Constants.address}/CryptBD";
                    await client.PostAsync(url, Data);
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Видалено біржу {deleteExchange} з ваших обраних", parseMode: ParseMode.Markdown);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"{deleteExchange} немає у ваших обраних", parseMode: ParseMode.Markdown);
                }
            }
        }

    }
}
