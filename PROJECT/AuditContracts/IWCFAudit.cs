﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AuditContracts
{
    [ServiceContract]
    public interface IWCFAudit
    {
        [OperationContract]
        string ConnectS(string msg);
    }
}
