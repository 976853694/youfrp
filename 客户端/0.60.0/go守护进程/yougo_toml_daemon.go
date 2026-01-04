package main

import (
	"crypto/md5"
	"database/sql"
	"flag"
	"fmt"
	"io"
	"log"
	"os"
	"os/exec"
	"path/filepath"
	"strconv"
	"strings"
	"time"

	_ "github.com/go-sql-driver/mysql"
)

// 全局变量
var userToken string
var selectedNodeID int
var frpcProcess *exec.Cmd

// 配置信息
type Config struct {
	DBHost     string
	DBUser     string
	DBPassword string
	DBName     string
	FrpcPath   string
}

// 隧道信息
type Proxy struct {
	ID                int
	Username          string
	ProxyName         string
	ProxyType         string
	LocalIP           string
	LocalPort         int
	UseEncryption     string
	UseCompression    string
	Domain            string
	Locations         string
	HostHeaderRewrite string
	RemotePort        string
	Sk                string
	HeaderXFromWhere  string
	Status            string
	LastUpdate        string
	Node              int
	CustomDomains     string
}

// 节点信息
type Node struct {
	ID       int
	Name     string
	Hostname string
	IP       string
	Port     int
	Token    string
}

// 用户信息
type User struct {
	ID       int
	Username string
	Token    string
	Status   string
}

func main() {
	// 解析命令行参数
	tokenFlag := flag.String("token", "", "访问密钥")
	nodeFlag := flag.String("node", "", "节点ID或IP")
	configPath := flag.String("config", "config.ini", "配置文件路径")
	frpcPath := flag.String("frpc", "frpc.exe", "frpc可执行文件路径")
	checkInterval := flag.Int("interval", 60, "检查配置变化的间隔（秒）")
	flag.Parse()

	fmt.Println("========================================")
	fmt.Println("    FRP客户端守护进程（自动重启版）")
	fmt.Println("========================================")
	fmt.Printf("检查间隔: %d秒\n", *checkInterval)
	fmt.Println("========================================\n")

	// 读取token
	var tokenInput string
	tokenFile := "token.txt"

	if *tokenFlag == "" {
		savedToken, err := os.ReadFile(tokenFile)
		if err == nil && len(savedToken) > 0 {
			tokenInput = strings.TrimSpace(string(savedToken))
			fmt.Printf("已使用保存的访问密钥: %s\n", tokenInput)
		} else {
			fmt.Print("请输入访问密钥: ")
			fmt.Scanln(&tokenInput)
			os.WriteFile(tokenFile, []byte(tokenInput), 0644)
		}
	} else {
		tokenInput = *tokenFlag
		os.WriteFile(tokenFile, []byte(tokenInput), 0644)
	}

	userToken = tokenInput

	// 加载配置
	config := loadConfig(*configPath)
	if *frpcPath != "frpc.exe" {
		config.FrpcPath = *frpcPath
	}

	// 连接数据库
	db, err := sql.Open("mysql", fmt.Sprintf("%s:%s@tcp(%s)/%s?charset=utf8mb4&parseTime=True",
		config.DBUser, config.DBPassword, config.DBHost, config.DBName))
	if err != nil {
		log.Fatalf("连接数据库失败: %v", err)
	}
	defer db.Close()

	// 验证token
	user, err := getUserByToken(db, tokenInput)
	if err != nil {
		log.Fatalf("验证访问密钥失败: %v", err)
	}

	// 处理节点选择
	nodeFile := "node.txt"
	if *nodeFlag != "" {
		selectedNodeID = parseNodeIdentifier(db, *nodeFlag)
		if selectedNodeID > 0 {
			saveNodeToFile(nodeFile, selectedNodeID)
		}
	} else {
		nodeData, err := os.ReadFile(nodeFile)
		if err == nil && len(nodeData) > 0 {
			nodeStr := strings.TrimSpace(string(nodeData))
			nodeID, err := strconv.Atoi(nodeStr)
			if err == nil && nodeID > 0 {
				node, err := getNodeByID(db, nodeID)
				if err == nil {
					selectedNodeID = nodeID
					fmt.Printf("使用保存的节点ID: %d (名称: %s, IP: %s)\n", selectedNodeID, node.Name, node.IP)
				}
			}
		}

		if selectedNodeID <= 0 {
			selectedNodeID = selectNode(db)
			if selectedNodeID > 0 {
				saveNodeToFile(nodeFile, selectedNodeID)
			}
		}
	}

	if selectedNodeID <= 0 {
		log.Fatalf("未选择节点，程序退出")
	}

	// 获取节点信息
	selectedNode, err := getNodeByID(db, selectedNodeID)
	if err != nil {
		log.Fatalf("获取节点信息失败: %v", err)
	}

	fmt.Println("\n========================================")
	fmt.Println("开始守护进程...")
	fmt.Println("按 Ctrl+C 退出程序")
	fmt.Println("========================================\n")

	// 首次生成配置并启动
	if err := generateAndStartFrpc(db, user.Username, selectedNode, config); err != nil {
		log.Fatalf("初始启动失败: %v", err)
	}

	// 定时检查配置变化
	ticker := time.NewTicker(time.Duration(*checkInterval) * time.Second)
	defer ticker.Stop()

	for range ticker.C {
		fmt.Printf("[%s] 检查配置变化...\n", time.Now().Format("2006-01-02 15:04:05"))

		if err := checkAndReloadConfig(db, user.Username, selectedNode, config); err != nil {
			log.Printf("检查配置失败: %v", err)
		}
	}
}

