# YouFRP客户端 - 项目概述

## 📋 项目信息

**项目名称**: YouFRP客户端  
**版本**: 1.0.0  
**开发语言**: C# (.NET 6.0)  
**界面框架**: Windows Forms  
**数据库**: MySQL  
**配置格式**: TOML (frp 0.60.0+)  
**开发日期**: 2024年12月15日

## 🎯 项目目标

基于参考的Go语言命令行客户端（yougo_toml.go），开发一个功能完整、界面美观的Windows桌面应用程序，实现FRP内网穿透的可视化管理。

## ✨ 核心功能

### 1. 用户认证系统
- ✅ Token登录验证
- ✅ 自动保存Token（可选）
- ✅ 数据库用户验证
- 🔲 用户名密码登录（预留接口）

### 2. 节点管理
- ✅ 从数据库加载可用节点列表
- ✅ 节点选择下拉框
- ✅ 自动保存节点选择
- ✅ 节点状态验证

### 3. 隧道管理
- ✅ 根据节点加载隧道列表
- ✅ 卡片式隧道展示
- ✅ 隧道状态可视化（启用/禁用）
- ✅ 支持多种隧道类型：TCP/UDP/HTTP/HTTPS/STCP/XTCP
- ✅ 显示映射信息（本地->远程）

### 4. 服务控制
- ✅ 一键启动frpc进程
- ✅ 一键停止frpc进程
- ✅ 自动生成TOML配置文件
- ✅ 进程状态监控
- ✅ 刷新功能

### 5. 日志系统
- ✅ 实时显示frpc输出
- ✅ 错误信息高亮
- ✅ 时间戳标记
- ✅ 自动滚动

### 6. 界面设计
- ✅ 现代化深色主题
- ✅ 扁平化设计风格
- ✅ 响应式布局
- ✅ 无边框窗口设计
- ✅ 窗口拖动支持
- ✅ 平滑动画效果

## 🏗️ 项目结构

```
FrpClient/
│
├── Models/                      # 数据模型层
│   └── Models.cs               # Config, User, Node, Proxy模型
│
├── Services/                    # 业务逻辑层
│   ├── DatabaseService.cs      # 数据库操作服务
│   │   ├── GetUserByToken()    # 用户Token验证
│   │   ├── GetAllNodes()       # 获取所有节点
│   │   ├── GetNodeByID()       # 根据ID获取节点
│   │   ├── GetProxiesByUsername() # 获取用户隧道
│   │   └── GetProxiesByNodeID() # 获取节点隧道
│   │
│   └── FrpcService.cs          # frpc进程管理服务
│       ├── GenerateConfig()    # 生成TOML配置
│       ├── StartFrpc()         # 启动frpc进程
│       ├── StopFrpc()          # 停止frpc进程
│       └── IsRunning           # 运行状态检查
│
├── Forms/                       # 界面层
│   ├── LoginForm.cs            # 登录窗体
│   │   ├── Token输入           # 访问密钥输入框
│   │   ├── 密码输入            # 密码输入框（预留）
│   │   ├── 自动登录            # 记住Token选项
│   │   └── 登录验证            # 调用数据库验证
│   │
│   └── MainForm.cs             # 主窗体
│       ├── 节点选择区          # ComboBox节点选择
│       ├── 隧道列表区          # FlowLayoutPanel卡片展示
│       ├── 控制面板区          # 启动/停止/刷新按钮
│       ├── 日志显示区          # 实时日志TextBox
│       └── 侧边导航栏          # 功能切换按钮
│
├── Program.cs                   # 程序入口
├── FrpClient.csproj            # 项目配置
├── FrpClient.sln               # Visual Studio解决方案
│
├── build.bat                    # 构建脚本
├── run.bat                      # 运行脚本
│
└── 文档/
    ├── README.md               # 完整文档
    ├── QUICKSTART.md           # 快速开始
    ├── 使用说明.txt            # 用户手册
    └── PROJECT.md              # 本文件
```

## 🔄 业务流程

### 登录流程
```
用户启动程序
    ↓
显示登录界面
    ↓
检查本地token.txt → 存在 → 自动填充
    ↓
用户输入Token
    ↓
点击登录按钮
    ↓
DatabaseService.GetUserByToken()
    ↓
验证成功 → 保存Token（可选）→ 打开主窗体
验证失败 → 显示错误消息
```

### 启动流程
```
用户选择节点
    ↓
DatabaseService.GetProxiesByNodeID()
    ↓
显示隧道列表
    ↓
用户点击启动
    ↓
FrpcService.GenerateConfig() → 生成frpc.toml
    ↓
FrpcService.StartFrpc() → 启动frpc进程
    ↓
订阅输出事件 → 显示实时日志
    ↓
更新运行状态
```

## 🎨 界面设计

### 颜色方案

