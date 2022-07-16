namespace Customer.Api.Loggings
{
    public interface ILoggerAdapter<T>
    {
        void LogWarning(string message, params string [] arg );
    }
}
