# 源签名变更跟踪

`SourceSignature` 属性允许你检测源实体结构何时发生变化,帮助你捕获对 DTO 的意外破坏性更改。

## 概述

当你在 `[Facet]` 属性上设置 `SourceSignature` 时,分析器会计算源类型属性的哈希值并将其与存储的签名进行比较。如果源实体发生变化(添加、删除属性或类型更改),你将在编译时收到警告,其中包含新签名。

## 用法

```csharp
[Facet(typeof(User), SourceSignature = "a1b2c3d4")]
public partial class UserDto;
```

## 工作原理

1. **哈希计算**: 签名是从以下内容计算的 8 字符 SHA-256 哈希:
   - 属性名称及其类型
   - 遵守 `Include`/`Exclude` 过滤器
   - 遵守 `IncludeFields` 设置

2. **编译时检查**: 分析器将存储的签名与当前计算的签名进行比较

3. **不匹配时警告**: 如果不同,你会收到诊断 `FAC022` 及新签名

4. **代码修复**: 使用提供的代码修复自动更新签名

## 示例工作流

### 初始设置

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// 首先,创建不带签名的 facet
[Facet(typeof(User))]
public partial class UserDto;

// 然后添加 SourceSignature 以跟踪更改(从分析器获取初始值)
[Facet(typeof(User), SourceSignature = "8f3a2b1c")]
public partial class UserDto;
```

### 当源发生变化时

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }  // 添加了新属性
}
```

你将看到警告:

```
FAC022: Source entity 'User' structure has changed. Update SourceSignature to 'd4e5f6a7' to acknowledge this change.
```

### 确认更改

使用提供的代码修复(灯泡/快速操作)自动更新签名,或手动更新:

```csharp
[Facet(typeof(User), SourceSignature = "d4e5f6a7")]
public partial class UserDto;
```

## 优势

- **有意的更改**: 强制你在源实体更改时明确确认
- **代码审查**: 使结构更改在差异中可见
- **团队沟通**: 在修改共享实体时提醒团队成员
- **API 稳定性**: 帮助维护稳定的 DTO 契约

## 与 Include/Exclude 配合使用

签名仅考虑实际将出现在 facet 中的属性:

```csharp
// 仅跟踪 Id, Name, Email (排除 Password)
[Facet(typeof(User), nameof(User.Password), SourceSignature = "1a2b3c4d")]
public partial class UserDto;

// 仅跟踪 FirstName 和 LastName
[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.LastName)], SourceSignature = "5e6f7a8b")]
public partial class UserNameDto;
```

## 何时使用

**推荐用于:**
- 公共 API DTO,破坏性更改会影响使用者
- 服务之间的共享模型
- 用于序列化/反序列化契约的 DTO
- 任何稳定性至关重要的 facet

**可选用于:**
- 仅内部使用的 DTO
- 开发期间快速演进的模型
- 简单/临时投影

## 诊断参考

| 代码 | 严重性 | 描述 |
|------|----------|-------------|
| FAC022 | 警告 | 源实体结构已更改 - 签名不匹配 |

## 提示

1. **从无签名开始**: 首先创建 facet,然后在模型稳定后添加签名

2. **审查更改**: 当看到 FAC022 时,在接受新签名之前审查源实体中的更改

3. **Git Blame**: 提交历史中的签名更新显示结构更改发生的时间

4. **多个 Facet**: 每个 facet 可以有自己的签名,跟踪其使用的特定属性
