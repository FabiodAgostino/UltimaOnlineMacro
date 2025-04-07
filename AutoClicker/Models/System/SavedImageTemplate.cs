namespace AutoClicker.Models.System
{
    public static class SavedImageTemplate
    {
        public static ImageTemplate ImageTemplatePickaxe { get; set; }

        public static void Initialize()
        {
            ImageTemplatePickaxe = new ImageTemplate("Pickaxe.png");
        }
    }
}
