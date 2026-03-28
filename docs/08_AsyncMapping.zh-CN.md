# 使用 Facet.Mapping 进行异步映射

本指南介绍 Facet.Mapping 的高级异步映射场景,包括实际示例和最佳实践。

## 概述

Facet.Mapping v2.0+ 为映射逻辑需要执行 I/O 操作的场景提供全面的异步映射支持,例如:

- 数据库查询
- 外部 API 调用
- 文件系统操作
- 网络请求
- 任何其他异步操作

## 核心异步接口

### IFacetMapConfigurationAsync<TSource, TTarget>

```csharp
public interface IFacetMapConfigurationAsync<TSource, TTarget>
{
    static abstract Task MapAsync(TSource source, TTarget target, CancellationToken cancellationToken = default);
}
```

### IFacetMapConfigurationHybrid<TSource, TTarget>

```csharp
public interface IFacetMapConfigurationHybrid<TSource, TTarget> :
    IFacetMapConfiguration<TSource, TTarget>,
    IFacetMapConfigurationAsync<TSource, TTarget>
{
    // 继承 Map() 和 MapAsync() 方法
}
```

## 实际示例

### 示例 1: 带外部数据的用户配置文件

```csharp
public class UserProfileAsyncMapper : IFacetMapConfigurationAsync<User, UserProfileDto>
{
    private static readonly HttpClient _httpClient = new();

    public static async Task MapAsync(User source, UserProfileDto target, CancellationToken cancellationToken = default)
    {
        // 并行异步操作以提高性能
        var tasks = new[]
        {
            LoadAvatarAsync(source.Id, target, cancellationToken),
            LoadReputationAsync(source.Email, target, cancellationToken),
            LoadBadgesAsync(source.Id, target, cancellationToken)
        };

        await Task.WhenAll(tasks);
    }
}
```

