using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Reflection;
using System.Runtime.InteropServices;

namespace JayGenPackage
{
    [Guid("FD5B68E5-3B43-49EB-A7F1-CE25471E2C15")]
    public class SourceGen : BaseCodeGeneratorWithSite
    {
        public const string Name = "JayGen";

        public const string Description = "WJay source generator";

        public const string PanelGuid = "B25D2171-ACA7-4B62-B2E4-96DB0A8A595D";

        public override string GetDefaultExtension() => ".cs";

        public SourceGen()
        {
            return;
        }
   
        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                Process process = new Process();
                string location = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "redist");
                string wjayEXE = Path.Combine(location, "wjay.exe");
                string inputFileBaseDir = Path.GetDirectoryName(inputFileName);
                string skeleton = Path.Combine(inputFileBaseDir, "skeleton.cs");
                if (!File.Exists(skeleton))
                {
                    skeleton = Path.Combine(location, "skeleton.cs.in");
                    OutputMessage("Used internal skeleton.cs file");
                }
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(inputFileName);
                if (File.Exists(wjayEXE))
                {
                    process.StartInfo.FileName = wjayEXE;
                    if (File.Exists(Path.Combine(inputFileBaseDir, "yydebug")))
                        process.StartInfo.Arguments = "-t -c -v " + inputFileName;
                    else
                        process.StartInfo.Arguments = "-c -v " + inputFileName;

                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.CreateNoWindow = true;

                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    StringBuilder outputLines = new StringBuilder();
                    process.OutputDataReceived += (sendingProcess, outLine) =>
                    {
                        if (outLine.Data != null)
                        {
                            outputLines.AppendLine();
                            outputLines.Append(outLine.Data);
                        }
                    };

                    List<string> errors = new List<string>();
                    process.ErrorDataReceived += (sendingProcess, outLine) =>
                    {
                        if (outLine.Data != null)
                        {
                            errors.Add(outLine.Data);
                            OutputMessage(outLine.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    StreamReader skeletonReader = File.OpenText(skeleton);
                    process.StandardInput.Write(skeletonReader.ReadToEnd());
                    process.StandardInput.Flush();
                    process.StandardInput.Close();

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        StreamReader youtput = new StreamReader(Path.Combine(process.StartInfo.WorkingDirectory, "y.output"));
                        List<String> slist = new List<string>();
                        while (!youtput.EndOfStream)
                        {
                            if (slist.Count > 10)
                                slist.RemoveAt(0);
                            slist.Add(youtput.ReadLine());
                        }
                        youtput.Close();

                        OutputMessage("Running wjay.exe:\n");
                        for (int k = 1; k < 3 && slist.Count - k > 0; k++)
                            if (!string.IsNullOrEmpty(slist[slist.Count - k]))
                                OutputMessage(slist[slist.Count - k] + "\n");

                        return Encoding.UTF8.GetBytes(outputLines.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                OutputMessage(ex.Message);
                OutputMessage(ex.StackTrace);
            }
            return null;
        }

        void OutputMessage(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsOutputWindow output =
                (IVsOutputWindow)ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow));

            if (output != null)
            {
                Guid paneGuid = new Guid(PanelGuid);

                // Retrieve the new pane.
                if (output.GetPane(ref paneGuid, out IVsOutputWindowPane pane) != VSConstants.S_OK)
                {
                    // Create a new pane.
                    output.CreatePane(
                        ref paneGuid,
                        Description,
                        Convert.ToInt32(true),
                        Convert.ToInt32(true));

                    output.GetPane(ref paneGuid, out pane);
                }

                pane.OutputStringThreadSafe(message);
            }
        }
    }
}
