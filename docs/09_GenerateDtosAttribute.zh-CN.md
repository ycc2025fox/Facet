# GenerateDtos 属性参考

`[GenerateDtos]` 属性自动为领域模型生成标准 CRUD DTO(Create、Update、Response、Query、Upsert、Patch),无需手动编写重复的 DTO 类。

## GenerateDtos 属性

为领域模型生成标准 CRUD DTO,完全控制要生成的类型及其配置。

### 用法

```csharp
[GenerateDtos(Types = DtoTypes.All, OutputType = OutputType.Record)]
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
```

### 参数

| 参数             | 类型        | 描述                                                           |
|----------------------|-------------|-----------------------------------------------------------------------|
| `Types`              | `DtoTypes`  | 要生成的 DTO 类型(默认: All)。                         |
| `OutputType`         | `OutputType`| 生成的 DTO 的输出类型(默认: Record)。               |
| `Namespace`          | `string?`   | 生成的 DTO 的自定义命名空间(默认: 与源类型相同)。 |
| `ExcludeProperties`  | `string[]`  | 从所有生成的 DTO 中排除的属性。                      |
| `ExcludeAuditFields` | `bool`      | 自动排除常见审计字段(默认: false)。参见 [排除审计字段](#排除审计字段)。 |
| `Prefix`             | `string?`   | 生成的 DTO 名称的自定义前缀。                              |
| `Suffix`             | `string?`   | 生成的 DTO 名称的自定义后缀。                              |
| `IncludeFields`      | `bool`      | 包含源类型的公共字段(默认: false)。        |
| `GenerateConstructors`| `bool`     | 为 DTO 生成构造函数(默认: true)。                 |
| `GenerateProjections`| `bool`      | 为 DTO 生成投影表达式(默认: true)。       |
| `UseFullName`        | `bool`      | 在生成的文件名中使用完整类型名称以避免冲突(默认: false)。 |

