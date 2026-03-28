# 扩展方法（LINQ、EF Core 等）

Facet.Extensions 提供了一组与提供程序无关的扩展方法，用于在领域实体和生成的 facet 类型之间进行映射和投影。
对于异步 EF Core 支持，请参阅单独的 Facet.Extensions.EFCore 包。

## 方法（Facet.Extensions）

### 映射

| 方法                              | 描述                                                      |
|------------------------------------- |------------------------------------------------------------------|
| `ToFacet<TSource, TTarget>()`        | 使用显式源类型映射单个对象（编译时）。   |
| `ToFacet<TTarget>()`                 | 使用推断的源类型映射单个对象（运行时）。        |
| `ToSource<TFacet, TFacetSource>()`   | 通过生成的 ToSource 方法将 facet 映射回源。         |
| `ToSource<TFacetSource>()`           | 使用推断的 facet 类型将 facet 映射回源。              |
| `SelectFacets<TSource, TTarget>()`   | 使用显式类型映射 `IEnumerable<TSource>`。              |
| `SelectFacets<TTarget>()`            | 使用推断的源类型映射 `IEnumerable`。                 |
| `SelectFacetSources<TFacet, TFacetSource>()` | 将 facet 映射回源。                             |
| `SelectFacetSources<TFacetSource>()` | 使用推断的 facet 类型将 facet 映射回源。            |
| `SelectFacet<TSource, TTarget>()`    | 使用显式类型投影 `IQueryable<TSource>`。           |
| `SelectFacet<TTarget>()`             | 使用推断的源类型投影 `IQueryable`。              |

### 补丁/更新方法（Facet -> Source）

| 方法                                      | 描述                                                      |
|---------------------------------------------|------------------------------------------------------------------|
| `ApplyFacet<TSource, TFacet>()`             | 将 facet 的更改属性应用到源  |
| `ApplyFacet<TFacet>()`                      | 使用推断的源类型应用更改的属性。              |
| `ApplyFacetWithChanges<TSource, TFacet>()`  | 应用更改并返回包含更改属性名称的 `FacetApplyResult`。 |

## 方法（Facet.Extensions.EFCore）

| 方法                              | 描述                                                      |
|------------------------------------- |------------------------------------------------------------------|
| `ToFacetsAsync<TSource, TTarget>()`  | 使用显式源类型异步投影到 `List<TTarget>`。    |
| `ToFacetsAsync<TTarget>()`           | 使用推断的源类型异步投影到 `List<TTarget>`。    |
| `FirstFacetAsync<TSource, TTarget>()`| 使用显式源类型异步投影到 first/default。      |
| `FirstFacetAsync<TTarget>()`         | 使用推断的源类型异步投影到 first/default。      |
| `SingleFacetAsync<TSource, TTarget>()`| 使用显式源类型异步投影到 single。            |
| `SingleFacetAsync<TTarget>()`        | 使用推断的源类型异步投影到 single。            |
| `UpdateFromFacet<TEntity, TFacet>()` | 使用 facet DTO 的更改属性更新实体。            |
| `UpdateFromFacetAsync<TEntity, TFacet>()`| 使用 facet DTO 的更改属性异步更新实体。  |
| `UpdateFromFacetWithChanges<TEntity, TFacet>()`| 更新实体并返回有关更改属性的信息。 |

## 方法（Facet.Extensions.EFCore.Mapping）

对于高级自定义异步映射器支持，请安装单独的包：

```bash
dotnet add package Facet.Extensions.EFCore.Mapping
```

| 方法                              | 描述                                                      |
|------------------------------------- |------------------------------------------------------------------|
| `ToFacetsAsync<TSource, TTarget>(mapper)` | 使用自定义实例映射器进行异步投影（支持 DI）。    |
| `ToFacetsAsync<TSource, TTarget, TAsyncMapper>()` | 使用静态异步映射器进行异步投影。              |
| `FirstFacetAsync<TSource, TTarget>(mapper)` | 使用自定义实例映射器获取第一个（支持 DI）。        |
| `FirstFacetAsync<TSource, TTarget, TAsyncMapper>()` | 使用静态异步映射器获取第一个。                   |
| `SingleFacetAsync<TSource, TTarget>(mapper)` | 使用自定义实例映射器获取单个（支持 DI）。      |
| `SingleFacetAsync<TSource, TTarget, TAsyncMapper>()` | 使用静态异步映射器获取单个。                 |

