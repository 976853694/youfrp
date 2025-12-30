package main

import (
	"crypto/md5"
	"encoding/hex"
	"flag"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"time"
)

// 配置
type Config struct {
	SourceDir  string // 源文件目录
	OutputDir  string // 输出目录
	CSSDir     string // CSS保存目录
	JSDir      string // JS保存目录
	Timeout    int    // 下载超时时间（秒）
	MaxRetries int    // 最大重试次数
}

// 资源信息
type Asset struct {
	URL       string // 原始URL
	LocalPath string // 本地路径
	Type      string // 类型：css 或 js
}

func main() {
	// 命令行参数
	sourceDir := flag.String("source", ".", "源文件目录")
	outputDir := flag.String("output", "localized", "输出目录")
	cssDir := flag.String("css-dir", "css", "CSS保存目录名")
	jsDir := flag.String("js-dir", "js", "JS保存目录名")
	timeout := flag.Int("timeout", 30, "下载超时时间（秒）")
	maxRetries := flag.Int("retries", 3, "最大重试次数")
	flag.Parse()

	config := Config{
		SourceDir:  *sourceDir,
		OutputDir:  *outputDir,
		CSSDir:     *cssDir,
		JSDir:      *jsDir,
		Timeout:    *timeout,
		MaxRetries: *maxRetries,
	}

	printHeader()

	// 创建输出目录
	if err := createDirectories(config); err != nil {
		fmt.Printf("❌ 创建目录失败: %v\n", err)
		return
	}

	// 处理所有HTML/PHP文件
	files, err := findHTMLFiles(config.SourceDir)
	if err != nil {
		fmt.Printf("❌ 查找文件失败: %v\n", err)
		return
	}

	if len(files) == 0 {
		fmt.Println("⚠️  没有找到HTML/PHP文件")
		return
	}

	fmt.Printf("📁 找到 %d 个文件\n\n", len(files))

	// 收集所有资源
	allAssets := make(map[string]*Asset)

	for _, file := range files {
		fmt.Printf("🔍 分析文件: %s\n", file)
		assets, err := extractAssets(file)
		if err != nil {
			fmt.Printf("  ⚠️  解析失败: %v\n", err)
			continue
		}

		for url, asset := range assets {
			if _, exists := allAssets[url]; !exists {
				allAssets[url] = asset
			}
		}
		fmt.Printf("  ✓ 找到 %d 个资源\n", len(assets))
	}

	if len(allAssets) == 0 {
		fmt.Println("\n⚠️  没有找到需要下载的资源")
		return
	}

	fmt.Printf("\n📦 共需下载 %d 个资源\n", len(allAssets))
	fmt.Println("========================================\n")

	// 下载所有资源
	successCount := 0
	failCount := 0
	i := 0

	for _, asset := range allAssets {
		i++
		fmt.Printf("[%d/%d] 下载: %s\n", i, len(allAssets), asset.URL)

		localPath := filepath.Join(config.OutputDir, asset.LocalPath)
		err := downloadFile(asset.URL, localPath, config)

		if err != nil {
			fmt.Printf("  ❌ 失败: %v\n", err)
			failCount++
		} else {
			fmt.Printf("  ✓ 保存到: %s\n", asset.LocalPath)
			successCount++
		}
	}

	fmt.Println("\n========================================")
	fmt.Printf("下载完成: 成功 %d | 失败 %d\n", successCount, failCount)
	fmt.Println("========================================\n")

	// 处理所有文件，替换URL
	fmt.Println("🔄 开始替换文件中的URL...\n")

	for _, file := range files {
		relPath, _ := filepath.Rel(config.SourceDir, file)
		outputFile := filepath.Join(config.OutputDir, relPath)

		fmt.Printf("📝 处理: %s\n", relPath)

		err := processFile(file, outputFile, allAssets)
		if err != nil {
			fmt.Printf("  ❌ 失败: %v\n", err)
		} else {
			fmt.Printf("  ✓ 已保存\n")
		}
	}

	fmt.Println("\n✅ 全部完成！")
	fmt.Printf("📂 输出目录: %s\n", config.OutputDir)
}

// 打印头部信息
func printHeader() {
	fmt.Println("========================================")
	fmt.Println("    网站资源本地化工具")
	fmt.Println("========================================")
	fmt.Println()
}

// 创建必要的目录
func createDirectories(config Config) error {
	dirs := []string{
		config.OutputDir,
		filepath.Join(config.OutputDir, config.CSSDir),
		filepath.Join(config.OutputDir, config.JSDir),
	}

	for _, dir := range dirs {
		if err := os.MkdirAll(dir, 0755); err != nil {
			return err
		}
	}

	return nil
}

// 查找所有HTML/PHP文件
func findHTMLFiles(dir string) ([]string, error) {
	var files []string

	err := filepath.Walk(dir, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}

		if info.IsDir() {
			return nil
		}

		ext := strings.ToLower(filepath.Ext(path))
		if ext == ".html" || ext == ".htm" || ext == ".php" {
			files = append(files, path)
		}

		return nil
	})

	return files, err
}

