using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService.service {
    [Serializable]
    class ServiceStatus {
        public int Id { get; set; }

        public ServiceState State { get; set; }

        public DateTime StartTime { get; set; }

        public ServiceInfo Info { get; set; }

        public ServiceStatus() {

        }

        public override string ToString() {
            return $"[{Id}] {State} {Info} {StartTime.ToShortTimeString()}";
        }
    }
}
