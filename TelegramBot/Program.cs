using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class Program
    {
        private const string Start = "/start";
        private const string Settings = "/settings";
        private const string Password = "МашаМилаша"; //KEK
        private const string Command1 = "Забронировать стол";
        private const string Command2 = "Соц. Сети";
        private const string Command3 = "Добавить получателя заявок";
        private const string Command4 = "Список получателей заявок";
        private const string Command5 = "Добавить Соц. Сети";
        private const string Command6 = "Список Соц. Сети";

        static TelegramBotClient Bot = new TelegramBotClient("5495390508:AAHi-SDCzmafeP9NTZOPdlaXbu5zfcGqdF4");
        static Booking booking = new Booking(Bot);
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
                    Console.WriteLine($"id: {id.ToString()} text: {text}");
                    Regex regex;

                    switch (text)
                    {
                        case Start:
                            await Bot.SendTextMessageAsync(id, "Здравствуй, давай перейдем сразу к делу!", replyMarkup: GetButtons());
                            break;
                        case Command1:
                            booking.Status = 0;
                            //Бронь стола
                            await Bot.SendTextMessageAsync(id, "Введите дату брони столика, В формате ДД.ММ.ГГГГ");
                            //CalendarPicker.Program.Main(null);
                            break;
                        case Command2:
                            booking.Status = 0;
                            await Bot.SendTextMessageAsync(id, "VK: https://vk.com/ru_agronom");
                            //соцсети
                            break;
                        case Settings:
                            await Bot.SendTextMessageAsync(id, "Введите пароль");
                            break;
                        case Password:
                            booking.Status = 0;
                            await Bot.SendTextMessageAsync(id, "Вы вошли в настройки",replyMarkup: GetSettingsButtons(id));
                            break;
                        default:
                            switch (booking.Status)
                            {
                                case 0:
                                    regex = new Regex("[0-9]{1,2}.[0-9]{1,2}.[0-9]{4}");
                                    if (regex.IsMatch(text))//Ввели дату брони
                                    {
                                        booking.Status = 1;
                                        booking.Date = text.ToString();
                                        await Bot.SendTextMessageAsync(id, "Введите время брони, в формате ЧЧ:ММ");
                                    }
                                    break;
                                case 1:
                                    regex = new Regex("[0-9]{1,2}:[0-9]{1,2}");
                                    if (regex.IsMatch(text))//Ввели время брони
                                    {
                                        booking.Status = 2;
                                        booking.Time = text.ToString();
                                        await Bot.SendTextMessageAsync(id, "Введите количество гостей");
                                    }
                                    break;
                                case 2:
                                    int _guests;
                                    if (int.TryParse(text, out _guests))
                                    {
                                        booking.Status = 3;
                                        booking.Guests = _guests;
                                        await Bot.SendTextMessageAsync(id, "Введите номер телефона");
                                    }
                                    break;
                                case 3:
                                    regex = new Regex(@"(\+7|8|\b)[\(\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[)\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)");
                                    if (regex.IsMatch(text))//Ввели номер телефона
                                    {
                                        booking.Status = 4;
                                        booking.Phone = text.ToString();
                                        await Bot.SendTextMessageAsync(id, "На чьё имя бронировать?");
                                    }
                                    break;
                                case 4:
                                    booking.Name = text;
                                    booking.Status = 5;
                                    booking.SendBooking();
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        private static IReplyMarkup GetSettingsButtons(long id, int deep = 1)
        {
            ReplyKeyboardMarkup markup = null;
            switch (deep)
            {
                case 1:
                    markup = new ReplyKeyboardMarkup(
                    new List<KeyboardButton>
                    {
                        new KeyboardButton("Добавить получателя заявок"),
                        new KeyboardButton("Список получателей заявок"),
                        new KeyboardButton("Добавить Соц. Сети"),
                        new KeyboardButton("Список Соц. Сети")
                    });
                    markup.ResizeKeyboard = true;
                    break;
                case 2:
                    break;
                default:
                    Bot.SendTextMessageAsync(id, "?");
                    break;
            }
            return markup;
        }

        private static IReplyMarkup GetDays(int days = 14)
        {
            var list = new List<KeyboardButton>();
            var listrow = new List<List<KeyboardButton>>();

            if (days > 7)
            {
                for (int i = 0; i < days / 7; i++)
                {
                    list.Clear();
                    for (int j = 0; j < 7; j++)
                    {
                        list.Add(new KeyboardButton($"{DateTime.Now.Day + i * 7 + j}"));
                    }

                    listrow.Add(new List<KeyboardButton>(list));
                }
                if (days % 7 > 0)
                {
                    for (int j = 0; j < days % 7; j++)
                    {
                        list.Add(new KeyboardButton($"{DateTime.Now.Day + days / 7 + j}"));
                    }
                }
                var markup = new ReplyKeyboardMarkup(listrow);
                markup.ResizeKeyboard = true;
                return markup;
            }
            else
            {
                for (int i = 0; i < days; i++)
                {
                    list.Add(new KeyboardButton($"{DateTime.Now.Day + i}"));
                }
                var markup = new ReplyKeyboardMarkup(list);
                markup.ResizeKeyboard = true;
                return markup;
            }
        }

        private static IReplyMarkup GetButtons()
        {
            var markup = new ReplyKeyboardMarkup(
            new List<KeyboardButton>
            {
                new KeyboardButton(Command1),
                new KeyboardButton(Command2)
            });

            markup.ResizeKeyboard = true;
            return markup;
        }
    }
}