# 高级场景

本节涵盖 Facet 的高级用例和配置选项。

## 从一个源创建多个 Facet

您可以从同一个源类型创建多个 facet：

```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Password { get; set; }
    public decimal Salary { get; set; }
    public string Department { get; set; }
}

// 公共资料（排除敏感数据）
[Facet(typeof(User), nameof(User.Password), nameof(User.Salary))]
public partial class UserPublicDto { }

// 仅联系信息（包含特定字段）
[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.LastName), nameof(User.Email)])]
public partial class UserContactDto { }

// 列表摘要（包含最少数据）
[Facet(typeof(User), Include = [nameof(User.Id), nameof(User.FirstName), nameof(User.LastName)])]
public partial class UserSummaryDto { }

// HR 视图（排除密码但包含薪资）
[Facet(typeof(User), nameof(User.Password))]
public partial class UserHRDto { }
```

## Include vs Exclude 模式

### Include 模式 - 构建聚焦的 DTO

当您希望 facet 仅包含特定属性时，使用 `Include` 模式：

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
    public string InternalNotes { get; set; }
    public decimal Cost { get; set; }
    public string SKU { get; set; }
}

// 仅包含面向客户的数据的 API 响应
[Facet(typeof(Product), Include = [nameof(Product.Id), nameof(Product.Name), nameof(Product.Description), nameof(Product.Price), nameof(Product.IsAvailable)])]
public partial record ProductApiDto;

// 包含最少数据的搜索结果
[Facet(typeof(Product), Include = [nameof(Product.Id), nameof(Product.Name), nameof(Product.Price)])]
public partial record ProductSearchDto;

// 包含成本数据的内部管理视图
[Facet(typeof(Product), Include = [nameof(Product.Id), nameof(Product.Name), nameof(Product.Price), nameof(Product.Cost), nameof(Product.SKU), nameof(Product.InternalNotes)])]
public partial class ProductAdminDto;
```

### Exclude 模式 - 隐藏敏感数据

当您需要大多数属性但需要隐藏特定属性时，使用 `Exclude` 模式：

```csharp
// 仅排除敏感信息
[Facet(typeof(User), nameof(User.Password))]
public partial class UserDto { }

// 排除多个敏感字段
[Facet(typeof(Employee), nameof(Employee.Salary), nameof(Employee.SSN))]
public partial class EmployeePublicDto { }
```

## 使用字段

### 包含字段示例

```csharp
public class LegacyEntity
{
    public int Id;
    public string Name;
    public DateTime CreatedDate;
    public string Status { get; set; }
    public string Notes { get; set; }
}

// 包含特定字段和属性
[Facet(typeof(LegacyEntity), Include = [nameof(LegacyEntity.Name), nameof(LegacyEntity.Status)], IncludeFields = true)]
public partial class LegacyEntityDto;

// 仅包含属性（即使列出也忽略字段）
[Facet(typeof(LegacyEntity), Include = [nameof(LegacyEntity.Status), nameof(LegacyEntity.Notes), nameof(LegacyEntity.Name)], IncludeFields = false)]
public partial class LegacyEntityPropsOnlyDto;
```

## 嵌套 Facet - 组合 DTO

Facet 通过 `NestedFacets` 参数支持嵌套对象的自动映射。这消除了手动声明嵌套属性和处理其映射的需要。

### 基本嵌套 Facet

```csharp
// 具有嵌套结构的源实体
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
}

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Address HeadquartersAddress { get; set; }
}

// 具有自动嵌套映射的 Facet DTO
[Facet(typeof(Address))]
public partial record AddressDto;

[Facet(typeof(Company), NestedFacets = [typeof(AddressDto)])]
public partial record CompanyDto;

// 用法
var company = new Company
{
    Name = "Acme Corp",
    HeadquartersAddress = new Address { City = "San Francisco" }
};

var companyDto = new CompanyDto(company);
// companyDto.HeadquartersAddress 自动成为 AddressDto
```

### 多级嵌套

```csharp
public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PasswordHash { get; set; }
    public decimal Salary { get; set; }
    public Company Company { get; set; }
    public Address HomeAddress { get; set; }
}

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Company Company { get; set; }
    public Employee Manager { get; set; }
}

