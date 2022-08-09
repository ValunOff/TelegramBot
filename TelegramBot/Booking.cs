using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{/// <summary> класс для создания заявки на бронь стола</summary>
    class Booking
    {
        /// <summary> Дата бронирования</summary>
        public string Date
        {
            set
            {
                string DatePattern = "[0-9]{1,2}.[0-9]{1,2}.[0-9]{4}";
                Regex regex = new Regex(DatePattern);
                if (regex.IsMatch(value))
                    _date = value;
            }
        }
        /// <summary> Время бронирования</summary>
        public string Time
        {
            set
            {
                if (_status == 2)//складываем часы и минуты
                    _time = value;
                else if (_status == 3)
                    _time += " " + value;
            }
        }
        /// <summary> Количество гостей</summary>
        public int Guests { set => _guests = value > 0 ? value : 0; }
        /// <summary> Имя на кого бронируют</summary>
        public string Name { set => _name = value != "" ? value : "Не указано"; }
        /// <summary> Телефон на кого бронируют</summary>
        public string Phone
        {
            set
            {
                //по хорошему надо проверять на валидность чтобы кракозябры не записывали но я это делал извне поэтому закоментил часть тут потому что лодырь)))
                //string PhoneNumberPattern = @"(8|\+7)[0-9]{1,2}:[0-9]{1,2}";
                //Regex regex = new Regex(PhoneNumberPattern);
                //if (regex.IsMatch(value))
                _phone = value;
            }
        }
        /// <summary> Статус заполнения заявки на бронь</summary>
        public int Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (value >= 0 && value < 7)
                {
                    _status = value;
                }
            }
        }

        private string _date;
        private string _time;
        private int _guests;
        private string _name;
        private string _phone;
        private int _status;
        private int _admins;//надо сделать массив(а лучше List) c челами кому писать что бот сломался

        private TelegramBotClient _bot;
        private Settings settings;
        public Booking(TelegramBotClient Bot,ref Settings settings)
        {
            if (Bot != null)
            {
                _bot = Bot;
                this.settings = settings;
                _admins = 611371555;//это я да) 
            }
            else
                throw new Exception("Значение Bot не может быть Null");
        }

        /// <summary>Уменьшает статус заявки на 1 уровень. Нужно если пользователь на прошлом этапе совершил ошибку и хочет исправить</summary>
        /// <returns>True - Успешно, False - Не успешно</returns>
        public bool StepBack()
        {
            if (_status > 0)
            {
                _status -= 1;
                return true;
            } 
            else
            {
                return false;
            }
        }
        public bool BotIsBroke(Exception e)
        {
            _bot.SendTextMessageAsync(_admins,"Телеграм бот сломался");
            _bot.SendTextMessageAsync(_admins, "Текст ошибки:" + e.Message);
            return true;
        }
        /// <summary>Отправка заявки</summary>
        public bool SendBooking()
        {
            //это я телеграму говорю что в след сообщении точно есть хэштег
            List<MessageEntity> Type = new List<MessageEntity>();
            Type.Add(new MessageEntity() { Type = MessageEntityType.Hashtag });

            byte[] qwe;
            //хэштег используется для того чтобы в телеге можно было посмотреть сколько заказов сделано на определенный день просто зажатием на него
            qwe = Encoding.ASCII.GetBytes("#");//у меня с хэштегом были проблемы И ВРОДЕБЫ это решение проблемы но это не точно
            if (_status == 6 && _name != "")
                foreach (var item in settings.Personals) //перебираем всех получателей заявок и отправляем каждому сообщение
                {
                    _bot.SendTextMessageAsync(item.UserId, $"На имя {_name} забронирован стол на " + Encoding.ASCII.GetString(qwe) + $"d{_date.Replace('.', '_')} в {_time} на {_guests} человек(а). Телефон:{_phone}", entities: Type);
                }
            _status = 0;
            return true;
        }
    }
}
