# DictionaryOperations

`DictionaryOperations` 提供了一组用于操作字典的静态辅助方法，包括字典合并和替换。

## 功能特性

- **Merge**: 支持将源字典合并到目标字典。
  - **模式 (DictionaryMergeMode)**:
    - `Shallow` (默认): 浅层合并。如果值也是字典，直接替换引用。
    - `Deep`: 深度合并。如果值也是字典（实现了 `IDictionary`），则递归合并其内容。
  - **冲突策略 (DictionaryConflictStrategy)**:
    - `Overwrite` (默认): 覆盖目标字典中的值。
    - `KeepTarget`: 保留目标字典中的值（忽略源字典中的冲突值）。
    - `Throw`: 如果发生键冲突，抛出 `ArgumentException`。

- **Replace**: 使用源字典的值替换目标字典中相同键的值。忽略源字典中存在但目标字典中不存在的键。

## 使用示例

### 基础合并

```csharp
using LbxyCommonLib.Collections;

var target = new Dictionary<string, string> { { "A", "1" } };
var source = new Dictionary<string, string> { { "B", "2" } };

DictionaryOperations.Merge(target, source);
// target: { "A": "1", "B": "2" }
```

### 深度合并

适用于嵌套字典结构（如配置对象）。

```csharp
var nestedTarget = new Dictionary<string, object> 
{ 
    { "Config", new Dictionary<string, object> { { "Timeout", 1000 } } } 
};

var nestedSource = new Dictionary<string, object> 
{ 
    { "Config", new Dictionary<string, object> { { "Retries", 3 } } } 
};

// 使用深度合并
DictionaryOperations.Merge(nestedTarget, nestedSource, DictionaryMergeMode.Deep);

// 结果 nestedTarget["Config"] 包含:
// { "Timeout": 1000, "Retries": 3 }
```

### 替换值

仅更新已存在的键。

```csharp
var target = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } };
var source = new Dictionary<string, int> { { "A", 100 }, { "C", 300 } };

DictionaryOperations.Replace(target, source);
// target: { "A": 100, "B": 2 }
// "C" 被忽略，因为 target 中不存在该键
```

## 注意事项

- `Merge` 和 `Replace` 方法均会直接修改 `target` 字典。
- 深度合并要求值对象实现 `IDictionary` 接口。
- 泛型参数 `<TKey, TValue>` 提供了类型安全，但深度合并主要针对 `IDictionary` 类型的值有效。