// 具有多个嵌套 facet 的 Employee DTO
[Facet(typeof(Employee), exclude: ["PasswordHash", "Salary"],
    NestedFacets = [typeof(CompanyDto), typeof(AddressDto)])]
public partial record EmployeeDto;

// 具有深度嵌套结构的 Department DTO
[Facet(typeof(Department), NestedFacets = [typeof(CompanyDto), typeof(EmployeeDto)])]
public partial record DepartmentDto;

// 用法 - 自动处理 3+ 级嵌套
var department = new Department
{
    Name = "Engineering",
    Company = new Company
    {
        Name = "Tech Corp",
        HeadquartersAddress = new Address { City = "Seattle" }
    },
    Manager = new Employee
    {
        FirstName = "Jane",
        Company = new Company
        {
            Name = "Tech Corp",
            HeadquartersAddress = new Address { City = "Seattle" }
        },
        HomeAddress = new Address { City = "Bellevue" }
    }
};

var departmentDto = new DepartmentDto(department);
// departmentDto.Manager.Company.HeadquartersAddress.City == "Seattle"
```

### NestedFacets 如何工作

**自动类型检测：**
- 生成器检查每个嵌套 facet 的源类型
- 父源中与嵌套 facet 源类型匹配的属性会自动替换
- 例如，如果 `CompanyDto` 从 `Company` facet，任何 `Company` 属性都变成 `CompanyDto`

**生成的代码：**
```csharp
// 对于：[Facet(typeof(Company), NestedFacets = [typeof(AddressDto)])]
public partial record CompanyDto
{
    public int Id { get; init; }
    public string Name { get; init; }
    public AddressDto HeadquartersAddress { get; init; } // 自动变成 AddressDto

    public CompanyDto(Company source)
        : this(source.Id, source.Name, new AddressDto(source.HeadquartersAddress)) // 自动嵌套
    { }

    public Company ToSource()
    {
        return new Company
        {
            Id = this.Id,
            Name = this.Name,
            HeadquartersAddress = this.HeadquartersAddress.ToSource() // 自动反向映射
        };
    }
}
```

### 带嵌套 Facet 的 EF Core 投影

```csharp
// 与 Entity Framework Core 无缝协作
var companies = await dbContext.Companies
    .Where(c => c.IsActive)
    .Select(CompanyDto.Projection)
    .ToListAsync();

// 生成的投影自动处理嵌套类型：
// c => new CompanyDto(c.Id, c.Name, new AddressDto(c.HeadquartersAddress))
```

### 优势

1. **无需手动属性声明**：不要重新声明嵌套属性
2. **自动构造函数映射**：嵌套构造函数自动调用
3. **ToSource 支持**：反向映射适用于嵌套结构
4. **EF Core 兼容**：投影在数据库查询中有效
5. **多级支持**：处理 3+ 级嵌套

## 集合嵌套 Facet - 使用列表和数组

Facet 完全支持集合中的嵌套 facet，自动将 `List<T>`、`ICollection<T>`、`T[]` 和其他集合类型映射到其相应的嵌套 facet 类型。

### 基本集合映射

```csharp
// 源实体
public class OrderItem
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItem> Items { get; set; }  // 嵌套对象的集合
}

// Facet DTO
[Facet(typeof(OrderItem))]
public partial record OrderItemDto;

[Facet(typeof(Order), NestedFacets = [typeof(OrderItemDto)])]
public partial record OrderDto;

// 用法
var order = new Order
{
    Id = 1,
    OrderNumber = "ORD-2025-001",
    OrderDate = DateTime.Now,
    Items = new List<OrderItem>
    {
        new() { Id = 1, ProductName = "Laptop", Price = 1200.00m, Quantity = 1 },
        new() { Id = 2, ProductName = "Mouse", Price = 25.00m, Quantity = 2 }
    }
};

var orderDto = new OrderDto(order);
// orderDto.Items 是 List<OrderItemDto>
// 每个 OrderItem 自动映射到 OrderItemDto
```

### 支持的集合类型

Facet 自动处理所有常见的集合类型：

```csharp
public class Project
{
    // 所有这些都适用于 NestedFacets：
    public List<Task> Tasks { get; set; }              // List<T>
    public ICollection<Team> Teams { get; set; }       // ICollection<T>
    public IList<Milestone> Milestones { get; set; }   // IList<T>
    public IEnumerable<Comment> Comments { get; set; } // IEnumerable<T>
    public Employee[] Employees { get; set; }          // T[]（数组）
}

