namespace DI
{
    public interface IMessage
    {
        string Content { get; }
    }

    public class ExampleMessage : IMessage
    {
        public string Content { get; }
    }
}