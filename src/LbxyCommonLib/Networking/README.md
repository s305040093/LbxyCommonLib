# Networking: Local IPv4 Provider

API
- LocalIpProvider.GetLocalIPv4Addresses(): 获取本机所有非虚拟、非回环 IPv4 地址（Distinct 排序），异常返回空列表
- LocalIpProvider.GetLocalIPv4AddressesAsync(CancellationToken ct = default): 异步获取；取消或异常返回空列表

规则
- 过滤 NetworkInterfaceType.Loopback、Tunnel
- 过滤名称/描述包含 virtual、vmware、hyper-v、vEthernet 的适配器
- 仅保留 OperationalStatus.Up 且 Unicast IPv4 地址；排除 127.* 回环

使用示例
```csharp
using LbxyCommonLib.Networking;
var ips = LocalIpProvider.GetLocalIPv4Addresses();
var ipsAsync = await LocalIpProvider.GetLocalIPv4AddressesAsync();
```

注意事项
- 在权限受限或网络异常情况下返回空列表，不抛异常
- 该实现面向 .NET Framework 4.5 与 .NET Standard 2.0/NET 6.0
