# Flatten 特性参考

`[Flatten]` 特性自动生成源类型的展平投影，将所有嵌套属性扩展为顶级属性。这对于 API 响应、报表以及需要数据非规范化视图的场景特别有用。

## 什么是展平？

展平将分层对象结构转换为扁平结构，所有嵌套属性都作为顶级属性。无需手动编写具有 `AddressStreet`、`AddressCity` 等属性的 DTO，Facet 通过遍历您的领域模型自动生成它们。

### 示例

**之前（领域模型）：**
```csharp
public class Person
{
    public string FirstName { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}
```

**之后（展平的 DTO）：**
```csharp
[Flatten(typeof(Person))]
public partial class PersonFlatDto
{
    // 自动生成：
    // public string FirstName { get; set; }
    // public string AddressStreet { get; set; }
    // public string AddressCity { get; set; }
}
```

## 关键特性

- **自动属性发现**：递归遍历嵌套对象
- **空安全访问**：在构造函数中使用空条件运算符（`?.`）
- **LINQ 投影**：为 Entity Framework 生成 `Projection` 表达式
- **深度控制**：可配置的最大遍历深度
- **排除路径**：排除特定的嵌套属性
- **ID 过滤**：可选的 `IgnoreNestedIds` 排除外键和嵌套 ID
- **FK 冲突检测**：可选的 `IgnoreForeignKeyClashes` 消除重复的外键数据
- **命名策略**：前缀或仅叶子命名
- **单向操作**：展平是有意设计为单向的（没有 ToSource 方法）

## 用法

### 基本展平

```csharp
[Flatten(typeof(Person))]
public partial class PersonFlatDto
{
    // 所有属性自动生成
}

// 用法
var person = new Person
{
    FirstName = "John",
    Address = new Address { Street = "123 Main St", City = "Springfield" }
};

var dto = new PersonFlatDto(person);
// dto.FirstName = "John"
// dto.AddressStreet = "123 Main St"
// dto.AddressCity = "Springfield"
```

### 用于 Entity Framework 的 LINQ 投影

```csharp
// 高效的数据库投影
var dtos = await dbContext.People
    .Where(p => p.IsActive)
    .Select(PersonFlatDto.Projection)
    .ToListAsync();
```

### 多级嵌套

```csharp
public class Person
{
    public string FirstName { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public Country Country { get; set; }
}

public class Country
{
    public string Name { get; set; }
    public string Code { get; set; }
}

[Flatten(typeof(Person))]
public partial class PersonFlatDto
{
    // 自动生成：
    // public string FirstName { get; set; }
    // public string AddressStreet { get; set; }
    // public string AddressCountryName { get; set; }
    // public string AddressCountryCode { get; set; }
}
```

## 参数

| 参数 | 类型 | 默认值 | 描述 |
|-----------|------|---------|-------------|
| `sourceType` | `Type` | *（必需）* | 要展平的源类型。 |
| `exclude` | `string[]` | `null` | 要从展平中排除的属性路径（例如 `"Address.Country"`、`"Password"`）。 |
| `Exclude` | `string[]` | `null` | 与 `exclude` 相同（命名属性）。 |
| `MaxDepth` | `int` | `3` | 展平嵌套对象时遍历的最大深度。设置为 `0` 表示无限制（不推荐）。 |
| `NamingStrategy` | `FlattenNamingStrategy` | `Prefix` | 展平属性的命名策略。 |
| `IncludeFields` | `bool` | `false` | 除属性外还包括公共字段。 |
| `IncludeCollections` | `bool` | `false` | 将集合属性按原样包含，而不展平其内容。 |
| `GenerateParameterlessConstructor` | `bool` | `true` | 为对象初始化生成无参数构造函数。 |
| `GenerateProjection` | `bool` | `true` | 为数据库查询生成 LINQ 投影表达式。 |
| `UseFullName` | `bool` | `false` | 在生成的文件名中使用完全限定类型名以避免冲突。 |
| `IgnoreNestedIds` | `bool` | `false` | 为 true 时，仅保留根级 `Id` 属性，排除所有外键 ID 和嵌套 ID。 |
| `IgnoreForeignKeyClashes` | `bool` | `false` | 为 true 时，自动跳过会重复外键数据的嵌套 ID 和 FK 属性。 |

## 命名策略