// 生成配置并启动frpc
func generateAndStartFrpc(db *sql.DB, username string, node Node, config Config) error {
	// 获取隧道列表
	proxies, err := getProxiesByUsername(db, username)
	if err != nil {
		return fmt.Errorf("获取隧道列表失败: %v", err)
	}

	// 筛选当前节点的隧道
	selectedProxies := []Proxy{}
	for _, proxy := range proxies {
		if proxy.Node == selectedNodeID {
			selectedProxies = append(selectedProxies, proxy)
		}
	}

	if len(selectedProxies) == 0 {
		return fmt.Errorf("节点(ID=%d)没有可用的隧道", selectedNodeID)
	}

	// 生成配置文件
	if err := generateConfigTOML("frpc.toml", node, selectedProxies); err != nil {
		return fmt.Errorf("生成配置文件失败: %v", err)
	}

	fmt.Printf("✓ 已生成配置文件: frpc.toml (%d个隧道)\n", len(selectedProxies))

	// 启动frpc
	return startFrpc(config.FrpcPath)
}

// 检查配置变化并重新加载
func checkAndReloadConfig(db *sql.DB, username string, node Node, config Config) error {
	// 获取最新的隧道列表
	proxies, err := getProxiesByUsername(db, username)
	if err != nil {
		return fmt.Errorf("获取隧道列表失败: %v", err)
	}

	// 筛选当前节点的隧道
	selectedProxies := []Proxy{}
	for _, proxy := range proxies {
		if proxy.Node == selectedNodeID {
			selectedProxies = append(selectedProxies, proxy)
		}
	}

	// 生成临时配置文件
	tempConfig := fmt.Sprintf("frpc_temp_%d.toml", time.Now().Unix())
	if err := generateConfigTOML(tempConfig, node, selectedProxies); err != nil {
		return fmt.Errorf("生成临时配置失败: %v", err)
	}
	defer os.Remove(tempConfig) // 清理临时文件

	// 对比配置文件
	if !isConfigChanged("frpc.toml", tempConfig) {
		fmt.Println("  配置未变化，无需重启")
		return nil
	}

	fmt.Println("  ⚠️  检测到配置变化！")

	// 停止当前frpc进程
	if err := stopFrpc(); err != nil {
		log.Printf("  停止frpc失败: %v", err)
	}

	// 替换配置文件
	if err := os.Rename(tempConfig, "frpc.toml"); err != nil {
		// 如果重命名失败，尝试复制
		if err := copyFile(tempConfig, "frpc.toml"); err != nil {
			return fmt.Errorf("更新配置文件失败: %v", err)
		}
	}

	fmt.Printf("  ✓ 已更新配置文件 (%d个隧道)\n", len(selectedProxies))

	// 重新启动frpc
	if err := startFrpc(config.FrpcPath); err != nil {
		return fmt.Errorf("重启frpc失败: %v", err)
	}

	fmt.Println("  ✓ frpc已重启")
	return nil
}

