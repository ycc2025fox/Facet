<div align="center">
  <img
    src="https://raw.githubusercontent.com/Tim-Maes/Facet/master/assets/Facet.png"
    alt="Facet logo"
    width="400">
</div>

<div align="center">
"一个主体、情境或对象的某个部分,该主体具有多个部分。"
</div>

<br>

<div align="center">

[![CI](https://github.com/Tim-Maes/Facet/actions/workflows/build.yml/badge.svg)](https://github.com/Tim-Maes/Facet/actions/workflows/build.yml)
[![Test](https://github.com/Tim-Maes/Facet/actions/workflows/test.yml/badge.svg)](https://github.com/Tim-Maes/Facet/actions/workflows/test.yml)
[![CD](https://github.com/Tim-Maes/Facet/actions/workflows/release.yml/badge.svg)](https://github.com/Tim-Maes/Facet/actions/workflows/release.yml)
[![NuGet](https://img.shields.io/nuget/v/Facet.svg)](https://www.nuget.org/packages/Facet)
[![Downloads](https://img.shields.io/nuget/dt/Facet.svg)](https://www.nuget.org/packages/Facet)
[![GitHub](https://img.shields.io/github/license/Tim-Maes/Facet.svg)](https://github.com/Tim-Maes/Facet/blob/main/LICENSE.txt)
[![Discord](https://img.shields.io/discord/1443287393825329223?color=%237289da&label=Discord&logo=discord&logoColor=%237289da&style=flat-square)](https://discord.gg/yGDBhGuNMB)

</div>

---

**Facet** 是一个 C# 源代码生成器,可以消除 DTO 样板代码。声明你想要的内容,Facet 会在编译时生成类型、构造函数、LINQ 投影和反向映射,零运行时开销。

## :gem: 什么是 Facet?

将你的领域模型想象成一颗**具有多个切面的宝石**!不同用途需要不同的视图:
- 公共 API 需要一个不包含敏感数据的切面
- 管理端点需要一个包含额外字段的不同切面
- 数据库查询需要高效的投影

**Facet** 让你通过简单的属性声明这些视图,并在编译时生成所有必要的代码。

## :sparkles: 特性

- :zap: **零运行时开销** - 所有代码在编译时生成
- :rocket: **类型安全** - 完全的编译时类型检查
- :wrench: **灵活** - 支持包含/排除属性、重命名、类型转换等
- :mag: **可调试** - 生成的代码可见且易于调试
- :package: **轻量级** - 无运行时依赖
- :link: **LINQ 集成** - 自动生成高效的 LINQ 投影
- :leftwards_arrow_with_hook: **反向映射** - 支持双向转换
- :recycle: **可组合** - Facet 可以基于其他 Facet 构建

## :package: 安装

### 安装 NuGet 包

```bash
dotnet add package Facet
```

LINQ 辅助工具:
```bash
dotnet add package Facet.Extensions
```

EF Core 支持:
```bash
dotnet add package Facet.Extensions.EFCore
```

高级 EF Core 自定义映射器(支持 DI):
```bash
dotnet add package Facet.Extensions.EFCore.Mapping
```

表达式转换工具:
```bash
dotnet add package Facet.Mapping.Expressions
```

## :rocket: 快速开始

### 基础示例

```csharp
// 领域模型
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime DateOfBirth { get; set; }
}

// 定义一个 Facet - 排除敏感字段
[Facet(typeof(User))]
[Exclude(nameof(User.PasswordHash))]
public partial class UserDto;
```

**生成的代码:**

```csharp
public partial class UserDto
{
    public int Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public DateTime DateOfBirth { get; init; }

    // 构造函数
    public UserDto(User source) { /* ... */ }

    // LINQ 投影
    public static Expression<Func<User, UserDto>> Projection
        => user => new UserDto { /* ... */ };

    // 反向映射
    public User ToUser() { /* ... */ }
}
```

### 使用方式

```csharp
// 从实体创建 DTO
var user = new User { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
var dto = new UserDto(user);

// LINQ 投影 (高效的数据库查询)
var dtos = dbContext.Users
    .Select(UserDto.Projection)
    .ToList();

// 反向映射
var userEntity = dto.ToUser();
```

## :book: 核心功能

### 1. 包含/排除属性

```csharp
// 只包含特定属性
[Facet(typeof(User))]
[Include(nameof(User.Id), nameof(User.FirstName), nameof(User.LastName))]
public partial class UserSummaryDto;

// 排除敏感属性
[Facet(typeof(User))]
[Exclude(nameof(User.PasswordHash), nameof(User.Salary))]
public partial class PublicUserDto;
```

### 2. 属性重命名 - MapFrom

```csharp
[Facet(typeof(User))]
public partial class UserDto
{
    // 重命名属性
    [MapFrom(nameof(User.FirstName))]
    public string GivenName { get; set; }

    [MapFrom(nameof(User.LastName))]
    public string FamilyName { get; set; }
}
```

### 3. 类型转换

```csharp
[Facet(typeof(User))]
public partial class UserDto
{
    // 自动类型转换
    public string Id { get; set; } // int -> string

    // 枚举转换
    [Facet(typeof(User), ConvertEnumsTo = EnumConversion.String)]
    public partial class UserWithStringEnums;
}
```

### 4. 嵌套对象和集合

```csharp
public class User
{
    public Address HomeAddress { get; set; }
    public List<Project> Projects { get; set; }
}

[Facet(typeof(User))]
public partial class UserDto; // 自动映射嵌套对象和集合
```

### 5. Flatten - 展平嵌套对象

```csharp
public class User
{
    public Address HomeAddress { get; set; }
}

[Facet(typeof(User))]
public partial class UserDto
{
    [Flatten(nameof(User.HomeAddress))]
    public string City { get; set; } // 从 HomeAddress.City 映射

    [Flatten(nameof(User.HomeAddress))]
    public string State { get; set; } // 从 HomeAddress.State 映射
}
```

### 6. 条件映射 - MapWhen

```csharp
[Facet(typeof(User))]
public partial class UserDto
{
    // 仅当用户激活时映射 Email
    [MapWhen(nameof(User.Email), "IsActive == true")]
    public string Email { get; set; }
}
```

### 7. Before/After 钩子

```csharp
[Facet(typeof(User))]
public partial class UserDto
{
    partial void BeforeMap(User source);
    partial void AfterMap(User source);
}

// 实现
public partial class UserDto
{
    partial void BeforeMap(User source)
    {
        // 映射前的验证或处理
    }

    partial void AfterMap(User source)
    {
        // 映射后的计算或处理
    }
}
```

### 8. 生成不同类型

```csharp
// 生成为 record
[Facet(typeof(User), GenerateAs = GenerateType.Record)]
public partial record UserDto;

// 生成为 struct
[Facet(typeof(User), GenerateAs = GenerateType.Struct)]
public partial struct UserDto;

// 生成为 record struct
[Facet(typeof(User), GenerateAs = GenerateType.RecordStruct)]
public partial record struct UserDto;
```

### 9. 自定义映射配置

```csharp
[Facet(typeof(User))]
[CustomMapping(typeof(UserMappingConfig))]
public partial class UserDto;

public class UserMappingConfig : IFacetMapper<User, UserDto>
{
    public void Map(User source, UserDto target)
    {
        // 自定义映射逻辑
        target.FullName = $"{source.FirstName} {source.LastName}";
    }
}
```

### 10. GenerateDtos - 自动生成 CRUD DTO 集

```csharp
[GenerateDtos(typeof(User))]
public partial class UserDtos;

// 自动生成:
// - UserCreateDto
// - UserUpdateDto
// - UserResponseDto
// - UserQueryDto
// - UserUpsertDto
// - UserPatchDto
```

## :link: Entity Framework Core 集成

### 自动导航加载

```csharp
// 无需 .Include() - Facet 自动处理!
var users = await dbContext.Users
    .Select(UserDto.Projection)
    .ToListAsync();
```

### 异步映射

```csharp
using Facet.Extensions.EFCore;

var dto = await dbContext.Users
    .Where(u => u.Id == userId)
    .ProjectToAsync<UserDto>(cancellationToken);
```

## :zap: 性能

- **零运行时开销** - 所有代码在编译时生成
- **无反射** - 直接属性访问
- **高效的 LINQ 投影** - 仅查询所需字段
- **EF Core 优化** - 自动生成最优 SQL 查询

## :wrench: 高级特性

### Wrapper - 引用委托

```csharp
[Facet(typeof(User), WrapperMode = WrapperMode.Reference)]
public partial class UserViewModel;

// 生成引用包装器,适用于 MVVM 场景
var viewModel = new UserViewModel(user);
viewModel.FirstName = "Jane"; // 直接修改原始对象
```

### 表达式转换

```csharp
using Facet.Mapping.Expressions;

// 将 User 谓词转换为 UserDto 谓词
Expression<Func<User, bool>> userPredicate = u => u.IsActive && u.Salary > 50000;
Expression<Func<UserDto, bool>> dtoPredicate = userPredicate.Transform<User, UserDto>();
```

### 全局配置

通过 MSBuild 属性覆盖项目范围的默认设置:

```xml
<PropertyGroup>
  <FacetDefaultGenerateAs>Record</FacetDefaultGenerateAs>
  <FacetDefaultConvertEnumsTo>String</FacetDefaultConvertEnumsTo>
  <FacetDefaultGenerateReverseMap>true</FacetDefaultGenerateReverseMap>
</PropertyGroup>
```

### 生成相等性比较

```csharp
[Facet(typeof(User), GenerateEquality = true)]
public partial class UserDto;

// 生成 Equals, GetHashCode, ==, != 运算符
var dto1 = new UserDto(user1);
var dto2 = new UserDto(user2);
bool areEqual = dto1 == dto2; // 基于值的比较
```

### 复制构造函数

```csharp
[Facet(typeof(User), GenerateCopyConstructor = true)]
public partial class UserDto;

// 生成复制构造函数
var original = new UserDto(user);
var copy = new UserDto(original); // 克隆
```

## :books: 文档

完整文档请访问 [docs](docs/) 目录:

- [包含/排除属性](docs/01_IncludeExclude.md)
- [MapFrom - 属性重命名](docs/02_MapFrom.md)
- [类型转换](docs/03_TypeConversion.md)
- [嵌套对象映射](docs/04_NestedObjects.md)
- [Flatten - 展平嵌套对象](docs/05_Flatten.md)
- [MapWhen - 条件映射](docs/06_MapWhen.md)
- [Before/After 钩子](docs/07_BeforeAfterHooks.md)
- [自定义映射配置](docs/08_CustomMapping.md)
- [GenerateDtos - CRUD DTO 生成](docs/09_GenerateDtos.md)
- [Wrapper 模式](docs/10_Wrapper.md)
- [表达式转换](docs/11_ExpressionTransformation.md)
- [EF Core 集成](docs/12_EFCoreIntegration.md)
- [全局配置](docs/21_GlobalConfigurationDefaults.md)
- [更多...](docs/)

## :bulb: 实际应用示例

### API 响应 DTO

```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public decimal Salary { get; set; }
    public List<Order> Orders { get; set; }
}

// 公共 API - 排除敏感数据
[Facet(typeof(User))]
[Exclude(nameof(User.PasswordHash), nameof(User.Salary))]
public partial class PublicUserDto;

// 管理 API - 包含所有字段
[Facet(typeof(User))]
public partial class AdminUserDto;

// 用户列表 - 仅基本信息
[Facet(typeof(User))]
[Include(nameof(User.Id), nameof(User.FirstName), nameof(User.LastName))]
public partial class UserListItemDto;
```

### 数据库查询优化

```csharp
// 高效查询 - 仅选择所需字段
var users = await dbContext.Users
    .Where(u => u.IsActive)
    .Select(PublicUserDto.Projection)
    .ToListAsync();

// 生成的 SQL 仅包含 DTO 中的字段
// SELECT Id, FirstName, LastName, Email FROM Users WHERE IsActive = 1
```

### MVVM 场景

```csharp
[Facet(typeof(User), WrapperMode = WrapperMode.Reference)]
public partial class UserViewModel;

// ViewModel 直接修改原始实体
var user = await dbContext.Users.FindAsync(userId);
var viewModel = new UserViewModel(user);

viewModel.FirstName = "Jane"; // 直接修改 user.FirstName
await dbContext.SaveChangesAsync(); // 保存更改
```

### 复杂嵌套对象展平

```csharp
public class Order
{
    public int Id { get; set; }
    public User Customer { get; set; }
    public Address ShippingAddress { get; set; }
    public decimal Total { get; set; }
}

[Facet(typeof(Order))]
public partial class OrderSummaryDto
{
    public int Id { get; set; }

    [Flatten(nameof(Order.Customer))]
    public string CustomerFirstName { get; set; }

    [Flatten(nameof(Order.Customer))]
    public string CustomerLastName { get; set; }

    [Flatten(nameof(Order.ShippingAddress))]
    public string City { get; set; }

    public decimal Total { get; set; }
}
```

## :chart_with_upwards_trend: 与其他方案对比

| 特性 | Facet | AutoMapper | Mapster | 手动映射 |
|------|-------|------------|---------|----------|
| 运行时开销 | 零 | 高 | 中 | 零 |
| 类型安全 | ✅ | ❌ | ✅ | ✅ |
| LINQ 投影 | ✅ | ✅ | ✅ | 手动 |
| 编译时生成 | ✅ | ❌ | ❌ | N/A |
| 可调试性 | ✅ | ❌ | ❌ | ✅ |
| 学习曲线 | 低 | 高 | 中 | N/A |
| 配置复杂度 | 低 | 高 | 中 | N/A |

## :handshake: 贡献

欢迎贡献! 请查看我们的 [贡献指南](CONTRIBUTING.md) 了解详情。

### 开发设置

```bash
# 克隆仓库
git clone https://github.com/Tim-Maes/Facet.git

# 构建项目
dotnet build

# 运行测试
dotnet test
```

## :busts_in_silhouette: 社区

- [Discord](https://discord.gg/yGDBhGuNMB) - 加入我们的 Discord 社区
- [GitHub Issues](https://github.com/Tim-Maes/Facet/issues) - 报告 bug 或请求功能
- [GitHub Discussions](https://github.com/Tim-Maes/Facet/discussions) - 提问和讨论

## :page_facing_up: 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE.txt](LICENSE.txt) 文件。

## :star: 支持项目

如果你觉得 Facet 有用,请给我们一个 ⭐️!

## :pray: 致谢

感谢所有为 Facet 做出贡献的开发者!

## :link: 相关链接

- [NuGet 包](https://www.nuget.org/packages/Facet)
- [GitHub 仓库](https://github.com/Tim-Maes/Facet)
- [文档](docs/)
- [更新日志](CHANGELOG.md)

## :question: 常见问题

### Facet 与 AutoMapper 有什么区别?

Facet 是一个编译时源代码生成器,而 AutoMapper 是运行时映射库。Facet 生成的代码零运行时开销,完全类型安全,且易于调试。

### 我可以在现有项目中使用 Facet 吗?

可以! Facet 可以与现有的映射解决方案共存,你可以逐步迁移。

### Facet 支持哪些 .NET 版本?

Facet 支持 .NET 8、.NET 9 和 .NET 10。

### 如何查看生成的代码?

生成的代码位于 `obj/` 目录中,你可以在 IDE 中使用 "Go to Definition" 查看。

### Facet 是否支持异步映射?

是的,通过 `Facet.Extensions.EFCore` 包支持异步映射。

## :rocket: 快速链接

- [快速开始](#rocket-快速开始)
- [核心功能](#book-核心功能)
- [EF Core 集成](#link-entity-framework-core-集成)
- [文档](#books-文档)
- [示例](#bulb-实际应用示例)

---

<div align="center">
使用 ❤️ 和 C# 构建
</div>

