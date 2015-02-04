using System;
using System.Threading;
using System.Threading.Tasks;


namespace FancontrolLogger
{
    /// <summary>
    /// Download and install Notebook FanControl (http://www.computerbase.de/forum/showthread.php?t=1070494)
    /// and make sure the backend service is running
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // create a model to hold the CPU information
            var model = new NbfcServiceClient.ViewModels.MainWindowViewModel();
            // print out the CpuTemperature when changed
            model.PropertyChanged += (s, e) => { 
                if (e.PropertyName.Equals("CpuTemperature")) 
                    Console.WriteLine("Temp is {0}C", model.CpuTemperature); 
            };

            // update intervals
            var updateIntervalInSeconds = 10;
            var updateIntervalInTimeSpan = TimeSpan.FromSeconds(updateIntervalInSeconds);
            var client = new NbfcServiceClient.FanControlClient(model, updateIntervalInSeconds);
            client.StartFanControl();

            // stop recursive calling and end the program
            var tokenSource = new CancellationTokenSource();

            // read the import on another thread
            // loop below does not allow user input
            Task.Factory.StartNew(() => { 
                Console.ReadLine(); 
                Console.WriteLine("Stopping ..."); 
                tokenSource.Cancel();
            });

            // until ReadLine in Task above 
            while (tokenSource.IsCancellationRequested == false) {
                // update view model to trigger changes
                try {
                    client.UpdateViewModel();
                    Task.Delay(updateIntervalInTimeSpan, tokenSource.Token).Wait();
                } 
                catch (Exception) { 
                    // ignore 
                }
            }
        }
    }
}
