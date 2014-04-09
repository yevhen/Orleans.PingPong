Ping-Pong benchmark for Orleans
=========================================

Pre-requisites: Installed OrleansSDK, VS2013

1. Build solution
2. Start local silo by running $(OrleansSDK)\SDK\StartLocalSilo.cmd
3. Start benchmark by running $(OrleansSDK)\SDK\LocalSilo\Orleans.PingPong.exe
4. Enter number of clients (total actor count will be times 2) and number of repeated pings (total number of messages will be times 2)

Current results so far :(
-------------------------
OSVersion: Microsoft Windows NT 6.1.7601 Service Pack 1
ProcessorCount: 8
ClockSpeed: 3401 MHZ

Actor Count: 1024
Total: 1,024,000.00 messages
Time: 15.63 sec
TPS: 65,497 per/sec

* Running on workstation with SandyBridge i2600K with 8GB of RAM