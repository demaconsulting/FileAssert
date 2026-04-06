using System;
using System.Collections.Generic;
using System.IO;
using DemaConsulting.FileAssert.Cli;
using DemaConsulting.FileAssert.Configuration;
using DemaConsulting.FileAssert.Modeling;

namespace TestInvalidXPathHtml;

public class Program
{
    public static void Main()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "<html><body><p>test</p></body></html>");
            
            var data = new List<FileAssertQueryData> 
            { 
                new() { Query = "//p[invalid", Count = 1 } 
            };
            
            var htmlAssert = FileAssertHtmlAssert.Create(data);
            using var context = Context.Create(new[] { "--silent" });
            
            Console.WriteLine("Running HtmlAssert with invalid XPath...");
            try
            {
                htmlAssert.Run(context, tempFile);
                Console.WriteLine($"Exit code: {context.ExitCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UNHANDLED EXCEPTION: {ex.GetType().Name} - {ex.Message}");
            }
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
