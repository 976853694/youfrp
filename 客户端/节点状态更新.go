package main

import (
	"database/sql"
	"flag"
	"fmt"
	"log"
	"net"
	"net/http"
	"time"

	_ "github.com/go-sql-driver/mysql"
)

// 配置信息
type Config struct {
	DBHost     string
	DBUser     string
	DBPassword string
	DBName     string
	Timeout    int
	Interval   int
	CheckAPI   bool // 是否检查API接口
}

// 节点信息
type Node struct {
	ID        int
	Name      string
	IP        string
	Port      int
	AdminPort int
	AdminPass string
	Status    string
}

// 检查结果
type CheckResult struct {
	NodeID       int
	NodeName     string
	Address      string
	IsOnline     bool
	ResponseTime int64 // 响应时间（毫秒）
	Error        string
}

func main() {
	// 解析命令行参数
	timeout := flag.Int("timeout", 5, "连接超时时间（秒）")
	interval := flag.Int("interval", 60, "检查间隔（秒），0表示只检查一次")
	checkAPI := flag.Bool("check-api", false, "是否检查FRP管理API接口")
	verbose := flag.Bool("verbose", false, "显示详细信息")
	flag.Parse()

	// 加载配置
	config := Config{
		DBHost:     "127.0.0.1:3306",
		DBUser:     "frp",
		DBPassword: "tgx123456",
		DBName:     "frp",
		Timeout:    *timeout,
		Interval:   *interval,
		CheckAPI:   *checkAPI,
	}

	// 连接数据库
	db, err := connectDB(config)
	if err != nil {
		log.Fatalf("连接数据库失败: %v", err)
	}
	defer db.Close()

	printHeader(config)

	// 执行检查
	if config.Interval > 0 {
		// 循环检查模式
		for {
			results := checkAllNodes(db, config, *verbose)
			printSummary(results)

			fmt.Printf("\n等待 %d 秒后进行下一次检查...\n", config.Interval)
			fmt.Println("按 Ctrl+C 退出程序")
			fmt.Println("========================================\n")
			time.Sleep(time.Duration(config.Interval) * time.Second)
		}
	} else {
		// 单次检查模式
		results := checkAllNodes(db, config, *verbose)
		printSummary(results)
		fmt.Println("\n检查完成")
	}
}

// 打印程序头部信息
func printHeader(config Config) {
	fmt.Println("========================================")
	fmt.Println("    FRP 节点在线状态检查程序")
	fmt.Println("========================================")
	fmt.Printf("数据库: %s\n", config.DBHost)
	fmt.Printf("超时时间: %d秒\n", config.Timeout)
	if config.Interval > 0 {
		fmt.Printf("检查间隔: %d秒\n", config.Interval)
	} else {
		fmt.Println("模式: 单次检查")
	}
	if config.CheckAPI {
		fmt.Println("API检查: 启用")
	}
	fmt.Println("========================================\n")
}

// 连接数据库
func connectDB(config Config) (*sql.DB, error) {
	dsn := fmt.Sprintf("%s:%s@tcp(%s)/%s?charset=utf8mb4&parseTime=True",
		config.DBUser, config.DBPassword, config.DBHost, config.DBName)

	db, err := sql.Open("mysql", dsn)
	if err != nil {
		return nil, err
	}

	// 设置连接池参数
	db.SetMaxOpenConns(10)
	db.SetMaxIdleConns(5)
	db.SetConnMaxLifetime(time.Hour)

	// 测试连接
	err = db.Ping()
	if err != nil {
		return nil, err
	}

	fmt.Println("✓ 数据库连接成功\n")
	return db, nil
}

// 检查所有节点
func checkAllNodes(db *sql.DB, config Config, verbose bool) []CheckResult {
	nodes, err := getAllNodes(db)
	if err != nil {
		log.Printf("获取节点列表失败: %v", err)
		return nil
	}

	if len(nodes) == 0 {
		fmt.Println("⚠ 没有找到任何节点")
		return nil
	}

	fmt.Printf("开始检查 %d 个节点...\n", len(nodes))
	fmt.Printf("检查时间: %s\n", time.Now().Format("2006-01-02 15:04:05"))
	fmt.Println("----------------------------------------")

	results := make([]CheckResult, 0, len(nodes))

	for i, node := range nodes {
		fmt.Printf("[%d/%d] 检查节点: %s (ID:%d)\n", i+1, len(nodes), node.Name, node.ID)

		result := checkNode(node, config, verbose)
		results = append(results, result)

		// 更新数据库
		newStatus := "500"
		if result.IsOnline {
			newStatus = "200"
		}

		err := updateNodeStatus(db, node.ID, newStatus)
		if err != nil {
			log.Printf("  ✗ 更新数据库失败: %v", err)
		} else if verbose {
			fmt.Printf("  ✓ 数据库已更新 (status=%s)\n", newStatus)
		}

		// 输出检查结果
		if result.IsOnline {
			fmt.Printf("  ✓ 在线 | 响应时间: %dms\n", result.ResponseTime)
		} else {
			fmt.Printf("  ✗ 离线 | 原因: %s\n", result.Error)
		}
		fmt.Println()
	}

	return results
}