### Prefix 策略（默认）

连接完整路径以创建属性名：

```csharp
[Flatten(typeof(Person), NamingStrategy = FlattenNamingStrategy.Prefix)]
public partial class PersonFlatDto { }

// 生成的属性：
// - FirstName
// - AddressStreet
// - AddressCity
// - AddressCountryName
```

### LeafOnly 策略

仅使用叶子属性名：

```csharp
[Flatten(typeof(Person), NamingStrategy = FlattenNamingStrategy.LeafOnly)]
public partial class PersonFlatDto { }

// 生成的属性：
// - FirstName
// - Street
// - City
// - Name // 警告：可能导致名称冲突！
```

**警告**：`LeafOnly` 如果多个嵌套对象具有相同名称的属性，可能会导致名称冲突。Facet 会自动添加数字后缀（例如 `Name2`、`Name3`）来解决冲突。

## 排除属性

### 排除顶级属性

```csharp
[Flatten(typeof(Person), exclude: "DateOfBirth", "InternalNotes")]
public partial class PersonFlatDto { }
```

### 排除嵌套属性

使用点表示法排除特定的嵌套路径：

```csharp
[Flatten(typeof(Person), exclude: "Address.Country")]
public partial class PersonFlatDto
{
    // 生成：
    // - FirstName
    // - AddressStreet
    // - AddressCity
    // （Country 属性被排除）
}
```

### 排除整个嵌套对象

```csharp
[Flatten(typeof(Person), exclude: "ContactInfo")]
public partial class PersonFlatDto
{
    // ContactInfo 及其所有嵌套属性被排除
}
```

## 忽略嵌套 ID

`IgnoreNestedIds` 参数提供了一种方便的方式来排除除根级 `Id` 之外的所有 ID 属性。这对于 API 响应特别有用，您希望显示数据但不需要所有外键 ID 和嵌套实体 ID。

### 不使用 IgnoreNestedIds（默认）

```csharp
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public DateTime OrderDate { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
}

[Flatten(typeof(Order))]
public partial class OrderFlatDto
{
    // 生成：
    // public int Id { get; set; }
    // public int CustomerId { get; set; }        // 根级外键
    // public int CustomerId2 { get; set; }       // Customer.Id（名称冲突，获得后缀）
    // public string CustomerName { get; set; }
    // public DateTime OrderDate { get; set; }
}
```

### 使用 IgnoreNestedIds

```csharp
[Flatten(typeof(Order), IgnoreNestedIds = true)]
public partial class OrderFlatDto
{
    // 生成：
    // public int Id { get; set; }                // 保留根级 Id
    // public string CustomerName { get; set; }
    // public DateTime OrderDate { get; set; }
    // （CustomerId 和 CustomerId2/Customer.Id 被排除）
}
```

### 行为规则

1. **始终保留根级 `Id`**：源类型的顶级 `Id` 属性被包含
2. **排除外键 ID**：根级的 `CustomerId`、`ProductId` 等属性被排除
3. **排除所有嵌套 ID**：嵌套对象的任何 `Id` 属性被排除

### 用例

- **API 响应**，您不想暴露数据库键
- **报表**，ID 会使输出混乱
- **搜索结果**，您只需要主 ID 用于导航
- **导出文件**，更倾向于人类可读的数据而不是外键

### 示例：清洁的 API 响应

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public int ManufacturerId { get; set; }
    public Manufacturer Manufacturer { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class Manufacturer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Country { get; set; }
}

[Flatten(typeof(Product), IgnoreNestedIds = true)]
public partial class ProductDisplayDto
{
    // 生成：
    // public int Id { get; set; }                    // 保留根 ID
    // public string Name { get; set; }
    // public string CategoryName { get; set; }       // 排除 Category.Id
    // public string ManufacturerName { get; set; }   // 排除 Manufacturer.Id
    // public string ManufacturerCountry { get; set; }
    // （根级的 CategoryId 和 ManufacturerId 被排除）
}

