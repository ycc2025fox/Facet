# Facet 分析器规则

Facet 包含全面的 Roslyn 分析器，可在 IDE 中提供实时反馈。这些分析器在设计时（甚至在编译代码之前）捕获常见错误和配置问题。

## 快速参考

| 规则 ID | 严重性 | 类别 | 描述 |
|---------|----------|----------|-------------|
| [FAC001](#fac001) | Error | Usage | 类型必须使用 [Facet] 注解 |
| [FAC002](#fac002) | Info | Performance | 考虑使用双泛型变体 |
| [FAC003](#fac003) | Error | Declaration | [Facet] 类型缺少 partial 关键字 |
| [FAC004](#fac004) | Error | Usage | Exclude/Include 中的属性名无效 |
| [FAC005](#fac005) | Error | Usage | 源类型无效 |
| [FAC006](#fac006) | Error | Usage | Configuration 类型无效 |
| [FAC007](#fac007) | Warning | Usage | NestedFacets 类型无效 |
| [FAC008](#fac008) | Warning | Performance | 循环引用风险 |
| [FAC009](#fac009) | Error | Usage | 同时指定了 Include 和 Exclude |
| [FAC010](#fac010) | Warning | Performance | MaxDepth 值异常 |
| [FAC011](#fac011) | Error | Usage | [GenerateDtos] 用于非类类型 |
| [FAC012](#fac012) | Warning | Usage | ExcludeProperties 无效 |
| [FAC013](#fac013) | Warning | Usage | 未选择 DTO 类型 |
| [FAC014](#fac014) | Error | Declaration | [Flatten] 类型缺少 partial 关键字 |
| [FAC015](#fac015) | Error | Usage | [Flatten] 中的源类型无效 |
| [FAC016](#fac016) | Warning | Performance | [Flatten] 中的 MaxDepth 异常 |
| [FAC017](#fac017) | Info | Usage | LeafOnly 命名冲突风险 |
| [FAC022](#fac022) | Warning | SourceTracking | 源实体结构已更改 |

---

## 扩展方法分析器

### FAC001

**类型必须使用 [Facet] 注解**

- **严重性**：Error
- **类别**：Usage

#### 描述

使用扩展方法如 `ToFacet<T>()`、`ToSource<T>()`、`SelectFacet<T>()` 等时，目标类型必须使用 `[Facet]` 特性注解。

#### 错误代码

```csharp
// UserDto 没有 [Facet] 特性
public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

var dto = user.ToFacet<User, UserDto>(); // ❌ FAC001
```

#### 正确代码

```csharp
[Facet(typeof(User))]
public partial class UserDto { }

var dto = user.ToFacet<User, UserDto>(); // ✅ OK
```

---

### FAC002

**考虑使用双泛型变体以获得更好性能**

- **严重性**：Info
- **类别**：Performance

#### 描述

使用单泛型扩展方法如 `ToFacet<TTarget>()` 时，库使用反射来发现源类型。为了更好的性能，使用双泛型变体 `ToFacet<TSource, TTarget>()`。

#### 触发警告的代码

```csharp
var dto = user.ToFacet<UserDto>(); // ℹ️ FAC002: 考虑使用 ToFacet<User, UserDto>()
```

#### 推荐代码

```csharp
var dto = user.ToFacet<User, UserDto>(); // ✅ 更好的性能
```

#### 影响

性能差异很小（几纳秒），但在紧密循环或高吞吐量场景中可能会累积。

---

## [Facet] 特性分析器

### FAC003

**带有 [Facet] 特性的类型必须声明为 partial**

- **严重性**：Error
- **类别**：Declaration

#### 描述

源生成器要求类型为 `partial`，以便它们可以添加生成的成员。任何标记为 `[Facet]` 的类型都必须声明为 `partial`。

#### 错误代码

```csharp
[Facet(typeof(User))]
public class UserDto { } // ❌ FAC003: 缺少 'partial' 关键字
```

#### 正确代码

```csharp
[Facet(typeof(User))]
public partial class UserDto { } // ✅ OK
```

---

### FAC004

**属性名在源类型中不存在**

- **严重性**：Error
- **类别**：Usage

#### 描述

在 `Exclude` 或 `Include` 参数中指定的属性名必须存在于源类型中。

#### 错误代码

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[Facet(typeof(User), "PasswordHash")] // ❌ FAC004: User 没有 PasswordHash
public partial class UserDto { }
```

#### 正确代码

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }
}

[Facet(typeof(User), "PasswordHash")] // ✅ OK
public partial class UserDto { }
```

---

### FAC005

**源类型不可访问或不存在**

- **严重性**：Error
- **类别**：Usage

#### 描述

在 `[Facet]` 特性中指定的源类型必须是有效的、可访问的类型。

#### 错误代码

```csharp
[Facet(typeof(NonExistentType))] // ❌ FAC005
public partial class UserDto { }
```

#### 正确代码

```csharp
[Facet(typeof(User))] // ✅ OK
public partial class UserDto { }
```

---

### FAC006

**Configuration 类型未实现所需接口**

- **严重性**：Error
- **类别**：Usage

#### 描述

Configuration 类型必须实现 `IFacetMapConfiguration<TSource, TTarget>`、`IFacetMapConfigurationAsync<TSource, TTarget>` 或提供静态 `Map` 方法。

#### 错误代码

```csharp
public class UserMapper // ❌ 无接口，无 Map 方法
{
    public void DoSomething(User source, UserDto target) { }
}

[Facet(typeof(User), Configuration = typeof(UserMapper))] // ❌ FAC006
public partial class UserDto { }
```

#### 正确代码

```csharp
// 选项 1：实现接口
public class UserMapper : IFacetMapConfiguration<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
    }
}

// 选项 2：提供静态 Map 方法
public class UserMapper
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
    }
}

[Facet(typeof(User), Configuration = typeof(UserMapper))] // ✅ OK
public partial class UserDto { }
```

---

### FAC007

**嵌套 facet 类型未标记 [Facet] 特性**

- **严重性**：Warning
- **类别**：Usage

#### 描述

在 `NestedFacets` 数组中指定的所有类型都必须标记 `[Facet]` 特性。

#### 错误代码

```csharp
public class AddressDto { } // ❌ 缺少 [Facet] 特性

[Facet(typeof(User), NestedFacets = [typeof(AddressDto)])] // ⚠️ FAC007
public partial class UserDto { }
```

#### 正确代码

```csharp
[Facet(typeof(Address))]
public partial class AddressDto { }

[Facet(typeof(User), NestedFacets = [typeof(AddressDto)])] // ✅ OK
public partial class UserDto { }
```

---

### FAC008

**循环引用可能导致栈溢出**

- **严重性**：Warning
- **类别**：Performance

#### 描述

当 `MaxDepth` 设置为 0（无限制）且 `PreserveReferences` 为 `false` 时，对象图中的循环引用可能导致栈溢出异常。

#### 错误代码

```csharp
[Facet(typeof(User),
    MaxDepth = 0,
    PreserveReferences = false,
    NestedFacets = [typeof(CompanyDto)])] // ⚠️ FAC008
public partial class UserDto { }
```

#### 正确代码

```csharp
// 选项 1：启用 PreserveReferences（默认）
[Facet(typeof(User),
    NestedFacets = [typeof(CompanyDto)])] // ✅ OK (PreserveReferences 默认为 true)

// 选项 2：设置 MaxDepth 限制
[Facet(typeof(User),
    MaxDepth = 5,
    NestedFacets = [typeof(CompanyDto)])] // ✅ OK

// 选项 3：两者都设置
[Facet(typeof(User),
    MaxDepth = 10,
    PreserveReferences = true,
    NestedFacets = [typeof(CompanyDto)])] // ✅ OK (最安全)
```

---

### FAC009

**不能同时指定 Include 和 Exclude**

- **严重性**：Error
- **类别**：Usage

#### 描述

`Include` 和 `Exclude` 参数互斥。使用 `Include` 来白名单属性或使用 `Exclude` 来黑名单属性，但不能同时使用。

#### 错误代码

```csharp
[Facet(typeof(User),
    nameof(User.PasswordHash),  // Exclude 参数
    Include = [nameof(User.Id), nameof(User.Name)])] // ❌ FAC009: 不能同时使用
public partial class UserDto { }
```

#### 正确代码

```csharp
// 选项 1：Exclude 方法
[Facet(typeof(User), nameof(User.PasswordHash), nameof(User.SecretKey))] // ✅ OK
public partial class UserDto { }

// 选项 2：Include 方法
[Facet(typeof(User), Include = [nameof(User.Id), nameof(User.Name), nameof(User.Email)])] // ✅ OK
public partial class UserDto { }
```

---

### FAC010

**MaxDepth 值异常**

- **严重性**：Warning
- **类别**：Performance

#### 描述

MaxDepth 值通常应在 1 到 10 之间。负值无效，超过 100 的值可能表示配置错误。

#### 触发警告的代码

```csharp
[Facet(typeof(User), MaxDepth = -1)] // ⚠️ FAC010: 负值
[Facet(typeof(User), MaxDepth = 500)] // ⚠️ FAC010: 过大
```

#### 正确代码

```csharp
[Facet(typeof(User), MaxDepth = 5)] // ✅ OK
[Facet(typeof(User), MaxDepth = 10)] // ✅ OK (默认)
```

---

## [GenerateDtos] 特性分析器

### FAC011

**[GenerateDtos] 只能应用于类**

- **严重性**：Error
- **类别**：Usage

#### 描述

`[GenerateDtos]` 和 `[GenerateAuditableDtos]` 特性专为类类型设计，不能应用于结构体、接口或其他类型。

#### 错误代码

```csharp
[GenerateDtos(DtoTypes.All)]
public struct Product { } // ❌ FAC011: 不能用于结构体
```

#### 正确代码

```csharp
[GenerateDtos(DtoTypes.All)]
public class Product { } // ✅ OK
```

---

### FAC012

**排除的属性不存在**

- **严重性**：Warning
- **类别**：Usage

#### 描述

在 `ExcludeProperties` 中指定的属性应存在于源类型中。

#### 错误代码

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[GenerateDtos(DtoTypes.All,
    ExcludeProperties = ["InternalNotes"])] // ⚠️ FAC012: 不存在
public class Product { }
```

#### 正确代码

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string InternalNotes { get; set; }
}

[GenerateDtos(DtoTypes.All,
    ExcludeProperties = ["InternalNotes"])] // ✅ OK
public class Product { }
```

---

### FAC013

**未选择要生成的 DTO 类型**

- **严重性**：Warning
- **类别**：Usage

#### 描述

将 `Types` 设置为 `DtoTypes.None` 将不会生成任何 DTO。

#### 错误代码

```csharp
[GenerateDtos(Types = DtoTypes.None)] // ⚠️ FAC013: 不会生成 DTO
public class Product { }
```

#### 正确代码

```csharp
[GenerateDtos(Types = DtoTypes.All)] // ✅ OK
public class Product { }

// 或指定特定类型
[GenerateDtos(Types = DtoTypes.Create | DtoTypes.Update | DtoTypes.Response)]
public class Product { }
```

---

## [Flatten] 特性分析器

### FAC014

**带有 [Flatten] 特性的类型必须声明为 partial**

- **严重性**：Error
- **类别**：Declaration

#### 描述

与 `[Facet]` 类似，标记为 `[Flatten]` 的类型必须是 `partial`。

#### 错误代码

```csharp
[Flatten(typeof(Person))]
public class PersonFlat { } // ❌ FAC014
```

#### 正确代码

```csharp
[Flatten(typeof(Person))]
public partial class PersonFlat { } // ✅ OK
```

---

### FAC015

**源类型不可访问或不存在**

- **严重性**：Error
- **类别**：Usage

#### 描述

在 `[Flatten]` 特性中指定的源类型必须有效且可访问。

#### 错误代码

```csharp
[Flatten(typeof(NonExistentType))] // ❌ FAC015
public partial class PersonFlat { }
```

#### 正确代码

```csharp
[Flatten(typeof(Person))] // ✅ OK
public partial class PersonFlat { }
```

---

### FAC016

**MaxDepth 值异常**

- **严重性**：Warning
- **类别**：Performance

#### 描述

对于扁平化场景，MaxDepth 值通常应在 1 到 5 之间。超过 10 的值可能导致生成过多属性。

#### 触发警告的代码

```csharp
[Flatten(typeof(Person), MaxDepth = -1)] // ⚠️ FAC016: 负值
[Flatten(typeof(Person), MaxDepth = 50)] // ⚠️ FAC016: 过大
```

#### 正确代码

```csharp
[Flatten(typeof(Person), MaxDepth = 3)] // ✅ OK (默认)
[Flatten(typeof(Person), MaxDepth = 5)] // ✅ OK
```

---

### FAC017

**LeafOnly 命名策略可能导致属性名冲突**

- **严重性**：Info
- **类别**：Usage

#### 描述

使用 `FlattenNamingStrategy.LeafOnly` 可能在多个嵌套对象具有相同名称的属性时导致名称冲突。考虑使用 `Prefix` 策略。

#### 触发警告的代码

```csharp
[Flatten(typeof(Person),
    NamingStrategy = FlattenNamingStrategy.LeafOnly)] // ℹ️ FAC017
public partial class PersonFlat { }
```

#### 潜在问题

```csharp
public class Person
{
    public Address HomeAddress { get; set; }
    public Address WorkAddress { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}

// 使用 LeafOnly，两个地址都映射到 "Street" 和 "City" → 冲突！
```

#### 推荐代码

```csharp
[Flatten(typeof(Person),
    NamingStrategy = FlattenNamingStrategy.Prefix)] // ✅ 更好
public partial class PersonFlat { }

// 生成：HomeAddressStreet, HomeAddressCity, WorkAddressStreet, WorkAddressCity
```

---

## 源签名分析器

### FAC022

**源实体结构已更改**

- **严重性**：Warning
- **类别**：SourceTracking

#### 描述

当您在 `[Facet]` 特性上设置 `SourceSignature` 时，分析器会计算源类型属性的哈希值并将其与存储的签名进行比较。当源实体的结构发生变化时会引发此警告。

#### 触发警告的代码

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }  // 新增属性
}

[Facet(typeof(User), SourceSignature = "oldvalue")]  // ⚠️ FAC022
public partial class UserDto { }
```

#### 解决方法

使用提供的代码修复来更新签名，或手动更新：

```csharp
[Facet(typeof(User), SourceSignature = "newvalue")]  // ✅ OK
public partial class UserDto { }
```

#### 注意事项

- 签名是从属性名称和类型计算的 8 字符哈希值
- 计算签名时遵循 `Include`/`Exclude` 过滤器
- 代码修复提供程序自动提供更新签名的选项
- 详见[源签名变更跟踪](16_SourceSignature.md)

---

## 抑制分析器规则

如果需要抑制特定的分析器规则，可以使用：

### 在代码中

```csharp
#pragma warning disable FAC002
var dto = user.ToFacet<UserDto>();
#pragma warning restore FAC002
```

### 在 .editorconfig 中

```ini
[*.cs]
dotnet_diagnostic.FAC002.severity = none
```

### 对于整个项目

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);FAC002</NoWarn>
</PropertyGroup>
```

---

## 配置

所有分析器默认启用。您可以在 `.editorconfig` 文件中配置其严重性：

```ini
[*.cs]

# 将规则设置为错误
dotnet_diagnostic.FAC007.severity = error

# 将规则设置为警告
dotnet_diagnostic.FAC002.severity = warning

# 禁用规则
dotnet_diagnostic.FAC017.severity = none
```

---

## 另请参阅

- [特性参考](03_AttributeReference.md)
- [自定义映射](04_CustomMapping.md)
- [高级场景](06_AdvancedScenarios.md)
