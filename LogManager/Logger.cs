using Serilog;

namespace LogManager
{
    public abstract class Logger
    {
        protected Logger()
        {
            LoggerConfig.Initialize();
        }
        public abstract void Initialize(object text);
        public abstract void Loggin(string message, bool err=false);

        public void Serilog(string message, bool err)
        {
            if(err)
                Log.Error(message);
            else
                Log.Debug(message);
        }

    }
}
