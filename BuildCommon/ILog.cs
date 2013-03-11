namespace BuildCommon
{
    public interface ILog
    {
        void Information(string text);
        void Warning(string text);
        void Error(string text);
    }
}