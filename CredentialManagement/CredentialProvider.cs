// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CredentialManagement
{
    public class CredentialProvider : ICredentialProvider
    {
        string purpose;

        public CredentialProvider(string purpose)
        {
            this.purpose = purpose;
        }

        Credential Credential
        {
            get { return new Credential { Target = purpose }; }
        }

        public NetworkCredential GetCredential()
        {
            var c = Credential;
            if (c.Exists())
            {
                c.Load();
            }
            else
            {
                ICredentialsPrompt prompt = new CredentialManagement.VistaPrompt
                {
                    ShowSaveCheckBox = true,
                    Title = purpose,
                    GenericCredentials = true
                };

                if (prompt.ShowDialog(IntPtr.Zero) != CredentialManagement.DialogResult.OK)
                {
                    throw new Exception();
                }

                c = prompt.GetCredential();

                if (prompt.SaveChecked)
                {
                    c.Save();
                }
            }

            return c.NetworkCredential;
        }

        public void Reset()
        {
            var c = Credential;
            if (c.Exists())
            {
                c.Delete();
            }
        }
    }
}
