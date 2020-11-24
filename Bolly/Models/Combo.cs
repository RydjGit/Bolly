namespace Bolly.Models
{
    public class Combo
    {
        public string Username { get; }
        public string Password { get; }
        public bool IsValid { get; }

        private const string Separator = ":";
        private const int Count = 2;

        public Combo(string combo)
        {
            string[] comboSplit = combo.Split(Separator);

            if (comboSplit.Length != Count) return;

            Username = comboSplit[0];
            Password = comboSplit[1];

            IsValid = true;
        }

        public override string ToString()
        {
            return string.Join(Separator, Username, Password);
        }
    }
}
