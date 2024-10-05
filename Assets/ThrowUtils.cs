using System;

public static class ThrowUtils
{
    // -- CONSTS

    public const string ENUMERATION_COLLECTION_MODIFIED = "Collection was modified; enumeration operation may not execute";
    public const string ENUMERATION_CANNOT_HAPPEN = "Enumeration cannot happen";

    // -- METHODS

    public static void ThrowIfArgumentNull(
        string name,
        object argument
        )
    {
        if( argument == null )
        {
            throw new ArgumentNullException( name );
        }
    }

    public static void ThrowIfArgumentOutOfRange(
        string name,
        int index,
        int count
        )
    {
        ThrowIfArgumentOutOfRange( name, index, 0, count );
    }

    public static void ThrowIfArgumentOutOfRange(
        string name,
        int index,
        int start_index,
        int count
        )
    {
        if( index < start_index
            || index >= count
            )
        {
            throw new ArgumentOutOfRangeException( name );
        }
    }
}
