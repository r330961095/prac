using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Text;

// The exam program. It gets the path of an input file, and prints the report.
// Run it from the exam folder:  python check/run_checks.py run
// Without a path, it reads data/sample.txt.
class Program
{
    static void Main(string[] args)
    {
        // Hebrew is printed as UTF-8, also on Windows
        Console.OutputEncoding = new UTF8Encoding(false);
        string path = args.Length > 0 ? args[0] : FindSample();
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        int reportDay = ReadReportDay(lines);
        List<string[]> rows = ReadRows(lines);
        Console.Error.WriteLine("skeleton ok: read " + Path.GetFileName(path) + " - report day " + reportDay + " - " + rows.Count + " rows");
        int fines = StudentFine(34, rows);
        // Write your code here. 'reportDay' and 'rows' are ready.
    }

    static int ReadReportDay(string[] lines)
    {
        return int.Parse(lines[0].Split(':')[1].Trim());
    }

    // The lines after the two first ones, each split into its fields
    static List<string[]> ReadRows(string[] lines)
    {
        List<string[]> rows = new List<string[]>();
        for (int i = 2; i < lines.Length; i++)
        {
            if (lines[i].Trim() == "") continue;
            string[] fields = lines[i].Split(',');
            for (int j = 0; j < fields.Length; j++) fields[j] = fields[j].Trim();
            rows.Add(fields);
        }
        return rows;
    }

    // Without a path, look for data/sample.txt next to the project, also when started from Visual Studio
    static string FindSample()
    {
        DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            string candidate = Path.Combine(dir.FullName, "data", "sample.txt");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        return Path.Combine("data", "sample.txt");
    }

    static int StudentFine(int reportDay, List<string[]> rows)
    {
        int day = 0;
        for (int i = 0; i < rows.Count; i++) {
            if (rows[i][3] == "student")
            {
                day = rows[i][4] - reportDay;
                if (day > 15)
                {
                    return day;
                }
            }
        }
        return 0;
    }
    static List<string[]> AllRulesFine(int reportDay, List<string[]> rows)
    {
        int returnDay = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i][3] == "student")
            {
                returnDay = rows[i][4] - reportDay;
                if (returnDay <= 15)
                {
                    rows[i][6] = 0
                    return rows;
                }
                else if (returnDay < 31)
                {
                    rows[i][6] = returnDay
                    return rows;
                }
                else
                    return rows;
            }
            else if (rows[i][3] == "guest")
            {
                returnDay = rows[i][4] - reportDay;
                if (returnDay < 7)
                {
                    return rows;
                }
                else
                {
                    int num = 0;
                    num = (rows[i][4] - reportDay) * 2.5;
                    rows[i][6] = num;
                    return rows;
                }
            }
            else
            {
                returnDay = rows[i][4] - reportDay;
                if(returnDay < 61)
                {
                    rows[i][6] = 0;
                    return rows;
                }
            }
        }
        return rows;
    }
    static List<string[]> OneLine(List<string[]> rows)
    {

    }
}