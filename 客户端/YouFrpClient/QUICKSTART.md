# YouFRP客户端 - 快速开始指南

## 第一步：安装.NET环境

### Windows系统

1. 访问 [.NET下载页面](https://dotnet.microsoft.com/download/dotnet/6.0)
2. 下载并安装 **.NET 6.0 Runtime** (桌面运行时)
3. 安装完成后，打开命令提示符，输入 `dotnet --version` 验证安装

## 第二步：准备frpc客户端

1. 访问 [FRP发布页面](https://github.com/fatedier/frp/releases)
2. 下载Windows版本的frp客户端（例如：frp_xxx_windows_amd64.zip）
3. 解压后，将 `frpc.exe` 文件复制到本程序目录下

## 第三步：运行程序

### 开发模式运行

如果你有源代码：

1. 双击运行 `run.bat` 脚本
2. 或在命令行中执行：`dotnet run`

### 独立程序运行

如果你想生成独立可执行文件：

1. 双击运行 `build.bat` 脚本
2. 等待编译完成
3. 进入 `bin\Release\net6.0-windows\win-x64\publish\` 目录
4. 将 `frpc.exe` 复制到该目录
5. 双击 `南溪FRP.exe` 运行程序

## 第四步：登录使用

1. **首次登录**
   - 启动程序后，会显示登录界面
   - 输入您的访问Token（联系管理员获取）
   - 勾选"自动登录"可保存Token

2. **选择节点**
   - 登录成功后进入主界面
   - 在"节点选择"下拉框中选择合适的节点
   - 系统会自动加载该节点的隧道列表

3. **启动连接**
   - 检查隧道列表，确认需要的隧道已启用（绿色圆点）
   - 点击"启动"按钮
   - 查看运行日志，确认连接成功

4. **停止连接**
   - 点击"停止"按钮即可断开连接

## 目录结构

```
FrpClient/
├── bin/                    # 编译输出目录
├── Forms/                  # 窗体文件
├── Models/                 # 数据模型
├── Services/               # 业务服务
├── build.bat              # 构建脚本
├── run.bat                # 运行脚本
├── FrpClient.csproj       # 项目文件
├── Program.cs             # 程序入口
├── README.md              # 详细说明
└── QUICKSTART.md          # 本文件
```

## 常用命令

```bash
# 编译项目
dotnet build

# 运行项目
dotnet run

# 发布独立程序
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

# 清理编译输出
dotnet clean
```

## 常见问题

**Q: 提示"找不到frpc.exe"怎么办？**
A: 请确保frpc.exe文件与程序在同一目录下。

**Q: 登录时提示"访问密钥无效"？**
A: 请检查Token是否正确，或联系管理员确认Token状态。

**Q: 启动后没有反应？**
A: 查看运行日志区域，里面会显示详细的错误信息。

**Q: 隧道列表是空的？**
A: 可能该节点下没有配置隧道，尝试切换其他节点或联系管理员。

## 获取帮助

遇到问题？请查看完整的 [README.md](README.md) 文档。

---

**祝您使用愉快！** 🎉
