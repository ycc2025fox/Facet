# 生成了什么代码？

本页展示了 Facet 在不同场景下生成的具体代码示例。这些示例基于实际的测试套件，反映了 Facet 的当前版本。

> **注意：** 生成的代码包含完整的 XML 文档注释。为简洁起见，下面的某些示例中省略了这些注释，但它们在实际生成的代码中是存在的。

---

## 1. 基本类 Facet（排除模式）

**输入：**
```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; }
}

[Facet(typeof(User), "Password", "CreatedAt", GenerateToSource = true)]
public partial class UserDto
{
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
}
```

**生成：**
```csharp
public partial class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    /// <summary>
    /// 从指定的 <see cref="User"/> 初始化 <see cref="UserDto"/> 类的新实例。
    /// </summary>
    public UserDto(User source) : this(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance))
    {
    }

    /// <summary>
    /// 带深度跟踪的构造函数，防止循环引用导致的堆栈溢出。
    /// </summary>
    public UserDto(User source, int __depth, HashSet<object>? __processed)
    {
        this.Id = source.Id;
        this.FirstName = source.FirstName;
        this.LastName = source.LastName;
        this.Email = source.Email;
    }

    /// <summary>
    /// 从指定的 <see cref="User"/> 创建 <see cref="UserDto"/> 的新实例。
    /// </summary>
    public static UserDto FromSource(User source)
    {
        return new UserDto(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    /// <summary>
    /// 使用默认值初始化 <see cref="UserDto"/> 类的新实例。
    /// </summary>
    public UserDto()
    {
    }

    /// <summary>
    /// 获取用于将 <see cref="User"/> 转换为 <see cref="UserDto"/> 的投影表达式。
    /// 用于 LINQ 和 Entity Framework 查询投影。
    /// </summary>
    public static Expression<Func<User, UserDto>> Projection =>
        source => new UserDto
        {
            Id = source.Id,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email
        };

    /// <summary>
    /// 将此 <see cref="UserDto"/> 实例转换为源类型的实例。
    /// </summary>
    public User ToSource()
    {
        return new User
        {
            Id = this.Id,
            FirstName = this.FirstName,
            LastName = this.LastName,
            Email = this.Email
        };
    }

    [Obsolete("请使用 ToSource() 代替。此方法将在未来版本中移除。")]
    public User BackTo() => ToSource();
}
```

**生成了什么：**
- 除排除属性外的所有源属性（"Password"、"CreatedAt"）
- 保留了 partial 类中用户定义的属性（"FullName"、"Age"）
- 带循环引用保护的构造函数
- `FromSource()` 静态工厂方法，用于优化运行时性能
- 用于反序列化的无参数构造函数
- 用于 LINQ/EF 查询的 `Projection` 表达式
- `ToSource()` 方法用于反向映射（因为 `GenerateToSource = true`）
- 已过时的 `BackTo()` 方法用于向后兼容

---

## 2. 包含模式

**输入：**
```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

[Facet(typeof(User), Include = [nameof(User.FirstName), nameof(User.LastName), nameof(User.Email)], GenerateToSource = true)]
public partial class UserIncludeDto;
```

**生成：**
```csharp
public partial class UserIncludeDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    public UserIncludeDto(User source) : this(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance))
    {
    }

    public UserIncludeDto(User source, int __depth, HashSet<object>? __processed)
    {
        this.FirstName = source.FirstName;
        this.LastName = source.LastName;
        this.Email = source.Email;
    }

    public static UserIncludeDto FromSource(User source)
    {
        return new UserIncludeDto(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    public UserIncludeDto()
    {
    }

    public static Expression<Func<User, UserIncludeDto>> Projection =>
        source => new UserIncludeDto
        {
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email
        };

    public User ToSource()
    {
        return new User
        {
            FirstName = this.FirstName,
            LastName = this.LastName,
            Email = this.Email
        };
    }

    [Obsolete("请使用 ToSource() 代替。")]
    public User BackTo() => ToSource();
}
```

**关键点：** 包含模式会自动生成 `ToSource()`，排除的属性会使用默认值初始化。

---

## 3. 自定义映射配置

**输入：**
```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class UserDtoMapper : IFacetMapConfiguration<User, UserDto>
{
    public static void Map(User source, UserDto target)
    {
        target.FullName = $"{source.FirstName} {source.LastName}";
        target.Age = CalculateAge(source.DateOfBirth);
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }
}

[Facet(typeof(User), "Password", "CreatedAt", Configuration = typeof(UserDtoMapper))]
public partial class UserDto
{
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
}
```

