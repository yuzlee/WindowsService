using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WindowsService.service {
    enum ServiceState {
        Ready, // if installed but not start
        Running, // started
        Stopped // stooped manually
    }

    class ServiceHanlder {
        private bool is_begin_redirect_io = false;

        private Process process { get; } = new Process();

        public ServiceInfo Info { get; }

        public string Name => Info.Name;

        public int Id => this.process.Id;

        public ServiceState State { get; private set; }

        public ServiceStatus Status {
            get {
                var status = new ServiceStatus();
                status.State = this.State;
                status.Id = this.Id;
                status.StartTime = this.process.StartTime;
                status.Info = this.Info;
                return status;
            }
        }

        public ServiceHanlder(ServiceInfo info) {
            this.Info = info;

            InitializeProcess();
        }

        private void InitializeProcess() {
            this.process.StartInfo.FileName = this.Info.Path;
            this.process.StartInfo.Arguments = string.Join(" ", this.Info.Args);
            this.process.StartInfo.CreateNoWindow = false;
            this.process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.RedirectStandardOutput = true;
            this.process.StartInfo.RedirectStandardError = true;

            this.process.OutputDataReceived += (sender, e) => {
                //Console.WriteLine(e.Data);
            };
            this.process.ErrorDataReceived += (sener, e) => {
                //Console.WriteLine(e.Data);
            };

            this.process.Exited += (sender, e) => this.State = ServiceState.Ready;

            this.process.EnableRaisingEvents = true;

            this.State = ServiceState.Ready;
        }

        public void Start(out ServiceMessage message) {
            try {
                if (this.State != ServiceState.Running) {
                    this.process.Start();
                    this.State = ServiceState.Running;

                    if (!this.is_begin_redirect_io) {
                        this.process.BeginOutputReadLine();
                        this.process.BeginErrorReadLine();
                        this.is_begin_redirect_io = true;
                    }
                    message = null;
                } else {
                    message = new ServiceMessage($"[{Name}] Start: this service has ben started already");
                }
            } catch (Exception e) {
                message = new ServiceMessage($"[{Name}] Start: {e.Message}");
            }
        }

        public void Stop(out ServiceMessage message) {
            try {
                if (this.State == ServiceState.Running) {
                    this.process.Kill();
                    this.State = ServiceState.Stopped;
                    message = null;
                } else {
                    message = new ServiceMessage($"[{Name}] Stop: this service is not running");
                }
            } catch (Exception e) {
                message = new ServiceMessage($"[{Name}] Stop: {e.Message}");
            }
        }

        public void Restart(out ServiceMessage message) {
            try {
                this.Stop(out message);
                this.Start(out message);
            } catch (Exception e) {
                message = new ServiceMessage($"[{Name}] Restart: {e.Message}");
            }
        }
    }
}
