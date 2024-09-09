using System;

namespace LuaSerialization
{
    [AttributeUsage(AttributeTargets.Field)]
    public class IgnoreLuaAttribute : Attribute
    {
    }
}