[Facet(typeof(Project), NestedFacets = [typeof(TaskDto), typeof(TeamDto), /* ... */])]
public partial record ProjectDto;
// 所有集合自动映射到其相应的 DTO 集合类型：
// - List<Task> → List<TaskDto>
// - ICollection<Team> → ICollection<TeamDto>（实现为 List）
// - Employee[] → EmployeeDto[]
```

### 集合的生成代码

生成器创建高效的基于 LINQ 的转换：

```csharp
// 生成的构造函数
public OrderDto(Order source)
{
    Id = source.Id;
    OrderNumber = source.OrderNumber;
    OrderDate = source.OrderDate;

    // 使用 LINQ Select 映射每个元素
    Items = source.Items.Select(x => new OrderItemDto(x)).ToList();
}

// 生成的 ToSource 方法
public Order ToSource()
{
    return new Order
    {
        Id = this.Id,
        OrderNumber = this.OrderNumber,
        OrderDate = this.OrderDate,

        // 将每个 DTO 映射回实体
        Items = this.Items.Select(x => x.ToSource()).ToList()
    };
}
```

### 多级集合嵌套

集合可以在任何深度嵌套：

```csharp
public class OrderItem
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public List<OrderItemOption> Options { get; set; }  // 嵌套集合
}

public class Order
{
    public int Id { get; set; }
    public List<OrderItem> Items { get; set; }  // 包含集合的对象的集合
}

[Facet(typeof(OrderItemOption))]
public partial record OrderItemOptionDto;

[Facet(typeof(OrderItem), NestedFacets = [typeof(OrderItemOptionDto)])]
public partial record OrderItemDto;

[Facet(typeof(Order), NestedFacets = [typeof(OrderItemDto)])]
public partial record OrderDto;

// 用法
var order = new Order
{
    Items = new List<OrderItem>
    {
        new()
        {
            ProductName = "Laptop",
            Options = new List<OrderItemOption>
            {
                new() { Name = "Extended Warranty" },
                new() { Name = "Gift Wrap" }
            }
        }
    }
};

var dto = new OrderDto(order);
// dto.Items[0].Options 是 List<OrderItemOptionDto>
```

### 混合集合和单个属性

您可以在同一实体中同时拥有集合和单个嵌套 facet：

```csharp
public class Order
{
    public int Id { get; set; }
    public Address ShippingAddress { get; set; }      // 单个嵌套对象
    public Address BillingAddress { get; set; }       // 另一个单个嵌套对象
    public List<OrderItem> Items { get; set; }        // 嵌套对象的集合
}

[Facet(typeof(Address))]
public partial record AddressDto;

[Facet(typeof(OrderItem))]
public partial record OrderItemDto;

[Facet(typeof(Order), NestedFacets = [typeof(AddressDto), typeof(OrderItemDto)])]
public partial record OrderDto;

// 生成的 OrderDto 将具有：
// - AddressDto ShippingAddress
// - AddressDto BillingAddress
// - List<OrderItemDto> Items
```

### 带集合嵌套 Facet 的 EF Core 投影

集合与 Entity Framework Core 查询无缝协作：

```csharp
// 高效的数据库投影
var orders = await dbContext.Orders
    .Include(o => o.Items)  // 包含相关数据
    .Where(o => o.OrderDate >= DateTime.Today.AddDays(-30))
    .Select(OrderDto.Projection)
    .ToListAsync();

// 生成的 Projection 属性自动处理集合：
// source => new OrderDto
// {
//     Id = source.Id,
//     OrderNumber = source.OrderNumber,
//     Items = source.Items.Select(x => new OrderItemDto(x)).ToList()
// }
```

### 集合嵌套 Facet 最佳实践

1. **按依赖顺序定义 facet**：在父 facet 之前定义子 facet
2. **对一对多关系使用集合**：非常适合 Entity Framework 导航属性
3. **考虑大集合的性能**：在内存中映射大集合时要注意
4. **处理空集合**：初始化集合以避免空引用异常

### 集合嵌套 Facet 的优势

1. **自动集合映射**：无需手动 LINQ Select 调用
2. **类型安全**：编译器验证的集合元素类型
3. **双向支持**：正向和反向（`ToSource()`）映射
4. **EF Core 优化**：与数据库投影高效协作
5. **保留集合类型**：列表保持列表，数组保持数组
6. **多级支持**：集合的无限嵌套深度

## 继承和基类

### 包含基类的属性

Include 模式与继承无缝协作：

```csharp
public class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
}

