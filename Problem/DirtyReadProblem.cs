using Dapper;
using dapperHelper;
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
            DirtyReadProblem dirtyReadProblem = new DirtyReadProblem(db);
            List<Task> tasks = new List<Task>();
            tasks.Add(dirtyReadProblem.Transaction1());
            tasks.Add(dirtyReadProblem.Transaction2());
            Task.WaitAll(tasks.ToArray());
     */
    /// <summary>
    /// DirtyReadProblem (髒讀取問題)
    /// </summary>
    class DirtyReadProblem
    {
        private DBContext db;

        public DirtyReadProblem(DBContext db)
        {
            this.db = db;
        }
        public async Task Transaction1()
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, 
                TransactionScopeAsyncFlowOption.Enabled))
            {
                int productID = 1;
                int orderedQuantity = 1;


                int availableQuantity = await db.QuerySingleAsync<int>(@"
select AvailableQuantity from Product2 where productid = @productid
", new { productID });
                Console.WriteLine($"Transaction1- AvailableQuantity: {availableQuantity}");

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
                Console.WriteLine($"Transaction1- AvailableQuantity: {availableQuantity}");

                // do someting ...
                Thread.Sleep(10 * 1000);

                scope.Dispose();

                availableQuantity = await db.QuerySingleAsync<int>(@"
select AvailableQuantity from Product2 where productid = @productid
", new { productID });
                Console.WriteLine($"Transaction1- AvailableQuantity: {availableQuantity}");
            }

        }
        public async Task Transaction2()
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, // ReadUncommitted
                TransactionScopeAsyncFlowOption.Enabled))
            {
                int productID = 1;
                int orderedQuantity = 1;


                // do someting ...
                Thread.Sleep(2 * 1000);

                int availableQuantity = await db.QuerySingleAsync<int>(@"
select AvailableQuantity from Product2 where productid = @productid
", new { productID });
                Console.WriteLine($"Transaction2- AvailableQuantity: {availableQuantity}");


            }


        }
    }
}
