using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram_Messaging_Bot_2022
{
    class Program
    {
        static TelegramBotClient Bot = new TelegramBotClient("5495390508:AAHi-SDCzmafeP9NTZOPdlaXbu5zfcGqdF4");
        static void Main(string[] args)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                {
                    UpdateType.Message
                }
            };
            
            Bot.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);

            Console.ReadLine();
        }

        private static Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
            throw new NotImplementedException();
        }

        private static async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken arg3)
        {
            if (update.Type == UpdateType.Message)
            {
                if (update.Message.Type == MessageType.Text)
                {
                    var text = update.Message.Text;
                    var id = update.Message.Chat.Id;
                    switch (text)
                    {
                        case "/command1":
                            await Bot.SendTextMessageAsync(id, "На какую дату бронируем?");
                            await Bot.AnswerCallbackQueryAsync("Ответ");
                            break;
                        case "/command2":
                            //соцсети
                            break;
                        default:
                            Console.WriteLine("Такой команды не обнаружено");
                            break;
                    }
                    Console.WriteLine($"{text}");
                }
            }
        }
    }
}