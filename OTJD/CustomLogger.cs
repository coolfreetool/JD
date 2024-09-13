using System;
using System.IO;
using System.Text;

namespace OTJD
{
    public class CustomLogger : TextWriter
    {
        private TextWriter originalConsole; // To restore the original console later.
        // private readonly StringWriter logWriter;

        public CustomLogger()
        {
            // // Create a StringWriter to capture the logs
            // logWriter = new StringWriter();
            // // Save the original console output
            originalConsole = Console.Out;
        }

        // Override the Write methods to capture logs
        public override void Write(char value)
        {
        //     // Write to the custom log
        //     logWriter.Write("char:" + value);
            // Optionally, write to the original console too
            //Console.Write("CHAR: " + value);
        }

        public override void WriteLine(string value)
        {
            // // Write to the custom log
            // logWriter.WriteLine(value);
            // // Optionally, write to the original console too
            // originalConsole.WriteLine(value);
            //Console.WriteLine("LINE: " + value);
        }

        public override Encoding Encoding => originalConsole.Encoding;

        // Optionally, implement a method to restore original console output.
        public void RestoreOriginalConsole()
        {
            Console.SetOut(originalConsole);
        }
    }
}
