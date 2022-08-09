namespace TelegramBot
{
    /// <summary>
    /// класс нужен только чтобы записать и считать контакты тех кого нужно уведомлять и служит для удобства
    /// </summary>
    class Personal
    {
        public long Id { get; set; }

        public string? Vcard { get; set; }

        public string FirstName { get; set; } = "";

        public string? LastName { get; set; } = "";

        public string PhoneNumber { get; set; } = "";
    }
}
