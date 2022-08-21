using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

        Setting settings;//объект для записи и считывания настроек из файла Settings.json

        public Settings()
        {
            settings = JsonSerializer.Deserialize<Setting>(System.IO.File.ReadAllText(GetFileName()));//считываем настройки из файла Settings.json
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
                select true).Count<bool>() == 1)//если есть контакт с таким же номером телефона то удаляем его. Если не проверять то бот будет ломаться
            {
                Personals.Remove(settings.Personals.Single(r => r.PhoneNumber == PhoneNumber));

                settings.Personals.Remove(settings.Personals.Single(r => r.PhoneNumber == PhoneNumber));
                System.IO.File.WriteAllText(GetFileName(), JsonSerializer.Serialize(settings));//обновляем файл Settings.json
                return true;
            }
            return false;
        }

        public bool AddPersonal(long? UserId,string Vcard,string FirstName, string LastName = "",string PhoneNumber = "")
        {
            Contact personal = new Contact() { FirstName= FirstName,LastName= LastName ,PhoneNumber = PhoneNumber,UserId= UserId ,Vcard = Vcard };
            if ((from p in settings.Personals
                where p.PhoneNumber == PhoneNumber
                select true).Count<bool>()==0)//Если телефон чувака отсутствует в списке получателей заявок то добавляем его. Если не проверять и добавить чувака несколько раз то уведомления будут приходить несколько раз  P.s. надо бы проверить как будет себя вести этот блок кода если добавлять несколько контактов без номера телефона
            {
                Personals.Add(personal);

                //settings.Personals.Add(personal);
                System.IO.File.WriteAllText(GetFileName(), JsonSerializer.Serialize(settings));//обновляем файл Settings.json
                return true;
            }
            return false;
        }

        public bool UdateSocial(string socialText)
        {
            Social = socialText;

            settings.Social = socialText;
            System.IO.File.WriteAllText(GetFileName(), JsonSerializer.Serialize(settings)); //обновляем файл Settings.json
            return true;
        }

        public void Logger(long Id,int status, string Text)
        {
            //если запускаешь бота на серве(Release) то один путь, а если отлаживаешь у себя на компе(Debug) то другой
            //только не забудь правильно сменить этот параметр когда собираешь прогу, а то прыгать по папкам придется на серве
#if RELEASE
            using (StreamWriter writer = new StreamWriter("/root/TelegramBot/log.txt", true))
            {
                writer.WriteLineAsync($"{DateTime.Now.ToString(CultureInfo.GetCultureInfo("ru-RU"))}  id: {Id} status: {status} text: {Text}");
            }
#endif
#if DEBUG
            using (StreamWriter writer = new StreamWriter("log.txt", true))
            {
                writer.WriteLineAsync($"{DateTime.Now.ToString(CultureInfo.GetCultureInfo("ru-RU"))}  id: {Id} status: {status} text: {Text}");
            }
#endif
        }

        private string GetFileName()
        {
            //если запускаешь бота на серве(Release) то один путь, а если отлаживаешь у себя на компе(Debug) то другой
            //только не забудь правильно сменить этот параметр когда собираешь прогу, а то прыгать по папкам придется на серве
#if RELEASE
            return "/root/TelegramBot/Settings.json";
#endif
#if DEBUG
            return "Settings.json";
#endif
        }
    }
}
