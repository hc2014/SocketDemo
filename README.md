SocketDemo
==========

SocketDemo
**c#-Socket 代码**
这个代码也是2012年写的，实现了服务端和多客户端的数据通信，数据按1024个长度分包发送的，当时对这些东西完全不懂跌跌撞撞的花了个把月才
整出这么一个小demo。

代码可以直接使用，分为server和client 两个文件夹. 都是用控制台写的. server这一端没什么问题，直接f5运行就行了。然后再来启动client
 
启动client的时候 先要修改一下代码

**在client的Program.cs的main 函数的地方需要配置一下服务器的ip地址(本地测试的话直接用本地ip就行了)
如果是想在服务器上测试的话,那么开始就要把server 拿到该台服务器上运行.
如果没有安装vs2012的话 直接运行Bin文件下的server.exe 和client.exe也是可以的**
