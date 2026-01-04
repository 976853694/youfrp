# FRP 客户端 Docker 镜像

这是一个基于 Docker 的 FRP 客户端，可以通过环境变量和挂载卷的方式进行配置。

## 构建镜像

在当前目录下执行以下命令构建 Docker 镜像：

```bash
docker build -t frp-client .
```

## 运行容器

### 基本用法

```bash
docker run -d --name frp-client \
  -e FRP_TOKEN="your_token_here" \
  -v $(pwd)/frp-config:/app/config \
  frp-client
```

### 参数说明

- `-e FRP_TOKEN="your_token_here"`：通过环境变量传入 token
- `-v $(pwd)/frp-config:/app/config`：将宿主机的 ./frp-config 目录挂载到容器内的 /app/config，便于存储生成的配置文件

### 目录结构

在宿主机的 ./frp-config 目录中，将自动生成以下文件：

```
frp-config/
├── config.ini       # 配置文件
├── frpc             # Linux客户端执行文件
├── frpc.ini         # 自动生成的 FRP 配置文件
└── token.txt        # 自动保存的 Token
```

如果需要使用 Windows 客户端，可以手动将 frpc.exe 文件放入 frp-config 目录。

## 自定义配置

如果需要传递额外的启动参数，可以在运行容器时添加：

```bash
docker run -d --name frp-client \
  -e FRP_TOKEN="your_token_here" \
  -v $(pwd)/frp-config:/app/config \
  frp-client -mode generate
```

可用的命令行参数：

- `-token string`：访问密钥
- `-config string`：配置文件路径
- `-mode string`：运行模式，可选值：generate(仅生成配置)、run(仅运行frpc)、both(生成配置并运行)
- `-frpc string`：frpc可执行文件路径
- `-overwrite`：是否覆盖已有的frpc.ini文件
- `-auto-run`：是否自动运行frpc 