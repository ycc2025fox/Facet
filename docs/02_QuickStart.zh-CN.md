# 快速开始指南

本指南将帮助你在几个步骤内快速上手 Facet。

## 1. 安装 NuGet 包

```
dotnet add package Facet
```

LINQ 辅助工具:
```
dotnet add package Facet.Extensions
```

## 2. 定义源模型

```csharp
public class Person
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}
```

## 3. 创建 Facet (DTO/投影)

```csharp
using Facet;

// 类
[Facet(typeof(Person), exclude: nameof(Person.Email))]
public partial class PersonDto { }

// Record (从 'record' 关键字推断)
[Facet(typeof(Person))]
public partial record PersonDto { }

// Struct (从 'struct' 关键字推断)
[Facet(typeof(Person))]
public partial struct PersonDto { }
```

## 4. 使用生成的类型

```csharp
var person = new Person { Name = "Alice", Email = "a@b.com", Age = 30 };

var dto = new PersonDto(person); // 使用生成的构造函数
```

## 5. LINQ 集成

```csharp
var query = dbContext.People.Select(PersonDto.Projection).ToList();
```

或使用 Facet.Extensions:

```csharp
using Facet.Extensions;

var dto = person.ToFacet<PersonDto>();

var dtos = personList.SelectFacets<PersonDto>();
```

---

查看 [属性参考](03_AttributeReference.md) 和 [扩展方法](05_Extensions.md) 了解更多详情。