**生成：**
```csharp
public partial class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }

    public UserDto(User source) : this(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance))
    {
    }

    public UserDto(User source, int __depth, HashSet<object>? __processed)
    {
        this.Id = source.Id;
        this.FirstName = source.FirstName;
        this.LastName = source.LastName;
        this.DateOfBirth = source.DateOfBirth;
        UserDtoMapper.Map(source, this);  // 属性复制后应用自定义映射
    }

    public static UserDto FromSource(User source)
    {
        return new UserDto(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    public UserDto()
    {
    }

    public static Expression<Func<User, UserDto>> Projection =>
        source => new UserDto
        {
            Id = source.Id,
            FirstName = source.FirstName,
            LastName = source.LastName,
            DateOfBirth = source.DateOfBirth
        };
}
```

**注意：** 自定义映射在构造函数中应用，但不在 `Projection` 表达式中（因为表达式无法调用任意方法）。

---

## 4. Record Facet 与位置参数

**输入：**
```csharp
public record ClassicUser(string Id, string FirstName, string LastName, string? Email);

[Facet(typeof(ClassicUser), GenerateToSource = true)]
public partial record ClassicUserDto;
```

**生成：**
```csharp
public partial record ClassicUserDto(string Id, string FirstName, string LastName, string? Email);

public partial record ClassicUserDto
{
    [SetsRequiredMembers]
    public ClassicUserDto(ClassicUser source) : this(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance))
    {
    }

    [SetsRequiredMembers]
    public ClassicUserDto(ClassicUser source, int __depth, HashSet<object>? __processed)
        : this(source.Id, source.FirstName, source.LastName, source.Email)
    {
    }

    public ClassicUserDto() : this(string.Empty, string.Empty, string.Empty, null)
    {
    }

    public static Expression<Func<ClassicUser, ClassicUserDto>> Projection =>
        source => new ClassicUserDto
        {
            Id = source.Id,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email
        };

    public ClassicUser ToSource()
    {
        return new ClassicUser(this.Id, this.FirstName, this.LastName, this.Email);
    }

    [Obsolete("请使用 ToSource() 代替。")]
    public ClassicUser BackTo() => ToSource();
}
```

**注意：** Record 除了标准成员外，还会生成位置参数。

---

## 5. NullableProperties 用于查询/过滤 DTO

**输入：**
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
}

