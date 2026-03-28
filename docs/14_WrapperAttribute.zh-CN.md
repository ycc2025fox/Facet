# Wrapper 特性 - 基于引用的属性委托

## 概述

`[Wrapper]` 特性生成包装类，**委托**到源对象实例，创建基于引用的外观模式。与创建独立值副本的 `[Facet]` 不同，包装器维护对源对象的引用，因此对包装器属性的更改会影响底层源。

## 何时使用 Wrapper 与 Facet

| 使用场景 | 使用 Wrapper | 使用 Facet |
|----------|-------------|-----------|
| API/序列化的 DTO | ❌ | ✅ |
| EF Core 查询投影 | ❌ | ✅ |
| 外观模式（隐藏属性） | ✅ | ❌ |
| 实时绑定的 ViewModel | ✅ | ❌ |
| 装饰器模式 | ✅ | ❌ |
| 只读视图 | ✅ | ❌ |
| 内存效率（避免重复） | ✅ | ❌ |
| 断开连接的数据传输 | ❌ | ✅ |

## 基本用法

```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public decimal Salary { get; set; }
}

// 隐藏敏感属性的包装器
[Wrapper(typeof(User), "Password", "Salary")]
public partial class PublicUserWrapper { }

// 用法
var user = new User
{
    Id = 1,
    FirstName = "John",
    Password = "secret123",
    Salary = 75000
};

var wrapper = new PublicUserWrapper(user);

// 从包装器读取（委托到源）
Console.WriteLine(wrapper.FirstName); // "John"

// 通过包装器修改（影响源！）
wrapper.FirstName = "Jane";
Console.WriteLine(user.FirstName); // "Jane"

// 敏感属性不可访问
// wrapper.Password;  // ❌ 编译错误
// wrapper.Salary;    // ❌ 编译错误
```

## 特性参数

### 构造函数参数

```csharp
[Wrapper(Type sourceType, params string[] exclude)]
```

- **sourceType**：要包装和委托的类型
- **exclude**：要从包装器中排除的属性/字段名称

### 命名参数

| 参数 | 类型 | 默认值 | 描述 |
|-----------|------|---------|-------------|
| `Include` | `string[]?` | `null` | 仅包含这些属性（与 Exclude 互斥） |
| `IncludeFields` | `bool` | `false` | 包含源类型的公共字段 |
| `ReadOnly` | `bool` | `false` | 生成只读属性（不可变外观） |
| `NestedWrappers` | `Type[]?` | `null` | 嵌套复杂属性的包装器类型 |
| `CopyAttributes` | `bool` | `false` | 从源复制验证特性到包装器 |
| `UseFullName` | `bool` | `false` | 为生成的文件使用完整类型名 |

## Include/Exclude 模式

### Exclude 模式（默认）

```csharp
// 排除特定属性
[Wrapper(typeof(User), "Password", "Salary", "SocialSecurity")]
public partial class PublicUserWrapper { }
```

### Include 模式

```csharp
// 仅包含特定属性
[Wrapper(typeof(User), Include = [nameof(User.Id), nameof(User.FirstName), nameof(User.LastName), nameof(User.Email)])]
public partial class UserContactWrapper { }
```

## 只读包装器

生成防止意外修改的不可变外观：

```csharp
[Wrapper(typeof(Product), ReadOnly = true)]
public partial class ReadOnlyProductView { }

var product = new Product { Name = "Laptop", Price = 1299.99m };
var view = new ReadOnlyProductView(product);

// 可以读取
Console.WriteLine(view.Name);    // "Laptop"
Console.WriteLine(view.Price);   // 1299.99

// 不能写入（编译错误 CS0200）
// view.Name = "Desktop";  // 属性是只读的
// view.Price = 999.99m;   // 属性是只读的

// 仍然反映源的更改
product.Name = "Desktop";
Console.WriteLine(view.Name);    // "Desktop"
```

### 只读包装器的使用场景

- **安全性**：防止修改敏感的领域对象
- **API 设计**：为消费者提供只读视图
- **防御性编程**：确保某些上下文不能改变状态
- **事件处理器**：传递不可变视图以防止副作用

## 复制特性

从源复制验证和其他特性到包装器：

```csharp
public class Product
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Range(0, 10000)]
    public decimal Price { get; set; }
}

[Wrapper(typeof(Product), CopyAttributes = true)]
public partial class ProductWrapper { }
```

生成的代码：

```csharp
public partial class ProductWrapper
{
    [Required]
    [StringLength(100)]
    public string Name
    {
        get => _source.Name;
        set => _source.Name = value;
    }

    [Range(0, 10000)]
    public decimal Price
    {
        get => _source.Price;
        set => _source.Price = value;
    }
}
```

## 包含字段

默认情况下，仅包装属性。启用字段包装：

```csharp
public class Entity
{
    public int Id;  // 字段
    public string Name { get; set; }  // 属性
}

[Wrapper(typeof(Entity), IncludeFields = true)]
public partial class EntityWrapper { }
```

## 嵌套包装器

使用自己的包装器类型包装复杂的嵌套对象。这实现了深层属性隐藏并创建分层外观模式。

