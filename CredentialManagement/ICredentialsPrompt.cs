using System;
using System.Security;

namespace CredentialManagement
{
    public interface ICredentialsPrompt: IDisposable
    {
        DialogResult ShowDialog();
        DialogResult ShowDialog(IntPtr owner);

        string Username { get; set; }
        SecureString SecurePassword { get; set; }

        string Title { get; set; }
        string Message { get; set; }
        
        bool SaveChecked { get; set; }

        bool GenericCredentials { get; set; }
        bool ShowSaveCheckBox { get; set; }
        int ErrorCode { get; set; }

    }

    public static class ICredentialsPromptExtensions
    {
        public static Credential GetCredential(this ICredentialsPrompt prompt)
        {
            return new Credential(prompt.Username, prompt.SecurePassword, prompt.Title);
        }
    }
}
