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
            PhantomReadProblem phantomReadProblem = new PhantomReadProblem(db);
            List<Task> tasks = new List<Task>();
            tasks.Add(phantomReadProblem.Transaction1());
            tasks.Add(phantomReadProblem.Transaction2());
            Task.WaitAll(tasks.ToArray());
     */
    /// <summary>
    /// PhantomReadProblem(幻讀問題)
    /// </summary>
    class PhantomReadProblem
    {
        private DBContext db;

        public PhantomReadProblem(DBContext db)
        {
            this.db = db;
        }
        public async Task Transaction1()
        {
            string transaction = "Transaction1";
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }, // RepeatableRead OR Serializable
                TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    List<Person4> availableQuantity = await db.QueryAsync<Person4>(@"
SELECT  * FROM    Person4 WHERE   ID BETWEEN 1 AND 5
");
                    Console.WriteLine($"{transaction} - 第一次 {JsonConvert.SerializeObject(availableQuantity)}");


                    // do someting ...
                    Thread.Sleep(5 * 1000);


                    List<Person4> availableQuantityNew = await db.QueryAsync<Person4>(@"
SELECT  * FROM    Person4 WHERE   ID BETWEEN 1 AND 5
");
                    Console.WriteLine($"{transaction} - 第二次 {JsonConvert.SerializeObject(availableQuantityNew)}");


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
                new TransactionOptions { IsolationLevel = IsolationLevel.Serializable },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    List<Person4> availableQuantity = await db.QueryAsync<Person4>(@"
SELECT  * FROM    Person4 WHERE   ID BETWEEN 1 AND 5
");
                    Console.WriteLine($"{transaction} - {JsonConvert.SerializeObject(availableQuantity)}");


                    int count = await db.ExecuteAsync(@"INSERT  INTO Person4 VALUES  ( 2, 'Name2' )");


                    List<Person4> availableQuantityNew = await db.QueryAsync<Person4>(@"
SELECT  * FROM    Person4 WHERE   ID BETWEEN 1 AND 5
");
                    Console.WriteLine($"{transaction} - NEW {JsonConvert.SerializeObject(availableQuantityNew)}");


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

    internal class Person4
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
