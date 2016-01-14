using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WordCount.WebService.Models
{
    [DataContract]
    public class AddWordResponse
    {
        [DataMember]
        public Guid PartitionId { get; set; }
        [DataMember]
        public string ServiceAddress { get; set; }

        [DataMember]
        public string Word { get; set; }
    }
}
