# 使用 Facet.Mapping.Expressions 进行表达式映射

使用 `Facet.Mapping.Expressions` 在实体和 DTO 之间转换业务逻辑表达式。此库使你能够为实体定义一次业务规则、过滤器和选择器,并无缝地将它们用于 Facet DTO。

## 概述

表达式映射解决了在使用实体及其对应 DTO 时重复业务逻辑的常见问题。无需为每种类型重写谓词和选择器,你可以转换现有表达式以使用不同但兼容的类型。

## 安装

```bash
dotnet add package Facet.Mapping.Expressions
```

## 核心概念

### 谓词映射

将过滤表达式从实体类型转换为 DTO 类型:

```csharp
using Facet.Mapping.Expressions;

// 为实体定义的业务规则
Expression<Func<User, bool>> activeUsers = u => u.IsActive && !u.IsDeleted;

// 转换为与 DTO 一起使用
Expression<Func<UserDto, bool>> activeDtoUsers = activeUsers.MapToFacet<UserDto>();

// 与集合一起使用
var filteredDtos = dtoCollection.Where(activeDtoUsers.Compile()).ToList();
```

### 选择器映射

转换排序和选择表达式:

```csharp
// 实体排序的原始选择器
Expression<Func<User, string>> sortByLastName = u => u.LastName;

// 转换为与 DTO 一起使用
Expression<Func<UserDto, string>> dtoSortByLastName = sortByLastName.MapToFacet<UserDto, string>();

// 用于排序 DTO
var sortedDtos = dtoCollection.OrderBy(dtoSortByLastName.Compile()).ToList();
```

