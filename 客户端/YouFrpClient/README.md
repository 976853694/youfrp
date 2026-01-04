# YouFRP客户端

一个美观、现代化的FRP内网穿透客户端，基于C# WinForms开发。

## 功能特性

✨ **美观的现代化界面**
- 深色主题设计，减少视觉疲劳
- 扁平化UI风格，简洁易用
- 流畅的动画效果

🚀 **强大的功能**
- Token登录验证
- 多节点支持，自由切换
- 隧道列表可视化管理
- 实时日志输出
- 一键启动/停止

🔧 **便捷的配置**
- 自动保存Token和节点选择
- 自动生成frpc配置文件(TOML格式)
- 支持多种隧道类型(TCP/UDP/HTTP/HTTPS/STCP/XTCP)

## 系统要求

- Windows 10/11
- .NET 6.0 Runtime
- MySQL数据库连接

## 安装说明

### 方式一：从源码构建

1. 确保已安装 [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

2. 克隆或下载本项目到本地

3. 打开命令行，进入项目目录

4. 执行构建命令：
```bash
dotnet build -c Release
```

5. 运行程序：
```bash
dotnet run
```

### 方式二：发布独立可执行文件

发布单文件版本：
```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

发布后的可执行文件位于：`bin\Release\net6.0-windows\win-x64\publish\`

## 使用说明

### 首次使用

1. **准备frpc.exe**
   - 从 [FRP官方发布页](https://github.com/fatedier/frp/releases) 下载frp客户端
   - 将 `frpc.exe` 放置在程序同一目录下

2. **获取Token**
   - 联系管理员获取访问Token
   - 或通过注册账号获取

3. **启动程序**
   - 双击运行程序
   - 在登录界面输入Token
   - 勾选"自动登录"可保存Token

### 日常使用

1. **选择节点**
   - 在节点选择下拉框中选择合适的节点
   - 程序会自动保存选择，下次启动时自动加载

2. **查看隧道**
   - 隧道列表会显示该节点下所有可用的隧道
   - 绿色圆点表示隧道已启用
   - 灰色圆点表示隧道已禁用

3. **启动连接**
   - 点击"启动"按钮开始连接
   - 运行日志会实时显示连接状态
   - 连接成功后状态显示"运行中"

4. **停止连接**
   - 点击"停止"按钮断开连接
   - 状态显示"未运行"

## 配置说明

### 数据库配置

程序内置默认数据库配置，如需修改，请编辑源码中的 `Config` 类：

```csharp
public class Config
{
    public string DBHost { get; set; } = "your_host:3306";
    public string DBUser { get; set; } = "your_user";
    public string DBPassword { get; set; } = "your_password";
    public string DBName { get; set; } = "your_database";
    public string FrpcPath { get; set; } = "frpc.exe";
}
```

### frpc路径配置

程序会自动检测当前目录下的 `frpc.exe`，如果frpc位于其他位置，可以修改配置。

## 文件说明

程序运行后会生成以下文件：

- `token.txt` - 保存的访问Token(勾选自动登录时)
- `node.txt` - 保存的节点选择
- `frpc.toml` - 自动生成的frpc配置文件

## 隧道类型支持

程序支持以下隧道类型：

- **TCP** - TCP端口映射
- **UDP** - UDP端口映射
- **HTTP** - HTTP协议映射(支持自定义域名)
- **HTTPS** - HTTPS协议映射(支持自定义域名)
- **STCP** - 安全的TCP映射(需要密钥)
- **XTCP** - 点对点TCP映射(需要密钥)

## 界面预览

### 登录界面
- 左侧：品牌展示区，深色背景
- 右侧：登录表单区，支持Token登录

### 主界面
- 左侧栏：导航菜单(软件首页/运行日志/隧道管理)
- 顶部栏：标题栏和窗口控制按钮
- 主区域：
  - 节点选择区
  - 隧道列表区(卡片式展示)
  - 控制面板(启动/停止/刷新)
  - 运行日志区(实时输出)

## 技术栈

- **框架**: .NET 6.0 + WinForms
- **数据库**: MySQL (MySql.Data)
- **序列化**: Newtonsoft.Json
- **配置格式**: TOML (frp 0.60.0+)

## 常见问题

### Q: 登录时提示"访问密钥无效"
A: 请检查：
1. Token是否正确
2. 数据库连接是否正常
3. Token状态是否为激活状态(status='0')

### Q: 启动后提示找不到frpc.exe
A: 请确保frpc.exe文件与程序在同一目录下，或修改配置指定正确路径

### Q: 隧道列表为空
A: 可能原因：
1. 该节点下没有配置隧道
2. 用户没有隧道权限
3. 数据库查询失败，请查看日志

### Q: 启动后无法连接
A: 请检查：
1. 网络连接是否正常
2. 节点服务器是否在线
3. 本地端口是否已被占用
4. 查看运行日志获取详细错误信息

## 开发说明

### 项目结构

```
FrpClient/
├── Models/              # 数据模型
│   └── Models.cs
├── Services/            # 业务服务
│   ├── DatabaseService.cs    # 数据库服务
│   └── FrpcService.cs        # frpc进程管理
├── Forms/               # 窗体界面
│   ├── LoginForm.cs          # 登录窗体
│   ├── LoginForm.Designer.cs
│   ├── MainForm.cs           # 主窗体
│   └── MainForm.Designer.cs
├── Program.cs           # 程序入口
└── FrpClient.csproj     # 项目文件
```

### 扩展开发

如需添加新功能，建议：

1. **添加新的隧道类型支持**
   - 修改 `FrpcService.GenerateConfig` 方法
   - 添加新的配置生成逻辑

2. **自定义界面主题**
   - 修改各窗体中的颜色定义
   - 统一在 `Color.FromArgb()` 处修改

3. **添加新的功能页面**
   - 在 `Forms` 目录下创建新窗体
   - 在主窗体侧边栏添加导航按钮

## 更新日志

### v1.0.0 (2024-12-15)
- 🎉 首次发布
- ✨ 实现Token登录
- ✨ 实现节点选择
- ✨ 实现隧道管理
- ✨ 实现frpc进程管理
- ✨ 实现配置文件自动生成
- 🎨 现代化深色主题UI

## 许可证

本项目基于MIT许可证开源。

## 致谢

- [FRP](https://github.com/fatedier/frp) - 优秀的内网穿透工具
- [.NET](https://dotnet.microsoft.com/) - 强大的开发框架

## 联系方式

如有问题或建议，欢迎联系开发者。

---

**YouFRP** - 让内网穿透更简单！