### 基本嵌套包装器用法

```csharp
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address Address { get; set; }
    public string SocialSecurityNumber { get; set; }
}

// 为嵌套类型定义包装器
[Wrapper(typeof(Address), "Country")]
public partial class PublicAddressWrapper { }

// 在父包装器中引用嵌套包装器
[Wrapper(typeof(Person), "SocialSecurityNumber", NestedWrappers = new[] { typeof(PublicAddressWrapper) })]
public partial class PublicPersonWrapper { }

// 用法
var person = new Person
{
    Id = 1,
    Name = "John Doe",
    Address = new Address
    {
        Street = "123 Main St",
        City = "Springfield",
        ZipCode = "12345",
        Country = "USA"
    },
    SocialSecurityNumber = "123-45-6789"
};

var wrapper = new PublicPersonWrapper(person);

// 访问嵌套包装器
Console.WriteLine(wrapper.Address.City);       // "Springfield"
Console.WriteLine(wrapper.Address.ZipCode);    // "12345"

// Country 被 PublicAddressWrapper 排除
// wrapper.Address.Country  // ❌ 编译错误

// 更改通过嵌套包装器传播
wrapper.Address.City = "Boston";
Console.WriteLine(person.Address.City);        // "Boston"
```

### 嵌套包装器工作原理

当属性类型与嵌套包装器的源类型匹配时，生成器会：

1. **在 get 时包装**：返回包装嵌套对象的新包装器实例
2. **在 set 时解包**：在赋值前调用 `Unwrap()` 提取源对象

生成的嵌套属性代码：

```csharp
// 简单属性（无嵌套包装器）
public int Id
{
    get => _source.Id;
    set => _source.Id = value;
}

// 嵌套包装器属性
public PublicAddressWrapper Address
{
    get => new PublicAddressWrapper(_source.Address);
    set => _source.Address = value.Unwrap();
}

// 可空嵌套包装器
public PublicAddressWrapper? OptionalAddress
{
    get => _source.OptionalAddress != null
        ? new PublicAddressWrapper(_source.OptionalAddress)
        : null;
    set => _source.OptionalAddress = value?.Unwrap();
}
```

## 最佳实践

### 应该做的

- 将包装器用于**运行时外观**和 **ViewModel**
- 当需要包装器和源之间**同步更改**时使用包装器
- 使用 `ReadOnly = true` 实现**不可变视图**和**安全性**
- 使用包装器从外部消费者**隐藏敏感属性**
- 使用**嵌套包装器**实现多级属性隐藏和分层安全
- 保持嵌套包装器层次结构**浅层**（最多 2-3 级）
- 当需要不同目的的两种模式时，与 Facet 结合使用

### 不应该做的

- 不要将包装器用于 **DTO** 或**数据传输**（改用 Facet）
- 不要将包装器用于 **EF Core 查询投影**（改用 Facet）
- 不要将包装器用于**序列化**（改用 Facet）
- 不要包装 **Facet** - 两者都应针对相同的源类型
- 不要在嵌套包装器层次结构中创建**循环引用**

## 性能考虑

- **内存**：包装器增加最小开销（一个引用字段）
- **CPU**：属性访问是简单的字段解引用（非常快）
- **GC**：只要包装器存在，包装器就会保持源存活
- **无反射**：所有属性访问都是直接的、编译时绑定的
- **嵌套包装器**：每次访问创建新的包装器实例（短期、GC 友好）
  - 如果在循环中频繁访问，缓存嵌套包装器引用
  - 嵌套包装器创建很快（单次分配 + 字段赋值）

## 比较：Wrapper 与 Facet

```csharp
public class User { public string Name { get; set; } }

// Facet：创建独立副本
[Facet(typeof(User))]
public partial class UserDto { }

var user = new User { Name = "John" };
var dto = user.ToFacet<User, UserDto>();
dto.Name = "Jane";
Console.WriteLine(user.Name);  // "John" - 独立

// Wrapper：委托到源
[Wrapper(typeof(User))]
public partial class UserWrapper { }

var wrapper = new UserWrapper(user);
wrapper.Name = "Jane";
Console.WriteLine(user.Name);  // "Jane" - 同步！
```

| 方面 | Facet | Wrapper |
|--------|-------|---------|
| 数据存储 | 独立副本 | 源的引用 |
| 内存 | 重复数据 | 无重复 |
| 更改 | 独立 | 同步到源 |
| 使用场景 | DTO、EF 投影 | 外观、ViewModel |
| EF Core | 查询投影 | 不适用 |
| 序列化 | 安全 | 序列化包装器，非源 |

## 限制

以下功能计划在未来版本中实现：

- **集合包装器**：包装嵌套包装器类型的集合
- **自定义映射**：通过配置添加计算属性
- **仅初始化属性**：支持 C# 9+ init 访问器
- **完整 Record 支持**：增强的 record 类型支持

## 另请参阅

- [Facet 特性](03_AttributeReference.md) - 值复制行为
- [高级场景](06_AdvancedScenarios.md) - 复杂模式
- [快速入门](02_QuickStart.md) - Facet 入门
