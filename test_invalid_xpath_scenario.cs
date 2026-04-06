using System;
using System.Collections.Generic;
using System.IO;
using DemaConsulting.FileAssert.Cli;
using DemaConsulting.FileAssert.Configuration;
using DemaConsulting.FileAssert.Modeling;

namespace TestInvalidXPath;

public class Program
{
    public static void Main()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "<root><item>test</item></root>");
            
            var data = new List<FileAssertQueryData> 
            { 
                new() { Query = "//item[invalid", Count = 1 } 
            };
            
            var xmlAssert = FileAssertXmlAssert.Create(data);
            using var context = Context.Create(new[] { "--silent" });
            
            Console.WriteLine("Running XmlAssert with invalid XPath...");
            try
            {
                xmlAssert.Run(context, tempFile);
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