// 清洁的 API 响应，不暴露内部 ID
[HttpGet("products/{id}")]
public async Task<IActionResult> GetProduct(int id)
{
    var product = await dbContext.Products
        .Where(p => p.Id == id)
        .Select(ProductDisplayDto.Projection)
        .FirstOrDefaultAsync();

    return Ok(product);
    // 响应：{ "id": 1, "name": "Widget", "categoryName": "Tools",
    //         "manufacturerName": "ACME", "manufacturerCountry": "USA" }
}
```

## 忽略外键冲突

`IgnoreForeignKeyClashes` 参数有助于在展平具有外键关系的实体时消除重复的 ID 数据。启用后，它会自动检测并跳过会表示与外键属性相同数据的属性。

### 问题：外键重复

在具有外键和导航属性的 Entity Framework 模型中，您通常同时拥有：
1. 外键属性（例如 `AddressId`）
2. 导航属性（例如 `Address`）
3. 引用实体的 ID（例如 `Address.Id`）

展平时，`AddressId` 和 `Address.Id` 都变成 `AddressId`，导致命名冲突并两次表示相同的数据。

### 不使用 IgnoreForeignKeyClashes（默认）

```csharp
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? AddressId { get; set; }  // 外键
    public Address Address { get; set; }  // 导航属性
}

public class Address
{
    public int Id { get; set; }
    public string Line1 { get; set; }
    public string City { get; set; }
}

[Flatten(typeof(Person))]
public partial class PersonFlatDto
{
    // 生成：
    // public int Id { get; set; }
    // public string Name { get; set; }
    // public int? AddressId { get; set; }        // FK 属性
    // public int? AddressId2 { get; set; }       // Address.Id（冲突！）
    // public string AddressLine1 { get; set; }
    // public string AddressCity { get; set; }
}
```

### 使用 IgnoreForeignKeyClashes

```csharp
[Flatten(typeof(Person), IgnoreForeignKeyClashes = true)]
public partial class PersonFlatDto
{
    // 生成：
    // public int Id { get; set; }
    // public string Name { get; set; }
    // public int? AddressId { get; set; }     // 保留 FK 属性
    // public string AddressLine1 { get; set; }
    // public string AddressCity { get; set; }
    // （跳过 Address.Id - 会与 AddressId 重复）
}
```

### 行为规则

1. **检测 FK 模式**：识别以"Id"结尾且具有匹配导航属性的属性
2. **跳过嵌套 ID**：展平导航属性时，如果其 `Id` 属性会与 FK 匹配，则跳过
3. **跳过嵌套 FK**：嵌套对象内的外键也被跳过以避免深层重复
4. **保留根 FK**：根级的外键始终包含
5. **适用于所有深度**：处理复杂场景，如 `Customer.HomeAddressId` 和 `Customer.HomeAddress.Id`

### 示例：复杂嵌套外键

```csharp
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public int CustomerId { get; set; }           // 根 FK
    public Customer Customer { get; set; }
    public int? ShippingAddressId { get; set; }   // 根 FK
    public Address ShippingAddress { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int? HomeAddressId { get; set; }       // 嵌套 FK
    public Address HomeAddress { get; set; }
}

public class Address
{
    public int Id { get; set; }
    public string Line1 { get; set; }
    public string City { get; set; }
}

