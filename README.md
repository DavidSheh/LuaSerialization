# Lua Serialization
将 C# 对象序列化成 Lua 代码的一种实现。可以用于将 C# 环境中的实例对象保存成 Lua 代码形式的配置文件，以便于在 Lua 环境中使用。比如 用 Unity3D + Lua 的游戏开发工作流中。

## Features
1. 支持将 C# 对象序列化成 Lua 代码。
2. 支持多种数据类型的导出，除基本类型（int、float、double、 bool、string、enum、array）外，还支持 List、Dictionary 等。
3. 支持忽略某些字段的导出。
4. 支持序列化前的预处理操作，比如导出前初始化某些字段。
5. 支持 Lua 原生代码导出。比如导出 Lua 函数和自定义枚举。
6. 自定义导出格式。

## 使用说明

### 基本用法

数据类定义：
```csharp
public class People
{
    public int id;
    public string name;
    public int age;
    public float weight;
    public List<int> luckyNumbers;
    public Dictionary<int, string> luckyNumberMap;
    [IgnoreLua]public string description;
}
```

序列化处理：
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
        { 2, "好事成双" },
        { 5, "五福临门" },
        { 6, "六六大顺" },
        { 8, "八方来财" },
        { 9, "长长久久" },
    },
    description = "Field that is marked as not serialized.",
};

string strLua = LuaSerializer.Serialize(people);
Console.WriteLine(strLua);
```

输出结果：
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
        [2]="好事成双",
        [5]="五福临门",
        [6]="六六大顺",
        [8]="八方来财",
        [9]="长长久久"
    },
}
```

### 进阶用法
#### 一、序列化前的预处理操作
只需要实现 `IBeforeLuaSerialization` 接口，并在 `OnBeforeLuaSerialize()` 方法中实现预处理逻辑即可。

数据类定义：
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

序列化处理：
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

输出结果：
```lua
{
    ["id"]=10002,
    ["name"]="Lucky",
    ["age"]=18,
    ["weight"]=60.5,
    ["brief"]="id: 10002, name: Lucky, age: 18, weight: 60.5"
}
```

#### 二、Lua 原生代码导出
需要 Lua 原生导出的 string 类型字段，只需要在字符串前面使用 `@` 标记即可。

数据类定义：
```csharp
public class People
{
    public int id;
    public string name;
    public string gender;
    public string description;
}
```

序列化处理：
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

输出结果：
```lua
{
    ["id"]=10003,
    ["name"]="Lucky",
    ["gender"]=GenderType.Male,
    ["description"]=function() print('I am a student.') end
}
```

#### 三、自定义导出格式
自定义导出格式只需要将数据类实现 `ILuaSerializable` 接口，并在 `SerializeToLua()` 方法中实现自定义的导出逻辑即可。

数据类定义：
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

序列化处理：
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

输出结果：
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
 
