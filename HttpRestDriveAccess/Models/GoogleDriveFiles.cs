using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpRestDriveAccess.Models
{
    public class GoogleDriveFiles
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long? Size { get; set; }
    }
}
