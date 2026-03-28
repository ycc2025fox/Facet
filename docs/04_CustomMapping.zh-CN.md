# 使用 IFacetMapConfiguration 自定义映射

Facet 通过多个接口支持高级场景的自定义映射逻辑，这些接口处理不同的映射需求。从最新版本开始，Facet 现在支持**静态映射器和基于实例的依赖注入映射器**。

## 可用的映射接口

### 静态映射器（无依赖注入）
| 接口 | 用途 | 使用场景 |
|-----------|---------|----------|
| `IFacetMapConfiguration<TSource, TTarget>` | 同步映射 | 快速的内存操作 |
| `IFacetMapConfigurationAsync<TSource, TTarget>` | 异步映射 | I/O 操作、数据库调用、API 调用 |
| `IFacetMapConfigurationHybrid<TSource, TTarget>` | 同步/异步组合 | 混合操作的最佳性能 |

### 实例映射器（支持依赖注入）
| 接口 | 用途 | 使用场景 |
|-----------|---------|----------|
| `IFacetMapConfigurationInstance<TSource, TTarget>` | 同步映射 | 使用注入服务的快速操作 |
| `IFacetMapConfigurationAsyncInstance<TSource, TTarget>` | 异步映射 | 使用注入服务的 I/O 操作 |
| `IFacetMapConfigurationHybridInstance<TSource, TTarget>` | 同步/异步组合 | 使用注入服务的混合操作 |

## 何时使用每种方法

### 静态映射器
- **最适合**：简单转换、计算属性、格式化
- **优势**：零开销、编译时优化、不需要 DI 容器
- **限制**：无法注入服务、无法访问外部依赖

### 实例映射器
- **最适合**：需要外部服务（数据库、API、文件系统）的复杂场景
- **优势**：完整的依赖注入支持、更易测试、更好的关注点分离
- **用法**：传递带有注入依赖的映射器实例

## Facet.Mapping 扩展方法

以下扩展方法可用于异步映射场景：

| 方法 | 描述 |
|--------|-------------|
| `ToFacetAsync<TSource, TTarget, TMapper>()` | 使用显式类型参数映射单个实例（编译时） |
| `ToFacetAsync<TTarget, TMapper>()` | 使用推断的源类型映射单个实例（运行时反射） |
| `ToFacetsAsync<TSource, TTarget, TMapper>()` | 使用显式类型顺序映射集合 |
| `ToFacetsAsync<TTarget, TMapper>()` | 使用推断的源类型顺序映射集合 |
| `ToFacetsParallelAsync<TSource, TTarget, TMapper>()` | 使用显式类型并行映射集合 |
| `ToFacetsParallelAsync<TTarget, TMapper>()` | 使用推断的源类型并行映射集合 |
| `ToFacetHybridAsync<TSource, TTarget, TMapper>()` | 使用显式类型的混合同步/异步映射 |
| `ToFacetHybridAsync<TTarget, TMapper>()` | 使用推断的源类型的混合同步/异步映射 |

> **性能说明**：带有显式类型参数的方法（`<TSource, TTarget, TMapper>`）提供更好的编译时性能，而简化方法（`<TTarget, TMapper>`）使用运行时反射以改善开发体验。

## 静态映射（无 DI）

### 1. 实现接口

```csharp
using Facet.Mapping;

public class UserMapConfig : IFacetMapConfiguration<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
        target.DisplayEmail = source.Email.ToLower();
    }
}
```

### 2. 在 Facet 特性中引用

```csharp
[Facet(typeof(User), Configuration = typeof(UserMapConfig))]
public partial class UserDto
{
    public string FullName { get; set; }
    public string DisplayEmail { get; set; }
}
```

生成的构造函数将在复制属性后调用您的 `Map` 方法。

## 实例映射（支持依赖注入）

### 1. 定义您的服务

