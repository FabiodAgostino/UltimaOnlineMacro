namespace AutoClicker.Service.ExtensionMethod
{
    public static class Key
    {
        public static List<string> PopolaComboKey()
        {
            List<string> keysList = new List<string>();

            for (int i = 1; i <= 12; i++)
            {
                // Utilizziamo l'enum per recuperare la stringa (ad es. "F1")
                string keyName = ((Keys)Enum.Parse(typeof(Keys), "F" + i.ToString())).ToString();
                keysList.Add(keyName);
            }

            for (int i = 0; i <= 9; i++)
            {
                keysList.Add(i.ToString());
            }
            for (char ch = 'A'; ch <= 'Z'; ch++)
            {
                keysList.Add(ch.ToString());
            }
            string[] accentedLetters = { "À", "È", "É", "Ì", "Ò", "Ù", "à", "è", "é", "ì", "ò", "ù" };
            keysList.AddRange(accentedLetters);

            var keysOrdered = keysList.OrderBy(k => k).ToList();

            return keysOrdered;
        }
    }
}