// 启动frpc进程
func startFrpc(frpcPath string) error {
	frpcProcess = exec.Command(frpcPath, "-c", "frpc.toml")
	frpcProcess.Stdout = os.Stdout
	frpcProcess.Stderr = os.Stderr

	if err := frpcProcess.Start(); err != nil {
		return err
	}

	fmt.Printf("✓ frpc已启动 (PID: %d)\n", frpcProcess.Process.Pid)
	return nil
}

// 停止frpc进程
func stopFrpc() error {
	if frpcProcess == nil || frpcProcess.Process == nil {
		return nil
	}

	fmt.Println("  停止frpc进程...")

	if err := frpcProcess.Process.Kill(); err != nil {
		return err
	}

	frpcProcess.Wait()
	frpcProcess = nil

	fmt.Println("  ✓ frpc已停止")
	return nil
}

// 检查配置文件是否变化
func isConfigChanged(file1, file2 string) bool {
	hash1, err1 := getFileHash(file1)
	hash2, err2 := getFileHash(file2)

	if err1 != nil || err2 != nil {
		return true // 如果有错误，假设已变化
	}

	return hash1 != hash2
}

// 获取文件MD5哈希
func getFileHash(filePath string) (string, error) {
	file, err := os.Open(filePath)
	if err != nil {
		return "", err
	}
	defer file.Close()

	hash := md5.New()
	if _, err := io.Copy(hash, file); err != nil {
		return "", err
	}

	return fmt.Sprintf("%x", hash.Sum(nil)), nil
}

// 复制文件
func copyFile(src, dst string) error {
	sourceFile, err := os.Open(src)
	if err != nil {
		return err
	}
	defer sourceFile.Close()

	destFile, err := os.Create(dst)
	if err != nil {
		return err
	}
	defer destFile.Close()

	_, err = io.Copy(destFile, sourceFile)
	return err
}

// 以下是原有的辅助函数（保持不变）

func loadConfig(configPath string) Config {
	config := Config{
		DBHost:     "138.2.24.169:3306",
		DBUser:     "youfrp",
		DBPassword: "tgx123456.",
		DBName:     "youfrp",
		FrpcPath:   "frpc.exe",
	}

	currentDir, err := os.Getwd()
	if err == nil {
		frpcExePath := filepath.Join(currentDir, "frpc.exe")
		frpcPath := filepath.Join(currentDir, "frpc")

		if _, err := os.Stat(frpcExePath); err == nil {
			config.FrpcPath = frpcExePath
		} else if _, err := os.Stat(frpcPath); err == nil {
			config.FrpcPath = frpcPath
		}
	}

	return config
}

func getUserByToken(db *sql.DB, token string) (User, error) {
	var user User
	query := "SELECT id, username, token, status FROM tokens WHERE token = ? AND status = '0'"
	err := db.QueryRow(query, token).Scan(&user.ID, &user.Username, &user.Token, &user.Status)
	if err != nil {
		return user, fmt.Errorf("访问密钥无效: %v", err)
	}
	return user, nil
}

func getProxiesByUsername(db *sql.DB, username string) ([]Proxy, error) {
	proxies := []Proxy{}
	query := "SELECT id, username, proxy_name, proxy_type, local_ip, local_port, " +
		"use_encryption, use_compression, domain, locations, host_header_rewrite, " +
		"remote_port, sk, `header_X-From-Where`, status, lastupdate, node, customdomains " +
		"FROM proxies WHERE username = ?"
	rows, err := db.Query(query, username)
	if err != nil {
		return proxies, err
	}
	defer rows.Close()

	for rows.Next() {
		var p Proxy
		err := rows.Scan(
			&p.ID, &p.Username, &p.ProxyName, &p.ProxyType, &p.LocalIP, &p.LocalPort,
			&p.UseEncryption, &p.UseCompression, &p.Domain, &p.Locations, &p.HostHeaderRewrite,
			&p.RemotePort, &p.Sk, &p.HeaderXFromWhere, &p.Status, &p.LastUpdate, &p.Node, &p.CustomDomains,
		)
		if err != nil {
			continue
		}
		proxies = append(proxies, p)
	}

	return proxies, nil
}

func getNodeByID(db *sql.DB, id int) (Node, error) {
	var node Node
	query := "SELECT id, name, hostname, ip, port, token FROM nodes WHERE id = ? AND status = '200'"
	err := db.QueryRow(query, id).Scan(&node.ID, &node.Name, &node.Hostname, &node.IP, &node.Port, &node.Token)
	return node, err
}

