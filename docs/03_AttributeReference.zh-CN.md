# Facet 特性参考

`[Facet]` 特性用于基于现有源类型声明新的投影（facet）类型。

## 用法

### 排除模式（默认）
```csharp
[Facet(typeof(SourceType), exclude: "Property1", "Property2")]
public partial class MyFacet { }
```

### 包含模式（新增）
```csharp
[Facet(typeof(SourceType), Include = [nameof(SourceType.Property1), nameof(SourceType.Property2)])]
public partial class MyFacet { }
```

## 参数

| 参数                      | 类型      | 描述                                                                 |
|--------------------------------|-----------|-----------------------------------------------------------------------------|
| `sourceType`                   | `Type`    | 要投影的源类型（必需）。                                        |
| `exclude`                      | `string[]`| 要从生成类型中排除的属性/字段名称（可选）。   |
| `Include`                      | `string[]`| 要包含在生成类型中的属性/字段名称（可选）。与 `exclude` 互斥。 |
| `NestedFacets`                 | `Type[]?` | 嵌套 facet 类型数组，用于自动映射嵌套对象（默认：null）。 |
| `IncludeFields`                | `bool`    | 包含源类型的公共字段（包含模式默认：false，排除模式默认：false）。 |
| `GenerateConstructor`          | `bool`    | 生成从源复制值的构造函数（默认：true）。   |
| `GenerateParameterlessConstructor` | `bool` | 生成用于测试和初始化的无参构造函数（默认：true）。 |
| `ChainToParameterlessConstructor` | `bool` | 使用 `: this()` 将生成的构造函数链接到用户定义的无参构造函数（默认：false）。参见下方[构造函数链接](#构造函数链接)。 |
| `Configuration`                | `Type?`   | 自定义映射配置类型（参见[自定义映射](04_CustomMapping.md)）。      |
| `GenerateProjection`           | `bool`    | 生成静态 LINQ 投影（默认：true）。                          |
| `GenerateToSource`             | `bool`    | 生成从 facet 映射回源类型的方法（默认：false）。    |
| `PreserveInitOnlyProperties`   | `bool`    | 保留源属性的 init-only 修饰符（record 默认：true）。 |
| `PreserveRequiredProperties`   | `bool`    | 保留源属性的 required 修饰符（record 默认：true）。 |
| `NullableProperties`           | `bool`    | 使生成的 facet 中的所有属性可空（默认：false）。 |
| `CopyAttributes`               | `bool`    | 将源类型成员的特性复制到生成的 facet 成员（默认：false）。参见下方[特性复制](#特性复制)。 |
| `UseFullName`                  | `bool`    | 在生成的文件名中使用完整类型名以避免冲突（默认：false）。 |
| `MaxDepth`                     | `int`     | 嵌套 facet 递归的最大深度，防止栈溢出（默认：3）。设为 0 表示无限制（不推荐）。参见下方[循环引用保护](#循环引用保护)。 |
| `PreserveReferences`           | `bool`    | 使用对象跟踪启用运行时循环引用检测（默认：true）。参见下方[循环引用保护](#循环引用保护)。 |
| `SourceSignature`              | `string?` | 用于跟踪源实体变更的哈希签名。当源结构变更时发出 FAC022 警告。参见[源签名变更跟踪](16_SourceSignature.md)。 |
| `ConvertEnumsTo`               | `Type?`   | 设置后，所有枚举属性将转换为指定类型（`typeof(string)` 或 `typeof(int)`）。默认为 null（枚举保留原始类型）。参见[枚举转换](20_ConvertEnumsTo.md)。 |
| `GenerateCopyConstructor`      | `bool`    | 生成接受同类型 facet 实例并复制所有成员值的复制构造函数（默认：false）。参见下方[复制构造函数](#复制构造函数)。 |
| `GenerateEquality`             | `bool`    | 生成基于值的相等性成员（`Equals`、`GetHashCode`、`==`、`!=`）并实现 `IEquatable<T>`（默认：false）。对 record 忽略。参见下方[相等性生成](#相等性生成)。 |

## Include 与 Exclude

`Include` 和 `Exclude` 参数互斥：

- **排除模式**：包含所有属性，除了 `exclude` 中列出的（默认行为）
- **包含模式**：仅包含 `Include` 数组中列出的属性

### 包含模式行为

使用 `Include` 模式时：
- 仅将 `Include` 数组中指定的属性复制到 facet
- `IncludeFields` 默认为 `false`（包含模式默认禁用）
- 源类型的所有其他属性被排除
- 支持继承 - 可以包含基类的属性

## 示例

### 基本 Include 用法
```csharp
// 仅包含 FirstName、LastName 和 Email
[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.LastName), nameof(User.Email)])]
public partial class UserContactDto;
```

### 单属性 Include
```csharp
// 仅包含 Name 属性
[Facet(typeof(Product), Include = [nameof(Product.Name)])]
public partial class ProductNameDto;
```

### Include 与自定义属性
```csharp
// 包含特定属性并添加自定义属性
[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.LastName)])]
public partial class UserSummaryDto
{
    public string FullName { get; set; } = string.Empty; // 自定义属性
}
```

### Include 与字段
```csharp
// 包含字段和属性
[Facet(typeof(EntityWithFields), Include = [nameof(EntityWithFields.Name), nameof(EntityWithFields.Age)], IncludeFields = true)]
public partial class EntityDto;
```

### Include 与 Record
```csharp
// 生成仅包含特定属性的 record 类型
[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.LastName)])]
public partial record UserNameRecord;
```

### 传统 Exclude 用法
```csharp
// 排除敏感属性（原始行为）
[Facet(typeof(User), exclude: nameof(User.Password))]
public partial record UserDto;
```

### 查询模型的可空属性
```csharp
// 使所有属性可空，用于查询/过滤场景
[Facet(typeof(Product), nameof(Product.InternalNotes), NullableProperties = true, GenerateToSource = false)]
public partial class ProductQueryDto;

// 用法：所有字段都是可选的，用于过滤
var query = new ProductQueryDto
{
    Name = "Widget",
    Price = 50.00m
    // 其他字段保持 null
};
```

**注意：** 使用 `NullableProperties = true` 时，建议设置 `GenerateToSource = false`，因为将可空属性映射回非空源属性在逻辑上不合理。

### 嵌套 Facet 组合 DTO
```csharp
// 为嵌套类型定义 facet
[Facet(typeof(Address))]
public partial record AddressDto;

[Facet(typeof(Company), NestedFacets = [typeof(AddressDto)])]
public partial record CompanyDto;

[Facet(typeof(Employee),
    exclude: [nameof(Employee.PasswordHash), nameof(Employee.Salary)],
    NestedFacets = [typeof(CompanyDto), typeof(AddressDto)])]
public partial record EmployeeDto;

// 用法 - 自动处理嵌套映射
var employee = new Employee
{
    FirstName = "John",
    Company = new Company
    {
        Name = "Acme Corp",
        HeadquartersAddress = new Address { City = "San Francisco" }
    },
    HomeAddress = new Address { City = "Oakland" }
};

var employeeDto = new EmployeeDto(employee);
// employeeDto.Company 是 CompanyDto
// employeeDto.Company.HeadquartersAddress 是 AddressDto
// employeeDto.HomeAddress 是 AddressDto

// ToSource 也会自动处理嵌套类型
var mappedEmployee = employeeDto.ToSource();
// 所有嵌套对象都被正确重建
```

**NestedFacets 工作原理：**
- 生成器自动检测源类型中哪些属性与嵌套 facet 的源类型匹配
- 对于每个匹配项，将属性类型替换为嵌套 facet 类型
- 构造函数自动为嵌套属性调用 `new NestedFacetType(source.Property)`
- 投影通过构造函数链接无缝支持 EF Core 查询
- ToSource 方法在嵌套 facet 上调用 `.ToSource()` 以重建原始类型层次结构

**优势：**
- 无需手动声明嵌套类型的属性
- 构造函数、投影和 ToSource 方法中的自动映射
- 支持多级嵌套
- 在同一父类型上支持多个嵌套 facet

## 何时使用 Include 与 Exclude

### 使用 **Include** 当：
- 想要从大型源类型中仅获取几个特定属性的 facet
- 创建聚焦的 DTO（例如，摘要视图、仅联系信息）
- 构建仅应公开某些字段的 API 响应模型
- 创建包含最少数据的搜索结果 DTO

### 使用 **Exclude** 当：
- 想要大部分属性但需要隐藏一些敏感属性
- 源类型的大部分应包含在 facet 中
- 遵循原始 Facet 模式以保持向后兼容性

### 使用 **NullableProperties** 当：
- 创建查询/过滤 DTO，其中所有搜索条件都是可选的
- 构建补丁/更新模型，其中仅提供更改的字段
- 实现支持部分数据的灵活 API 请求模型
- 生成类似于 `GenerateDtos` 中查询 DTO 的 DTO

**重要考虑：**
- 值类型（int、bool、DateTime、枚举）变为可空（int?、bool? 等）
- 引用类型（string、对象）保持引用类型但标记为可空
- 禁用 `GenerateToSource` 以避免从可空到非空类型的映射问题

## 构造函数链接

`ChainToParameterlessConstructor` 参数允许生成的构造函数使用 `: this()` 链接到用户定义的无参构造函数。这确保在属性映射之前运行构造函数中的任何自定义初始化逻辑。

### 用法

```csharp
public class ModelType
{
    public int MaxValue { get; set; }
    public string Name { get; set; } = string.Empty;
}

[Facet(typeof(ModelType), GenerateParameterlessConstructor = false, ChainToParameterlessConstructor = true)]
public partial class MyDto
{
    public int Value { get; set; }
    public bool Initialized { get; set; }

    public MyDto()
    {
        // 在映射之前运行的自定义初始化逻辑
        Value = 100;
        Initialized = true;
    }
}

// 用法
var source = new ModelType { MaxValue = 42, Name = "Test" };
var dto = new MyDto(source);
// dto.Value == 100 (来自无参构造函数)
// dto.Initialized == true (来自无参构造函数)
// dto.MaxValue == 42 (来自源映射)
// dto.Name == "Test" (来自源映射)
```

### 生成的代码

使用 `ChainToParameterlessConstructor = true` 时，生成的构造函数链接到您的无参构造函数：

```csharp
public MyDto(ModelType source) : this()  // <-- 链接到您的构造函数
{
    this.MaxValue = source.MaxValue;
    this.Name = source.Name;
}
```

### 何时使用

- 当无参构造函数中有需要在映射期间运行的初始化逻辑时
- 当需要设置不是简单从源复制的默认值时
- 当有需要初始值的计算或派生属性时

**注意：** 设置 `GenerateParameterlessConstructor = false` 以防止生成器创建自己的无参构造函数，这会与您的构造函数冲突。

## 特性复制

`CopyAttributes` 参数允许您将源类型成员的特性复制到生成的 facet 成员。这对于在为 API 模型创建 DTO 时保留数据验证特性特别有用。

### 用法

```csharp
public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(0, 150)]
    public int Age { get; set; }

    public string Password { get; set; } = string.Empty;
}

[Facet(typeof(User), nameof(User.Password), CopyAttributes = true)]
public partial class UserDto;
```

生成的 `UserDto` 将包含所有验证特性：

```csharp
public partial class UserDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Range(0, 150)]
    public int Age { get; set; }
}
```

### 复制的内容

特性复制功能智能过滤特性，仅复制对目标有意义的特性：

**常见复制的特性包括：**
- 数据验证特性：`Required`、`StringLength`、`Range`、`EmailAddress`、`Phone`、`Url`、`RegularExpression`、`CreditCard` 等
- 显示特性：`Display`、`DisplayName`、`Description`
- JSON 序列化特性：`JsonPropertyName`、`JsonIgnore` 等
- 继承自 `ValidationAttribute` 的自定义验证特性

**自动排除的特性：**
- 内部编译器生成的特性（例如 `System.Runtime.CompilerServices.*`）
- `ValidationAttribute` 基类本身（仅复制派生的验证特性）
- 根据 `AttributeUsage` 对目标成员类型无效的特性

### 特性参数

所有特性参数都以正确的 C# 语法保留：

```csharp
public class Product
{
    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be 3-100 characters")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 10000.00)]
    public decimal Price { get; set; }

    [RegularExpression(@"^[A-Z]{3}-\d{4}$", ErrorMessage = "Invalid SKU format")]
    public string Sku { get; set; } = string.Empty;
}

[Facet(typeof(Product), CopyAttributes = true)]
public partial class ProductDto;
```

所有参数（包括命名参数、带转义序列的字符串字面量和数值）都被正确保留。

### 与嵌套 Facet 配合

`CopyAttributes` 与 `NestedFacets` 无缝配合：

```csharp
[Facet(typeof(Address), CopyAttributes = true)]
public partial class AddressDto;

[Facet(typeof(Order), nameof(Order.InternalNotes), CopyAttributes = true, NestedFacets = [typeof(AddressDto)])]
public partial class OrderDto;
```

父级和嵌套 facet 都将从各自的源类型复制特性。

### 何时使用 CopyAttributes

**使用 `CopyAttributes = true` 当：**
- 创建需要验证的 API 请求/响应 DTO
- 为 ASP.NET Core 模型验证构建 DTO
- 为 UI 框架保留显示元数据
- 维护 JSON 序列化特性
- 希望在领域模型和 DTO 之间保持一致的验证

**不要使用当：**
- 希望 DTO 有不同的验证规则
- 源类型具有特定于其领域关注点的特性（例如 ORM 映射特性）
- 更喜欢直接在 facet 上定义验证特性

### 默认行为

默认情况下，`CopyAttributes = false`，意味着不复制特性。这保持了向后兼容性，并让您明确控制何时应复制特性。

## 循环引用保护

在使用嵌套 facet 时，对象图中的循环引用可能导致栈溢出异常和 IDE 崩溃。Facet 库提供两个互补功能来防止这种情况：

### MaxDepth

控制嵌套 facet 可以实例化的深度级别。这可以防止代码生成和运行时的无限递归。

**默认值：** `3`（大多数场景推荐）

```csharp
// 处理：Order -> LineItems -> Product -> Category
[Facet(typeof(Order), NestedFacets = [typeof(LineItemDto)])]
public partial record OrderDto;

// 对于更深的嵌套，增加 MaxDepth
[Facet(typeof(Organization), MaxDepth = 5, NestedFacets = [typeof(DepartmentDto)])]
public partial record OrganizationDto;

// 禁用深度限制（谨慎使用！）
[Facet(typeof(SimpleType), MaxDepth = 0)]
public partial record SimpleTypeDto;
```

**MaxDepth 工作原理：**
- **级别 0**：根对象（例如 Order）
- **级别 1**：第一级嵌套对象（例如 LineItems）
- **级别 2**：第二级嵌套对象（例如 Product）
- **级别 3**：第三级嵌套对象（例如 Category）- 默认 MaxDepth = 3 时在此停止
- 超过 MaxDepth 的属性设置为 `null`

### PreserveReferences

启用运行时跟踪对象实例，以检测同一对象被多次处理的情况。这可以防止对象相互引用的循环引用。

**默认值：** `true`（推荐以确保安全）

```csharp
// 启用循环引用检测（默认）
[Facet(typeof(Author), PreserveReferences = true, NestedFacets = [typeof(BookDto)])]
public partial record AuthorDto;

[Facet(typeof(Book), PreserveReferences = true, NestedFacets = [typeof(AuthorDto)])]
public partial record BookDto;

// 禁用以获得最大性能（仅当确定不存在循环引用时）
[Facet(typeof(FlatDto), PreserveReferences = false)]
public partial record FlatDto;
```

**PreserveReferences 工作原理：**
- 使用带引用相等性的 `HashSet<object>` 跟踪已处理的对象
- 创建嵌套 facet 时，检查源对象是否已被处理
- 对已处理的对象返回 `null` 以打破循环引用
- 使用 `.Where(x => x != null)` 从集合中过滤掉重复项

### 最佳实践

**对于循环引用（例如 Author <> Book、Employee <> Manager）：**
```csharp
[Facet(typeof(Author), MaxDepth = 2, PreserveReferences = true,
       NestedFacets = [typeof(BookDto)])]
public partial record AuthorDto;

[Facet(typeof(Book), MaxDepth = 2, PreserveReferences = true,
       NestedFacets = [typeof(AuthorDto)])]
public partial record BookDto;
```

**对于自引用类型（例如 Employee -> Manager -> Manager）：**
```csharp
[Facet(typeof(Employee), MaxDepth = 5, PreserveReferences = true,
       NestedFacets = [typeof(EmployeeDto)])]
public partial record EmployeeDto;
```

**对于没有循环引用的简单层次结构：**
```csharp
// 如果确定没有循环引用，可以减少开销
[Facet(typeof(Category), MaxDepth = 10, PreserveReferences = false,
       NestedFacets = [typeof(CategoryDto)])]
public partial record CategoryDto;
```

**对于没有嵌套 facet 的扁平 DTO：**
```csharp
// 可以禁用两者以获得最大性能
[Facet(typeof(Product), MaxDepth = 0, PreserveReferences = false)]
public partial record ProductDto;
```

### 性能考虑

- **MaxDepth**：开销可忽略 - 仅深度计数器检查
- **PreserveReferences**：最小开销 - HashSet 引用查找（通常 < 1% 性能影响）
- 默认情况下启用这两个功能是安全的
- 仅在分析应用程序并确定这些是瓶颈时才禁用

### 常见场景

| 场景 | MaxDepth | PreserveReferences | 示例 |
|----------|----------|-------------------|---------|
| 扁平 DTO（无嵌套） | 0 | false | 简单用户配置文件 |
| 简单父子关系 | 2 | false | Order -> Customer |
| 多级层次结构 | 3-5 | false | Order -> LineItem -> Product -> Category |
| 循环引用 | 2-3 | true | Author <> Book、Post <> Comments |
| 自引用 | 3-5 | true | 员工树、分类树 |
| 复杂对象图 | 3-5 | true | 任何复杂领域模型 |

### 故障排除

**代码生成期间栈溢出：**
- 增加 `MaxDepth`，源生成器遇到了无限递归
- 使用 `PreserveReferences = true` 时确保 `MaxDepth > 0`

**运行时栈溢出：**
- 启用 `PreserveReferences = true`
- 如果合法的嵌套深度超过当前值，增加 `MaxDepth`

**缺少嵌套数据：**
- 检查嵌套深度是否超过 `MaxDepth`
- 验证 `PreserveReferences` 没有过滤掉有效引用

---

## MapWhen 特性

`[MapWhen]` 特性基于源值启用条件属性映射。

### 基本用法

```csharp
[Facet(typeof(Order))]
public partial class OrderDto
{
    [MapWhen("Status == OrderStatus.Completed")]
    public DateTime? CompletedAt { get; set; }
}
```

### 参数

| 参数 | 类型 | 描述 |
|-----------|------|-------------|
| `Condition` | `string` | 要评估的条件表达式（必需） |
| `Default` | `object?` | 条件为 false 时的自定义默认值 |
| `IncludeInProjection` | `bool` | 在 Projection 表达式中包含条件（默认：true） |

### 支持的条件

- 布尔值：`[MapWhen("IsActive")]`
- 相等性：`[MapWhen("Status == OrderStatus.Completed")]`
- 空值检查：`[MapWhen("Email != null")]`
- 比较：`[MapWhen("Age >= 18")]`
- 否定：`[MapWhen("!IsDeleted")]`

### 多个条件

多个特性使用 AND 逻辑组合：

```csharp
[MapWhen("IsActive")]
[MapWhen("Status == OrderStatus.Completed")]
public DateTime? CompletedAt { get; set; }
```

参见[MapWhen 条件映射](17_MapWhen.md)获取完整文档。

---

## 枚举转换

`ConvertEnumsTo` 属性将所有枚举属性转换为生成的 facet 中的 `string` 或 `int`。

### 基本用法

```csharp
// 将枚举转换为字符串（用于 API 响应）
[Facet(typeof(User), ConvertEnumsTo = typeof(string))]
public partial class UserDto;

// 将枚举转换为整数（用于存储）
[Facet(typeof(User), ConvertEnumsTo = typeof(int))]
public partial class UserDto;
```

### 与反向映射配合

```csharp
[Facet(typeof(User), ConvertEnumsTo = typeof(string), GenerateToSource = true)]
public partial class UserDto;

var dto = new UserDto(user);
dto.Status // "Active" (字符串)

var entity = dto.ToSource();
entity.Status // UserStatus.Active (枚举)
```

### 可空枚举

可空枚举属性在转换后保留其可空性：
- `UserStatus?` → `string?`（源为 null 时为 null）
- `UserStatus?` → `int?`（可空整数）

参见[枚举转换](20_ConvertEnumsTo.md)获取完整文档。

---

## 复制构造函数

`GenerateCopyConstructor` 属性生成一个接受同类型 facet 实例并复制所有成员值的构造函数。这对于 MVVM 场景、克隆 DTO 或创建独立副本很有用。

### 基本用法

```csharp
[Facet(typeof(User), GenerateCopyConstructor = true)]
public partial class UserDto;

// 用法
var original = new UserDto(user);
var copy = new UserDto(original); // 复制构造函数

// 修改副本不会影响原始对象
copy.FirstName = "Changed";
original.FirstName; // 仍然是 "John"
```

### 生成的代码

```csharp
public partial class UserDto
{
    /// <summary>
    /// 通过从另一个实例复制所有成员值来初始化新实例。
    /// </summary>
    public UserDto(UserDto other)
    {
        if (other is null) throw new ArgumentNullException(nameof(other));
        this.Id = other.Id;
        this.FirstName = other.FirstName;
        this.LastName = other.LastName;
        this.Email = other.Email;
    }
}
```

### 何时使用

- **MVVM 视图模型**：克隆视图模型以进行编辑，同时保留原始对象用于取消/恢复
- **DTO 克隆**：为缓存或比较创建 DTO 的独立副本
- **撤销/重做**：在修改前快照 DTO 状态
- **继承场景**：与基类继承一起使用，需要将 facet 属性复制到派生类型

---

## 相等性生成

`GenerateEquality` 属性为类和结构体 facet 生成基于值的相等性成员。这包括 `Equals(T)`、`Equals(object)`、`GetHashCode()` 以及 `==` / `!=` 运算符。生成的类型还实现 `IEquatable<T>`。

> **注意：** 此选项对 record 类型忽略，因为 record 已经从 C# 语言内置了基于值的相等性。

### 基本用法

```csharp
[Facet(typeof(User), GenerateEquality = true)]
public partial class UserDto;

// 基于值的比较
var dto1 = new UserDto(user);
var dto2 = new UserDto(user);
dto1.Equals(dto2); // true
dto1 == dto2;      // true
dto1.GetHashCode() == dto2.GetHashCode(); // true

// 在集合中工作
var set = new HashSet<UserDto> { dto1 };
set.Contains(dto2); // true - 相同的值
```

### 与复制构造函数结合

```csharp
[Facet(typeof(User), GenerateCopyConstructor = true, GenerateEquality = true)]
public partial class UserDto;

var original = new UserDto(user);
var copy = new UserDto(original);
original == copy; // true - 相同的值，不同的实例
```

### 生成的代码

```csharp
public partial class UserDto : System.IEquatable<UserDto>
{
    public bool Equals(UserDto? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return this.Id == other.Id
            && EqualityComparer<string>.Default.Equals(this.FirstName, other.FirstName)
            && EqualityComparer<string>.Default.Equals(this.LastName, other.LastName);
    }

    public override bool Equals(object? obj) => obj is UserDto other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Id.GetHashCode();
            hash = hash * 31 + (FirstName?.GetHashCode() ?? 0);
            hash = hash * 31 + (LastName?.GetHashCode() ?? 0);
            return hash;
        }
    }

    public static bool operator ==(UserDto? left, UserDto? right) { ... }
    public static bool operator !=(UserDto? left, UserDto? right) => !(left == right);
}
```

### 何时使用

- **基于类的 DTO**，需要值比较而不转换为 record
- **变更检测**：比较 DTO 以检查数据是否已修改
- **缓存**：将 DTO 用作字典键或在哈希集中
- **测试**：在单元测试中断言 DTO 相等性

### 何时不使用

- **Record**：Record 已经具有基于值的相等性 - `GenerateEquality` 会自动忽略
- **需要引用相等性**：如果需要基于标识的比较，不要启用此功能

---

参见[自定义映射](04_CustomMapping.md)了解高级场景。
