using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WordCount.WebService.Models
{
    [DataContract]
    public class CountResponse
    {
        [DataMember]
        public long Total { get; set; }

        [DataMember]
        public List<Info> Infos { get; set; }

        public CountResponse()
        {
            Infos = new List<Models.Info>();
        }
    }

    [DataContract]
    public class Info
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public long Hits { get; set; }

        [DataMember]
        public long LowKey { get; set; }

        [DataMember]
        public long HighKey { get; set; }

    }
}