```csharp
public interface IProfilePictureService
{
    Task<string> GetProfilePictureAsync(int userId, CancellationToken cancellationToken = default);
}

public interface IReputationService
{
    Task<decimal> CalculateReputationAsync(string email, CancellationToken cancellationToken = default);
}

public class ProfilePictureService : IProfilePictureService
{
    private readonly IDbContext _dbContext;

    public ProfilePictureService(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GetProfilePictureAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync(userId, cancellationToken);
        return user?.ProfilePictureUrl ?? "/images/default-avatar.png";
    }
}
```

### 2. 实现实例接口

```csharp
using Facet.Mapping;

public class UserAsyncMapperWithDI : IFacetMapConfigurationAsyncInstance<User, UserDto>
{
    private readonly IProfilePictureService _profilePictureService;
    private readonly IReputationService _reputationService;

    public UserAsyncMapperWithDI(IProfilePictureService profilePictureService, IReputationService reputationService)
    {
        _profilePictureService = profilePictureService;
        _reputationService = reputationService;
    }

    public async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        // 使用注入的服务进行异步操作
        target.ProfilePictureUrl = await _profilePictureService.GetProfilePictureAsync(source.Id, cancellationToken);
        target.ReputationScore = await _reputationService.CalculateReputationAsync(source.Email, cancellationToken);

        // 设置计算属性
        target.FullName = $"{source.FirstName} {source.LastName}";
    }
}
```

### 3. 使用依赖注入

```csharp
// 在 DI 容器中注册服务
services.AddScoped<IProfilePictureService, ProfilePictureService>();
services.AddScoped<IReputationService, ReputationService>();
services.AddScoped<UserAsyncMapperWithDI>();

// 在应用程序中使用
public class UserController : ControllerBase
{
    private readonly UserAsyncMapperWithDI _userMapper;

    public UserController(UserAsyncMapperWithDI userMapper)
    {
        _userMapper = userMapper;
    }

    public async Task<UserDto> GetUser(int id)
    {
        var user = await GetUserFromDatabase(id);

        // 新功能：传递带有注入依赖的映射器实例
        return await user.ToFacetAsync(_userMapper);
    }

    public async Task<List<UserDto>> GetUsers()
    {
        var users = await GetUsersFromDatabase();

        // 新功能：使用 DI 的集合映射
        return await users.ToFacetsAsync(_userMapper);
    }
}
```

## 异步映射（静态 - 原始方法）

### 1. 实现异步接口

```csharp
using Facet.Mapping;

public class UserAsyncMapper : IFacetMapConfigurationAsync<User, UserDto>
{
    public static async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        // 异步数据库查找（无 DI）
        target.ProfilePictureUrl = await GetProfilePictureAsync(source.Id, cancellationToken);

        // 异步 API 调用（无 DI）
        target.ReputationScore = await CalculateReputationAsync(source.Email, cancellationToken);

        // 异步文件操作
        target.Preferences = await LoadUserPreferencesAsync(source.Id, cancellationToken);
    }

    private static async Task<string> GetProfilePictureAsync(int userId, CancellationToken cancellationToken)
    {
        // 数据库查询示例（无 DI - 不推荐用于复杂场景）
        using var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync($"https://api.example.com/users/{userId}/avatar", cancellationToken);
        return response;
    }

    private static async Task<decimal> CalculateReputationAsync(string email, CancellationToken cancellationToken)
    {
        // 外部 API 调用示例
        await Task.Delay(100, cancellationToken); // 模拟 API 延迟
        return Random.Shared.Next(1, 6) + (decimal)Random.Shared.NextDouble();
    }

    private static async Task<UserPreferences> LoadUserPreferencesAsync(int userId, CancellationToken cancellationToken)
    {
        // 文件 I/O 示例
        var filePath = $"preferences/{userId}.json";
        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return JsonSerializer.Deserialize<UserPreferences>(json) ?? new UserPreferences();
        }
        return new UserPreferences();
    }
}
```

### 2. 定义您的 DTO