[Flatten(typeof(Order), IgnoreForeignKeyClashes = true)]
public partial class OrderFlatDto
{
    // 生成：
    // public int Id { get; set; }
    // public DateTime OrderDate { get; set; }
    // public int CustomerId { get; set; }                 // 包含根 FK
    // public string CustomerName { get; set; }
    // public string CustomerEmail { get; set; }
    // public string CustomerHomeAddressLine1 { get; set; }
    // public string CustomerHomeAddressCity { get; set; }
    // public int? ShippingAddressId { get; set; }        // 包含根 FK
    // public string ShippingAddressLine1 { get; set; }
    // public string ShippingAddressCity { get; set; }
    //
    // 跳过（会重复）：
    // - Customer.Id（会与 CustomerId 冲突）
    // - Customer.HomeAddressId（嵌套 FK）
    // - Customer.HomeAddress.Id（会与 CustomerHomeAddressId 冲突）
    // - ShippingAddress.Id（会与 ShippingAddressId 冲突）
}
```

### 用例

- **Entity Framework 模型**，具有显式外键属性
- **API 响应**，不需要重复的 ID 数据
- **更清洁的 DTO**，没有来自 ID 的命名冲突
- **数据库优先模型**，遵循 FK 约定

### 与 IgnoreNestedIds 结合使用

您可以同时使用 `IgnoreNestedIds` 和 `IgnoreForeignKeyClashes`：

```csharp
[Flatten(typeof(Order), IgnoreNestedIds = true, IgnoreForeignKeyClashes = true)]
public partial class OrderDisplayDto
{
    // 这种组合：
    // 1. 忽略除根之外的所有 ID 属性（IgnoreNestedIds）
    // 2. 无需担心 FK 冲突，因为 FK 也是 ID（两者协同工作）
    //
    // 生成：
    // public int Id { get; set; }              // 仅根 ID
    // public DateTime OrderDate { get; set; }
    // public string CustomerName { get; set; }
    // public string CustomerEmail { get; set; }
    // （所有外键和 ID 被排除）
}
```

**注意**：同时使用两者时，`IgnoreNestedIds` 优先，因为它会删除所有 ID 属性（包括 FK）。但是，如果您想保留根级 FK 同时避免冲突重复，`IgnoreForeignKeyClashes` 提供了更细粒度的控制。

## 控制深度

### 默认深度（3 级）

```csharp
[Flatten(typeof(Person))]
public partial class PersonFlatDto { }
// 最多遍历 3 级深度
```

### 自定义深度

```csharp
[Flatten(typeof(Person), MaxDepth = 2)]
public partial class PersonFlatDto { }
// 仅遍历 2 级深度
// Person.Address.Country 不会被包含
```

### 安全限制

即使使用 `MaxDepth = 0`（无限制），Facet 也会强制执行 10 级的安全限制，以防止循环引用或极深层次结构导致的堆栈溢出。

## 什么会被展平？

Facet 自动确定哪些类型应该作为"叶子"属性展平，哪些应该递归：

### 始终展平（叶子属性）
- 基本类型（`int`、`bool`、`decimal` 等）
- `string`
- 枚举
- `DateTime`、`DateTimeOffset`、`TimeSpan`
- `Guid`
- 具有 0-2 个属性的值类型

### 递归（嵌套对象）
- 具有属性的复杂引用类型
- 具有 3+ 个属性的值类型

### 完全忽略（默认）
- 集合（List、Array、IEnumerable 等）- 这些被完全跳过，不生成任何展平属性

## 包含集合

默认情况下，集合被排除在展平类型之外。但是，您可以使用 `IncludeCollections` 参数选择包含集合属性。启用后，集合属性会"提升"到展平类型中，而不尝试展平其内容。

### 不使用 IncludeCollections（默认）

```csharp
public class ApiResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Item> Items { get; set; }
    public string[] Tags { get; set; }
}

[Flatten(typeof(ApiResponse))]
public partial class ApiResponseFlatDto
{
    // 生成：
    // public int Id { get; set; }
    // public string Name { get; set; }
    // （Items 和 Tags 被排除）
}
```

### 使用 IncludeCollections

```csharp
[Flatten(typeof(ApiResponse), IncludeCollections = true)]
public partial class ApiResponseFlatDto
{
    // 生成：
    // public int Id { get; set; }
    // public string Name { get; set; }
    // public List<Item> Items { get; set; }      // 集合按原样包含
    // public string[] Tags { get; set; }         // 数组按原样包含
}
```

### 行为规则

1. **集合不展平**：集合类型及其元素类型完全按声明保留
2. **嵌套对象仍然展平**：嵌套对象的标量属性继续展平
3. **适用于所有集合类型**：`List<T>`、`Array`、`IEnumerable<T>`、`ICollection<T>`、`IList<T>`、`HashSet<T>` 等
4. **命名策略适用**：集合属性名遵循与其他属性相同的命名策略

### 用例

**API 响应：**
```csharp
public class OrderResponse
{
    public int OrderId { get; set; }
    public Customer Customer { get; set; }
    public List<OrderLine> Lines { get; set; }
}

[Flatten(typeof(OrderResponse), IncludeCollections = true)]
public partial class OrderResponseFlat
{
    // 生成：
    // public int OrderId { get; set; }
    // public string CustomerName { get; set; }     // 从 Customer 展平
    // public string CustomerEmail { get; set; }    // 从 Customer 展平
    // public List<OrderLine> Lines { get; set; }   // 集合保留
}
```

**导出/报表数据：**
```csharp
public class ProductExport
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Category Category { get; set; }
    public string[] Tags { get; set; }
    public List<Image> Images { get; set; }
}

