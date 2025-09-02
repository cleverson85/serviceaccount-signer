using Google.Protobuf;
using ServiceAccount.Signer.Validation.Exceptions;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace ServiceAccount.Signer.Validation;

public static class UnknownFieldsAccessorValidator
{
    private static readonly ConcurrentDictionary<Type, Func<IMessage, UnknownFieldSet?>> _cache = new();

    private static UnknownFieldSet? Get(IMessage message)
    {
        var t = message.GetType();
        var getter = _cache.GetOrAdd(t, static tt =>
        {
            var property = tt.GetProperty("UnknownFields");
            if (property is not null && property.PropertyType == typeof(UnknownFieldSet))
            {
                var pMsg = Expression.Parameter(typeof(IMessage), "m");
                var cast = Expression.Convert(pMsg, tt);
                var prop = Expression.Property(cast, property);
                var body = Expression.Convert(prop, typeof(UnknownFieldSet));

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

        return getter(message);
    }

    public static void EnsureNoUnknownFields(IMessage root)
    {
        if (HasUnknowns(root))
            throw new UnknownFieldException();

        static bool HasUnknowns(IMessage message)
        {
            var unknownFieldSet = Get(message);
            if (unknownFieldSet is not null && unknownFieldSet.CalculateSize() > 0)
                return true;

            foreach (var field in message.Descriptor.Fields.InDeclarationOrder())
            {
                var value = field.Accessor.GetValue(message);
                switch (value)
                {
                    case IMessage child when child is not null:
                        if (HasUnknowns(child))
                            return true;
                        break;

                    case System.Collections.IEnumerable seq when value is not string:
                        foreach (var item in seq)
                            if (item is IMessage m && HasUnknowns(m))
                                return true;
                        break;
                }
            }

            return false;
        }
    }
}
