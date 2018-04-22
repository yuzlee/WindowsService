using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsService.service;
using DasMulli.Win32.ServiceUtils;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.ServiceProcess;

namespace WindowsService {
    class Program {
        static string ServicesConfigPath = @"D:/Workspace/Experiment/WindowsService/WindowsService/services.xml";

        static void Main(string[] args) {
            if (args.Length == 1) {
                if (args[0] == ServiceHost.RunAsServiceHostFlag) {
                    RunAsServiceHost();
                } else if(args[0] != "status") {
                    ServiceHostOperation(args[0]);
                }
            } else if (args.Length > 1) {
                UserDefinedServiceOperation(args.ToList());
            }
        }

        static void ServiceHostOperation(string method) {
            if (method == "start") {
                Console.WriteLine("start service host");
                StartServiceHost();
            } else if (method == "stop") {
                StopServiceHost();
            } else {
                Console.WriteLine("invalid arguments");
            }
        }

        static void RunAsServiceHost() {
            try {
                new Win32ServiceHost(new ServiceHost()).Run();
            } catch (Exception e) {
                Console.WriteLine($"RunAsServiceHost: {e.Message}");
            }
        }

        static void StartServiceHost(bool use_shell = false) {
            if (use_shell) {
                var host = new ServiceHost();
                host.Start(new string[0], () => {
                    Console.WriteLine("service has stopped");
                });
                Console.ReadLine();
                return;
            }
            new ServiceHost().Register((e) => Console.WriteLine(e));
        }

        static void StopServiceHost() {
            // new ServiceHost().Unregister((e) => Console.WriteLine(e));
            try {
                var controller = new ServiceController(ServiceHost.Name);
                controller.Stop();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        static void UserDefinedServiceOperation(List<string> args) {
            IpcClientChannel clientChannel = new IpcClientChannel();

            try {
                ChannelServices.RegisterChannel(clientChannel, false);
                RemotingConfiguration.RegisterWellKnownClientType(typeof(ServiceDocker), ServiceHost.IpcUrl);
            } catch (RemotingException) {
                println("can not connect to the service host");
                try {
                    ChannelServices.UnregisterChannel(clientChannel);
                } catch (Exception) { }
                return;
            }

            var docker = new ServiceDocker();

            if (args[0] == "status" && args.Count == 1) { // user defined services status list
                Console.WriteLine(docker.List());
                return;
            }

            var name = args[0]; args.RemoveAt(0);
            var method = args[0]; args.RemoveAt(0);

            ServiceMessage message = null;
            if (method == "start") {
                if (!docker.Exists(name)) {
                    var info = ServiceInfo.FindFromXMLConfig(ServicesConfigPath, name);
                    Console.WriteLine(info);
                    if (info != null) {
                        docker.Install(info, out message);
                        println($"add service: {name}");
                    } else {
                        println($"can not find definition for {name}");
                        return;
                    }
                }
                docker.Start(name, out message);
            } else if (method == "stop") {
                docker.Stop(name, out message);
            } else if (method == "restart") {
                docker.Restart(name, out message);
            } else if (method == "status") {
                println(docker.Status(name, out message));
            }
            println(message);
        }

        static void println(object o) {
            if (o != null) {
                Console.WriteLine(o);
            }
        }
    }
}
