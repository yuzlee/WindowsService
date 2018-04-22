using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WindowsService.service {
    [Serializable]
    class ServiceInfo {
        public string Name { get; }

        public string Path { get; }

        public List<string> Args { get; }

        public string Description { get; }

        public ServiceInfo(string name, string path, List<string> args, string description) {
            this.Name = name;
            this.Path = path;
            this.Args = args;
            this.Description = description;
        }

        public static ServiceInfo FindFromXMLConfig(string config, string name) {
            var info = XDocument.Load(config).Root.Elements()
                .Where(s => s.Element("name").Value.Trim() == name).FirstOrDefault();
            if (info == null) {
                return null;
            }
            var path = info.Element("path").Value.Trim();
            var args = info.Descendants("arg").Select(arg => arg.Value.Trim()).ToList();
            var description = info.Element("description").Value.Trim();
            return new ServiceInfo(name, path, args, description);
        }

        public override string ToString() {
            return $"[{Name}] {Path} {string.Join(" ", Args)} => {Description}";
        }
    }
}
