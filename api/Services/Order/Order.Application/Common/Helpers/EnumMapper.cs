namespace Order.Application.Common.Helpers;

internal static class EnumMapper
{
    internal static TTarget MapTo<TTarget>(this Enum source) where TTarget : struct, Enum
    {
        return Enum.Parse<TTarget>(source.ToString());
    }
}
