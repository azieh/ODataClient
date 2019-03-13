using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODataClient.Synchronizer;
using ODataClient.Synchronizer.ApplicationClass;
using ODataClient.Synchronizer.BusinessLine;
using ODataClient.Synchronizer.ChannelClasses;
using ODataClient.Synchronizer.CompanyLocations;
using ODataClient.Synchronizer.Countries;
using ODataClient.Synchronizer.ExchangeRates;
using ODataClient.Synchronizer.IndustryUsage;
using ODataClient.Synchronizer.ProductGroups;
using ODataClient.Synchronizer.ProductLine;
using ODataClient.Synchronizer.Regions;

namespace ODataClient
{
    class Program
    {
        static void Main(string[] args)
        {
            if ((args != null) && (args.Length > 0))
            {
                foreach (string arg in args)
                {
                    if (string.IsNullOrEmpty(arg)) continue;

                    switch (arg.ToLowerInvariant())
                    {
                        case "/r:businessline":
                            new BusinessLineSynchronizer().Update();
                            Environment.Exit(0);
                            return;

                        case "/r:industryusage":
                            new IndustryUsageSynchronizer().Update();
                            Environment.Exit(0);
                            return;

                        case "/r:lineitem":
                            new ProductLineSynchronizer().Update();
                            Environment.Exit(0);
                            return;

                        case "/r:exhangerate":
                            new ExchangeRateSynchronizer().Update();
                            Environment.Exit(0);
                            return;

                        case "/r:application":
                            new ApplicationClassSynchronizer().Update();
                            Environment.Exit(0);
                            return;

                        case "/r:productgroup":
                            new ProductGroupsSynchronizer().Update();
                            Environment.Exit(0);
                            return;

                        case "/r:customerchannel":
                            new ChannelClassesSynchronizer().Update();
                            Environment.Exit(0);
                            return;

                        case "/r:region":
                            new RegionsSynchronizer().Update();
                            Environment.Exit(0);
                            return;

                        case "/r:country":
                            new CountriesSynchronizer().Update();
                            Environment.Exit(0);
                            return;

                        case "/r:location":
                            new CompanyLocationSynchronizer().Update();
                            Environment.Exit(0);
                            return;

                        case "/?":
                        case "?":
                        case "help":
                        case "/help":
                            Console.WriteLine(SynchronizerResource.CommandLineHelp);
                            Environment.Exit(0);
                            return;

                        case "/all":
                            break;

                        default:
                            Console.WriteLine(string.Format(SynchronizerResource.UnknownCommand, arg));
                            Environment.Exit(0);
                            return;
                    }
                }
            }

            List<Task> taskList = new List<Task>();
            taskList.Add(new ApplicationClassSynchronizer().RunUpdateTask());
            taskList.Add(new BusinessLineSynchronizer().RunUpdateTask());
            taskList.Add(new ChannelClassesSynchronizer().RunUpdateTask());
            taskList.Add(new CompanyLocationSynchronizer().RunUpdateTask());
            taskList.Add(new ExchangeRateSynchronizer().RunUpdateTask());
            taskList.Add(new IndustryUsageSynchronizer().RunUpdateTask());
            taskList.Add(new ProductGroupsSynchronizer().RunUpdateTask());
            taskList.Add(new ProductLineSynchronizer().RunUpdateTask());
            taskList.Add(new RegionsSynchronizer().RunUpdateTask()
                                .ContinueWith(delegate { new CountriesSynchronizer().RunUpdateTask(); })); //CountriesSynchronizer have to wait for RegionSynchronizer

            Task.WaitAll(taskList.ToArray());
            Environment.Exit(0);
        }
    }
}
