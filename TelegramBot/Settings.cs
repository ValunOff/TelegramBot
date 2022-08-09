using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    class Settings
    {
        public string Token { set; get; }

        public string Social { set; get; }

        public List<Contact> Personals { get; set; }

        public List<List<KeyboardButton>> HoursButtons { get; set; }

        Setting settings;

        public Settings()
        {
            string jsonString = System.IO.File.ReadAllText(GetFileName());
            settings = JsonSerializer.Deserialize<Setting>(jsonString);
            Token = settings.Token;
            Social = settings.Social;
            Personals = settings.Personals;
            HoursButtons = settings.HoursButtons;
        }

        /// <summary>Возвращает список сотрудников в формате @Логин - ФИО</summary>
        /// <returns>1 строка - 1 сотрудник</returns>
        //public string SelectPersonal()
        //{
        //    string personal = "";
        //    foreach (var item in settings.Personals)
        //    {
        //        string fam = item.FirstName == "" ? "" : item.FirstName + " "; 
        //        string name = item.LastName == "" ? "" : item.LastName + " ";
        //        string otch = item.PhoneNumber == "" ? "" : item.PhoneNumber + " ";
        //        personal += $"{item.Login} - {fam} {name} {otch}";
        //    }
        //    if (personal == "")
        //        return "Список получателей пуст";
        //    return personal;
        //}

        public bool RemovePersonal(string PhoneNumber)
        {
            if((from p in settings.Personals
                where p.PhoneNumber == PhoneNumber
                select true).Count<bool>() == 1)
            {
                var itemToRemove = settings.Personals.Single(r => r.PhoneNumber == PhoneNumber);
                settings.Personals.Remove(itemToRemove);

                string jsonString = JsonSerializer.Serialize(settings);
                System.IO.File.WriteAllText(GetFileName(), jsonString);
                return true;
            }
            return false;
        }

        public bool AddPersonal(long? UserId,string Vcard,string FirstName, string LastName = "",string PhoneNumber = "")
        {
            Contact personal = new Contact() { FirstName= FirstName,LastName= LastName ,PhoneNumber = PhoneNumber,UserId= UserId ,Vcard = Vcard };
            if ((from p in settings.Personals
                where p.PhoneNumber == PhoneNumber
                select true).Count<bool>()!=1)
            {
                Personals.Add(personal);
                settings.Personals.Add(personal);

                string jsonString = JsonSerializer.Serialize(settings);
                System.IO.File.WriteAllText(GetFileName(), jsonString);
                return true;
            }
            return false;
        }

        public bool UdateSocial(string socialText)
        {
            Social = socialText;
            settings.Social = socialText;
            string jsonString = JsonSerializer.Serialize(settings);
            System.IO.File.WriteAllText(GetFileName(), jsonString);
            return true;
        }

        public void Logger(long Id,int status, string Text)
        {
#if RELEASE
            using (StreamWriter writer = new StreamWriter("/root/TelegramBot/log.txt", true))
            {
                writer.WriteLineAsync($"{DateTime.Now}  id: {Id} status: {status} text: {Text}");
            }
#endif
#if DEBUG
            using (StreamWriter writer = new StreamWriter("/log.txt", true))
            {
                writer.WriteLineAsync($"{DateTime.Now}  id: {Id} status: {status} text: {Text}");
            }
#endif
        }

        private string GetFileName()
        {
#if RELEASE
            return "/root/TelegramBot/Settings.json";
#endif
#if DEBUG
            return "Settings.json";
#endif
        }
    }
}