[Flatten(typeof(ProductExport), IncludeCollections = true)]
public partial class ProductExportFlat
{
    // 标量属性展平，集合保留
}
```

### 与其他选项结合

您可以将 `IncludeCollections` 与其他 Flatten 选项结合使用：

```csharp
[Flatten(typeof(Order),
    IncludeCollections = true,
    IgnoreNestedIds = true,
    NamingStrategy = FlattenNamingStrategy.SmartLeaf)]
public partial class OrderFlatDto
{
    // 包含集合
    // 排除嵌套 ID
    // SmartLeaf 命名处理冲突
}
```

## 完整示例

### API 响应 DTO

```csharp
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public Customer Customer { get; set; }
    public Address ShippingAddress { get; set; }
    public decimal Total { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
}

[Flatten(typeof(Order))]
public partial class OrderListDto
{
    // 自动生成：
    // public int Id { get; set; }
    // public DateTime OrderDate { get; set; }
    // public int CustomerId { get; set; }
    // public string CustomerName { get; set; }
    // public string CustomerEmail { get; set; }
    // public string ShippingAddressStreet { get; set; }
    // public string ShippingAddressCity { get; set; }
    // public string ShippingAddressState { get; set; }
    // public string ShippingAddressZipCode { get; set; }
    // public decimal Total { get; set; }
}

// API 用法
[HttpGet("orders")]
public async Task<IActionResult> GetOrders()
{
    var orders = await dbContext.Orders
        .Where(o => o.Status == OrderStatus.Completed)
        .Select(OrderListDto.Projection)
        .ToListAsync();

    return Ok(orders);
}
```

### 报表生成

```csharp
public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Department Department { get; set; }
    public Address HomeAddress { get; set; }
    public decimal Salary { get; set; }
}

public class Department
{
    public string Name { get; set; }
    public string Code { get; set; }
    public Manager Manager { get; set; }
}

public class Manager
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

[Flatten(typeof(Employee), exclude: "Salary")] // 排除敏感数据
public partial class EmployeeReportDto { }

// 生成报表
var report = await dbContext.Employees
    .Select(EmployeeReportDto.Projection)
    .ToListAsync();

await GenerateExcelReport(report);
```

### 限制深度以提高性能

```csharp
// 深层次结构
public class Organization
{
    public string Name { get; set; }
    public Location Headquarters { get; set; }
}

public class Location
{
    public string Name { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public City City { get; set; }
}

public class City
{
    public string Name { get; set; }
    public State State { get; set; }
}

public class State
{
    public string Name { get; set; }
    public Country Country { get; set; }
}

public class Country
{
    public string Name { get; set; }
    public string Code { get; set; }
}

// 限制深度为 3 级
[Flatten(typeof(Organization), MaxDepth = 3)]
public partial class OrganizationSummaryDto
{
    // 包含：
    // - Name
    // - HeadquartersName
    // - HeadquartersAddressStreet
    // 不包含 City、State、Country（超过深度 3）
}
```

## 最佳实践

1. **用于只读场景**：展平非常适合 API 响应、报表和显示模型
2. **设置适当的 MaxDepth**：不要展平超过需要的内容 - 它会影响性能和可读性
3. **排除敏感数据**：使用 `exclude` 参数省略密码、薪资或内部字段
4. **考虑 IgnoreNestedIds**：对于公共 API 和报表，使用 `IgnoreNestedIds = true` 避免暴露数据库实现细节
5. **优先使用 Prefix 命名**：避免使用 `LeafOnly`，除非您确定不会有名称冲突
6. **与 LINQ 结合**：使用 `Projection` 属性进行高效的数据库查询
7. **记录展平类型**：添加 XML 注释来解释展平了什么以及为什么

## 常见模式

### 搜索结果

```csharp
[Flatten(typeof(SearchResult), MaxDepth = 2)]
public partial class SearchResultDto { }
```

### 仪表板小部件

```csharp
[Flatten(typeof(DashboardMetrics))]
public partial class MetricsSummaryDto { }
```

### 导出/导入

```csharp
[Flatten(typeof(Product), exclude: ["InternalNotes", "CostPrice"], IgnoreNestedIds = true)]
public partial class ProductExportDto { }
```

## 故障排除

### 名称冲突

**问题**：使用 `LeafOnly` 策略时，多个属性最终具有相同的名称。

**解决方案**：使用 `Prefix` 策略（默认）或在领域模型中手动重命名属性。

### 属性缺失

**问题**：预期的属性未包含在展平的 DTO 中。

**解决方案**：检查 `MaxDepth` 设置 - 您可能需要增加它，或者属性类型可能不被视为"叶子"类型。

### 循环引用

**问题**：担心循环引用导致无限循环。

**解决方案**：Facet 具有内置保护 - 它跟踪访问的类型，不会无限递归，并且有 10 级的安全限制。

## 比较：Flatten vs 带 NestedFacets 的 Facet

| 特性 | `[Flatten]` | 带 `NestedFacets` 的 `[Facet]` |
|---------|-------------|-------------------------------|
| **属性结构** | 所有属性在顶层 | 保留嵌套结构 |
| **命名** | `AddressStreet` | `Address.Street` |
| **ToSource 方法** | 否（仅单向） | 是（双向） |
| **用例** | API 响应、报表、导出 | 完整 CRUD、领域映射 |
| **设置** | 单个特性，自动 | 需要定义每个嵌套 facet |
| **灵活性** | 较少（自动） | 更多（显式控制） |

## FlattenTo 属性（将集合解包为行）

虽然 `[Flatten]` 特性将嵌套对象展平为单个 DTO，但 `[Facet]` 特性上的 `FlattenTo` 属性将集合属性解包为多行。这对于报表、导出以及需要对父子关系进行非规范化的场景很有用。

### 什么是 FlattenTo？

FlattenTo 将具有集合属性的 facet 转换为多行，将父级的属性与每个集合项组合：

```csharp
// 源实体
public class DataEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<ExtendedEntity> Extended { get; set; }
}

