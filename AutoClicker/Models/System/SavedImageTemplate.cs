namespace AutoClicker.Models.System
{
    public static class SavedImageTemplate
    {
        public static ImageTemplate ImageTemplatePickaxe { get; set; }
        public static ImageTemplate ImageTemplatePaperdollWithPickaxe { get; set; }
        public static ImageTemplate ImageTemplateIron { get; set; }


        public static void Initialize()
        {
            ImageTemplatePickaxe = new ImageTemplate("Pickaxe2.png");
            ImageTemplatePaperdollWithPickaxe = new ImageTemplate("PaperdollWithPickaxe.png");
            ImageTemplateIron = new ImageTemplate("iron.png");
        }
    }
}