## 使用示例

### Extensions

```bash
dotnet add package Facet.Extensions
```

```csharp
using Facet.Extensions;

// 正向映射：Source > Facet
var dto = person.ToFacet<PersonDto>();

// 可枚举映射
var dtos = people.SelectFacets<PersonDto>();

// 反向映射：Facet > Source（应用更改）
var updateDto = new PersonDto { Name = "Jane", Email = "jane@example.com" };
person.ApplyFacet(updateDto);  // 仅更新更改的属性

// 跟踪更改以进行审计
var result = person.ApplyFacetWithChanges<Person, PersonDto>(updateDto);

if (result.HasChanges)
{
    Console.WriteLine($"Changed: {string.Join(", ", result.ChangedProperties)}");
}
```

### EF Core 扩展

```bash
dotnet add package Facet.Extensions.EFCore
```

```csharp
// IQueryable (LINQ/EF Core)

using Facet.Extensions.EFCore;

var query = dbContext.People.SelectFacet<PersonDto>();

// 异步（EF Core）
var dtosAsync = await dbContext.People.ToFacetsAsync<PersonDto>();
var dtosInferred = await dbContext.People.ToFacetsAsync<PersonDto>();

var firstDto = await dbContext.People.FirstFacetAsync<Person, PersonDto>();
var firstInferred = await dbContext.People.FirstFacetAsync<PersonDto>();

var singleDto = await dbContext.People.SingleFacetAsync<Person, PersonDto>();
var singleInferred = await dbContext.People.SingleFacetAsync<PersonDto>();
```

#### 自动导航属性加载（无需 `.Include()`！）

使用嵌套 facet 时，EF Core 会自动加载导航属性，无需显式调用 `.Include()`：

```csharp
// 定义嵌套 facet
[Facet(typeof(Address))]
public partial record AddressDto;

[Facet(typeof(Company), NestedFacets = [typeof(AddressDto)])]
public partial record CompanyDto;

// 导航属性自动加载！
var companies = await dbContext.Companies
    .Where(c => c.IsActive)
    .ToFacetsAsync<CompanyDto>();

// HeadquartersAddress 导航属性自动包含
// EF Core 看到投影中的属性访问并生成 JOIN

// 适用于所有投影方法：
await dbContext.Companies.ToFacetsAsync<CompanyDto>();
await dbContext.Companies.FirstFacetAsync<CompanyDto>();
await dbContext.Companies.SelectFacet<CompanyDto>().ToListAsync();

// 也适用于集合：
[Facet(typeof(OrderItem))]
public partial record OrderItemDto;

[Facet(typeof(Order), NestedFacets = [typeof(OrderItemDto), typeof(AddressDto)])]
public partial record OrderDto;

var orders = await dbContext.Orders.ToFacetsAsync<OrderDto>();
// 自动包含 Items 集合和 ShippingAddress！
```

### 反向映射：ApplyFacet

用于通用补丁/更新场景

```csharp
using Facet.Extensions;

[HttpPut("{id}")]
public IActionResult UpdatePerson(int id, [FromBody] PersonDto dto)
{
    var person = repository.GetById(id);
    if (person == null) return NotFound();

    // 将 facet 的更改应用到源（不需要 DbContext）
    var result = person.ApplyFacetWithChanges<Person, PersonDto>(dto);

    if (result.HasChanges)
    {
        repository.Save(person);
        logger.LogInformation("Person {Id} updated: {Changes}",
            id, string.Join(", ", result.ChangedProperties));
    }

    return NoContent();
}
```

### 反向映射：UpdateFromFacet（EF Core）

用于 EF Core 特定场景，集成变更跟踪：

