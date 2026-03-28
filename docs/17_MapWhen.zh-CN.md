# MapWhen 属性

`[MapWhen]` 属性基于源属性值启用条件属性映射。仅当指定条件评估为 true 时才映射属性。

## 基本用法

```csharp
[Facet(typeof(Order))]
public partial class OrderDto
{
    [MapWhen("Status == OrderStatus.Completed")]
    public DateTime? CompletedAt { get; set; }
}
```

当 `Status` 为 `Completed` 时,从源映射 `CompletedAt`。否则,默认为 `null`。

## 支持的条件

### 布尔属性

```csharp
[MapWhen("IsActive")]
public string? Email { get; set; }
```

### 相等比较

```csharp
[MapWhen("Status == OrderStatus.Completed")]
public DateTime? CompletedAt { get; set; }

[MapWhen("Status != OrderStatus.Cancelled")]
public string? TrackingNumber { get; set; }
```

### 空值检查

```csharp
[MapWhen("Email != null")]
public string? Email { get; set; }
```

### 数值比较

```csharp
[MapWhen("Age >= 18")]
public string? AdultContent { get; set; }
```

### 取反

```csharp
[MapWhen("!IsDeleted")]
public string? Content { get; set; }
```

## 多个条件

同一属性上的多个 `[MapWhen]` 属性使用 AND 逻辑组合:

```csharp
[MapWhen("IsActive")]
[MapWhen("Status == OrderStatus.Completed")]
public DateTime? CompletedAt { get; set; }
```

仅当 `IsActive` 为 true 且 `Status` 为 `Completed` 时才映射 `CompletedAt`。

## 生成的代码

### 构造函数

```csharp
// 输入
[Facet(typeof(Order))]
public partial class OrderDto
{
    [MapWhen("Status == OrderStatus.Completed")]
    public DateTime? CompletedAt { get; set; }
}

// 生成的构造函数
public OrderDto(Order source)
{
    Id = source.Id;
    Status = source.Status;
    CompletedAt = (source.Status == OrderStatus.Completed)
        ? source.CompletedAt
        : default;
}
```

### 投影

相同的条件逻辑应用于 Projection 表达式,用于 Entity Framework Core:

```csharp
public static Expression<Func<Order, OrderDto>> Projection => source => new OrderDto
{
    Id = source.Id,
    Status = source.Status,
    CompletedAt = (source.Status == OrderStatus.Completed)
        ? source.CompletedAt
        : default
};
```

## 属性参数

| 属性 | 类型 | 描述 |
|----------|------|-------------|
| `Condition` | `string` | 条件表达式 (必需) |
| `Default` | `object?` | 条件为 false 时的自定义默认值 |
| `IncludeInProjection` | `bool` | 是否包含在 Projection 表达式中 (默认: true) |

### 自定义默认值

```csharp
[MapWhen("HasPrice", Default = 0)]
public decimal Price { get; set; }
```

### 从投影中排除

对于无法转换为 SQL 的条件:

```csharp
[MapWhen("IsActive", IncludeInProjection = false)]
public string? Email { get; set; }
```

## 表达式语法

条件字符串使用 C# 表达式语法:

### 支持的运算符
- 比较: `==`, `!=`, `<`, `>`, `<=`, `>=`
- 逻辑: `&&`, `||`, `!`

### 属性访问
- 简单: `IsActive`, `Status`, `Email`
- 枚举值: `OrderStatus.Completed`, `Status == OrderStatus.Pending`

### 字面量
- 布尔值: `true`, `false`
- Null: `null`
- 数字: `18`, `0`

## 示例用例

### 状态依赖字段

```csharp
[Facet(typeof(Subscription))]
public partial class SubscriptionDto
{
    public SubscriptionStatus Status { get; set; }

    [MapWhen("Status == SubscriptionStatus.Active")]
    public DateTime? NextBillingDate { get; set; }

    [MapWhen("Status == SubscriptionStatus.Cancelled")]
    public string? CancellationReason { get; set; }
}
```

### 条件敏感数据

```csharp
[Facet(typeof(Employee))]
public partial class EmployeeDto
{
    public string Name { get; set; }

    [MapWhen("!IsSalaryConfidential")]
    public decimal? Salary { get; set; }
}
```

### 年龄限制内容

```csharp
[Facet(typeof(User))]
public partial class UserDto
{
    public int Age { get; set; }

    [MapWhen("Age >= 18")]
    public string? AdultPreferences { get; set; }
}
```

## 限制

- 不支持条件中的方法调用 (如 `string.IsNullOrEmpty()`)
- 不支持嵌套属性访问 (如 `Address.City`)
- 复杂表达式可能需要简化

## 最佳实践

1. **保持条件简单** - 使用基本比较和布尔检查
2. **使用可空属性** - 由于条件可能为 false,属性通常应该是可空的
3. **考虑 EF Core 转换** - 如果在数据库中使用投影,确保条件可以转换为 SQL
4. **测试两条路径** - 为条件为 true 和 false 的情况编写测试
