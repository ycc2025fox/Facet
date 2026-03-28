# Facet 文档索引

欢迎来到 Facet 文档! 本索引将帮助你浏览所有可用的指南和参考,以使用 Facet 及其扩展。

## 目录

- [Facetting](01_Facetting.zh-CN.md): Facetting 简介
- [快速开始](02_QuickStart.zh-CN.md): 快速开始指南
- [属性参考](03_AttributeReference.zh-CN.md): Facet 属性参考
- [自定义映射](04_CustomMapping.zh-CN.md): 使用 IFacetMapConfiguration 进行自定义映射及异步支持
- [属性映射](15_MapFromAttribute.zh-CN.md): 使用 MapFrom 属性进行声明式属性重命名
- [MapWhen 条件映射](17_MapWhen.zh-CN.md): 基于源值条件映射属性
- [继承映射](18_InheritanceMapping.zh-CN.md): 在源类型和 facet 基类中使用继承层次结构
- [映射钩子](19_MappingHooks.zh-CN.md): Before/After 映射钩子用于验证、默认值和计算值
- [扩展方法](05_Extensions.zh-CN.md): 扩展方法 (LINQ, EF Core 等)
- [高级场景](06_AdvancedScenarios.zh-CN.md): 高级使用场景
  - 从一个源创建多个 facet
  - Include/Exclude 模式
  - 嵌套 Facet (单个对象和集合)
  - 集合支持 (List, Array, ICollection, IEnumerable)
  - 继承和基类
- [生成文件输出配置](12_GeneratedFilesOutput.zh-CN.md): 配置生成文件的写入位置并使其在解决方案资源管理器中可见
- [生成了什么?](07_WhatIsBeingGenerated.zh-CN.md): 前后对比示例
- [异步映射指南](08_AsyncMapping.zh-CN.md): 使用 Facet.Mapping 进行异步映射
- [GenerateDtos 属性](09_GenerateDtosAttribute.zh-CN.md): 使用 GenerateDtos 和 GenerateAuditableDtos 自动生成 CRUD DTO
- [表达式映射](10_ExpressionMapping.zh-CN.md): 使用 Facet.Mapping.Expressions 在实体和 DTO 之间转换业务逻辑表达式
- [Flatten 属性](11_FlattenAttribute.zh-CN.md): 自动将嵌套对象展平为顶级属性,用于 API 响应和报告
- [Wrapper 属性](14_WrapperAttribute.zh-CN.md): 为外观和装饰器模式生成基于引用的包装器,支持属性委托
- [分析器规则](13_AnalyzerRules.zh-CN.md): Facet 的 Roslyn 分析器和诊断规则完整指南
- [源签名变更跟踪](16_SourceSignature.zh-CN.md): 通过编译时签名验证跟踪源实体变更
- [枚举转换](20_ConvertEnumsTo.zh-CN.md): 使用 ConvertEnumsTo 将枚举属性转换为 string 或 int
- [全局配置默认值](21_GlobalConfigurationDefaults.zh-CN.md): 使用 MSBuild 属性全局覆盖默认属性设置

## 生态系统包
- [Facet.Extensions.EFCore](../src/Facet.Extensions.EFCore/README.md): EF Core 异步扩展方法
- [Facet.Mapping 参考](../src/Facet.Mapping/README.md): 完整的 Facet.Mapping 文档
- [Facet.Mapping.Expressions 参考](../src/Facet.Mapping.Expressions/README.md): 完整的表达式映射文档
