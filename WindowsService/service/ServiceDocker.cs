using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService.service {
    class ServiceDocker : MarshalByRefObject {
        public string HostArgs { get; set; }

        private Dictionary<string, ServiceHanlder> Services { get; } = new Dictionary<string, ServiceHanlder>();

        public ServiceDocker() {
        }

        public string List() {
            return string.Join("\n", this.Services.Select(service => $"{service.Value.Name} {service.Value.Id} {service.Value.State}").ToList());
        }

        public bool Exists(ServiceHanlder service) {
            return this.Services.ContainsKey(service.Name);
        }

        public bool Exists(string name) {
            return this.Services.ContainsKey(name);
        }

        public void Install(ServiceInfo info, out ServiceMessage message) {
            if (this.Exists(info.Name)) {
                message = new ServiceMessage($"[{info.Name}] Add: conflict service name");
                return;
            }
            this.Services.Add(info.Name, new ServiceHanlder(info));
            message = null;
        }

        public void Uninstall(string name, out ServiceMessage message) {
            if (!this.Exists(name)) {
                message = new ServiceMessage($"[{name}] Remove: invalid service name");
                return;
            }
            this.Services[name].Stop(out message);
            this.Services.Remove(name);
        }

        public void Start(string name, out ServiceMessage message) {
            if (!this.Exists(name)) {
                message = new ServiceMessage($"[{name}] Start: invalid service name");
                return;
            }
            this.Services[name].Start(out message);
        }

        public void Stop(string name, out ServiceMessage message) {
            if (!this.Exists(name)) {
                message = new ServiceMessage($"[{name}] Stop: invalid service name");
                return;
            }
            this.Services[name].Stop(out message);
        }

        public void Restart(string name, out ServiceMessage message) {
            if (!this.Exists(name)) {
                message = new ServiceMessage($"[{name}] Restart: invalid service name");
                return;
            }
            this.Services[name].Restart(out message);
        }

        public ServiceStatus Status(string name, out ServiceMessage message) {
            if (this.Exists(name)) {
                message = null;
                return this.Services[name].Status;
            }
            message = new ServiceMessage($"[{name}] Status: invalid service name");
            return null;
        }
    }
}
