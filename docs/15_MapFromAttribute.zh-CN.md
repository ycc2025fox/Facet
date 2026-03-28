# 使用 MapFrom 进行属性映射

`[MapFrom]` 属性提供声明式属性映射,允许你重命名属性而无需实现完整的自定义映射配置。这非常适合简单的属性重命名、API 响应塑形以及在领域和 DTO 属性名称之间保持清晰分离。

## 基本用法

在 Facet 类的属性上使用 `[MapFrom]` 来指定要从哪个源属性映射。使用 `nameof()` 进行类型安全的属性引用:

```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}

[Facet(typeof(User), GenerateToSource = true)]
public partial class UserDto
{
    [MapFrom(nameof(User.FirstName), Reversible = true)]
    public string Name { get; set; } = string.Empty;

    [MapFrom(nameof(User.LastName), Reversible = true)]
    public string FamilyName { get; set; } = string.Empty;
}
```

这将生成:
- **构造函数**: 将 `source.FirstName` 映射到 `Name`,将 `source.LastName` 映射到 `FamilyName`
- **投影**: 对 EF Core 查询使用相同的映射
- **ToSource()**: 自动反向映射

## 工作原理

使用 `[MapFrom]` 时:

1. **源属性不会自动生成** - 你使用新名称声明目标属性
2. **映射是自动的** - 构造函数、投影和 ToSource 都使用映射
3. **其他属性保持不变** - 没有 `[MapFrom]` 的属性正常工作

```csharp
var user = new User
{
    Id = 1,
    FirstName = "John",
    LastName = "Doe",
    Email = "john@example.com",
    Age = 30
};

var dto = new UserDto(user);
// dto.Id = 1 (自动映射)
// dto.Name = "John" (从 FirstName 映射)
// dto.FamilyName = "Doe" (从 LastName 映射)
// dto.Email = "john@example.com" (自动映射)
// dto.Age = 30 (自动映射)

// 反向映射
var entity = dto.ToSource();
// entity.FirstName = "John" (从 Name 映射)
// entity.LastName = "Doe" (从 FamilyName 映射)
```

## 属性参数

### Source (必需)

要映射的源属性名称或表达式。使用 `nameof()` 进行类型安全引用:

```csharp
// 类型安全的属性引用(推荐)
[MapFrom(nameof(User.FirstName))]
public string Name { get; set; }

// 包含多个属性的表达式(需要字符串)
[MapFrom(nameof(User.FirstName) + " + \" \" + " + nameof(User.LastName))]
public string FullName { get; set; }

// 或简单使用字符串表达式
[MapFrom("FirstName + \" \" + LastName")]
public string FullName { get; set; }
```

### Reversible

控制映射是否包含在 `ToSource()` 中。默认为 `false`(选择加入)。

```csharp
// 此属性将映射回源
[MapFrom(nameof(User.FirstName), Reversible = true)]
public string Name { get; set; } = string.Empty;

// 此属性不会映射回(默认)
[MapFrom(nameof(User.LastName))]
public string DisplayName { get; set; } = string.Empty;
```

使用 `Reversible = true` 的场景:
- 需要双向映射(源 ↔ DTO)
- 属性应包含在 `ToSource()` 输出中

保持 `Reversible = false`(默认)的场景:
- 单向映射(源 → DTO)
- 不需要反向映射的只读 DTO
- 不应修改源实体的属性

### IncludeInProjection

控制映射是否包含在静态 `Projection` 表达式中。默认为 `true`。

```csharp
// 此属性不会包含在 EF Core 投影中
[MapFrom("ComplexField", IncludeInProjection = false)]
public string Computed { get; set; } = string.Empty;
```

使用 `IncludeInProjection = false` 的场景:
- 无法转换为 SQL 的映射
- 需要客户端评估的属性
- EF Core 不支持的复杂表达式

## 示例

### 简单属性重命名

```csharp
[Facet(typeof(Customer), GenerateToSource = true)]
public partial class CustomerDto
{
    [MapFrom(nameof(Customer.CompanyName), Reversible = true)]
    public string Company { get; set; } = string.Empty;

    [MapFrom(nameof(Customer.ContactName), Reversible = true)]
    public string Contact { get; set; } = string.Empty;
}
```

### 单向映射(默认)

```csharp
[Facet(typeof(Product))]
public partial class ProductDto
{
    // 仅显示属性,默认不可逆
    [MapFrom(nameof(Product.Name))]
    public string ProductTitle { get; set; } = string.Empty;
}
```

### 计算表达式

使用表达式组合或转换属性:

```csharp
[Facet(typeof(User))]
public partial class UserDto
{
    // 连接名和姓
    [MapFrom("FirstName + \" \" + LastName")]
    public string FullName { get; set; } = string.Empty;

    // 数学表达式
    [MapFrom("Price * Quantity")]
    public decimal Total { get; set; }

    // 方法调用(在构造函数中有效,可能无法转换为 SQL)
    [MapFrom("Name.ToUpper()")]
    public string UpperName { get; set; } = string.Empty;
}
```

**注意:** 复杂表达式可能无法在 EF Core 投影中转换为 SQL。对于需要客户端评估的表达式,使用 `IncludeInProjection = false`。

## 何时使用 MapFrom vs 自定义配置

| 场景 | MapFrom | 自定义配置 |
|----------|---------|---------------|
| 简单属性重命名 | ✅ 最佳选择 | 过度设计 |
| 多个重命名 | ✅ 最佳选择 | 过度设计 |
| 计算值(如连接) | ✅ 支持 | 替代方案 |
| 数学表达式 | ✅ 支持 | 替代方案 |
| 异步操作 | ❌ | ✅ 必需 |
| 复杂转换 | ❌ | ✅ 必需 |
| 类型转换 | ❌ | ✅ 必需 |
| 条件逻辑 | ❌ | ✅ 必需 |

## 最佳实践

1. **使用 `nameof()` 保证类型安全** - 防止拼写错误并支持重构
2. **为计算属性设置 Reversible = false** - 表达式无法反向
3. **考虑投影兼容性** - 对无法转换为 SQL 的表达式设置 `IncludeInProjection = false`
4. **必要时与自定义映射器结合** - MapFrom 处理基础,自定义映射器处理其余

## 限制

- **需要相同类型** - 源和目标属性类型必须匹配
- **表达式不可逆** - 计算表达式是单向的(源 → DTO)
- **嵌套路径无空值检查** - `"Company.Address.City"` 如果 Company 或 Address 为 null 将抛出异常

对于异步操作或条件逻辑等复杂场景,请改用 [自定义映射](04_CustomMapping.md)。