// 提取文件中的CSS和JS资源
func extractAssets(filePath string) (map[string]*Asset, error) {
	content, err := os.ReadFile(filePath)
	if err != nil {
		return nil, err
	}

	assets := make(map[string]*Asset)
	text := string(content)

	// 匹配CSS链接
	cssPattern := regexp.MustCompile(`<link[^>]+href=["']([^"']+\.css[^"']*)["'][^>]*>`)
	cssMatches := cssPattern.FindAllStringSubmatch(text, -1)

	for _, match := range cssMatches {
		if len(match) > 1 {
			url := match[1]
			if isRemoteURL(url) {
				asset := &Asset{
					URL:       url,
					LocalPath: generateLocalPath(url, "css"),
					Type:      "css",
				}
				assets[url] = asset
			}
		}
	}

	// 匹配JS脚本
	jsPattern := regexp.MustCompile(`<script[^>]+src=["']([^"']+\.js[^"']*)["'][^>]*>`)
	jsMatches := jsPattern.FindAllStringSubmatch(text, -1)

	for _, match := range jsMatches {
		if len(match) > 1 {
			url := match[1]
			if isRemoteURL(url) {
				asset := &Asset{
					URL:       url,
					LocalPath: generateLocalPath(url, "js"),
					Type:      "js",
				}
				assets[url] = asset
			}
		}
	}

	return assets, nil
}

// 判断是否为远程URL
func isRemoteURL(urlStr string) bool {
	return strings.HasPrefix(urlStr, "http://") || strings.HasPrefix(urlStr, "https://") || strings.HasPrefix(urlStr, "//")
}

// 生成本地路径
func generateLocalPath(urlStr string, assetType string) string {
	// 处理 // 开头的URL
	if strings.HasPrefix(urlStr, "//") {
		urlStr = "https:" + urlStr
	}

	parsedURL, err := url.Parse(urlStr)
	if err != nil {
		// 如果解析失败，使用MD5生成文件名
		hash := md5.Sum([]byte(urlStr))
		filename := hex.EncodeToString(hash[:]) + "." + assetType
		return filepath.Join(assetType, filename)
	}

	// 获取文件名
	filename := filepath.Base(parsedURL.Path)

	// 如果文件名为空或不包含扩展名，生成一个
	if filename == "" || filename == "." || !strings.Contains(filename, ".") {
		hash := md5.Sum([]byte(urlStr))
		filename = hex.EncodeToString(hash[:8]) + "." + assetType
	}

	// 清理文件名中的查询参数
	if strings.Contains(filename, "?") {
		parts := strings.Split(filename, "?")
		filename = parts[0]
	}

	return filepath.Join(assetType, filename)
}

// 下载文件
func downloadFile(urlStr string, localPath string, config Config) error {
	// 处理 // 开头的URL
	if strings.HasPrefix(urlStr, "//") {
		urlStr = "https:" + urlStr
	}

	// 创建目录
	dir := filepath.Dir(localPath)
	if err := os.MkdirAll(dir, 0755); err != nil {
		return err
	}

	// 如果文件已存在，跳过
	if _, err := os.Stat(localPath); err == nil {
		return nil
	}

	// 重试下载
	var lastErr error
	for i := 0; i < config.MaxRetries; i++ {
		if i > 0 {
			fmt.Printf("  🔄 重试 %d/%d...\n", i, config.MaxRetries)
			time.Sleep(time.Second * 2)
		}

		client := &http.Client{
			Timeout: time.Duration(config.Timeout) * time.Second,
		}

		resp, err := client.Get(urlStr)
		if err != nil {
			lastErr = err
			continue
		}
		defer resp.Body.Close()

		if resp.StatusCode != 200 {
			lastErr = fmt.Errorf("HTTP %d", resp.StatusCode)
			continue
		}

		// 创建文件
		out, err := os.Create(localPath)
		if err != nil {
			lastErr = err
			continue
		}
		defer out.Close()

		// 写入文件
		_, err = io.Copy(out, resp.Body)
		if err != nil {
			lastErr = err
			os.Remove(localPath)
			continue
		}

		return nil
	}

	return lastErr
}

// 处理文件，替换URL
func processFile(inputPath string, outputPath string, assets map[string]*Asset) error {
	content, err := os.ReadFile(inputPath)
	if err != nil {
		return err
	}

	text := string(content)

	// 替换所有资源URL
	for originalURL, asset := range assets {
		// 转换为相对路径
		localURL := "/" + strings.ReplaceAll(asset.LocalPath, "\\", "/")
		text = strings.ReplaceAll(text, originalURL, localURL)
	}

	// 创建输出目录
	dir := filepath.Dir(outputPath)
	if err := os.MkdirAll(dir, 0755); err != nil {
		return err
	}

	// 写入文件
	return os.WriteFile(outputPath, []byte(text), 0644)
}
