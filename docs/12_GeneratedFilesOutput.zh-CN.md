# 生成文件输出配置

默认情况下,Roslyn 源代码生成器(包括 Facet)将生成的文件输出到 `obj/Generated/` 文件夹,该文件夹在解决方案资源管理器中是隐藏的。但是,你可以使用标准 MSBuild 属性配置生成文件的写入位置。

## 使生成的文件可见

要使生成的文件在项目中可见并控制其输出位置,请将以下属性添加到 `.csproj` 文件:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

### 配置选项

#### 1. 输出到项目文件夹(推荐)

将生成的文件放在项目中的 `Generated` 文件夹:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>

<!-- 从编译中排除以避免重复定义 -->
<ItemGroup>
  <Compile Remove="Generated\**" />
  <!-- 在解决方案资源管理器中保持可见 -->
  <None Include="Generated\**" />
</ItemGroup>
```

此配置:
- 使生成的文件在解决方案资源管理器中可见
- 允许你浏览和检查生成的代码
- 将它们从编译中排除(它们已作为生成的文件编译)
- 对调试和理解正在生成的代码很有用

#### 2. 输出到 obj 文件夹(默认行为)

将生成的文件保留在 obj 文件夹中但使其可见:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

这是 Facet 测试项目内部使用的方式。

#### 3. 输出到共享项目

在单独的共享项目中生成文件:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>..\MySharedProject\Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

**重要**: 输出到共享项目时,确保生成的类型是 `partial`,以便它们可以与源项目中的声明合并。

## 文件结构

使用默认配置,生成的文件按生成器组织:

```
Generated/
├── Facet/
│   ├── Facet.Generators.FacetGenerator/
│   │   ├── UserDto.g.cs
│   │   ├── ProductDto.g.cs
│   │   └── ...
│   ├── Facet.Generators.FlattenGenerator/
│   │   └── ...
│   └── Facet.Generators.GenerateDtosGenerator/
│       └── ...
└── OtherGenerator/
    └── ...
```

## 可见生成文件的优势

1. **调试**: 更容易查看正在生成的代码并调试问题
2. **学习**: 通过检查输出了解 Facet 如何生成代码
3. **代码审查**: 如果需要,在代码审查中包含生成的文件
4. **文档**: 自动生成的代码可作为文档

## 重要说明

- **不要手动编辑生成的文件** - 它们将在下次构建时被覆盖
- 生成的文件在**每次构建时都会重新创建**,基于你的源代码和属性
- **不要将生成的文件提交到源代码控制**,除非有特定原因(添加到 `.gitignore`)
- 将文件放在项目文件夹中时,**始终使用 `<Compile Remove="Generated\**" />` 将它们从编译中排除**

## 示例 .gitignore

如果选择使项目中的生成文件可见,请将此添加到 `.gitignore`:

```gitignore
# Facet 生成的文件
Generated/
**/Generated/
```

## 故障排除

### 重复类型定义

如果看到有关重复类型定义的错误,请确保已从编译中排除 Generated 文件夹:

```xml
<ItemGroup>
  <Compile Remove="Generated\**" />
</ItemGroup>
```

### 文件未出现

1. 清理并重新构建解决方案
2. 验证 `EmitCompilerGeneratedFiles` 设置为 `true`
3. 检查输出路径是否存在且可访问
4. 确保使用最新的 .NET SDK (6.0+)

### 性能考虑

将生成的文件输出到磁盘对性能影响很小。但是,如果有数千个生成的文件并使用源代码控制,请考虑:
- 将它们保留在 `obj` 文件夹中(默认)
- 将输出目录添加到 `.gitignore`
