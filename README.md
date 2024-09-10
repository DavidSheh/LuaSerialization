<div align="center">
<p><strong><a href="README.md">English</a> | <a href="https://davidsheh.github.io/post/lua-serialization/">简体中文</a></strong></p>
</div>

---

# Lua Serialization
[Lua Serialization](https://github.com/DavidSheh/LuaSerialization) is a lightweight tool for serializing C# objects into Lua code. It allows you to save instance objects in C# as configuration files in Lua code format, making it convenient to use in a Lua environment. This is particularly useful in game development workflows, such as Unity3D + Lua.

## Features
1. Supports serializing C# objects into Lua code.
2. Supports exporting various data types, including primitive types (int, float, double, bool, string, enum, array), as well as complex types like List and Dictionary.
3. Supports ignoring specific fields during export.
4. Supports preprocessing before serialization, such as initializing certain fields before export.
5. Supports exporting native Lua code (like Lua functions and custom enums).
6. Customizable export formats.

## Instructions

### Basic Usage

Data class definition:
```csharp
public class People
{
    public int id;
    public string name;
    public int age;
    public float weight;
    public List<int> luckyNumbers;
    public Dictionary<int, string> luckyNumberMap;
    [IgnoreLua] public string description;
}
```

Serialization:
```csharp
People people = new People()
{
    id = 10001,
    name = "Lua",
    age = 18,
    weight = 60.5f,
    luckyNumbers = new List<int>() { 2, 5, 6, 8, 9 },
    luckyNumberMap = new Dictionary<int, string>()
    {
        { 2, "Good things come in pairs" },
        { 5, "Five blessings arrive" },
        { 6, "Smooth sailing" },
        { 8, "Wealth from all directions" },
        { 9, "Longevity" },
    },
    description = "Field that is marked as not serialized.",
};

string strLua = LuaSerializer.Serialize(people);
Console.WriteLine(strLua);
```

Output:
```lua
{
    ["id"]=10001,
    ["name"]="Lua",
    ["age"]=18,
    ["weight"]=60.5,
    ["luckyNumbers"]={
        2,
        5,
        6,
        8,
        9
    },
    ["luckyNumberMap"]={
        [2]="Good things come in pairs",
        [5]="Five blessings arrive",
        [6]="Smooth sailing",
        [8]="Wealth from all directions",
        [9]="Longevity"
    },
}
```

### Advanced Usage
#### 1. Preprocessing Before Serialization
Implement the `IBeforeLuaSerialization` interface and add preprocessing logic in the `OnBeforeLuaSerialize()` method.

Data class definition:
```csharp
public class People : IBeforeLuaSerialization
{
    public int id;
    public string name;
    public int age;
    public float weight;
    private string brief;

    public void OnBeforeLuaSerialize()
    {
        brief = $"id: {id}, name: {name}, age: {age}, weight: {weight}";
    }
}
```

Serialization:
```csharp
People people = new People()
{
    id = 10002,
    name = "Lucky",
    age = 18,
    weight = 60.5f,
};
string strLua = LuaSerializer.Serialize(people);
Console.WriteLine(strLua);
```

Output:
```lua
{
    ["id"]=10002,
    ["name"]="Lucky",
    ["age"]=18,
    ["weight"]=60.5,
    ["brief"]="id: 10002, name: Lucky, age: 18, weight: 60.5"
}
```

#### 2. Exporting Native Lua Code
For fields that need to export native Lua code, simply prefix the string with `@`.

Data class definition:
```csharp
public class People
{
    public int id;
    public string name;
    public string gender;
    public string description;
}
```

Serialization:
```csharp
People people = new People()
{
    id = 10003,
    name = "Lucky",
    gender = "@GenderType.Male",
    description = "@function() print('I am a student.') end",
};
string strLua = LuaSerializer.Serialize(people);
Console.WriteLine(strLua);
```

Output:
```lua
{
    ["id"]=10003,
    ["name"]="Lucky",
    ["gender"]=GenderType.Male,
    ["description"]=function() print('I am a student.') end
}
```

#### 3. Custom Export Format
Implement the `ILuaSerializable` interface and define custom export logic in the `SerializeToLua()` method.

Data class definition:
```csharp
public class People
{
    public int id;
    public string name;
    public int age;
    public Child[] children;
}

public class Child : ILuaSerializable
{
    public string name;
    public int age;
    
    public object SerializeToLua()
    {
        return $"[name = '{name}', age = {age}]";
    }
}
```

Serialization:
```csharp
People people = new People()
{
    id = 10004,
    name = "Lucy",
    age = 32,
    children = new Child[]{
        new Child(){
            name = "Jack",
            age = 1
        },
        new Child(){
            name = "Nancy",
            age = 2
        }
    }
};
string strLua = LuaSerializer.Serialize(people);
Console.WriteLine(strLua);
```

Output:
```lua
{
    ["id"]=10004,
    ["name"]="Lucy",
    ["age"]=32,
    ["children"]={
        "[name = 'Jack', age = 1]",
        "[name = 'Nancy', age = 2]"
    }
}
```

## Repository
A lightweight C# to Lua serialization tool with no third-party dependencies [Lua Serialization](https://github.com/DavidSheh/LuaSerialization): https://github.com/DavidSheh/LuaSerialization.
