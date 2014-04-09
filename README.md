Ping-Pong benchmark for Orleans
=========================================

Pre-requisites: Installed OrleansSDK, VS2013

1. Build solution
2. Start local silo by running $(OrleansSDK)\SDK\StartLocalSilo.cmd
3. Start benchmark by running $(OrleansSDK)\SDK\LocalSilo\Orleans.PingPong.exe
4. Enter number of clients (total actor count will be times 2) and number of repeated pings (total number of messages will be times 2)

Current results so far
-------------------------
- Actor Count: 1024
- Total: 1,024,000.00 messages
- Time: 15.63 sec
- TPS: 65,497 per/sec

> Running on workstation with quad-core SandyBridge i2600K (3401Mhz) with 8GB of RAM, Win7 SP1
> No tuning was done, default local silo settings were used

NOTE: Akka.net uses local actors, while Orleans always expect distribution. So 10M messages per/sec between local actors in Akka.net are not comparable to 65K messages per/s in Orleans. 
