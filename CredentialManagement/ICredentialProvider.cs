// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace CredentialManagement
{
    /// <summary>
    /// Supplies a credential for a certain purpose
    /// </summary>
    interface ICredentialProvider
    {
        NetworkCredential GetCredential();
        
        /// <summary>
        /// Clears eventually cached credential information
        /// </summary>
        void Reset();
    }
}
