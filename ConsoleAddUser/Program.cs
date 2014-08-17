using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;

namespace ConsoleAddUser
{
    class Program
    {
        static void Main(string[] args)
        {
            FileStream fs = null;
            int sheet = 1;
            int startRow = 9;

            try
            {
                string filename = "../../file/FIbre+ Automation Users List.xlsx";
                fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

                using (ExcelPackage p = new ExcelPackage())
                {
                    p.Load(fs);

                    int endRow = 46;

                    GenerateScript(p.Workbook, sheet, startRow, endRow);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            finally
            {
                if (fs != null)
                    fs.Dispose();
            }

            Console.WriteLine("done");
            Console.ReadKey();
        }

        private static void GenerateScript(ExcelWorkbook wb, int sheet, int startRow, int endRow)
        {
            ExcelWorksheet sh = wb.Worksheets[sheet];
            StreamWriter sw = null;

            try
            {
                string file = "c:\\insert_user.sql";
                sw = new StreamWriter(file);
                StringBuilder sb = new StringBuilder();

                for (int i = startRow; i <= endRow; i++)
                {
                    string category = sh.Cells["B" + i].Text;
                    string name = sh.Cells["C" + i].Text;
                    string email = sh.Cells["D" + i].Text;

                    sb.Append("insert into [User] (UserEmail, PhoneNo, Name, Status) values (")
                    .AppendFormat("'{0}', '{1}', '{2}', {3})", email, "", name, 1);

                    sw.WriteLine(sb.ToString());
                    sb.Clear();
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {
                if (sw != null)
                    sw.Dispose();
            }
        }
    }
}
