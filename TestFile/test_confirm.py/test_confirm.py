from pythonosc.udp_client import SimpleUDPClient
import time

# 替换为你手机在局域网中的 IP 地址
PHONE_IP = "172.20.10.8"
PHONE_PORT = 9000

client = SimpleUDPClient(PHONE_IP, PHONE_PORT)

# 发送一次 /confirm，值为 1.0
print(f"📡 向 {PHONE_IP}:{PHONE_PORT} 发送 /confirm")
client.send_message("/confirm", 1.0)
