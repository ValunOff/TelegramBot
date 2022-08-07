namespace TelegramBot
{
    class Personal
    {
        public long Id { get; set; }

        public string? Vcard { get; set; }

        public string FirstName { get; set; } = "";

        public string? LastName { get; set; } = "";

        public string PhoneNumber { get; set; } = "";
    }
}
