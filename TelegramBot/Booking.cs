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
                if (_status == 2)
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
                //string PhoneNumberPattern = @"(8|\+7)[0-9]{1,2}:[0-9]{1,2}";
                //Regex regex = new Regex(PhoneNumberPattern);
                //if (regex.IsMatch(value))
                _phone = value;
            }
        }
        /// <summary> Статус заполнения заявки на бронь
        /// <
        /// </summary>
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

        private TelegramBotClient _bot;
        private List<int> _addressee = new List<int>();
        public Booking(TelegramBotClient Bot)
        {
            if (Bot != null)
            {
                _bot = Bot;
                _addressee.Add(611371555);//надо вести список получателей заявок
            }
            else
                throw new Exception("Значение Bot не может быть Null");
        }

        /// <summary> </summary>
        public bool SendBooking()
        {
            List<MessageEntity> Type = new List<MessageEntity>();
            Type.Add(new MessageEntity() { Type = MessageEntityType.Hashtag });
            byte[] qwe;
            qwe = Encoding.ASCII.GetBytes("#");
            if (_status == 6 && _name != "")
                foreach (var item in _addressee)
                {
                    MessageEntity qweqwe = new MessageEntity();
                    qweqwe.Type = MessageEntityType.Hashtag;
                    _bot.SendTextMessageAsync(item, $"На имя {_name} забронирован стол на " + Encoding.ASCII.GetString(qwe) + $"d{_date.Replace('.', '_')} в {_time} на {_guests} человек(а). Телефон:{_phone}", entities: Type);
                }
            _status = 0;
            return true;
        }
    }
}
