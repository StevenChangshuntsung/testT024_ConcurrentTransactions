using dapperHelper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace testT024_ConcurrentTransactions
{
    class Program
    {
        static async Task Main(string[] args)
        {
            DBContext db = new DBContext();
            db.ConnectionStr = "Data Source=MSI\\SQLEXPRESS;Initial Catalog=Sample;Integrated Security=True;";


            ReCreateProduct2(db).Wait();
            ReCreatePerson4(db).Wait();


            #region DirtyReadProblem (髒讀取問題)
            //CleanUpProduct2(db).Wait();
            //DirtyReadProblem dirtyReadProblem = new DirtyReadProblem(db);
            //List<Task> tasks = new List<Task>();
            //tasks.Add(dirtyReadProblem.Transaction1());
            //tasks.Add(dirtyReadProblem.Transaction2());
            //Task.WaitAll(tasks.ToArray());
            #endregion

            #region LostUpdateProblem(更新遺失問題)
            //CleanUpProduct2(db).Wait();
            //LostUpdateProblem lostUpdateProblem = new LostUpdateProblem(db);
            //List<Task> tasks = new List<Task>();
            //tasks.Add(lostUpdateProblem.Transaction1());
            //tasks.Add(lostUpdateProblem.Transaction2());
            //Task.WaitAll(tasks.ToArray());
            #endregion

            #region NonRepeatableReadProblem(不可重複讀取問題)
            //CleanUpProduct2(db).Wait();
            //NonRepeatableReadProblem nonRepeatableReadProblem = new NonRepeatableReadProblem(db);
            //List<Task> tasks = new List<Task>();
            //tasks.Add(nonRepeatableReadProblem.Transaction1());
            //tasks.Add(nonRepeatableReadProblem.Transaction2());
            //Task.WaitAll(tasks.ToArray());
            #endregion

            #region PhantomReadProblem(幻讀問題)
            //CleanUpPerson4(db).Wait();
            //PhantomReadProblem phantomReadProblem = new PhantomReadProblem(db);
            //List<Task> tasks = new List<Task>();
            //tasks.Add(phantomReadProblem.Transaction1());
            //tasks.Add(phantomReadProblem.Transaction2());
            //Task.WaitAll(tasks.ToArray());
            #endregion

            Console.ReadKey();
        }


        private static async Task CleanUpProduct2(DBContext db)
        {
            int count = await UpdateAvailableQuantityTo20(db);
            if (count > 0)
            {
                List<int> availableQuantitys = await GetAvailableQuantity(db);
                foreach (var item in availableQuantitys)
                {
                    Console.WriteLine($"AvailableQuantity: {item}");
                }
            }
        }
        private static async Task CleanUpPerson4(DBContext db)
        {
            int count = await ReCreatePerson4(db);
        }

        private static async Task<int> ReCreatePerson4(DBContext db)
        {
            try
            {
                string sql = @"
IF ( EXISTS ( SELECT    *
              FROM      INFORMATION_SCHEMA.TABLES
              WHERE     TABLE_NAME = 'Person4' ) )
    BEGIN
        TRUNCATE TABLE dbo.Person4;
        DROP TABLE Person4;
    END;
CREATE TABLE Person4
    (
      ID INT PRIMARY KEY
             NOT NULL ,
      [Name] NVARCHAR(50)
    );
INSERT  INTO Person4
VALUES  ( 1, 'Name1' );
INSERT  INTO Person4
VALUES  ( 5, 'Name5' );
";
                int count = await db.ExecuteAsync(sql);
                if (count == 2)
                {
                    Console.WriteLine("Do ReCreatePerson4");
                }
                return count;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private static async Task<object> ReCreateProduct2(DBContext db)
        {
            try
            {
                string sql = @"
    IF ( EXISTS ( SELECT    *
                  FROM      INFORMATION_SCHEMA.TABLES
                  WHERE     TABLE_NAME = 'Product2' ) )
        BEGIN
            TRUNCATE TABLE dbo.Product2;
            DROP TABLE Product2;
        END;
    CREATE TABLE Product2
        (
          ProductID INT IDENTITY(1, 1)
                        PRIMARY KEY
                        NOT NULL ,
          ProductName NVARCHAR(100) ,
          AvailableQuantity INT
        );
    INSERT  INTO Product2
    VALUES  ( 'Product1', 20 );
    ";
                int count = await db.ExecuteAsync(sql);
                if (count == 1)
                {
                    Console.WriteLine("Do ReCreateProduct2");
                }
                return count;

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        /// <summary>
        /// int result = await UpdateAvailableQuantityTo20(db);
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private static async Task<int> UpdateAvailableQuantityTo20(DBContext db)
        {
            try
            {
                string sql = @"
UPDATE Product2 
set AvailableQuantity = @AvailableQuantity
where ProductID = @ProductID
";
                return await db.ExecuteAsync(sql, new { AvailableQuantity = 20, ProductID = 1 });
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// List<int> availableQuantitys = await GetAvailableQuantity(db);
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private static async Task<List<int>> GetAvailableQuantity(DBContext db)
        {
            return await db.QueryAsync<int>("select AvailableQuantity from Product2");
        }
    }
}
