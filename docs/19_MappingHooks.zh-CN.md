# Before/After 映射钩子

Facet 提供钩子在自动属性映射之前和之后运行自定义逻辑。这对于验证、设置默认值、计算派生值或应用转换很有用。

## 概述

| 接口 | 调用时机 | 使用场景 |
|-----------|-------------|----------|
| `IFacetBeforeMapConfiguration<TSource, TTarget>` | 属性复制之前 | 验证、默认值、状态准备 |
| `IFacetAfterMapConfiguration<TSource, TTarget>` | 属性复制之后 | 计算值、转换、后处理 |
| `IFacetMapHooksConfiguration<TSource, TTarget>` | 之前和之后 | 组合场景 |

## 何时使用映射钩子

### 使用 BeforeMap 的场景:
- 在映射开始前验证输入
- 在目标上设置默认值/时间戳
- 准备影响映射的状态
- 在映射完成前抛出验证错误

### 使用 AfterMap 的场景:
- 从映射的属性计算派生值
- 复制后转换值
- 对结果应用业务规则
- 验证最终映射结果

