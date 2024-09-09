using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace LuaSerialization
{
    public class LuaSerializer
    {
        private const string IndentString = "    "; // 缩进使用四个空格键
        private static Dictionary<int, string> indentMap = new Dictionary<int, string>();

        public static string Serialize(object obj)
        {
            if (obj == null)
            {
                return "nil";
            }

            StringBuilder builder = new StringBuilder();
            SerializeObject(obj, builder, 0);

            return builder.ToString();
        }

        private static void SerializeObject(object obj, StringBuilder builder, int indentLevel)
        {
            if (obj == null)
            {
                builder.Append("nil");

                return;
            }

            if (obj is IBeforeLuaSerialization beforeLuaSerialization)
            {
                beforeLuaSerialization.OnBeforeLuaSerialize();
            }

            Type type = obj.GetType();
            if (IsPrimitiveType(type))
            {
                builder.Append(obj);
            }
            else if (type == typeof(string))
            {
                SerializeString(obj.ToString(), builder);
            }
            else if (type.IsEnum)
            {
                builder.Append(type.FullName).Append('.').Append(obj);
            }
            else if (type.IsArray || typeof(IList).IsAssignableFrom(type))
            {
                SerializeList(obj as IList, builder, indentLevel);
            }
            else if (obj is IDictionary dict)
            {
                SerializeDictionary(dict, builder, indentLevel);
            }
            else if (obj is ILuaSerializable serializable)
            {
                SerializeObject(serializable.SerializeToLua(), builder, indentLevel);
            }
            else
            {
                SerializeFields(obj, builder, indentLevel);
            }
        }

        private static bool IsPrimitiveType(Type type)
        {
            return type == typeof(int) || type == typeof(float) || type == typeof(double) || type == typeof(bool);
        }

        private static void SerializeString(string str, StringBuilder builder)
        {
            // 以 @ 符号开头的字符串序列化成Lua时不解析成字符串，直接解析成原始类型，比如 function 类型和枚举类型
            if (str.StartsWith("@"))
            {
                builder.Append(str.Substring(1));
            }
            else
            {
                str = str.Replace("\"", "\\\"");
                str = str.Replace("\n", "\\n");
                builder.Append('"').Append(str).Append('"');
            }
        }

        private static void SerializeList(IList list, StringBuilder builder, int indentLevel)
        {
            string indent = GetIndent(indentLevel);

            builder.Append("{\n");

            for (int i = 0; i < list.Count; i++)
            {
                builder.Append(indent);
                builder.Append(IndentString);
                SerializeObject(list[i], builder, indentLevel + 1);
                if (i < list.Count - 1)
                {
                    builder.Append(",");
                }

                builder.Append("\n");
            }

            builder.Append(indent).Append("}");
        }

        private static void SerializeDictionary(IDictionary dict, StringBuilder builder, int indentLevel)
        {
            string indent = GetIndent(indentLevel);
            IDictionaryEnumerator enumerator = dict.GetEnumerator();
            builder.Append("{\n");
            int count = dict.Count;
            int currentIndex = 0;
            while (enumerator.MoveNext())
            {
                builder.Append(indent);
                builder.Append(IndentString);
                builder.Append("[");
                SerializeObject(enumerator.Key, builder, 0);
                builder.Append("]=");
                SerializeObject(enumerator.Value, builder, indentLevel + 1);
                if (++currentIndex < count)
                {
                    builder.Append(",");
                }

                builder.Append("\n");
            }

            builder.Append(indent).Append("}");
        }

        private static void SerializeFields(object obj, StringBuilder builder, int indentLevel)
        {
            string indent = GetIndent(indentLevel);

            builder.Append("{\n");
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var validFields = fields.Where(f => !f.IsDefined(typeof(IgnoreLuaAttribute)));
            int count = fields.Length;
            int currentIndex = 0;
            foreach (var field in validFields)
            {
                builder.Append(indent);
                builder.Append(IndentString);
                builder.Append("[");
                SerializeString(field.Name, builder);
                builder.Append("]=");
                SerializeObject(field.GetValue(obj), builder, indentLevel + 1);
                if (++currentIndex < count)
                {
                    builder.Append(",");
                }

                builder.Append("\n");
            }

            builder.Append(indent).Append("}");
        }

        private static string GetIndent(int indentLevel)
        {
            if (indentLevel <= 0)
            {
                return string.Empty;
            }

            if (indentMap.TryGetValue(indentLevel, out string strIndent))
            {
                return strIndent;
            }

            StringBuilder indent = new StringBuilder();
            for (int i = 0; i < indentLevel; i++)
            {
                indent.Append(IndentString);
            }

            strIndent = indent.ToString();
            indentMap.Add(indentLevel, strIndent);

            return strIndent;
        }
    }
}
