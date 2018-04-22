using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsService {
    [Serializable]
    class ServiceMessage {
        public List<string> MessageList { get; } = new List<string>();

        public ServiceMessage() {
        }

        public ServiceMessage(string message) {
            this.MessageList.Add(message);
        }

        public static void Send(Action<ServiceMessage> actor, string message) {
            actor(new ServiceMessage(message));
        }

        public static void Send(Action<ServiceMessage> actor, ServiceMessage message) {
            actor(message);
        }

        public override string ToString() {
            if (this.MessageList.Count == 0) {
                return string.Empty;
            }
            if (this.MessageList.Count == 1) {
                return this.MessageList[0];
            }
            var sb = new StringBuilder();
            foreach (var msg in this.MessageList) {
                sb.AppendLine(msg);
            }
            return sb.ToString();
        }
    }
}
