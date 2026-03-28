# 使用 ConvertEnumsTo 进行枚举转换

`[Facet]` 属性上的 `ConvertEnumsTo` 属性允许你自动将源类型的所有枚举属性转换为不同的表示形式(`string` 或 `int`)。这对于 API DTO、序列化场景和数据库存储很有用,在这些场景中你需要枚举值作为字符串或整数而不是枚举类型。

## 基本用法

### 将枚举转换为字符串

```csharp
public enum UserStatus
{
    Active,
    Inactive,
    Pending,
    Suspended
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public UserStatus Status { get; set; }
    public string Email { get; set; }
}

// 所有枚举属性在生成的 facet 中变为 string
[Facet(typeof(User), ConvertEnumsTo = typeof(string))]
public partial class UserDto;

// 生成的属性: public string Status { get; set; }
// 而不是:         public UserStatus Status { get; set; }
```

### 将枚举转换为整数

```csharp
// 所有枚举属性在生成的 facet 中变为 int
[Facet(typeof(User), ConvertEnumsTo = typeof(int))]
public partial class UserDto;

// 生成的属性: public int Status { get; set; }
```

