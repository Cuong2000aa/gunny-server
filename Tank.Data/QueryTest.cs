using System;
using Microsoft.Data.SqlClient;

namespace Tank.Data
{
    public class QueryTest
    {
        public static void Run(string connStr, string sql)
        {
            using var conn = new SqlConnection(connStr);
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                Console.Write(reader.GetName(i) + "\t");
            }
            Console.WriteLine();
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.Write(reader[i]?.ToString() + "\t");
                }
                Console.WriteLine();
            }
        }
    }
}
