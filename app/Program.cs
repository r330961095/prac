using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
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
        List<string> errors = new List<string>();
        List<string[]> rows = ReadRows(lines, errors);
        Console.Error.WriteLine("skeleton ok: read " + Path.GetFileName(path) + " - report day " + reportDay + " - " + (rows.Count + errors.Count) + " rows");

        List<string> readerOrder = new List<string>();
        Dictionary<string, string> readerNames = new Dictionary<string, string>();
        Dictionary<string, int> finesByReader = new Dictionary<string, int>();
        Dictionary<string, int> positiveFineLoansByReader = new Dictionary<string, int>();
        Dictionary<string, int> loansByBook = new Dictionary<string, int>();
        for (int i = 0; i < rows.Count; i++)
        {
            string readerId = rows[i][0];
            if (!finesByReader.ContainsKey(readerId))
            {
                readerOrder.Add(readerId);
                readerNames[readerId] = rows[i][1];
                finesByReader[readerId] = 0;
                positiveFineLoansByReader[readerId] = 0;
            }
            int fine = FineAmount(reportDay, rows[i]);
            finesByReader[readerId] += fine;
            if (fine > 0) positiveFineLoansByReader[readerId]++;

            string book = rows[i][3];
            if (!loansByBook.ContainsKey(book)) loansByBook[book] = 0;
            loansByBook[book]++;
        }

        Console.WriteLine("FINES");
        int totalFine = 0;
        for (int i = 0; i < readerOrder.Count; i++)
        {
            string readerId = readerOrder[i];
            int fine = finesByReader[readerId];
            totalFine += fine;
            Console.WriteLine(readerId + "," + readerNames[readerId] + "," + fine);
        }
        Console.WriteLine("FLAGGED");
        for (int i = 0; i < rows.Count; i++)
        {
            if (IsFlaggedTeacher(reportDay, rows[i]))
            {
                Console.WriteLine(rows[i][0] + "," + rows[i][1] + "," + rows[i][3]);
            }
        }
        Console.WriteLine("TOP_BOOKS");
        List<KeyValuePair<string, int>> topBooks = new List<KeyValuePair<string, int>>(loansByBook);
        StringComparer hebrewComparer = StringComparer.Create(new CultureInfo("he-IL"), false);
        topBooks.Sort((first, second) =>
        {
            int countComparison = second.Value.CompareTo(first.Value);
            return countComparison != 0 ? countComparison : hebrewComparer.Compare(first.Key, second.Key);
        });
        for (int i = 0; i < topBooks.Count && i < 3; i++)
        {
            Console.WriteLine(topBooks[i].Key + "," + topBooks[i].Value);
        }
        Console.WriteLine("AT_RISK");
        for (int i = 0; i < readerOrder.Count; i++)
        {
            string readerId = readerOrder[i];
            if (positiveFineLoansByReader[readerId] > 2)
            {
                Console.WriteLine(readerId + "," + readerNames[readerId] + "," + positiveFineLoansByReader[readerId]);
            }
        }
        Console.WriteLine("ERRORS");
        for (int i = 0; i < errors.Count; i++)
        {
            Console.WriteLine(errors[i]);
        }
        Console.WriteLine("TOTAL," + totalFine);
    }

    static int ReadReportDay(string[] lines)
    {
        return int.Parse(lines[0].Split(':')[1].Trim());
    }

    // The lines after the two first ones, each split into its fields
    static List<string[]> ReadRows(string[] lines, List<string> errors)
    {
        List<string[]> rows = new List<string[]>();
        for (int i = 2; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            for (int j = 0; j < fields.Length; j++) fields[j] = fields[j].Trim();
            if (IsValidRow(fields))
            {
                rows.Add(fields);
            }
            else
            {
                errors.Add(lines[i]);
            }
        }
        return rows;
    }

    static bool IsValidRow(string[] fields)
    {
        if (fields.Length != 6) return false;
        for (int i = 0; i < 5; i++)
        {
            if (fields[i] == "") return false;
        }
        if (fields[2] != "student" && fields[2] != "teacher" && fields[2] != "guest" && fields[2] != "alumna") return false;
        if (!int.TryParse(fields[4], out int loanDay)) return false;
        if (fields[5] != "")
        {
            if (!int.TryParse(fields[5], out int returnDay) || returnDay < loanDay) return false;
        }
        return true;
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

    static int FineAmount(int reportDay, string[] row)
    {
        int loanDay = int.Parse(row[4]);
        int endDay = row[5] == "" ? reportDay : int.Parse(row[5]);
        int elapsedDays = endDay - loanDay;

        if (row[2] == "student")
        {
            int lateDays = elapsedDays - 14;
            if (lateDays <= 0) return 0;
            return lateDays > 40 ? 40 : lateDays;
        }
        if (row[2] == "guest")
        {
            int lateDays = elapsedDays - 7;
            return lateDays > 0 ? (int)Math.Ceiling(lateDays * 2.5) : 0;
        }
        if (row[2] == "alumna")
        {
            int lateDays = elapsedDays - 10;
            if (lateDays <= 0) return 0;
            int fine = (int)Math.Ceiling(lateDays * 1.5);
            return fine > 25 ? 25 : fine;
        }
        return 0;
    }

    static bool IsFlaggedTeacher(int reportDay, string[] row)
    {
        if (row[2] != "teacher" || row[5] != "") return false;
        int loanDay = int.Parse(row[4]);
        return reportDay - loanDay > 60;
    }
}