public class Product : BaseEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}

// 包含基类和派生类的属性
[Facet(typeof(Product), Include = [nameof(Product.Id), nameof(Product.Name), nameof(Product.Price)])]
public partial class ProductSummaryDto;

// 仅包含派生类属性
[Facet(typeof(Product), Include = [nameof(Product.Name), nameof(Product.Category)])]
public partial class ProductInfoDto;
```

### 嵌套类

include 和 exclude 都适用于嵌套类：

```csharp
public class OuterClass
{
    [Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.LastName)])]
    public partial class NestedUserDto { }
}
```

## 与 Include 结合的自定义映射

您可以将 Include 模式与自定义映射结合使用：

```csharp
public class UserIncludeMapper : IFacetMapConfiguration<User, UserFormattedDto>
{
    public static void Map(User source, UserFormattedDto target)
    {
        target.DisplayName = $"{source.FirstName} {source.LastName}".ToUpper();
    }
}

[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.LastName)], Configuration = typeof(UserIncludeMapper))]
public partial class UserFormattedDto
{
    public string DisplayName { get; set; } = string.Empty;
}
```

## 用于查询和补丁模型的可空属性

`NullableProperties` 参数使生成的 facet 中的所有非可空属性变为可空，这对于查询 DTO 和部分更新场景非常有用。

### 查询/过滤 DTO

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
}

// 所有属性变为可空以实现灵活查询
[Facet(typeof(Product), "CreatedAt", NullableProperties = true, GenerateToSource = false)]
public partial class ProductQueryDto;

// 用法：仅指定要过滤的字段
var query = new ProductQueryDto
{
    Name = "Widget",           // 按名称过滤
    Price = 50.00m,            // 按价格过滤
    IsAvailable = true         // 按可用性过滤
    // Id、CategoryId 保持 null（不是过滤的一部分）
};

// 在 LINQ 查询中使用
var results = products.Where(p =>
    (query.Name == null || p.Name.Contains(query.Name)) &&
    (query.Price == null || p.Price == query.Price) &&
    (query.IsAvailable == null || p.IsAvailable == query.IsAvailable)
).ToList();
```

### 补丁/更新模型

```csharp
// 创建补丁模型，仅更新非空字段
[Facet(typeof(User), "Id", "CreatedAt", NullableProperties = true, GenerateToSource = false)]
public partial class UserPatchDto;

// 用法：仅更新特定字段
var patch = new UserPatchDto
{
    Email = "newemail@example.com",  // 更新电子邮件
    IsActive = false                 // 更新活动状态
    // 其他属性保持 null（不会更新）
};

// 应用补丁
void ApplyPatch(User user, UserPatchDto patch)
{
    if (patch.FirstName != null) user.FirstName = patch.FirstName;
    if (patch.LastName != null) user.LastName = patch.LastName;
    if (patch.Email != null) user.Email = patch.Email;
    if (patch.IsActive != null) user.IsActive = patch.IsActive.Value;
    // ... 等等
}
```

### NullableProperties 如何工作

- **值类型**：变为可空（`int` → `int?`、`bool` → `bool?`、`DateTime` → `DateTime?`、枚举 → `EnumType?`）
- **引用类型**：保持引用类型但标记为可空（`string` → `string`）
- **已经可空的类型**：保持可空（`DateTime?` 保持 `DateTime?`）

### 重要考虑事项

1. **禁用 GenerateToSource**：使用 `NullableProperties = true` 时，设置 `GenerateToSource = false`，因为将可空属性映射回非可空源属性在逻辑上不合理。

2. **构造函数行为**：生成的构造函数仍会正确地从源映射到可空属性。