// 获取所有节点
func getAllNodes(db *sql.DB) ([]Node, error) {
	nodes := []Node{}
	query := "SELECT id, name, ip, port, admin_port, admin_pass, status FROM nodes"

	rows, err := db.Query(query)
	if err != nil {
		return nodes, err
	}
	defer rows.Close()

	for rows.Next() {
		var node Node
		var adminPort sql.NullInt64
		var adminPass sql.NullString

		err := rows.Scan(&node.ID, &node.Name, &node.IP, &node.Port,
			&adminPort, &adminPass, &node.Status)
		if err != nil {
			log.Printf("扫描节点行失败: %v", err)
			continue
		}

		if adminPort.Valid {
			node.AdminPort = int(adminPort.Int64)
		} else {
			node.AdminPort = 7000
		}

		if adminPass.Valid {
			node.AdminPass = adminPass.String
		}

		nodes = append(nodes, node)
	}

	return nodes, nil
}

// 检查单个节点
func checkNode(node Node, config Config, verbose bool) CheckResult {
	result := CheckResult{
		NodeID:   node.ID,
		NodeName: node.Name,
		Address:  fmt.Sprintf("%s:%d", node.IP, node.AdminPort),
	}

	// 检查TCP连接
	startTime := time.Now()
	isOnline, err := checkTCPConnection(node.IP, node.AdminPort, config.Timeout)
	responseTime := time.Since(startTime).Milliseconds()

	result.IsOnline = isOnline
	result.ResponseTime = responseTime

	if !isOnline {
		result.Error = err.Error()
		return result
	}

	// 如果启用API检查，进一步验证
	if config.CheckAPI && node.AdminPass != "" {
		if verbose {
			fmt.Printf("  → 检查API接口...\n")
		}
		apiOnline, apiErr := checkAPIStatus(node, config.Timeout)
		if !apiOnline {
			result.IsOnline = false
			result.Error = fmt.Sprintf("API检查失败: %v", apiErr)
		} else if verbose {
			fmt.Printf("  ✓ API接口正常\n")
		}
	}

	return result
}

// 检查TCP连接
func checkTCPConnection(ip string, port int, timeout int) (bool, error) {
	address := fmt.Sprintf("%s:%d", ip, port)
	conn, err := net.DialTimeout("tcp", address, time.Duration(timeout)*time.Second)
	if err != nil {
		return false, err
	}
	defer conn.Close()
	return true, nil
}

// 检查API状态
func checkAPIStatus(node Node, timeout int) (bool, error) {
	url := fmt.Sprintf("http://admin:%s@%s:%d/api/status",
		node.AdminPass, node.IP, node.AdminPort)

	client := &http.Client{
		Timeout: time.Duration(timeout) * time.Second,
	}

	resp, err := client.Get(url)
	if err != nil {
		return false, err
	}
	defer resp.Body.Close()

	if resp.StatusCode == 200 {
		return true, nil
	}

	return false, fmt.Errorf("HTTP状态码: %d", resp.StatusCode)
}

// 更新节点状态
func updateNodeStatus(db *sql.DB, nodeID int, status string) error {
	query := "UPDATE nodes SET status = ? WHERE id = ?"
	_, err := db.Exec(query, status, nodeID)
	return err
}

// 打印汇总信息
func printSummary(results []CheckResult) {
	if len(results) == 0 {
		return
	}

	fmt.Println("========================================")
	fmt.Println("检查结果汇总")
	fmt.Println("========================================")

	onlineCount := 0
	offlineCount := 0
	totalResponseTime := int64(0)

	for _, result := range results {
		if result.IsOnline {
			onlineCount++
			totalResponseTime += result.ResponseTime
		} else {
			offlineCount++
		}
	}

	fmt.Printf("总节点数: %d\n", len(results))
	fmt.Printf("在线节点: %d (%.1f%%)\n", onlineCount,
		float64(onlineCount)/float64(len(results))*100)
	fmt.Printf("离线节点: %d (%.1f%%)\n", offlineCount,
		float64(offlineCount)/float64(len(results))*100)

	if onlineCount > 0 {
		avgResponseTime := totalResponseTime / int64(onlineCount)
		fmt.Printf("平均响应时间: %dms\n", avgResponseTime)
	}

	// 显示离线节点详情
	if offlineCount > 0 {
		fmt.Println("\n离线节点详情:")
		for _, result := range results {
			if !result.IsOnline {
				fmt.Printf("  • %s (ID:%d) - %s\n",
					result.NodeName, result.NodeID, result.Error)
			}
		}
	}

	fmt.Println("========================================")
}
