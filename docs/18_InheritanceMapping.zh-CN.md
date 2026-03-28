# 继承映射

Facet 完全支持源类型和 facet 类型中的继承层次结构。本指南介绍如何处理继承属性、基类和多态场景。

## Facet 中的继承工作原理

当你从具有基类的源类型创建 facet 时,Facet 会自动包含整个继承链中的所有继承属性。同样,你的 facet 类型可以从基类继承以共享公共属性。

## 映射继承属性

### 具有继承的源类型

```csharp
// 基础领域模型
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }  // 敏感
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// 派生领域模型
public class Employee : User
{
    public string EmployeeId { get; set; }
    public string Department { get; set; }
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }  // 敏感
}

// 进一步派生
public class Manager : Employee
{
    public string TeamName { get; set; }
    public int TeamSize { get; set; }
    public decimal Budget { get; set; }  // 敏感
}
```

