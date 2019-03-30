using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    public static class LoginHelper
    {
        public static string GetToken(string tenant = null)
        {
            Process process = new Process
            {
                EnableRaisingEvents = false
            };
            process.StartInfo.FileName = Application.dataPath + "/StreamingAssets/VDC.Login.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            if (tenant != null)
            {
                process.StartInfo.Arguments = tenant;
            }
            process.Start();
            var output = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            return output.Replace("\r\n","");
        }

        static void DataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            // Handle it
        }
    }
}
