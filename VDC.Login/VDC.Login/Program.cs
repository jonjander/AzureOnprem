using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VDC.Login
{
    class Program
    {
        static void Main(string[] args)
        {
            string Authority;
            if (args.Length == 0)
            {
                Form prompt = new Form()
                {
                    Width = 500,
                    Height = 150,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = "Enter tenant name",
                    StartPosition = FormStartPosition.CenterScreen
                };
                Label textLabel = new Label() { Left = 50, Top = 20, Width = 400, Text = "Enter tenant name (company.onmicrosoft.com)" };
                TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
                Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(textLabel);
                prompt.AcceptButton = confirmation;

                var tenant = prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";

                Authority = $"https://login.windows.net/{tenant}";
            }
            else
            {
                Authority = $"https://login.windows.net/{args[0]}";
            }
            
            var PlatformParamters = new PlatformParameters(PromptBehavior.SelectAccount);
            var ADALUserIdentifier = UserIdentifier.AnyUser;
            var redirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");
            
            var authenticationContext = new AuthenticationContext(Authority);
            var AuthenticationResult = authenticationContext.AcquireTokenAsync("https://management.azure.com/", "1950a258-227b-4e31-a9cf-717495945fc2", redirectUri, PlatformParamters).Result ;
            var accesstoken = $"{AuthenticationResult.AccessTokenType} {AuthenticationResult.AccessToken}";
            Console.WriteLine(accesstoken);
        }
    }
}