[Facet(typeof(Product), "InternalNotes", "CreatedAt", NullableProperties = true, GenerateToSource = false)]
public partial class ProductQueryDto;
```

**生成：**
```csharp
public partial class ProductQueryDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public bool? IsAvailable { get; set; }

    public ProductQueryDto(Product source) : this(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance))
    {
    }

    public ProductQueryDto(Product source, int __depth, HashSet<object>? __processed)
    {
        this.Id = source.Id;
        this.Name = source.Name;
        this.Price = source.Price;
        this.IsAvailable = source.IsAvailable;
    }

    public static ProductQueryDto FromSource(Product source)
    {
        return new ProductQueryDto(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    public ProductQueryDto()
    {
    }

    public static Expression<Func<Product, ProductQueryDto>> Projection =>
        source => new ProductQueryDto
        {
            Id = source.Id,
            Name = source.Name,
            Price = source.Price,
            IsAvailable = source.IsAvailable
        };

    // 注意：当 GenerateToSource = false 时不生成 ToSource() 方法
}
```

**为什么使用可空属性？** 所有值类型变为可空（int -> int?、bool -> bool?、decimal -> decimal?），引用类型标记为可空。非常适合查询/过滤场景，所有条件都是可选的。

## 6. MapFrom 特性 - 属性重命名

**输入：**
```csharp
public class MapFromTestEntity
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

[Facet(typeof(MapFromTestEntity), GenerateToSource = true)]
public partial class MapFromSimpleFacet
{
    [MapFrom(nameof(MapFromTestEntity.FirstName), Reversible = true)]
    public string Name { get; set; } = string.Empty;
}
```

**生成：**
```csharp
public partial class MapFromSimpleFacet
{
    public int Id { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    public MapFromSimpleFacet(MapFromTestEntity source) : this(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance))
    {
    }

    public MapFromSimpleFacet(MapFromTestEntity source, int __depth, HashSet<object>? __processed)
    {
        this.Id = source.Id;
        this.LastName = source.LastName;
        this.Email = source.Email;
        this.Name = source.FirstName;  // 从 FirstName 映射
    }

    public static MapFromSimpleFacet FromSource(MapFromTestEntity source)
    {
        return new MapFromSimpleFacet(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    public MapFromSimpleFacet()
    {
    }

    public static Expression<Func<MapFromTestEntity, MapFromSimpleFacet>> Projection =>
        source => new MapFromSimpleFacet
        {
            Id = source.Id,
            LastName = source.LastName,
            Email = source.Email,
            Name = source.FirstName  // 投影中也进行映射
        };

    public MapFromTestEntity ToSource()
    {
        return new MapFromTestEntity
        {
            Id = this.Id,
            LastName = this.LastName,
            Email = this.Email,
            FirstName = this.Name  // 反向映射（因为 Reversible = true）
        };
    }

    [Obsolete("请使用 ToSource() 代替。")]
    public MapFromTestEntity BackTo() => ToSource();
}
```

**关键点：**
- `MapFrom` 在映射期间重命名属性
- `Reversible = true` 在 `ToSource()` 中启用反向映射
- `Reversible = false`（默认）表示属性不会在 `ToSource()` 中映射
- `IncludeInProjection = false` 从 `Projection` 表达式中排除该属性

---

## 7. MapWhen 特性 - 条件映射

**输入：**
```csharp
public class MapWhenTestEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public OrderStatus Status { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Cancelled
}

[Facet(typeof(MapWhenTestEntity))]
public partial class MapWhenMixedFacet
{
    [MapWhen("IsActive")]
    public string? Email { get; set; }

    [MapWhen("Status == OrderStatus.Completed")]
    public DateTime? CompletedAt { get; set; }
}
```

**生成：**
```csharp
public partial class MapWhenMixedFacet
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public OrderStatus Status { get; set; }
    public int Age { get; set; }

    public MapWhenMixedFacet(MapWhenTestEntity source) : this(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance))
    {
    }

    public MapWhenMixedFacet(MapWhenTestEntity source, int __depth, HashSet<object>? __processed)
    {
        this.Id = source.Id;
        this.Name = source.Name;
        this.IsActive = source.IsActive;
        this.Status = source.Status;
        this.Age = source.Age;

        // 条件映射
        if (source.IsActive)
        {
            this.Email = source.Email;
        }

        if (source.Status == OrderStatus.Completed)
        {
            this.CompletedAt = source.CompletedAt;
        }
    }

    public static MapWhenMixedFacet FromSource(MapWhenTestEntity source)
    {
        return new MapWhenMixedFacet(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    public MapWhenMixedFacet()
    {
    }

    public static Expression<Func<MapWhenTestEntity, MapWhenMixedFacet>> Projection =>
        source => new MapWhenMixedFacet
        {
            Id = source.Id,
            Name = source.Name,
            IsActive = source.IsActive,
            Status = source.Status,
            Age = source.Age,
            Email = source.IsActive ? source.Email : default,
            CompletedAt = source.Status == OrderStatus.Completed ? source.CompletedAt : default
        };
}
```

**关键点：**
- `MapWhen` 为属性映射添加条件逻辑
- 支持布尔属性、相等检查、空检查、比较
- 可以使用多个 `[MapWhen]` 特性实现 AND 逻辑
- `IncludeInProjection = false` 从投影中排除条件

---

## 8. Wrapper 特性 - 委托模式

**输入：**
```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public decimal Salary { get; set; }
}

[Wrapper(typeof(User), "Password", "Salary")]
public partial class PublicUserWrapper { }
```

**生成：**
```csharp
public partial class PublicUserWrapper
{
    private readonly User _source;

    public PublicUserWrapper(User source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public int Id
    {
        get => _source.Id;
        set => _source.Id = value;
    }

    public string FirstName
    {
        get => _source.FirstName;
        set => _source.FirstName = value;
    }

    public string LastName
    {
        get => _source.LastName;
        set => _source.LastName = value;
    }

    public string Email
    {
        get => _source.Email;
        set => _source.Email = value;
    }

    // Password 和 Salary 被排除 - 不生成属性

    public User Unwrap() => _source;
}
```

**关键点：**
- `Wrapper` 创建委托包装器，而不是副本
- 对包装器属性的更改会影响源对象
- `Unwrap()` 返回原始源对象
- 适用于隐藏敏感属性而不复制数据

---

## 9. Flatten 特性 - 展平嵌套对象

**输入：**
```csharp
public class Person
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Address Address { get; set; }
    public ContactInfo ContactInfo { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
    public Country Country { get; set; }
}

public class Country
{
    public string Name { get; set; }
    public string Code { get; set; }
}

public class ContactInfo
{
    public string Email { get; set; }
    public string Phone { get; set; }
}

[Flatten(typeof(Person))]
public partial class PersonFlatDto;
```

**生成：**
```csharp
public partial class PersonFlatDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    // 从 Address 展平
    public string AddressStreet { get; set; }
    public string AddressCity { get; set; }
    public string AddressZipCode { get; set; }

    // 从 Address.Country 展平
    public string AddressCountryName { get; set; }
    public string AddressCountryCode { get; set; }

    // 从 ContactInfo 展平
    public string ContactInfoEmail { get; set; }
    public string ContactInfoPhone { get; set; }

    public PersonFlatDto(Person source)
    {
        this.Id = source.Id;
        this.FirstName = source.FirstName;
        this.LastName = source.LastName;

        if (source.Address != null)
        {
            this.AddressStreet = source.Address.Street;
            this.AddressCity = source.Address.City;
            this.AddressZipCode = source.Address.ZipCode;

            if (source.Address.Country != null)
            {
                this.AddressCountryName = source.Address.Country.Name;
                this.AddressCountryCode = source.Address.Country.Code;
            }
        }

        if (source.ContactInfo != null)
        {
            this.ContactInfoEmail = source.ContactInfo.Email;
            this.ContactInfoPhone = source.ContactInfo.Phone;
        }
    }

    public PersonFlatDto()
    {
    }
}
```

**关键点：**
- `Flatten` 递归展平嵌套对象属性
- 命名策略：`父属性子属性`（可自定义）
- `MaxDepth` 参数控制嵌套深度
- `IgnoreNestedIds` 排除嵌套 Id 属性
- `NamingStrategy = FlattenNamingStrategy.LeafOnly` 或 `SmartLeaf` 用于不同命名
- 默认忽略集合

---

## 10. 嵌套 Facet

**输入：**
```csharp
public class UserForNestedFacet
{
    public int Id { get; set; }
    public string Name { get; set; }
    public UserAddressForNestedFacet Address { get; set; }
}

public class UserAddressForNestedFacet
{
    public string Street { get; set; }
    public string City { get; set; }
    public string FormattedAddress => $"{Street}, {City}";
}

[Facet(typeof(UserForNestedFacet), Include = [
    nameof(UserForNestedFacet.Id),
    nameof(UserForNestedFacet.Name),
    nameof(UserForNestedFacet.Address)
], NestedFacets = [typeof(UserDetailResponse.UserAddressItem)])]
public partial class UserDetailResponse
{
    [Facet(typeof(UserAddressForNestedFacet), Include = [
        nameof(UserAddressForNestedFacet.FormattedAddress)
    ])]
    public partial class UserAddressItem;
}
```

**生成：**
```csharp
// UserAddressItem 作为嵌套类生成
public partial class UserDetailResponse
{
    public partial class UserAddressItem
    {
        public string FormattedAddress { get; set; }

        public UserAddressItem(UserAddressForNestedFacet source) : this(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance))
        {
        }

        public UserAddressItem(UserAddressForNestedFacet source, int __depth, HashSet<object>? __processed)
        {
            this.FormattedAddress = source.FormattedAddress;
        }

        public static UserAddressItem FromSource(UserAddressForNestedFacet source)
        {
            return new UserAddressItem(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
        }

        public UserAddressItem()
        {
        }

        public static Expression<Func<UserAddressForNestedFacet, UserAddressItem>> Projection =>
            source => new UserAddressItem
            {
                FormattedAddress = source.FormattedAddress
            };
    }
}

// UserDetailResponse 使用嵌套 facet
public partial class UserDetailResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public UserAddressItem? Address { get; set; }

    public UserDetailResponse(UserForNestedFacet source) : this(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance))
    {
    }

    public UserDetailResponse(UserForNestedFacet source, int __depth, HashSet<object>? __processed)
    {
        this.Id = source.Id;
        this.Name = source.Name;
        this.Address = source.Address != null ? new UserAddressItem(source.Address, __depth + 1, __processed) : null;
    }

    public static UserDetailResponse FromSource(UserForNestedFacet source)
    {
        return new UserDetailResponse(source, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
    }

    public UserDetailResponse()
    {
    }

    public static Expression<Func<UserForNestedFacet, UserDetailResponse>> Projection =>
        source => new UserDetailResponse
        {
            Id = source.Id,
            Name = source.Name,
            Address = source.Address != null ? new UserAddressItem(source.Address) : null
        };
}
```

**关键点：**
- 嵌套 facet 创建嵌套 DTO
- 深度跟踪防止循环引用的无限递归
- 嵌套 facet 通过 `NestedFacets` 参数指定

---

## 11. 生成控制标志

Facet 提供细粒度控制生成内容：

```csharp
[Facet(typeof(Source),
    GenerateConstructor = false,              // 跳过源构造函数
    GenerateParameterlessConstructor = false, // 跳过无参数构造函数
    GenerateProjection = false,               // 跳过 Projection 表达式
    GenerateToSource = false)]                // 跳过 ToSource() 方法
public partial class MyDto;
```

**常见组合：**

- **查询 DTO**：`NullableProperties = true, GenerateToSource = false`
- **响应 DTO**：`GenerateToSource = false`（只读）
- **手动构造**：`GenerateConstructor = false`（用于自定义逻辑）
- **仅 EF 投影**：`GenerateConstructor = false`（仅使用 `Projection`）

---

## 12. ConvertEnumsTo - 枚举类型转换

### 输入

```csharp
public enum UserStatus { Active, Inactive, Pending, Suspended }

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public UserStatus Status { get; set; }
    public string Email { get; set; }
}