```csharp
[Facet(typeof(User))]
public partial class UserDto
{
    public string ProfilePictureUrl { get; set; } = "";
    public decimal ReputationScore { get; set; }
    public UserPreferences Preferences { get; set; } = new();
}
```

### 3. 使用静态异步扩展方法

```csharp
// 单实例异步映射（静态）
var userDto = await user.ToFacetAsync<UserDto, UserAsyncMapper>();

// 集合异步映射（顺序，静态）
var userDtos = await users.ToFacetsAsync<UserDto, UserAsyncMapper>();

// 集合异步映射（并行以获得更好性能，静态）
var userDtosParallel = await users.ToFacetsParallelAsync<UserDto, UserAsyncMapper>(
    maxDegreeOfParallelism: 4);
```

## 混合映射（最佳性能）

### 静态混合映射器

```csharp
public class UserHybridMapper : IFacetMapConfigurationHybrid<User, UserDto>
{
    // 快速同步操作
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
        target.DisplayEmail = source.Email.ToLower();
        target.AgeCategory = CalculateAgeCategory(source.BirthDate);
    }

    // 昂贵的异步操作
    public static async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.ProfilePictureUrl = await GetProfilePictureAsync(source.Id, cancellationToken);
        target.ReputationScore = await CalculateReputationAsync(source.Email, cancellationToken);
    }

    private static string CalculateAgeCategory(DateTime birthDate)
    {
        var age = DateTime.Now.Year - birthDate.Year;
        return age switch
        {
            < 18 => "Minor",
            < 65 => "Adult",
            _ => "Senior"
        };
    }

    // ... 异步方法实现
}

// 用法
var userDto = await user.ToFacetHybridAsync<UserDto, UserHybridMapper>();
```

### 实例混合映射器（支持 DI）

```csharp
public class UserHybridMapperWithDI : IFacetMapConfigurationHybridInstance<User, UserDto>
{
    private readonly IProfilePictureService _profilePictureService;
    private readonly IReputationService _reputationService;

    public UserHybridMapperWithDI(IProfilePictureService profilePictureService, IReputationService reputationService)
    {
        _profilePictureService = profilePictureService;
        _reputationService = reputationService;
    }

    // 快速同步操作
    public void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
        target.DisplayEmail = source.Email.ToLower();
        target.AgeCategory = CalculateAgeCategory(source.BirthDate);
    }

    // 使用注入服务的昂贵异步操作
    public async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        target.ProfilePictureUrl = await _profilePictureService.GetProfilePictureAsync(source.Id, cancellationToken);
        target.ReputationScore = await _reputationService.CalculateReputationAsync(source.Email, cancellationToken);
    }

    private static string CalculateAgeCategory(DateTime birthDate)
    {
        var age = DateTime.Now.Year - birthDate.Year;
        return age switch
        {
            < 18 => "Minor",
            < 65 => "Adult",
            _ => "Senior"
        };
    }
}

// 使用 DI
var userDto = await user.ToFacetHybridAsync(hybridMapperWithDI);
```

## API 对比

### 之前（仅静态）
```csharp
// 仅限静态方法，不支持 DI
var userDto = await user.ToFacetAsync<UserDto, UserAsyncMapper>();
```

### 之后（同时支持静态和实例）
```csharp
// 选项 1：静态方法（现有，不变）
var userDto1 = await user.ToFacetAsync<UserDto, UserAsyncMapper>();

// 选项 2：实例方法（新增，支持 DI）
var mapper = new UserAsyncMapperWithDI(profilePictureService, reputationService);
var userDto2 = await user.ToFacetAsync(mapper);
```

## 错误处理

### 带错误处理的实例映射器