| 用途 | 颜色代码 | RGB值 |
|-----|---------|-------|
| 主背景 | #191923 | (25, 25, 35) |
| 次背景 | #232332 | (35, 35, 50) |
| 卡片背景 | #2D2D3C | (45, 45, 60) |
| 输入框背景 | #323246 | (50, 50, 70) |
| 强调色（蓝） | #6496FF | (100, 150, 255) |
| 成功色（绿） | #50C850 | (80, 200, 80) |
| 错误色（红） | #C85050 | (150, 80, 80) |
| 文本主色 | #FFFFFF | (255, 255, 255) |
| 文本次色 | #B4B4C8 | (180, 180, 200) |

### 布局设计

**登录界面** (880x600)
- 左侧 (400px): 品牌展示区
- 右侧 (480px): 登录表单区

**主界面** (1200x700)
- 左侧栏 (180px): 导航菜单
- 顶部栏 (50px): 标题和窗口控制
- 主区域 (1020x650): 内容展示区

## 📊 数据库设计

### tokens表（用户Token）
```sql
- id: INT (主键)
- username: VARCHAR (用户名)
- token: VARCHAR (访问密钥)
- status: VARCHAR (状态: 0=正常)
```

### nodes表（节点信息）
```sql
- id: INT (主键)
- name: VARCHAR (节点名称)
- hostname: VARCHAR (主机名)
- ip: VARCHAR (IP地址)
- port: INT (端口)
- token: VARCHAR (节点Token)
- status: VARCHAR (状态: 200=可用)
```

### proxies表（隧道信息）
```sql
- id: INT (主键)
- username: VARCHAR (所属用户)
- proxy_name: VARCHAR (隧道名称)
- proxy_type: VARCHAR (隧道类型)
- local_ip: VARCHAR (本地IP)
- local_port: INT (本地端口)
- use_encryption: VARCHAR (是否加密)
- use_compression: VARCHAR (是否压缩)
- domain: VARCHAR (域名)
- remote_port: VARCHAR (远程端口)
- node: INT (所属节点ID)
- status: VARCHAR (状态: 0=启用)
- ...其他字段
```

## 🔧 技术特性

### 依赖包
- **MySql.Data** (8.3.0) - MySQL数据库连接
- **Newtonsoft.Json** (13.0.3) - JSON序列化

### 核心技术点

1. **异步数据库操作**: 使用using语句确保连接正确释放
2. **进程管理**: Process类管理frpc子进程
3. **事件驱动**: 使用事件机制处理输出重定向
4. **文件操作**: 自动生成和管理配置文件
5. **UI线程安全**: InvokeRequired处理跨线程UI更新
6. **异常处理**: 完善的try-catch异常捕获

## 📝 配置文件格式

### frpc.toml示例
```toml
serverAddr = "节点IP"
serverPort = 节点端口
transport.tcpMux = true
transport.protocol = "tcp"
auth.method = "token"
auth.token = "节点Token"
user = "用户Token"
dnsServer = "114.114.114.114"

[[proxies]]
name = "隧道名称"
type = "tcp"
localIP = "127.0.0.1"
localPort = 80
remotePort = 8080
transport.useEncryption = false
transport.useCompression = false
```

## 🚀 编译和发布

### 开发环境
- Visual Studio 2022 (推荐)
- .NET 6.0 SDK

### 编译命令
```bash
# Debug编译
dotnet build

# Release编译
dotnet build -c Release

# 发布单文件
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

### 发布文件
- 输出目录: `bin\Release\net6.0-windows\win-x64\publish\`
- 主程序: `南溪FRP.exe`
- 大小: 约60-80MB（包含.NET运行时）

## 📈 后续计划

### 待完善功能
- [ ] 用户名密码登录
- [ ] 隧道在线编辑
- [ ] 多隧道批量启动
- [ ] 系统托盘最小化
- [ ] 开机自启动设置
- [ ] 配置导入导出
- [ ] 主题切换功能
- [ ] 多语言支持

### 优化方向
- [ ] 性能优化
- [ ] 内存占用优化
- [ ] 启动速度优化
- [ ] UI响应速度优化

## 🎯 与原Go版本对比

| 功能 | Go版本 | C#版本 |
|-----|--------|--------|
| 用户界面 | 命令行 | 图形化界面 |
| 节点选择 | 手动输入/文件保存 | 下拉框选择 |
| 隧道展示 | 文本列表 | 卡片式可视化 |
| 日志查看 | 控制台输出 | 实时日志窗口 |
| Token管理 | 文件存储 | 界面+文件存储 |
| 配置生成 | ✅ | ✅ |
| 进程管理 | ✅ | ✅ |
| 数据库连接 | ✅ | ✅ |

## 📄 许可证

MIT License

---

**开发者**: 基于yougo_toml.go参考实现  
**最后更新**: 2024年12月15日
