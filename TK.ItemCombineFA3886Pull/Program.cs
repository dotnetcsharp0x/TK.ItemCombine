using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Data;
using System.Drawing.Text;
using System.Xml;
using TK.ItemCombineFA3886Pull.Actions;
using TK.ItemCombineFA3886Pull.Models;

namespace TK.ItemCombineFA3886Pull
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder();
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreComments = true;
            #region XML connect - settings.xml
            XmlDocument xApi = new XmlDocument();
            xApi.Load("settings.xml");
            XmlElement? xRootApi = xApi.DocumentElement;
            int timing = 0;
            Params prm = new Params();

            #endregion

            #region ReadXml
            if (xRootApi != null)
            {
                foreach (XmlElement xnode in xRootApi)
                {
                    if (xnode.Name == "Timing")
                    {
                        timing = Convert.ToInt32(xnode.InnerText.Replace("\r", "").Replace("\n", "").Replace("\t", ""));
                    }
                    
                    prm.Timing = timing;

                    
                }
            }
            #endregion
            Thread thread = new Thread(() => TkRunReservation(prm));
            thread.Start();
            await builder.RunConsoleAsync();
        }

        #region Background service worker
        public class MySpecialService : BackgroundService
        {
            private Params _prm = new Params();
            public MySpecialService(Params prm)
            {
                _prm = prm;
            }
            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                RunReservation rr = new RunReservation();
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await new RunReservation().CombineManual(_prm);

                        await Task.Delay(TimeSpan.FromSeconds(_prm.Timing));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }
        #endregion

        #region TkRunReservation function fromThread
        static private async void TkRunReservation(Params prm)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.AddSingleton(prm);
                    services.AddSingleton<IHostedService, MySpecialService>();
                });
            await builder.RunConsoleAsync();
        }
        #endregion
    }
}