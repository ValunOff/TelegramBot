using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    /// <summary>
    /// Класс нужен только чтобы записывать или считывать настройки с файла Settings.json
    /// </summary>
    class Setting
    {
        public string Token { set; get; }

        public string Social { set; get; }

        public List<Contact> Personals { get; set; }

        public List<List<KeyboardButton>> HoursButtons { get; set; }
    }
}