3. **与 GenerateDtos Query 的比较**：这提供了与 `GenerateDtos` 中的 Query DTO 相同的功能，但使用 `Facet` 特性提供了更多控制。

```csharp
// 类似于 GenerateDtos Query DTO
[Facet(typeof(Product), NullableProperties = true, GenerateToSource = false)]
public partial record ProductQueryRecord;
```

## 枚举转换

`ConvertEnumsTo` 属性将源类型中的所有枚举属性转换为生成的 facet 中的 `string` 或 `int`，这对于 API 响应、序列化和数据库存储很有用。

### 转换为字符串

```csharp
public enum OrderStatus { Draft, Submitted, Processing, Completed, Cancelled }

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
}

[Facet(typeof(Order), ConvertEnumsTo = typeof(string), GenerateToSource = true)]
public partial class OrderDto;

// 用法
var order = new Order { Id = 1, CustomerName = "Alice", Status = OrderStatus.Processing, Total = 99.99m };
var dto = new OrderDto(order);
dto.Status // "Processing"（字符串，不是 OrderStatus）

// 往返
var entity = dto.ToSource();
entity.Status // OrderStatus.Processing
```

### 转换为 Int

```csharp
[Facet(typeof(Order), ConvertEnumsTo = typeof(int), GenerateToSource = true)]
public partial class OrderIntDto;

var dto = new OrderIntDto(order);
dto.Status // 2（OrderStatus.Processing 的 int 值）
```

### 可空枚举处理

可空枚举属性在转换后保留其可空性：

```csharp
public class Entity
{
    public int Id { get; set; }
    public OrderStatus? Status { get; set; }       // 可空
    public OrderStatus Priority { get; set; }      // 非可空
}

[Facet(typeof(Entity), ConvertEnumsTo = typeof(string))]
public partial class EntityStringDto;
// Status: string（源为 null 时为 null）
// Priority: string

[Facet(typeof(Entity), ConvertEnumsTo = typeof(int))]
public partial class EntityIntDto;
// Status: int?（可空）
// Priority: int
```

### 与 NullableProperties 结合

```csharp
[Facet(typeof(Order), ConvertEnumsTo = typeof(string), NullableProperties = true, GenerateToSource = false)]
public partial class OrderQueryDto;
// 所有属性可空 + 枚举为字符串 - 非常适合过滤 DTO
```

### EF Core 投影

枚举转换包含在生成的 Projection 表达式中，并正确转换为 SQL：

```csharp
var results = await dbContext.Orders
    .Where(o => o.Status == OrderStatus.Completed)
    .Select(OrderDto.Projection)
    .ToListAsync();
// Status 列在 DTO 中作为字符串返回
```

### 重要说明

- **所有枚举都被转换**：该设置适用于每个枚举属性。对于混合行为，使用单独的 facet 或自定义配置。
- **非枚举属性不受影响**：仅转换枚举类型的属性。
- **支持的目标类型**：仅支持 `typeof(string)` 和 `typeof(int)`。

参见[枚举转换](20_ConvertEnumsTo.zh-CN.md)获取完整参考。

## 混合使用模式

### API 层模式

```csharp
// 控制器对不同端点使用不同的 facet
[ApiController]
public class UsersController : ControllerBase
{
    [HttpGet]
    public List<UserSummaryDto> GetUsers()
    {
        return users.SelectFacets<User, UserSummaryDto>().ToList();
    }

    [HttpGet("{id}")]
    public UserDetailDto GetUser(int id)
    {
        return user.ToFacet<User, UserDetailDto>();
    }

    [HttpPost]
    public IActionResult CreateUser(UserCreateDto dto)
    {
        var user = dto.ToSource();
        // 保存用户...
    }
}

// 不同用例的不同 DTO
[Facet(typeof(User), Include = [nameof(User.Id), nameof(User.FirstName), nameof(User.LastName)])]
public partial record UserSummaryDto;

[Facet(typeof(User), nameof(User.Password))] // 排除密码但包含其他所有内容
public partial class UserDetailDto;

[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.LastName), nameof(User.Email), nameof(User.Department)])]
public partial class UserCreateDto;
```

## 与 Include 结合的 Record 类型

Include 模式与现代 C# record 完美配合：

