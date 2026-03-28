# 全局配置默认值

## 概述

Facet 允许你使用 MSBuild 属性全局覆盖属性的默认值。这在你想要在所有 facet 中应用一致的设置而无需在每个属性上指定相同属性时很有用。

## 使用场景

当使用像 [Mapperly](https://mapperly.riok.app/) 这样的映射库时,你可能需要禁用某些 Facet 功能(如构造函数生成)以避免冲突。与其在每个 facet 上设置 `GenerateConstructor = false`,不如全局配置。

## 支持的配置属性

以下属性可以为 `[Facet]` 属性全局配置:

| 属性 | 默认值 | MSBuild 属性 |
|----------|---------|------------------|
| `GenerateConstructor` | `true` | `Facet_GenerateConstructor` |
| `GenerateParameterlessConstructor` | `true` | `Facet_GenerateParameterlessConstructor` |
| `GenerateProjection` | `true` | `Facet_GenerateProjection` |
| `GenerateToSource` | `false` | `Facet_GenerateToSource` |
| `IncludeFields` | `false` | `Facet_IncludeFields` |
| `ChainToParameterlessConstructor` | `false` | `Facet_ChainToParameterlessConstructor` |
| `NullableProperties` | `false` | `Facet_NullableProperties` |
| `CopyAttributes` | `false` | `Facet_CopyAttributes` |
| `UseFullName` | `false` | `Facet_UseFullName` |
| `GenerateCopyConstructor` | `false` | `Facet_GenerateCopyConstructor` |
| `GenerateEquality` | `false` | `Facet_GenerateEquality` |
| `MaxDepth` | `10` | `Facet_MaxDepth` |
| `PreserveReferences` | `true` | `Facet_PreserveReferences` |

