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
        private const string Calendar = "/calendar";
        private const string Password = "МашаМилаша"; //KEK
        private const string bron = "/bron";
        private const string social = "/social";
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
            Console.WriteLine("Start");
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
                        case bron:
                            booking.Status = 0;
                            //Бронь стола
                            await Bot.SendTextMessageAsync(id, "Введите дату брони столика, В формате ДД.ММ.ГГГГ");
                            //CalendarPicker.Program.Main(null);
                            break;
                        case social:
                            booking.Status = 0;
                            await Bot.SendTextMessageAsync(id, "VK: https://vk.com/ru_agronom");
                            //соцсети
                            break;
                        case Settings:
                            await Bot.SendTextMessageAsync(id, "Введите пароль");
                            break;
                        case Password:
                            booking.Status = 0;
                            await Bot.SendTextMessageAsync(id, "Вы вошли в настройки", replyMarkup: GetSettingsButtons(id));
                            break;
                        case Calendar:
                            Programm.OnMessage(id);
                            break;
                        default:
                            switch (booking.Status)
                            {
                                case 0:
                                    regex = new Regex("[0-9]{1,2}.[0-9]{1,2}.[0-9]{4}");
                                    if (regex.IsMatch(text))//Ввели дату брони
                                    {
                                        DateTime dt = DateTime.Now.Date;
                                        DateTime tn;
                                        DateTime.TryParse(text, out tn);
                                        if (tn >= dt)
                                        {
                                            booking.Status = 1;
                                            booking.Date = text.ToString();
                                            await Bot.SendTextMessageAsync(id, "Выберите во сколько часов бронировать", replyMarkup: GetHours());
                                        }
                                        else
                                        {
                                            await Bot.SendTextMessageAsync(id, "Нельзя создавать бронь задним числом");
                                        }

                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(id, "Дата брони не распознона, введите дату в формате ДД.ММ.ГГГГ");
                                    }
                                    break;
                                case 1:
                                    regex = new Regex("[0-9]{1,2} часов");
                                    if (regex.IsMatch(text))//Ввели часы брони
                                    {
                                        booking.Status = 2;
                                        booking.Time = text.ToString();
                                        await Bot.SendTextMessageAsync(id, "Введите во сколько минут бронировать", replyMarkup: GetMinutes());
                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(id, "Время не распознона, воспользуйтесь кнопками");
                                    }
                                    break;
                                case 2:
                                    regex = new Regex("[0-9]{2} минут");
                                    if (regex.IsMatch(text))//Ввели минуты брони
                                    {
                                        booking.Status = 3;
                                        booking.Time = text.ToString();
                                        await Bot.SendTextMessageAsync(id, "Введите количество гостей", replyMarkup: null);
                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(id, "Время не распознона, воспользуйтесь кнопками");
                                    }
                                    break;
                                case 3:
                                    byte _guests;

                                    if (byte.TryParse(text, out _guests))//Ввели количество гостей
                                    {
                                        booking.Status = 4;
                                        booking.Guests = _guests;
                                        await Bot.SendTextMessageAsync(id, "Введите номер телефона");
                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(id, "Количество гостей не распознано, введите целое положительное число не больше 255");
                                    }
                                    break;
                                case 4:
                                    regex = new Regex(@"(\+7|8|\b)[\(\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[)\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)");
                                    if (regex.IsMatch(text))//Ввели номер телефона
                                    {
                                        booking.Status = 5;
                                        booking.Phone = text.ToString();
                                        await Bot.SendTextMessageAsync(id, "На чьё имя бронировать?");
                                    }
                                    else
                                    {
                                        await Bot.SendTextMessageAsync(id, "Номер телефона не распознан, попробйте один из следующих форматов: 81234567890, +71234567890");
                                    }
                                    break;
                                case 5:
                                    booking.Name = text;
                                    booking.Status = 6;
                                    booking.SendBooking();
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        private static IReplyMarkup GetNullButtons()
        {
            var markup = new ReplyKeyboardMarkup(
            new List<KeyboardButton>
            {
                null
            });
            return markup;
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

        private static IReplyMarkup GetHours()
        {
            var markup = new ReplyKeyboardMarkup(
            new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>
                {
                    new KeyboardButton("9 часов"),
                    new KeyboardButton("10 часов"),
                    new KeyboardButton("11 часов"),
                    new KeyboardButton("12 часов")
                },
                new List<KeyboardButton>
                {
                    new KeyboardButton("13 часов"),
                    new KeyboardButton("14 часов"),
                    new KeyboardButton("15 часов"),
                    new KeyboardButton("16 часов")
                },
                new List<KeyboardButton>
                {
                    new KeyboardButton("17 часов"),
                    new KeyboardButton("18 часов"),
                    new KeyboardButton("19 часов"),
                    new KeyboardButton("20 часов")
                },
                new List<KeyboardButton>
                {
                    new KeyboardButton("21 час")
                }

            });

            markup.ResizeKeyboard = true;
            return markup;
        }

        private static IReplyMarkup GetMinutes()
        {
            var markup = new ReplyKeyboardMarkup(
            new List<KeyboardButton>
            {
                new KeyboardButton("00 минут"),
                new KeyboardButton("15 минут"),
                new KeyboardButton("30 минут"),
                new KeyboardButton("45 минут")
            });

            markup.ResizeKeyboard = true;
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
                new KeyboardButton(bron),
                new KeyboardButton(social)
            });

            markup.ResizeKeyboard = true;
            return markup;
        }
    }
}