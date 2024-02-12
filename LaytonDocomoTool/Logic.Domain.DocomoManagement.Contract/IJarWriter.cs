using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic.Business.LaytonDocomoTool.Contract.DataClasses;

namespace Logic.Domain.DocomoManagement.Contract
{
    public interface IJarWriter
    {
        void Write(JarArchiveEntry[] entries, Stream output);
    }
}