[Facet(typeof(User), ConvertEnumsTo = typeof(string), GenerateToSource = true)]
public partial class UserStringDto;
```

### 生成输出

```csharp
public partial class UserStringDto
{
    // Status 是 string 而不是 UserStatus
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }     // 从 UserStatus 转换
    public string Email { get; set; }

    public UserStringDto() { }

    public UserStringDto(User source)
    {
        this.Id = source.Id;
        this.Name = source.Name;
        this.Status = source.Status.ToString();   // 枚举转字符串
        this.Email = source.Email;
    }

    public static UserStringDto FromSource(User source)
    {
        return new UserStringDto
        {
            Id = source.Id,
            Name = source.Name,
            Status = source.Status.ToString(),
            Email = source.Email
        };
    }

    public static Expression<Func<User, UserStringDto>> Projection =>
        source => new UserStringDto
        {
            Id = source.Id,
            Name = source.Name,
            Status = source.Status.ToString(),    // 在 EF Core 中有效
            Email = source.Email
        };

    public User ToSource()
    {
        return new User
        {
            Id = this.Id,
            Name = this.Name,
            Status = (UserStatus)System.Enum.Parse(typeof(UserStatus), this.Status),  // 字符串转枚举
            Email = this.Email
        };
    }
}
```

### Int 转换

使用 `ConvertEnumsTo = typeof(int)` 时，生成的代码使用强制转换：

```csharp
// 构造函数
this.Status = (int)source.Status;