public class ExtendedEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DataValue { get; set; }
}

// 集合项的 Facet
[Facet(typeof(ExtendedEntity))]
public partial class ExtendedFacet;

// 带 FlattenTo 的父级 Facet
[Facet(typeof(DataEntity),
    NestedFacets = [typeof(ExtendedFacet)],
    FlattenTo = [typeof(DataFlattenedDto)])]
public partial class DataFacet;

// 展平目标（您定义所需的属性）
public partial class DataFlattenedDto
{
    // 来自父级（DataEntity）的属性
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    // 来自集合项（ExtendedEntity）的属性
    // 使用前缀避免 Name 冲突
    public string ExtendedName { get; set; }
    public int DataValue { get; set; }
}
```

### 用法

```csharp
var entity = new DataEntity
{
    Id = 1,
    Name = "Parent",
    Description = "Parent Description",
    Extended = new List<ExtendedEntity>
    {
        new() { Id = 10, Name = "Item 1", DataValue = 100 },
        new() { Id = 20, Name = "Item 2", DataValue = 200 }
    }
};

var facet = new DataFacet(entity);
var rows = facet.FlattenTo();

// 结果：2 行
// 行 1: { Id: 1, Name: "Parent", Description: "Parent Description", ExtendedName: "Item 1", DataValue: 100 }
// 行 2: { Id: 1, Name: "Parent", Description: "Parent Description", ExtendedName: "Item 2", DataValue: 200 }
```

### 关键特性

- **每项一行输出**：为每个集合项创建一个输出行
- **父数据复制**：父属性复制到每一行
- **类型安全**：目标类型使用所需的属性显式定义
- **空安全**：集合为 null 或空时返回空列表

### 比较：FlattenTo vs Flatten 特性

| 特性 | `FlattenTo` 属性 | `[Flatten]` 特性 |
|---------|---------------------|----------------------|
| **目的** | 将集合解包为行 | 将嵌套对象展平为属性 |
| **输出** | 多行（List） | 单个对象 |
| **输入** | 带集合的 Facet | 带嵌套对象的实体 |
| **控制** | 您定义目标属性 | 属性自动生成 |
| **用例** | 报表、导出 | API 响应、DTO |

## 另请参阅

- [Facet 特性参考](03_AttributeReference.zh-CN.md)
- [高级场景](06_AdvancedScenarios.zh-CN.md)
- [扩展方法](05_Extensions.zh-CN.md)
- [生成了什么代码？](07_WhatIsBeingGenerated.zh-CN.md)



