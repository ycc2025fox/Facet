# 什么是 Facetting?

Facetting 是在编译时定义大型模型的**聚焦视图**的过程。

无需手动编写单独的 DTO、映射器和投影,**Facet** 允许你声明想要保留的内容 — 然后生成其他所有内容。

你可以将其想象为**雕刻宝石的特定切面**:

- 你关心的部分
- 留下其余部分

## 为什么使用 Facetting?

- 减少 DTO、投影和 ViewModel 之间的重复
- 保持强类型,零运行时成本
- 保持 DRY(不要重复自己)原则,同时不牺牲性能
- 与 Entity Framework 等 LINQ 提供程序无缝协作

## 示例

源模型:

```csharp
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}
```

定义一个 Facet:
```csharp
[Facet(typeof(User), exclude: nameof(User.Email))]
public partial class UserDto { }
```

你将获得:

- 映射构造函数
- LINQ 表达式投影
- 可扩展的 partial class 或 record
