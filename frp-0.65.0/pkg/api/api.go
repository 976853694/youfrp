package api

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"net/http"
	"strconv"
	"time"

	v1 "github.com/fatedier/frp/pkg/config/v1"
	"github.com/fatedier/frp/pkg/config/types"
	"github.com/fatedier/frp/pkg/msg"
)

// 封装 GET 请求函数
func HttpGet(url string) (string, error) {
	client := &http.Client{
		Timeout: 10 * time.Second, // 设置超时时间
	}

	// 创建 GET 请求
	resp, err := client.Get(url)
	if err != nil {
		return "", err
	}
	defer resp.Body.Close()

	// 读取响应体
	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return "", err
	}

	return string(body), nil
}

// 封装 POST 请求函数（发送 JSON 数据）
func HttpPost(url string, data map[string]interface{}) (string, error) {
	client := &http.Client{
		Timeout: 10 * time.Second, // 设置超时时间
	}

	// 将请求数据编码为 JSON 格式
	jsonData, err := json.Marshal(data)
	if err != nil {
		return "", err
	}

	// 创建 POST 请求
	resp, err := client.Post(url, "application/json", bytes.NewBuffer(jsonData))
	if err != nil {
		return "", err
	}
	defer resp.Body.Close()

	// 读取响应体
	body, err := ioutil.ReadAll(resp.Body)
	if err != nil {
		return "", err
	}

	return string(body), nil
}

// 定义将要解析到的结构体
type CheckTokenResp struct {
	Status  int  `json:"status"`
	Success bool `json:"success"`
}

type Response struct {
	Status  int    `json:"status"`
	Success bool   `json:"success"`
	Message string `json:"message"`
}

// API 返回的限速信息结构体 (sakurafrp 格式) - 字符串版本
type LimitInfoStr struct {
	Inbound  string `json:"inbound"`
	Outbound string `json:"outbound"`
	Type     int    `json:"type"`
}

// API 返回的限速信息结构体 (sakurafrp 格式) - 整数版本
type LimitInfoInt struct {
	Inbound  int `json:"inbound"`
	Outbound int `json:"outbound"`
	Type     int `json:"type"`
}

// CheckToken 校验客户端 token   user=client中的token
func CheckToken(apiurl string, user string, John_San_Token string) (ok bool, err error) {

	var CheckToken_url string = apiurl + "?action=checktoken&user=" + user + "&apitoken=" + John_San_Token
	fmt.Println("John_San_检查客户端登录 ", CheckToken_url)
	resp, err := HttpGet(CheckToken_url)
	if err != nil {
		return false, err
	}
	// 将要解析到的变量
	var r CheckTokenResp

	// 解析JSON字符串到结构体
	err = json.Unmarshal([]byte(resp), &r)
	if err != nil {
		fmt.Println("解析checktoken失败", resp, err)
		return false, err
	}
	fmt.Println("api调用结束", resp)
	return r.Success == true, nil
}

