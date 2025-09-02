using Google.Protobuf;
using ServiceAccount.Signer.Validation.Exceptions;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace ServiceAccount.Signer.Validation;

public static class UnknownFieldsAccessorValidator
{
    private static readonly ConcurrentDictionary<Type, Func<IMessage, UnknownFieldSet?>> _cache = new();

    private static Func<IMessage, UnknownFieldSet?> GetGetter(Type messageType)
    {
        return _cache.GetOrAdd(messageType, static tt =>
        {
            var property = tt.GetProperty("UnknownFields", BindingFlags.Public | BindingFlags.Instance);
            if (property is not null && property.PropertyType == typeof(UnknownFieldSet))
            {
                var pMsg = Expression.Parameter(typeof(IMessage), "m");
                var cast = Expression.Convert(pMsg, tt);
                var body = Expression.Convert(Expression.Property(cast, property), typeof(UnknownFieldSet));

                return Expression
                    .Lambda<Func<IMessage, UnknownFieldSet?>>(body, pMsg)
                    .Compile();
            }

            var field = tt.GetField("_unknownFields", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field is not null && field.FieldType == typeof(UnknownFieldSet))
            {
                var pMsg = Expression.Parameter(typeof(IMessage), "m");
                var cast = Expression.Convert(pMsg, tt);
                var body = Expression.Convert(Expression.Field(cast, field), typeof(UnknownFieldSet));

                return Expression
                    .Lambda<Func<IMessage, UnknownFieldSet?>>(body, pMsg)
                    .Compile();
            }

            return _ => null;
        });
    }

    public static void EnsureNoUnknownFields(IMessage root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var stack = new Stack<IMessage>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var message = stack.Pop();
            var unknownFieldSet = GetGetter(message.GetType())(message);

            if (unknownFieldSet is not null && unknownFieldSet.CalculateSize() > 0)
                throw new UnknownFieldException();

            foreach (var field in message.Descriptor.Fields.InDeclarationOrder())
            {
                var valueField = field.Accessor.GetValue(message);
                switch (valueField)
                {
                    case IMessage child when child is not null:
                        stack.Push(child);
                        break;

                    case System.Collections.IEnumerable seq when valueField is not string:
                        foreach (var item in seq)
                            if (item is IMessage m)
                                stack.Push(m);
                        break;
                }
            }
        }
    }
}
