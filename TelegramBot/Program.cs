using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class Program
    {
        private const string Start = "/start";
        private const string Settings = "/settings";
        private const string Calendar = "/calendar";
        private const string StepBack = "Назад";
        private const string Password = "МашаМилаша"; //KEK
        private const string bron = "Забронировать стол";
        private const string social = "Соц. Сети";

        private static bool work = true; //стол бронируем и соц сети смотрим(true) или настройками занимаемя(false)
        private static int deep = 1;  // это чтобы понимать на какую команду ответ записывать
        private const string Command3 = "Добавить получателя заявок";
        private const string Command4 = "Список получателей заявок";
        private const string Command5 = "Удалить получателя заявок";
        private const string Command6 = "Изменить Соц. Сети";
        private const string Command7 = "Список Соц. Сетей";
        private const string Command8 = "Изменить график брони";
        //private const string Command9 = "Изменить пароль";

        //static string socialtext = "VK: https://vk.com/ru_agronom";
        static Settings settings = new Settings();
        static TelegramBotClient Bot = new TelegramBotClient(settings.Token);
        static Booking booking = new Booking(Bot,ref settings);
        static ManualResetEventSlim waiter = new ManualResetEventSlim(false);//нашел решение как запустить бота и чтобы при этом терминал не занимать. Только нужно обязательно запускать бота в фоне иначе придется серв перезагружать 

        static void Main(string[] args)
        {
            settings.Logger(0, -1, $"TelegramBot started");
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                {
                    UpdateType.Message //это то какие типы событий отлавливает бот, их можно добавить через запятую по необходимости
                }
            };
            Bot.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);
            waiter.Wait();
            Restart();
        }

        private static void Restart()
        {
#if DEBUG
            System.Diagnostics.Process.Start("C:\\Users\\User\\source\\repos\\TelegramBot\\TelegramBot\\bin\\Debug\\netcoreapp3.1\\TelegramBot.exe");
#endif
#if RELEASE
            System.Diagnostics.Process.Start("/root/TelegramBot/TelegramBot");
#endif
            Environment.Exit(0);
        }
        
        private static Task ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
        {//если бот сломается то он будет выполнять эти действия перед тем как отрубиться
            settings.Logger(0, -1, arg2.Message);//записываю ошибку в файл чтобы потом можно было понять что случилось
            booking.BotIsBroke(arg2);//отправляю себе сообщение в тг если бот сломался(зачастую он не может этого делать, но если сможет то поч нет LUL)
            waiter.Set();
            return Task.CompletedTask;
        }

        private static async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken arg3)
        {

            var id = update.Message.Chat.Id;

            if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text) //если бот получил сообщение ввиде текста
            {
                var text = update.Message.Text;
                settings.Logger(id, booking.Status, text);

                Regex regex;
                if (work)//проверяем бот работает или настройки меняет
                {
                    switch (text)
                    {
                        case Start:
                            await Bot.SendTextMessageAsync(id, "Здравствуйте! Рады приветствовать в нашем баре! Для бронирования столика заполните форму", replyMarkup: GetButtons());
                            break;
                        case bron:
                            booking.Status = 0;
                            await Bot.SendTextMessageAsync(id, "Введите дату брони столика, В формате ДД.ММ.ГГГГ", replyMarkup: GetButtons());
                            break;
                        case social:
                            booking.Status = 0;
                            await Bot.SendTextMessageAsync(id, settings.Social, replyMarkup: GetButtons());
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
                               
                                if (IsDate(text))//Ввели дату брони
                                {
                                    //вот эта херня с датами нужна потому что на серве даты хранятся как у пендосов мм.дд.гггг а у нас дд.мм.гггг и из за этого был тогда трабл с датами
                                    DateTime.TryParse(text, CultureInfo.CreateSpecificCulture("ru-RU"), DateTimeStyles.None, out DateTime tn);
                                    if (tn >= DateTime.Now.Date)
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
                                regex = new Regex("[0-9]{1,2} (часов|час|часа)");
                                if (regex.IsMatch(text))//Ввели часы брони
                                {
                                    booking.Status = 2;
                                    booking.Time = text.ToString();
                                    if(text == "03 часа")
                                        await Bot.SendTextMessageAsync(id, "Введите во сколько минут бронировать", replyMarkup: new ReplyKeyboardMarkup(new List<List<KeyboardButton>> {new List<KeyboardButton>{new KeyboardButton("00 минут"),new KeyboardButton(StepBack)}}) {ResizeKeyboard = true});
                                    else
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
                                regex = new Regex(@"(\+7|8|\b)[\(\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[)\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)[\s-]*(\d)");//лучше эту штуку не трогай я ее в инете нашел и она работает, но я тут вообще нихера не понимаю :D
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
                                await Bot.SendTextMessageAsync(id, "Отлично! Мы получили вашу заявку и забронировали стол! Ждём вас! (Бронь держится 15 минут)", replyMarkup: GetButtons());
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
                            await Bot.SendTextMessageAsync(id, "Отправьте контакт получателя заявок",replyMarkup: GetSettingsButtons(id));
                            break;
                        case Command4:
                            if (settings.Personals.Count > 0)
                                foreach (var item in settings.Personals)
                                {
                                    await Bot.SendContactAsync(id, item.PhoneNumber, item.FirstName, item.LastName, item.Vcard, replyMarkup: GetSettingsButtons(id));
                                }
                            else
                                await Bot.SendTextMessageAsync(id, "Список пуст", replyMarkup: GetSettingsButtons(id));
                            break;
                        case Command5:
                            deep = 3;
                            await Bot.SendTextMessageAsync(id, "Отправьте контакт сотрудника для удаления", replyMarkup: GetSettingsButtons(id));
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
                            
                            if(text != "")
                            switch (deep)
                            {
                                case 4://обновляем соцсети
                                    settings.UdateSocial(text);
                                    deep = 1;
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

            if (update.Message.Type == MessageType.Contact && !work) //если бот получил сообщение ввиде контакта и при этом он зашел в настройки(чтобы нельзя было добавлять кому попало)
            {
                string f = update.Message.Contact.FirstName;
                var l = update.Message.Contact.LastName;
                string pn = update.Message.Contact.PhoneNumber;
                if (pn[0] != '+')
                {
                    pn = "+7" + pn.TrimStart('8');
                }
                var UserId = update.Message.Contact.UserId;
                var vcard = update.Message.Contact.Vcard;
                settings.Logger(id, booking.Status, $"FirstName: {f} LastName: {l} PhoneNumber: {pn} UserId: {UserId}");

                switch (deep)
                {
                    case 2: //Добавить контакт
                        if(settings.AddPersonal(UserId, vcard, f, l, pn))
                            if(UserId == null)
                                await Bot.SendTextMessageAsync(id, "Контакт добавлен, но UserId не был определен из за чего он не будет получать уведомление о заказе стола. Отправьте еще контакт для добавления или нажмите \"Назад\"");
                            else
                                await Bot.SendTextMessageAsync(id, "Контакт добавлен. Отправьте еще контакт для добавления или нажмите \"Назад\"");
                        else
                            await Bot.SendTextMessageAsync(id, "Контакт уже существует. Отправьте другой контакт для добавления или нажмите \"Назад\"");
                        break;
                    case 3://Удалить контакт
                        if (settings.RemovePersonal(pn))
                            await Bot.SendTextMessageAsync(id, "Контакт удален. Отправьте еще контакт для удаления или нажмите \"Назад\"");
                        else
                            await Bot.SendTextMessageAsync(id, "Контакта не существует. Отправьте другой контакт для удаления или нажмите \"Назад\"");

                        break;
                }
            }
        }

        /// <summary>
        /// Проверяет соответствует ли строка формату даты
        /// </summary>
        /// <param name="text">Строка с датой</param>
        public static bool IsDate(string text)
        {
            Regex regex = new Regex("(0[1-9]|[12][0-9]|3[01]).(0[1-9]|[1][0-2]).([1][9][0-9][0-9]|[2][0][0-9][0-9])");
            if (regex.IsMatch(text)) return true;
            return false;
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
                        new KeyboardButton(Command3),
                        new KeyboardButton(Command4)

                    },
                    new List<KeyboardButton>
                    {
                        new KeyboardButton(Command5),
                        new KeyboardButton(Command8)
                    },
                    new List<KeyboardButton>
                    {
                        new KeyboardButton(Command6),
                        new KeyboardButton(Command7)
                    },
                    new List<KeyboardButton>
                    {
                        new KeyboardButton(StepBack)
                    }
                });
                markup.ResizeKeyboard = true;
                break;
                case 2:
                break;
                default:
                    Bot.SendTextMessageAsync(id, "?");//честно забыл что тут должно быть а при попыте разобраться ничего не понимаю :D
                    break;
            }
            return markup;
        }

        private static IReplyMarkup GetHours()//вот тут описан грфик брони
        {
            var markup = new ReplyKeyboardMarkup(
            new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>//строка №1
                {
                    new KeyboardButton("18 часов"),
                    new KeyboardButton("19 часов"),
                    new KeyboardButton("20 часов"),
                    new KeyboardButton("21 час")
                },
                new List<KeyboardButton>//строка №2
                {
                    new KeyboardButton("22 часа"),
                    new KeyboardButton("23 часа"),
                    new KeyboardButton("00 часов"),
                    new KeyboardButton("01 час")
                },
                new List<KeyboardButton>//строка №3
                {
                    new KeyboardButton("02 часа"),
                    new KeyboardButton("03 часа"),
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
                new List<KeyboardButton>//строка №1
                {
                    new KeyboardButton("00 минут"),
                    new KeyboardButton("15 минут"),
                    new KeyboardButton("30 минут"),
                    new KeyboardButton("45 минут")
                },
                new List<KeyboardButton>//строка №2
                {
                    new KeyboardButton(StepBack)
                }
            })
            {
                ResizeKeyboard = true
            };
            return markup;
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