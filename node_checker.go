package main

import (
	"database/sql"
	"flag"
	"fmt"
	"log"
	"net"
	"time"

	_ "github.com/go-sql-driver/mysql"
)

// 配置信息
type Config struct {
	DBHost     string
	DBUser     string
	DBPassword string
	DBName     string
	Timeout    int // 连接超时时间（秒）
	Interval   int // 检查间隔（秒）
}

// 节点信息
type Node struct {
	ID        int
	Name      string
	IP        string
	AdminPort int
	Status    string
}

func main() {
	// 解析命令行参数
	timeout := flag.Int("timeout", 5, "连接超时时间（秒）")
	interval := flag.Int("interval", 60, "检查间隔（秒），0表示只检查一次")
	flag.Parse()

	// 加载配置
	config := Config{
		DBHost:     "frpapi.gmns.top:3306",
		DBUser:     "youfrp",
		DBPassword: "tgx123456.",
		DBName:     "youfrp",
		Timeout:    *timeout,
		Interval:   *interval,
	}

	// 连接数据库
	db, err := connectDB(config)
	if err != nil {
		log.Fatalf("连接数据库失败: %v", err)
	}
	defer db.Close()

	fmt.Println("节点在线状态检查程序已启动")
	fmt.Printf("超时时间: %d秒\n", config.Timeout)
	if config.Interval > 0 {
		fmt.Printf("检查间隔: %d秒\n", config.Interval)
	} else {
		fmt.Println("模式: 单次检查")
	}
	fmt.Println("----------------------------------------")

	// 执行检查
	if config.Interval > 0 {
		// 循环检查模式
		for {
			checkAllNodes(db, config)
			fmt.Printf("\n等待 %d 秒后进行下一次检查...\n\n", config.Interval)
			time.Sleep(time.Duration(config.Interval) * time.Second)
		}
	} else {
		// 单次检查模式
		checkAllNodes(db, config)
		fmt.Println("\n检查完成")
	}
}

// 连接数据库
func connectDB(config Config) (*sql.DB, error) {
	dsn := fmt.Sprintf("%s:%s@tcp(%s)/%s?charset=utf8mb4&parseTime=True",
		config.DBUser, config.DBPassword, config.DBHost, config.DBName)

	db, err := sql.Open("mysql", dsn)
	if err != nil {
		return nil, err
	}

	// 测试连接
	err = db.Ping()
	if err != nil {
		return nil, err
	}

	fmt.Println("数据库连接成功")
	return db, nil
}

// 检查所有节点
func checkAllNodes(db *sql.DB, config Config) {
	nodes, err := getAllNodes(db)
	if err != nil {
		log.Printf("获取节点列表失败: %v", err)
		return
	}

	if len(nodes) == 0 {
		fmt.Println("没有找到任何节点")
		return
	}

	fmt.Printf("开始检查 %d 个节点...\n", len(nodes))
	fmt.Println("----------------------------------------")

	onlineCount := 0
	offlineCount := 0

	for _, node := range nodes {
		isOnline := checkNodeStatus(node, config.Timeout)

		// 确定新状态
		newStatus := "500" // 离线
		if isOnline {
			newStatus = "200" // 在线
			onlineCount++
		} else {
			offlineCount++
		}

		// 更新数据库
		err := updateNodeStatus(db, node.ID, newStatus)
		if err != nil {
			log.Printf("更新节点 %s (ID:%d) 状态失败: %v", node.Name, node.ID, err)
		}

		// 输出结果
		statusText := "离线"
		statusSymbol := "✗"
		if isOnline {
			statusText = "在线"
			statusSymbol = "✓"
		}

		fmt.Printf("[%s] ID:%d | 名称:%s | 地址:%s:%d | 状态:%s\n",
			statusSymbol, node.ID, node.Name, node.IP, node.AdminPort, statusText)
	}

	fmt.Println("----------------------------------------")
	fmt.Printf("检查完成 - 在线: %d | 离线: %d | 总计: %d\n",
		onlineCount, offlineCount, len(nodes))
}

// 获取所有节点
func getAllNodes(db *sql.DB) ([]Node, error) {
	nodes := []Node{}
	query := "SELECT id, name, ip, admin_port, status FROM nodes"

	rows, err := db.Query(query)
	if err != nil {
		return nodes, err
	}
	defer rows.Close()

	for rows.Next() {
		var node Node
		var adminPort sql.NullInt64

		err := rows.Scan(&node.ID, &node.Name, &node.IP, &adminPort, &node.Status)
		if err != nil {
			log.Printf("扫描节点行失败: %v", err)
			continue
		}

		// 处理可能为NULL的admin_port
		if adminPort.Valid {
			node.AdminPort = int(adminPort.Int64)
		} else {
			node.AdminPort = 7000 // 默认端口
		}

		nodes = append(nodes, node)
	}

	return nodes, nil
}

// 检查节点状态
func checkNodeStatus(node Node, timeout int) bool {
	address := fmt.Sprintf("%s:%d", node.IP, node.AdminPort)

	conn, err := net.DialTimeout("tcp", address, time.Duration(timeout)*time.Second)
	if err != nil {
		return false
	}
	defer conn.Close()

	return true
}

// 更新节点状态
func updateNodeStatus(db *sql.DB, nodeID int, status string) error {
	query := "UPDATE nodes SET status = ? WHERE id = ?"
	_, err := db.Exec(query, status, nodeID)
	return err
}