```csharp
public record ModernUser
{
    public required string Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string? Bio { get; set; }
}

// 仅生成具有特定属性的 record
[Facet(typeof(ModernUser), Include = [nameof(ModernUser.FirstName), nameof(ModernUser.LastName), nameof(ModernUser.Email)])]
public partial record ModernUserContactRecord;

// 包含 init-only 保留
[Facet(typeof(ModernUser),
       Include = [nameof(ModernUser.Id), nameof(ModernUser.FirstName), nameof(ModernUser.LastName)],
       PreserveInitOnlyProperties = true)]
public partial record ModernUserImmutableRecord;
```

## 性能考虑

### Include vs Exclude 性能

- **Include 模式**：生成更小的 facet，可以提高序列化性能并减少内存使用
- **Exclude 模式**：当您需要源类型的大多数属性时更好

### 生成代码比较

```csharp
// Include 模式 - 生成最少的代码
[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.Email)])]
public partial class UserMinimalDto;
// 生成：仅 FirstName 和 Email 属性

// Exclude 模式 - 生成更多代码
[Facet(typeof(User), nameof(User.Password))]
public partial class UserFullDto;
// 生成：除 Password 外的所有属性
```

## 与 Include 结合的 ToSource 方法行为

使用 Include 模式时，`ToSource()` 方法生成的源对象对于非包含的属性使用默认值：

```csharp
[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.LastName), nameof(User.Email)])]
public partial class UserContactDto;

var dto = new UserContactDto();
var sourceUser = dto.ToSource();

// sourceUser.FirstName = dto.FirstName（已复制）
// sourceUser.LastName = dto.LastName（已复制）
// sourceUser.Email = dto.Email（已复制）
// sourceUser.Id = 0（int 的默认值）
// sourceUser.Password = string.Empty（string 的默认值）
// sourceUser.IsActive = false（bool 的默认值）
```

## 用于验证和元数据的特性复制

`CopyAttributes` 参数启用从源类型成员到生成的 facet 的特性自动复制。这对于在 DTO 中保留验证特性、显示元数据和 JSON 序列化设置特别有用。

### 基本特性复制

```csharp
public class CreateUserRequest
{
    [Required(ErrorMessage = "名字是必需的")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "名字必须为 2-50 个字符")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "无效的电子邮件地址")]
    public string Email { get; set; } = string.Empty;

    [Range(18, 120, ErrorMessage = "年龄必须在 18 到 120 之间")]
    public int Age { get; set; }

    [Phone(ErrorMessage = "无效的电话号码")]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }
}

// 生成复制所有验证特性的 DTO
[Facet(typeof(CreateUserRequest), CopyAttributes = true)]
public partial class CreateUserDto;
```

生成的 `CreateUserDto` 将包含所有验证特性。特性复制适用于验证、显示和 JSON 特性。

### 与 Exclude/Include 结合

特性仅为 facet 中包含的属性复制：

```csharp
// 排除密码 - 其特性不会被复制
[Facet(typeof(User), "Password", CopyAttributes = true)]
public partial class UserDto;

// 仅包含特定属性 - 仅这些属性获得特性
[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.Email)], CopyAttributes = true)]
public partial class UserContactDto;
```

## 最佳实践

### 何时使用 Include

1. **API 响应**：为 API 端点创建聚焦的 DTO
2. **搜索结果**：仅包含搜索列表的基本数据
3. **移动应用**：使用目标 DTO 最小化数据传输
4. **微服务**：创建共享模型的服务特定视图

### 何时使用 Exclude

1. **安全性**：隐藏敏感字段同时保留其他所有内容
2. **遗留代码**：维护现有模式和行为
3. **大型模型**：当您需要复杂实体的大多数属性时

### 命名约定

```csharp
// 基于 include 的 DTO 的描述性名称
[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.LastName)])]
public partial class UserNameOnlyDto; // 清楚说明包含的内容

// 基于 exclude 的 DTO 的传统名称
[Facet(typeof(User), nameof(User.Password))]
public partial class UserDto; // 排除少数字段时的通用 DTO 名称
```

---

另请参阅[表达式映射](10_ExpressionMapping.zh-CN.md)了解高级查询场景，以及[自定义映射](04_CustomMapping.zh-CN.md)了解复杂转换逻辑。