// 投影
Status = (int)source.Status

// ToSource
Status = (UserStatus)this.Status
```

### 可空枚举处理

对于 `UserStatus? Status`，保留空安全性：

```csharp
// 字符串：构造函数
this.Status = source.Status?.ToString();

// Int：构造函数
this.Status = source.Status.HasValue ? (int?)source.Status.Value : null;
```

---

## 生成成员摘要

对于基本 facet，Facet 生成：

### 属性
- 除排除属性外的所有源属性
- partial 类中用户定义的属性
- 从源复制的 XML 文档
- 枚举属性转换为 `string` 或 `int`（当设置 `ConvertEnumsTo` 时）

### 构造函数
- `Dto(Source source)` - 主构造函数
- `Dto(Source source, int __depth, HashSet<object>? __processed)` - 深度跟踪构造函数
- `Dto()` - 无参数构造函数（可选）

### 方法
- `static Dto FromSource(Source source)` - 工厂方法，用于优化运行时性能
- `Source ToSource()` - 反向映射（当 `GenerateToSource = true` 时）
- `Source BackTo()` - 已过时，调用 `ToSource()`
- `Source Unwrap()` - 仅用于 Wrapper

### 表达式
- `static Expression<Func<Source, Dto>> Projection` - 用于 LINQ/EF 查询

---

## 与早期版本的主要差异

如果您从早期版本的 Facet 升级，以下是生成代码的主要变化：

1. **循环引用保护**：构造函数现在包含 `__depth` 和 `__processed` 参数
2. **`FromSource()` 方法**：新的静态工厂方法，用于优化性能
3. **`ToSource()` 替代 `BackTo()`**：`BackTo()` 现已过时
4. **全面的 XML 文档**：所有生成的成员都包含 XML 文档
5. **无参数构造函数**：现在默认生成
6. **新特性**：添加了 `MapFrom`、`MapWhen`、`Wrapper`、`Flatten`
7. **`[SetsRequiredMembers]` 特性**：用于具有必需成员的 record 的构造函数

---

另请参阅：
- [快速入门](02_QuickStart.zh-CN.md) - 基本用法和入门
- [特性参考](03_AttributeReference.zh-CN.md) - 完整的特性文档
- [高级场景](06_AdvancedScenarios.zh-CN.md) - 复杂映射场景
