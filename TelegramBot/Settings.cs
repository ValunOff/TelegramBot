using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        Setting settings;

        public Settings()
        {
#if RELEASE
            string fileName = "/root/TelegramBot/Settings.json";
#endif
#if DEBUG
            string fileName = "Settings.json";
#endif
            string jsonString = File.ReadAllText(fileName);
            settings = JsonSerializer.Deserialize<Setting>(jsonString);
            Token = settings.Token;
            Social = settings.Social;
            Personals = settings.Personals;
            HoursButtons = settings.HoursButtons;
        }

        /// <summary>Возвращает список сотрудников в формате @Логин - ФИО</summary>
        /// <returns>1 строка - 1 сотрудник</returns>
        public string SelectPersonal()
        {
            string personal = "";
            foreach (var item in settings.Personals)
            {
                string fam = item.Fam == "" ? "" : item.Fam + " ";
                string name = item.Name == "" ? "" : item.Name + " ";
                string otch = item.Otch == "" ? "" : item.Otch + " ";
                personal += $"{item.Login} - {fam} {name} {otch}";
            }
            if (personal == "")
                return "Список получателей пуст";
            return personal;
        }

        public void RemovePersonal(string Login)
        {
            if (Login[0] != '@')
                Login = "@" + Login;

            var itemToRemove = settings.Personals.Single(r => r.Login == Login);
            settings.Personals.Remove(itemToRemove);

#if RELEASE
            string fileName = "/root/TelegramBot/Settings.json";
#endif
#if DEBUG
            string fileName = "Settings.json";
#endif
            string jsonString = JsonSerializer.Serialize(settings);
            File.WriteAllText(fileName, jsonString);
        }

        public void AddPersonal(string Login, string F ="",string I="",string O="")
        {
            if (Login[0] != '@')
                Login = "@" + Login;
            Personal personal = new Personal() { Login = Login, Fam = F, Name = I, Otch = O };
            //settings.Personals.Add()
        }

        public void UdateSocial(string socialText)
        {

        }
    }
}