```csharp
public class SafeUserAsyncMapperWithDI : IFacetMapConfigurationAsyncInstance<User, UserDto>
{
    private readonly IProfilePictureService _profilePictureService;
    private readonly IReputationService _reputationService;
    private readonly ILogger<SafeUserAsyncMapperWithDI> _logger;

    public SafeUserAsyncMapperWithDI(
        IProfilePictureService profilePictureService,
        IReputationService reputationService,
        ILogger<SafeUserAsyncMapperWithDI> logger)
    {
        _profilePictureService = profilePictureService;
        _reputationService = reputationService;
        _logger = logger;
    }

    public async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        try
        {
            target.ProfilePictureUrl = await _profilePictureService.GetProfilePictureAsync(source.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load profile picture for user {UserId}", source.Id);
            target.ProfilePictureUrl = "/images/default-avatar.png";
        }

        try
        {
            target.ReputationScore = await _reputationService.CalculateReputationAsync(source.Email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to calculate reputation for user {Email}", source.Email);
            target.ReputationScore = 0m; // 错误时的默认值
        }
    }
}
```

## 性能考虑

### 何时使用每种方法

| 场景 | 推荐接口 | 原因 |
|----------|----------------------|---------|
| 简单属性转换 | `IFacetMapConfiguration`（静态） | 零开销，编译时优化 |
| 需要服务的复杂转换 | `IFacetMapConfigurationInstance` | 完整 DI 支持，更易测试 |
| 数据库查找 | `IFacetMapConfigurationAsyncInstance` | 使用注入的 DbContext 进行正确的 async/await |
| API 调用 | `IFacetMapConfigurationAsyncInstance` | 使用注入的 HttpClient 进行非阻塞 I/O |
| 混合快速/慢速操作 | `IFacetMapConfigurationHybridInstance` | 使用 DI 的两全其美 |
| 大型集合 | 使用实例的并行异步方法 | 使用共享服务提高吞吐量 |

### 集合处理指南

```csharp
// 对于使用 DI 的小型集合（< 100 项）
var mapper = serviceProvider.GetRequiredService<UserAsyncMapperWithDI>();
var results = await items.ToFacetsAsync(mapper);

// 对于使用 DI 的大型集合和 I/O 操作
var results = await items.ToFacetsParallelAsync(mapper, maxDegreeOfParallelism: Environment.ProcessorCount);

// 对于数据库密集型操作（避免压垮数据库）
var results = await items.ToFacetsParallelAsync(mapper, maxDegreeOfParallelism: 2);
```

## 迁移指南

### 现有静态映射器
您现有的静态映射器继续保持不变：

```csharp
// 这将继续像以前一样工作
var result = await user.ToFacetAsync<UserDto, ExistingAsyncMapper>();
```

### 添加 DI 支持
为现有场景添加依赖注入支持：

```csharp
// 1. 创建新的基于实例的映射器
public class ExistingAsyncMapperWithDI : IFacetMapConfigurationAsyncInstance<User, UserDto>
{
    private readonly ISomeService _service;

    public ExistingAsyncMapperWithDI(ISomeService service)
    {
        _service = service;
    }

    public async Task MapAsync(User source, UserDto target, CancellationToken cancellationToken = default)
    {
        // 使用 _service 而不是静态调用
        target.SomeProperty = await _service.GetDataAsync(source.Id, cancellationToken);
    }
}

// 2. 在 DI 容器中注册
services.AddScoped<ExistingAsyncMapperWithDI>();

// 3. 使用新方法
var mapper = serviceProvider.GetRequiredService<ExistingAsyncMapperWithDI>();
var result = await user.ToFacetAsync(mapper);
```

## 注意事项

- **向后兼容性**：所有现有的静态映射器接口和扩展方法继续保持不变
- **实例映射器**：所有映射方法必须是 `public`（非静态）并匹配接口签名
- **依赖注入**：实例映射器完全支持构造函数注入，可以在任何 DI 容器中注册
- **测试**：实例映射器更易于单元测试，因为依赖可以被模拟
- **性能**：与静态映射器相比，实例映射器的开销最小
- **线程安全**：每个实例应处理注入服务的线程安全要求
- **取消**：所有异步方法都支持 `CancellationToken` 以实现正确的取消
- **错误处理**：实例映射器可以注入日志记录器和其他服务以实现更好的错误处理

---
