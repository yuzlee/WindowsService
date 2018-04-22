using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using DasMulli.Win32.ServiceUtils;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace WindowsService.service {
    class ServiceHost : IWin32Service {
        private Win32ServiceManager Manager => new Win32ServiceManager();

        public string ServiceName => Name;

        public string ServiceDescription => "User-Defined Service Host";

        public static string Name => "User-Defined Service Host";

        public static string RunAsServiceHostFlag => "-run-as-service-host";

        public static string IpcChannelName => "user-defined-service-host";

        public static string IpcUrl => $"ipc://{IpcChannelName}/docker";

        //private static ServiceDocker Docker { get; } = new ServiceDocker();

        private IpcServerChannel IpcServerChannel;

        public ServiceHost() {
        }

        public void Start(string[] startupAruments, ServiceStoppedCallback serviceStoppedCallback) {
            try {
                // Register an IPC channel
                IpcServerChannel = new IpcServerChannel(IpcChannelName);
                ChannelServices.RegisterChannel(IpcServerChannel, false);

                // Expose an object
                RemotingConfiguration.RegisterWellKnownServiceType(typeof(ServiceDocker), "docker", WellKnownObjectMode.Singleton);
            } catch (Exception) {
                serviceStoppedCallback();
            }
        }

        public void Stop() {
            ChannelServices.UnregisterChannel(IpcServerChannel);
        }

        public void Register(Action<ServiceMessage> error) {
            try {
                var cmd = $"{Process.GetCurrentProcess().MainModule.FileName} {RunAsServiceHostFlag}";
                var serviceDefinition = new ServiceDefinitionBuilder(ServiceName)
                    .WithDisplayName(ServiceName)
                    .WithDescription(ServiceDescription)
                    .WithBinaryPath(cmd)
                    .WithCredentials(Win32ServiceCredentials.NetworkService)
                    .WithAutoStart(true)
                    .Build();
                this.Manager.CreateOrUpdateService(serviceDefinition, startImmediately: true);
            } catch (Exception e) {
                ServiceMessage.Send(error, $"Register: {e.Message}");
            }
        }

        public void Unregister(Action<ServiceMessage> error) {
            try {
                this.Manager.DeleteService(ServiceName);
            } catch (Exception e) {
                ServiceMessage.Send(error, $"Unregister: {e.Message}");
            }
        }
    }
}
