using Dapper;
using dapperHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace testT024_ConcurrentTransactions.Problem
{
    /*
            LostUpdateProblem lostUpdateProblem = new LostUpdateProblem(db);
            List<Task> tasks = new List<Task>();
            tasks.Add(lostUpdateProblem.Transaction1());
            tasks.Add(lostUpdateProblem.Transaction2());
            Task.WaitAll(tasks.ToArray());
     */
    /// <summary>
    /// LostUpdateProblem(更新遺失問題)
    /// </summary>
    class LostUpdateProblem
    {
        private DBContext db;

        public LostUpdateProblem(DBContext db)
        {
            this.db = db;
        }
        public async Task Transaction1()
        {
            string transaction = "Transaction1";
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead }, // ReadCommitted OR RepeatableRead
                TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    int productID = 1;
                    int orderedQuantity = 1;


                    int availableQuantity = await db.QuerySingleAsync<int>(@"
select AvailableQuantity from Product2 where productid = @productid
", new { productID });
                    Console.WriteLine($"{transaction} - AvailableQuantity: {availableQuantity}");


                    // do someting ...
                    Thread.Sleep(5 * 1000);

                    int count = await db.ExecuteAsync(@"
update Product2 set AvailableQuantity = @AvailableQuantity where productid = @productid
",
                             new
                             {
                                 AvailableQuantity = availableQuantity - orderedQuantity,
                                 productID,
                             });

                    availableQuantity = await db.QuerySingleAsync<int>(@"
select AvailableQuantity from Product2 where productid = @productid
", new { productID });
                    Console.WriteLine($"{transaction} - AvailableQuantity: {availableQuantity}");


                    scope.Complete();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"{transaction} -");
                    Console.WriteLine(JsonConvert.SerializeObject(db.ErrorMessages(ex)));
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

        }
        public async Task Transaction2()
        {
            string transaction = "Transaction2";
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    int productID = 1;
                    int orderedQuantity = 3;


                    int availableQuantity = await db.QuerySingleAsync<int>(@"
select AvailableQuantity from Product2 where productid = @productid
", new { productID });
                    Console.WriteLine($"{transaction} - AvailableQuantity: {availableQuantity}");


                    // do someting ...
                    Thread.Sleep(5 * 1000);

                    int count = await db.ExecuteAsync(@"
update Product2 set AvailableQuantity = @AvailableQuantity where productid = @productid
",
                             new
                             {
                                 AvailableQuantity = availableQuantity - orderedQuantity,
                                 productID,
                             });

                    availableQuantity = await db.QuerySingleAsync<int>(@"
select AvailableQuantity from Product2 where productid = @productid
", new { productID });
                    Console.WriteLine($"{transaction} - AvailableQuantity: {availableQuantity}");


                    scope.Complete();

                }
                catch(SqlException ex)
                {
                    Console.WriteLine($"{transaction} -");
                    Console.WriteLine(JsonConvert.SerializeObject(db.ErrorMessages(ex)));
                    // Message	"交易 (處理序識別碼 55) 在 鎖定 資源上被另一個處理序鎖死並已被選擇作為死結的犧牲者。請重新執行該交易。"
                    // Number	1205
                }
                catch (Exception ex)
                {
                    
                    throw;
                }
            }


        }
    }
}
