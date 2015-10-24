Ping-Pong benchmark for Orleans
=========================================

Pre-requisites: Installed OrleansSDK, VS2013

1. Build solution
2. Start local silo by running $(OrleansSDK)\SDK\StartLocalSilo.cmd
3. Start benchmark by running $(OrleansSDK)\SDK\LocalSilo\Orleans.PingPong.exe
4. Enter number of clients (total actor count will be times 2) and number of repeated pings (total number of messages will be times 2)

Current results so far
-------------------------
Running on workstation with quad-core SandyBridge i2600K (3401Mhz) with 8GB of RAM, Win7 SP1.
- Actor Count: 1024
- Total: 2,048,000.00 messages
- Time: 15.86 sec
- TPS: 129,161 per/sec

> No tuning was done, default local silo settings were used.

More [realistic](https://gitter.im/dotnet/orleans?at=55b11d166e982043058b0dde) benchmarks, done by Orleans team, has shown 10K-56K of TPS, on 6 years old, 8-core server. Compared to [recently](https://github.com/akkadotnet/akka.net/issues/1355) discovered performance of [327 msg/sec](https://github.com/akkadotnet/akka.net/issues/1355#issuecomment-145510978) for synchronous request-response in Akka(.Net), the Orleans' TPS is absolutely incredible.
