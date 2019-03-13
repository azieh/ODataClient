using System;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using EntityDbMock;

namespace ODataClient.Synchronizer
{
    /// <summary>
    /// ODataClient Synchronizer base class
    /// </summary>
    public abstract class ODataClientSynchronizer
    {
        /// <summary>
        /// Master data method name
        /// </summary>
        protected abstract string ODataClientMethodName { get; }

        /// <summary>
        /// Reporting period settings
        /// </summary>
        public static string ReportingPeriod => "ReportingPeriod";

        public static AmdEntities ODataClientClient { get; } = new ODataClient.AmdEntities(new Uri(Settings.ODataClientServiceUrl));

        /// <summary>
        /// Method execution and parsing
        /// </summary>
        protected abstract void LoadAndParseODataClientMethod();

        /// <summary>
        /// Update CQP database with parsed values
        /// </summary>
        /// <param name="db">Initialized CQP database context</param>
        protected abstract void UpdateDatabase(IEntityDb db);

        /// <summary>
        /// Execute synchronizer
        /// </summary>
        /// <returns></returns>

        public bool Update()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            string status = ": NOK";

            using (new Logger.LogCallWrapper(ODataClientMethodName))
            {
                try
                {
                    LoadAndParseODataClientMethod();

                    using (var tran = new TransactionScope(TransactionScopeOption.RequiresNew, Utilities.GetDefaultIsolationLevel()))
                    {
                        using (var db = new EntityDbMock())
                        {
                            db.Database.CommandTimeout = Settings.TimeOut;
                            UpdateDatabase(db);
                            db.SaveChanges();
                        }
                        tran.Complete();
                    }
                    status = ": OK";
                    return true;
                }
                catch (DbEntityValidationException ex)
                {
                    StringBuilder sb = new StringBuilder("DbEntityValidationException").AppendLine();
                    foreach (var failure in ex.EntityValidationErrors)
                    {
                        sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                        foreach (var error in failure.ValidationErrors)
                        {
                            sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                            sb.AppendLine();
                        }
                    }
                    Logger.LogException(ex, sb.ToString());
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
                finally
                {
                    watch.Stop();
                    Console.WriteLine(string.Format(SynchronizerResource.EndProcessing,
                        string.Format("{0} {1} in {2}ms", ODataClientMethodName.PadRight(20), status, watch.ElapsedMilliseconds)));
                    Logger.LogTrace(string.Format(SynchronizerResource.EndProcessing, ODataClientMethodName));
                }
                return false;
            }
        }

        /// <summary>
        /// Execute multithreading synchronizer
        /// </summary>
        /// <returns></returns>
        public async Task RunUpdateTask()
        {
            await Task.Run(() => Update());
        }
    }
}