```csharp
using Facet.Extensions.EFCore;

[HttpPut("{id}")]
public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
{
    var user = await context.Users.FindAsync(id);
    if (user == null) return NotFound();

    // 仅更新实际更改的属性 - 选择性更新
    // 与 EF Core 的变更跟踪集成
    user.UpdateFromFacet(dto, context);

    await context.SaveChangesAsync();
    return Ok();
}

// 带变更跟踪以进行审计
var result = user.UpdateFromFacetWithChanges(dto, context);
if (result.HasChanges)
{
    logger.LogInformation("User {UserId} updated. Changed: {Properties}",
        user.Id, string.Join(", ", result.ChangedProperties));
}

// 异步版本
await user.UpdateFromFacetAsync(dto, context);
```

**主要区别：**
- **`ApplyFacet`**（Facet.Extensions）：无 EF Core 依赖，使用反射，适用于任何对象
- **`UpdateFromFacet`**（Facet.Extensions.EFCore）：需要 `DbContext`，与 EF Core 变更跟踪集成

### 完整 API 示例

```csharp
// 领域模型
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }  // 敏感
    public DateTime CreatedAt { get; set; }  // 不可变
}

// 更新 DTO - 排除敏感/不可变属性
[Facet(typeof(User), "Password", "CreatedAt")]
public partial class UpdateUserDto { }

// API 控制器
[ApiController]
public class UsersController : ControllerBase
{
    // GET: Entity -> Facet
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return user.ToFacet<UserDto>();  // 正向映射
    }

    // PUT: Facet -> Entity
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.UpdateFromFacet(dto, context);  // 反向映射
        await context.SaveChangesAsync();

        return NoContent();
    }
}
```

```csharp
// 非 EF Core 版本使用 ApplyFacet
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repository;

    // PUT: Facet -> Entity（无 EF Core 的选择性更新）
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, UpdateUserDto dto)
    {
        var user = _repository.GetById(id);
        if (user == null) return NotFound();

        user.ApplyFacet(dto);  // 反向映射（无 DbContext）
        _repository.Save(user);

        return NoContent();
    }
}
```

### EF Core 自定义映射器（高级）

对于无法表达为 SQL 投影的复杂映射（例如调用外部服务、复杂类型转换如 Vector2 或异步操作），请安装高级映射包：

```bash
dotnet add package Facet.Extensions.EFCore.Mapping
```

```csharp
using Facet.Extensions.EFCore.Mapping;  // 高级映射器
using Facet.Mapping;

// 定义排除属性的 DTO
[Facet(typeof(User), exclude: ["X", "Y"])]
public partial class UserDto
{
    public Vector2 Position { get; set; }
}

// 选项 1：静态映射器（无 DI）
public class UserMapper : IFacetMapConfigurationAsync<User, UserDto>
{
    public static async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.Position = new Vector2(source.X, source.Y);
    }
}

// 选项 2：支持依赖注入的实例映射器
public class UserMapper : IFacetMapConfigurationAsyncInstance<User, UserDto>
{
    private readonly ILocationService _locationService;

    public UserMapper(ILocationService locationService)
    {
        _locationService = locationService;
    }

    public async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.Position = new Vector2(source.X, source.Y);
        target.Location = await _locationService.GetLocationAsync(source.LocationId);
    }
}

// 使用静态映射器
var users = await dbContext.Users
    .Where(u => u.IsActive)
    .ToFacetsAsync<User, UserDto, UserMapper>();

// 使用实例映射器（DI）
var users = await dbContext.Users
    .Where(u => u.IsActive)
    .ToFacetsAsync<User, UserDto>(userMapper);
```

**注意：** 自定义映射器方法首先具体化查询（执行 SQL），然后应用您的自定义逻辑。所有匹配的属性首先自动映射。

参见 [Facet.Extensions.EFCore.Mapping](https://www.nuget.org/packages/Facet.Extensions.EFCore.Mapping) 包了解更多详情。

---

参见[快速入门](02_QuickStart.md)了解设置，[Facet.Extensions.EFCore](https://www.nuget.org/packages/Facet.Extensions.EFCore) 了解异步 EF Core 支持，以及 [Facet.Extensions.EFCore.Mapping](https://www.nuget.org/packages/Facet.Extensions.EFCore.Mapping) 了解高级自定义异步映射器。
