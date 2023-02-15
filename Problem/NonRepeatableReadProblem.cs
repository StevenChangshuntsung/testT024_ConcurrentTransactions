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
            NonRepeatableReadProblem nonRepeatableReadProblem = new NonRepeatableReadProblem(db);
            List<Task> tasks = new List<Task>();
            tasks.Add(nonRepeatableReadProblem.Transaction1());
            tasks.Add(nonRepeatableReadProblem.Transaction2());
            Task.WaitAll(tasks.ToArray());
     */
    /// <summary>
    /// NonRepeatableReadProblem(不可重複讀取問題)
    /// </summary>
    class NonRepeatableReadProblem
    {
        private DBContext db;

        public NonRepeatableReadProblem(DBContext db)
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
                    int orderedQuantity = 1;


                    int availableQuantity = await db.QuerySingleAsync<int>(@"
select AvailableQuantity from Product2 where productid = @productid
", new { productID });
                    Console.WriteLine($"{transaction} - AvailableQuantity: {availableQuantity}");


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
                }
                catch (Exception ex)
                {
                    
                    throw;
                }
            }


        }
    }
}
