namespace Medior.Models.Messages;

internal class GenericMessage<T>
    where T : notnull
{
    public GenericMessage(T value)
    {
        Value = value;
    }

    public T Value { get; }
}
