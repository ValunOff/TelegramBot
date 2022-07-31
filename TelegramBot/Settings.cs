using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class Settings
    {
        public string Token { set; get; }

        public string Social { set; get; }

        public List<Personal> Personals { get; set; }

        public List<List<KeyboardButton>> HoursButtons { get; set; }

        public Settings()
        {
#if RELEACE
            string fileName = "/root/TelegramBot/Settings.json";
#endif
#if DEBUG
            string fileName = "Settings.json";
#endif
            string jsonString = File.ReadAllText(fileName);
            Setting settings = JsonSerializer.Deserialize<Setting>(jsonString);
            Token = settings.Token;
            Social = settings.Social;
            Personals = settings.Personals;
            HoursButtons = settings.HoursButtons;
        }

        /// <summary>Возвращает список сотрудников в формате @Логин - ФИО</summary>
        /// <returns>1 строка - 1 сотрудник</returns>
        public string SelectPersonal()
        {
            //Select
            return "Нету сотрудиков";
        }

        public void RemovePersonal(string Login)
        {
            if (Login[0] != '@')
                Login = "@" + Login;
            //Remove
        }

        public void AddPersonal(string Login, string F ="",string I="",string O="")
        {
            if (Login[0] != '@')
                Login = "@" + Login;
            //Add
        }

        public void UdateSocial(string socialText)
        {

        }
    }
}
