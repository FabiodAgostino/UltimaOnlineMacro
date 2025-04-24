namespace AutoClicker.Models.System
{
    public static class SavedImageTemplate
    {
        public static ImageTemplate ImageTemplatePickaxe { get; set; }
        public static ImageTemplate ImageTemplatePaperdollWithPickaxe { get; set; }
        public static ImageTemplate ImageTemplateIron { get; set; }
        public static List<ImageTemplateBgra> ImagesTemplateMulo { get; set; } = new();

        public static void Initialize()
        {
            try
            {
                // Usa percorsi relativi senza la cartella Assets/Images (viene gestita in ImageTemplate)
                ImageTemplatePickaxe = new ImageTemplate("Pickaxe2.png");
                ImageTemplatePaperdollWithPickaxe = new ImageTemplate("PaperdollWithPickaxe.png");
                ImageTemplateIron = new ImageTemplate("iron.png");

                for (int i = 1; i <= 1; i++)
                {
                    string path = Path.Combine("Mulo", $"mulo{i}.png");
                    ImagesTemplateMulo.Add(new ImageTemplateBgra(path));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRORE durante l'inizializzazione di SavedImageTemplate: {ex.Message}");
                // Puoi aggiungere qui un messaggio o un altro modo per notificare l'utente
            }
        }
    }
}