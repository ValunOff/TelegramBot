using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
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
        private const string StepBack = "Назад";
        private const string Password = "МашаМилаша"; //KEK
        private const string bron = "Забронировать стол";
        private const string social = "Соц. Сети";

        private static bool work = true;
        private static int deep = 1;
        private const string Command3 = "Добавить получателя заявок";
        private const string Command4 = "Список получателей заявок";
        private const string Command5 = "Удалить получателя заявок";
        private const string Command6 = "Изменить Соц. Сети";
        private const string Command7 = "Список Соц. Сетей";
        private const string Command8 = "Изменить график брони";
        private const string Command9 = "Изменить пароль";


        //static string socialtext = "VK: https://vk.com/ru_agronom";
        static Settings settings = new Settings();
        static TelegramBotClient Bot = new TelegramBotClient(settings.Token);
        static Booking booking = new Booking(Bot,settings);
        
        static void Main(string[] args)
        {
            //Console.WriteLine($"{settings.Token}");
            Console.WriteLine("TelegramBot Started");
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                {
                    UpdateType.Message
                }
            };
            Bot.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);
            while (true) // чтобы прога не останавливалась и не занимала консоль. Но надо поменять на синхронное ожидание сообщения чтобы цикл просто так не гулял и не нагружал сервак на 100%
            {
            }
        }

        private static Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {
#if RELEASE
            using (StreamWriter writer = new StreamWriter("/root/TelegramBot/log.txt", true))
            {
                writer.WriteLineAsync(DateTime.Today.ToString() + " " + arg2.Message);
            }
            booking.BotIsBroke(arg2);
#endif

            throw arg2;
        }

        private static async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken arg3)
        {
            if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
            {
                var text = update.Message.Text;
                var id = update.Message.Chat.Id;
                Regex regex;
#if RELEASE
                using (StreamWriter writer = new StreamWriter("/root/TelegramBot/log.txt", true))
                {
                    await writer.WriteLineAsync($"{DateTime.Today.ToString()}  id: {id.ToString()} status: {booking.Status.ToString()} text: {text}");
                }
#endif
#if DEBUG
                using (StreamWriter writer = new StreamWriter("/log.txt", true))
                {
                    await writer.WriteLineAsync($"{DateTime.Today.ToString()}  id: {id.ToString()} status: {booking.Status.ToString()} text: {text}");
                }
#endif
                if (work)
                {
                    switch (text)
                    {
                        case Start:
                            await Bot.SendTextMessageAsync(id, "Здравствуй, давай перейдем сразу к делу!", replyMarkup: GetButtons());
                            break;
                        case bron:
                            booking.Status = 0;
                            //Бронь стола
                            await Bot.SendTextMessageAsync(id, "Введите дату брони столика, В формате ДД.ММ.ГГГГ", replyMarkup: GetButtons());
                            //CalendarPicker.Program.Main(null);
                            break;
                        case "/bron":
                            booking.Status = 0;
                            //Бронь стола
                            await Bot.SendTextMessageAsync(id, "Введите дату брони столика, В формате ДД.ММ.ГГГГ", replyMarkup: GetButtons());
                            //CalendarPicker.Program.Main(null);
                            break;
                        case social:
                            booking.Status = 0;
                            await Bot.SendTextMessageAsync(id, settings.Social, replyMarkup: GetButtons());
                            //соцсети
                            break;
                        case Settings:
                            await Bot.SendTextMessageAsync(id, "Введите пароль");
                            break;
                        case Password:
                            booking.Status = 0;
                            work = false;
                            await Bot.SendTextMessageAsync(id, "Вы вошли в настройки", replyMarkup: GetSettingsButtons(id));
                            break;
                        case Calendar:
                            Programm.OnMessage(id);
                            break;
                        case StepBack:
                            booking.StepBack();
                            StepBackMessage(id, booking.Status);
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
                                    CultureInfo culture = CultureInfo.CreateSpecificCulture("ru-RU");
                                    DateTimeStyles styles = DateTimeStyles.None;
                                    DateTime.TryParse(text, culture, styles, out tn);
                                    //Console.WriteLine($"dt: {dt} tn: {tn}");
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
                                    await Bot.SendTextMessageAsync(id, "Время не распозноно, воспользуйтесь кнопками", replyMarkup: GetHours());
                                }
                                break;
                            case 2:
                                regex = new Regex("[0-9]{2} минут");
                                if (regex.IsMatch(text))//Ввели минуты брони
                                {
                                    booking.Status = 3;
                                    booking.Time = text.ToString();
                                    await Bot.SendTextMessageAsync(id, "Введите количество гостей", replyMarkup: GetNullButtons());
                                }
                                else
                                {
                                    await Bot.SendTextMessageAsync(id, "Время не распознона, воспользуйтесь кнопками", replyMarkup: GetMinutes());
                                }
                                break;
                            case 3:
                                byte _guests;

                                if (byte.TryParse(text, out _guests))//Ввели количество гостей
                                {
                                    booking.Status = 4;
                                    booking.Guests = _guests;
                                    await Bot.SendTextMessageAsync(id, "Введите номер телефона", replyMarkup: GetNullButtons());
                                }
                                else
                                {
                                    await Bot.SendTextMessageAsync(id, "Количество гостей не распознано, введите целое положительное число не больше 255", replyMarkup: GetNullButtons());
                                }
                                break;
                            case 4:
                                regex = new Regex(@"(\+7|8|\b)[\(\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[)\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)");
                                if (regex.IsMatch(text))//Ввели номер телефона
                                {
                                    booking.Status = 5;
                                    booking.Phone = text.ToString();
                                    await Bot.SendTextMessageAsync(id, "На чьё имя бронировать?", replyMarkup: GetNullButtons());
                                }
                                else
                                {
                                    await Bot.SendTextMessageAsync(id, "Номер телефона не распознан, попробйте один из следующих форматов: 81234567890, +71234567890", replyMarkup: GetNullButtons());
                                }
                                break;
                            case 5:
                                booking.Name = text;
                                booking.Status = 6;
                                booking.SendBooking();
                                await Bot.SendTextMessageAsync(id, "Стол забронирован. Спасибо что воспользовались нашей услугой ", replyMarkup: GetNullButtons());
                                break;
                        }
                        break;
                    }
                }
                else
                {
                    switch (text)
                    {
                        case Command3:
                            deep = 2;
                            await Bot.SendTextMessageAsync(id, "Введите логин и ФИО сотрудника для добавления(ФИО не обязательно). В формате @логин - ФИО",replyMarkup: GetSettingsButtons(id));
                            break;
                        case Command4:
                            await Bot.SendTextMessageAsync(id, settings.SelectPersonal(), replyMarkup: GetSettingsButtons(id));
                            break;
                        case Command5:
                            deep = 3;
                            await Bot.SendTextMessageAsync(id, "Введите логин сотрудника для удаления(Можно несколько через запятую). В формате @логин", replyMarkup: GetSettingsButtons(id));
                            break;
                        case Command6:
                            deep = 4;
                            await Bot.SendTextMessageAsync(id, "Введите, что отвечать при запросе Соц. Сетей", replyMarkup: GetSettingsButtons(id));
                            break;
                        case Command7:
                            await Bot.SendTextMessageAsync(id, settings.Social, replyMarkup: GetSettingsButtons(id));
                            break;
                        case Command8:
                            deep = 5;
                            await Bot.SendTextMessageAsync(id, "Введите часы работы в формате 9-21 (круглые сутки 0-0)", replyMarkup: GetSettingsButtons(id));
                            break;
                        case StepBack:
                            if (deep > 1)
                                deep = 1;
                            else
                            {
                                work = true;
                                await Bot.SendTextMessageAsync(id, "Вы вышли из настроек", replyMarkup: GetButtons());
                            }
                            break;
                        default:
                            //прием команд
                            if(text != "")
                            switch (deep)
                            {
                                case 1:
                                    await Bot.SendTextMessageAsync(id, "", replyMarkup: GetButtons());
                                    break;
                                case 2:
                                    string[] personalAdd = text.Split('-');
                                    if (personalAdd.Length > 1)
                                    {
                                        string[] FIO = personalAdd[1].Split(' ');
                                        switch (FIO.Length)
                                        {
                                            case 1:
                                                settings.AddPersonal(personalAdd[0], FIO[0]);
                                                break;
                                            case 2:
                                                settings.AddPersonal(personalAdd[0], FIO[0], FIO[1]);
                                                break;
                                            case 3:
                                                settings.AddPersonal(personalAdd[0], FIO[0], FIO[1], FIO[2]);
                                                break;
                                            default:
                                                settings.AddPersonal(personalAdd[0]);
                                                break;
                                        }
                                    }
                                    await Bot.SendTextMessageAsync(id, $"логин {personalAdd[0]} добавлен", replyMarkup: GetNullButtons());
                                    break;
                                case 3:
                                    string[] personalDel = text.Split(',');
                                    string message = "";
                                    if (personalDel.Length > 1)
                                    {
                                        message = "Следующие логины были удалены: ";
                                        foreach (var item in personalDel)
                                        {
                                            settings.RemovePersonal(item);
                                            message += item + ", ";
                                        }
                                    }
                                    else
                                    {
                                        settings.RemovePersonal(personalDel[0]);
                                        message = $"Логин {personalDel[0]} был удален";
                                    }
                                    deep = 1;
                                    await Bot.SendTextMessageAsync(id, message, replyMarkup: GetSettingsButtons(id));
                                    break;
                                case 4:
                                    settings.UdateSocial(text);
                                    await Bot.SendTextMessageAsync(id, "Соц. Сети изменены", replyMarkup: GetSettingsButtons(id));
                                    break;
                                case 5:
                                    //надо как то настроить изменение графика
                                    break;
                                default:
                                    break;
                            }
                            break;
                    }
                }
            }
        }

        private static void StepBackMessage(long id, int status)
        {
            switch (status)
            {
                case 0://нужно ввести дату
                    Bot.SendTextMessageAsync(id, "Введите дату брони столика, В формате ДД.ММ.ГГГГ", replyMarkup: GetButtons());
                    break;
                case 1://нужно ввести часы
                    Bot.SendTextMessageAsync(id, "Выберите во сколько часов бронировать", replyMarkup: GetHours());
                    break;
                case 2://нужно ввести время
                    Bot.SendTextMessageAsync(id, "Введите во сколько минут бронировать", replyMarkup: GetMinutes());
                    break;
                case 3://нужно ввести кол-во гостей
                    Bot.SendTextMessageAsync(id, "Введите количество гостей", replyMarkup: GetNullButtons());
                    break;
                case 4://нужно ввести номер телефона
                    Bot.SendTextMessageAsync(id, "Введите номер телефона", replyMarkup: GetNullButtons()); 
                    break;
                case 5:
                    break;
                case 6:
                    break;
            }
        }

        private static IReplyMarkup GetNullButtons()
        {
            var markup = new ReplyKeyboardMarkup(
            new List<KeyboardButton>
            {
                new KeyboardButton(StepBack)
            });
            markup.ResizeKeyboard = true;
            return markup;
        }

        private static IReplyMarkup GetSettingsButtons(long id, int deep = 1)
        {
            ReplyKeyboardMarkup markup = null;
            switch (deep)
            {
                case 1:
                markup = new ReplyKeyboardMarkup(
                new List<List<KeyboardButton>>
                {
                    new List<KeyboardButton>
                    {
                        new KeyboardButton("Добавить получателя заявок"),
                        new KeyboardButton("Список получателей заявок")

                    },
                    new List<KeyboardButton>
                    {
                        new KeyboardButton("Удалить получателя заявок"),
                        new KeyboardButton("Изменить график брони")
                    },
                    new List<KeyboardButton>
                    {
                        new KeyboardButton("Добавить Соц. Сети"),
                        new KeyboardButton("Список Соц. Сетей")
                    },
                    new List<KeyboardButton>
                    {
                        new KeyboardButton("Назад")
                    }
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
                    new KeyboardButton("21 час"),
                    new KeyboardButton(StepBack)
                }

            });

            markup.ResizeKeyboard = true;
            return markup;
        }

        private static IReplyMarkup GetMinutes()
        {
            var markup = new ReplyKeyboardMarkup(
            new List<List<KeyboardButton>> {
                new List<KeyboardButton>
                {
                    new KeyboardButton("00 минут"),
                    new KeyboardButton("15 минут"),
                    new KeyboardButton("30 минут"),
                    new KeyboardButton("45 минут")
                },
                new List<KeyboardButton>
                {
                    new KeyboardButton(StepBack)
                }
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