func parseNodeIdentifier(db *sql.DB, nodeStr string) int {
	nodeID, err := strconv.Atoi(nodeStr)
	if err == nil && nodeID > 0 {
		node, err := getNodeByID(db, nodeID)
		if err == nil {
			fmt.Printf("已选择节点ID: %d (名称: %s, IP: %s)\n", nodeID, node.Name, node.IP)
			return nodeID
		}
	}
	return 0
}

func saveNodeToFile(filePath string, nodeID int) {
	os.WriteFile(filePath, []byte(strconv.Itoa(nodeID)), 0644)
}

func selectNode(db *sql.DB) int {
	nodes, err := getAllNodes(db)
	if err != nil || len(nodes) == 0 {
		return 0
	}

	fmt.Println("\n===== 可用节点列表 =====")
	for i, node := range nodes {
		fmt.Printf("[%d] ID:%d 名称:%s IP:%s\n", i+1, node.ID, node.Name, node.IP)
	}

	var choice int
	fmt.Print("\n请选择节点编号: ")
	fmt.Scanln(&choice)

	if choice < 1 || choice > len(nodes) {
		return 0
	}

	return nodes[choice-1].ID
}

func getAllNodes(db *sql.DB) ([]Node, error) {
	nodes := []Node{}
	query := "SELECT id, name, hostname, ip, port, token FROM nodes WHERE status = '200'"
	rows, err := db.Query(query)
	if err != nil {
		return nodes, err
	}
	defer rows.Close()

	for rows.Next() {
		var node Node
		rows.Scan(&node.ID, &node.Name, &node.Hostname, &node.IP, &node.Port, &node.Token)
		nodes = append(nodes, node)
	}

	return nodes, nil
}

func generateConfigTOML(configFile string, node Node, proxies []Proxy) error {
	var builder strings.Builder

	builder.WriteString(fmt.Sprintf("serverAddr = \"%s\"\n", node.IP))
	builder.WriteString(fmt.Sprintf("serverPort = %d\n", node.Port))
	builder.WriteString("transport.tcpMux = true\n")
	builder.WriteString("transport.protocol = \"tcp\"\n")
	builder.WriteString("auth.method = \"token\"\n")
	builder.WriteString(fmt.Sprintf("auth.token = \"%s\"\n", node.Token))
	builder.WriteString(fmt.Sprintf("user = \"%s\"\n", userToken))
	builder.WriteString("dnsServer = \"114.114.114.114\"\n\n")

	for _, proxy := range proxies {
		if proxy.Status != "0" {
			continue
		}

		builder.WriteString("[[proxies]]\n")
		builder.WriteString(fmt.Sprintf("name = \"%s\"\n", proxy.ProxyName))
		builder.WriteString(fmt.Sprintf("type = \"%s\"\n", proxy.ProxyType))
		builder.WriteString(fmt.Sprintf("localIP = \"%s\"\n", proxy.LocalIP))
		builder.WriteString(fmt.Sprintf("localPort = %d\n", proxy.LocalPort))

		switch strings.ToLower(proxy.ProxyType) {
		case "http", "https":
			if proxy.CustomDomains != "" {
				builder.WriteString(fmt.Sprintf("customDomains = [\"%s\"]\n", proxy.CustomDomains))
			}
			if proxy.HostHeaderRewrite != "" {
				builder.WriteString(fmt.Sprintf("hostHeaderRewrite = \"%s\"\n", proxy.HostHeaderRewrite))
			}
		default:
			if proxy.RemotePort != "" {
				builder.WriteString(fmt.Sprintf("remotePort = %s\n", proxy.RemotePort))
			}
		}

		useEncryption := strings.ToLower(proxy.UseEncryption)
		builder.WriteString(fmt.Sprintf("transport.useEncryption = %v\n", useEncryption == "1" || useEncryption == "true"))

		useCompression := strings.ToLower(proxy.UseCompression)
		builder.WriteString(fmt.Sprintf("transport.useCompression = %v\n", useCompression == "1" || useCompression == "true"))

		builder.WriteString("\n")
	}

	return os.WriteFile(configFile, []byte(builder.String()), 0644)
}
