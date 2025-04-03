﻿using System;
using System.Data;

class Program
{
    static void Main()
    {
        string filePath = "C:\\sample_data.csv";
        if (!File.Exists(filePath))
        {
            Console.WriteLine("CSV file not found!");
            return;
        }

        DataTable dataTable = ReadCsvToDataTable(filePath);

        foreach (DataRow row in dataTable.Rows)
        {
            Console.WriteLine(string.Join(", ", row.ItemArray));
        }
    }

    static DataTable ReadCsvToDataTable(string filePath)
    {
        DataTable dt = new DataTable();

        using (StreamReader sr = new StreamReader(filePath))
        {
            string line;
            int rowIndex = 0;
            sr.ReadLine(); //To Read first line and ignore it 

            while ((line = sr.ReadLine()) != null)
            {
               
                string[] fields = line.Split(',');

                if (rowIndex == 0) 
                {
                    for (int i = 0; i < fields.Length; i++)
                    {
                        string columnName = fields[i].Trim();
                        int count = 1;

                        while (dt.Columns.Contains(columnName))
                        {
                            columnName = $"{fields[i].Trim()}_{count}";
                            count++;
                        }

                        dt.Columns.Add(columnName);
                    }
                }
                else if (rowIndex >= 2) 
                {
                    dt.Rows.Add(fields);
                }

                rowIndex++;
            }
        }

        return dt;
    }
}