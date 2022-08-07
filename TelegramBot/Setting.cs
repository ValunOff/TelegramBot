﻿using System.Collections.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class Setting
    {
        public string Token { set; get; }

        public string Social { set; get; }

        public List<Contact> Personals { get; set; }

        public List<List<KeyboardButton>> HoursButtons { get; set; }
    }
}
