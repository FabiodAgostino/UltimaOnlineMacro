namespace AutoClicker.Service
{
    public class AutoClickerLogger : LogManager.Logger
    {

        public AutoClickerLogger()
        {
        }

        public override void Initialize(object textBox)
        {
        }

        public override void Loggin(string message, bool err = false)
        {
            Serilog(message, err);
        }
    }
}