// CheckProxy 校验客户端代理
func CheckProxy(apiurl string, user string, pxyConf v1.ProxyConfigurer, pMsg *msg.NewProxy, John_San_Token string) (ok bool, err error) {
	// fmt.Println("下面是 pxyConf")
	// util.PrintHexAndDecFields(pxyConf)
	// fmt.Println("下面是 pxyConf.GetBaseConfig()")
	// util.PrintHexAndDecFields(pxyConf.GetBaseConfig())
	// fmt.Println("下面是 pMsg")
	// util.PrintHexAndDecFields(pMsg)

	// fmt.Println("结束",len(pMsg.CustomDomains),len(pMsg.SubDomain))

	var geturl string = "?action=checkproxy&user=" + user +
		"&apitoken=" + John_San_Token +
		"&proxy_name=" + pxyConf.GetBaseConfig().Name +
		"&proxy_type=" + pxyConf.GetBaseConfig().Type

	//自定义域名
	if len(pMsg.CustomDomains) > 0 {
		geturl = geturl + "&customdomains=" + string(pMsg.CustomDomains[0])
	}
	//自定义二级域名
	if len(pMsg.SubDomain) > 0 {
		geturl = geturl + "&subdomain=" + string(pMsg.SubDomain)
	}
	//远程端口 可能为空？
	remotp := strconv.Itoa(pMsg.RemotePort)
	if remotp != "" && remotp != "0" {
		geturl = geturl + "&remote_port=" + remotp
	}
	fmt.Println("apiurl=", apiurl+geturl)

	//http://127.0.0.1:7899/api?action=checkproxy&user=46414e6574f2969e&apitoken=wobushitoken9527|1&proxy_name=46414e6574f2969e.MyProxy&proxy_type=tcp&remote_port=46008
	//http://127.0.0.1:7899/api?action=checkproxy&user=46414e6574f2969e&apitoken=wobushitoken9527|1&proxy_name=46414e6574f2969e.test_http&proxy_type=http&customdomains=aaaa.aaa.comi&remote_port=0
	resp, err := HttpGet(apiurl + geturl)
	fmt.Println("CheckProxy 代理检查", resp, err)
	if err != nil {
		fmt.Println("CheckProxy Error:", resp, err)
		return false, err
	}
	var response Response
	err = json.Unmarshal([]byte(resp), &response)
	if err != nil {
		fmt.Println("CheckProxy decode Error:", resp, err)
		return false, err
	}
	if response.Success != true {
		fmt.Println("CheckProxy Error:", resp, err)
		return false, err
	}

	// 解析 message 字段中的限速信息（这是一个 JSON 字符串，需要二次解析）
	// 首先尝试解析为字符串格式
	var limitInfoStr LimitInfoStr
	err = json.Unmarshal([]byte(response.Message), &limitInfoStr)
	if err == nil {
		// 成功解析为字符串格式
		fmt.Printf("解析到的限速信息(字符串格式): Status=%d, Inbound=%s, Outbound=%s, Type=%d\n", response.Status, limitInfoStr.Inbound, limitInfoStr.Outbound, limitInfoStr.Type)
		
		// 转换字符串为整数
		inboundInt := 0
		outboundInt := 0
		
		if limitInfoStr.Inbound != "" {
			inboundInt, err = strconv.Atoi(limitInfoStr.Inbound)
			if err != nil {
				fmt.Println("Failed to convert inbound to integer:", err)
				inboundInt = 0
			}
		}
		
		if limitInfoStr.Outbound != "" {
			outboundInt, err = strconv.Atoi(limitInfoStr.Outbound)
			if err != nil {
				fmt.Println("Failed to convert outbound to integer:", err)
				outboundInt = 0
			}
		}
		
		fmt.Printf("John_San_解析到的限速信息: 入站=%d KB, 出站=%d KB\n", inboundInt, outboundInt)
		
		// 设置带宽限制模式为服务器端限制
		pxyConf.GetBaseConfig().Transport.BandwidthLimitMode = "server"
		
		// 设置出站带宽限制
		if outboundInt > 0 {
			outboundWithKB := strconv.Itoa(outboundInt) + "KB"
			bandwidthLimit, err := types.NewBandwidthQuantity(outboundWithKB)
			if err != nil {
				fmt.Println("Failed to create outbound bandwidth limit:", err)
				return false, err
			}
			pxyConf.GetBaseConfig().Transport.BandwidthLimit = bandwidthLimit
			fmt.Println("John_San_设置出站限速:", pxyConf.GetBaseConfig().Transport.BandwidthLimit)
		} else {
			// 如果出站限制为0或负数，则不设置限制
			fmt.Println("John_San_出站不限速")
		}
		
		// 注意：FRP标准实现出站限速，入站限速需要额外实现
		if inboundInt > 0 {
			fmt.Printf("John_San_注意：入站限速(%d KB)已指定，但FRP标准实现暂不支持\n", inboundInt)
		}
	} else {
		// 如果字符串格式解析失败，尝试解析为整数格式
		var limitInfoInt LimitInfoInt
		err = json.Unmarshal([]byte(response.Message), &limitInfoInt)
		if err != nil {
			fmt.Println("CheckProxy decode_message Error:", resp, err)
			return false, err
		}
		
		fmt.Printf("解析到的限速信息(整数格式): Status=%d, Inbound=%d, Outbound=%d, Type=%d\n", response.Status, limitInfoInt.Inbound, limitInfoInt.Outbound, limitInfoInt.Type)
		
		fmt.Printf("John_San_解析到的限速信息: 入站=%d KB, 出站=%d KB\n", limitInfoInt.Inbound, limitInfoInt.Outbound)
		
		// 设置带宽限制模式为服务器端限制
		pxyConf.GetBaseConfig().Transport.BandwidthLimitMode = "server"
		
		// 设置出站带宽限制
		if limitInfoInt.Outbound > 0 {
			outboundWithKB := strconv.Itoa(limitInfoInt.Outbound) + "KB"
			bandwidthLimit, err := types.NewBandwidthQuantity(outboundWithKB)
			if err != nil {
				fmt.Println("Failed to create outbound bandwidth limit:", err)
				return false, err
			}
			pxyConf.GetBaseConfig().Transport.BandwidthLimit = bandwidthLimit
			fmt.Println("John_San_设置出站限速:", pxyConf.GetBaseConfig().Transport.BandwidthLimit)
		} else {
			// 如果出站限制为0或负数，则不设置限制
			fmt.Println("John_San_出站不限速")
		}
		
		// 注意：FRP标准实现出站限速，入站限速需要额外实现
		if limitInfoInt.Inbound > 0 {
			fmt.Printf("John_San_注意：入站限速(%d KB)已指定，但FRP标准实现暂不支持\n", limitInfoInt.Inbound)
		}
	}

	return true, nil
}