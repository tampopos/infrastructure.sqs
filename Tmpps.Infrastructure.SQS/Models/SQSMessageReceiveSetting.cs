using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Tmpps.Infrastructure.SQS.Models
{
    public class SQSMessageReceiveSetting
    {
        public SQSMessageReceiveSetting(IConfiguration configuration)
        {
            this.QueueUrl = configuration.GetValue<string>(nameof(this.QueueUrl));
            this.InstanceCount = configuration.GetValue(nameof(this.InstanceCount), 1);
            this.MappingTypes = configuration
                .GetSection(nameof(this.MappingTypes))
                .GetChildren()
                .ToDictionary(
                    x => x.GetValue<string>("Name"),
                    x => x.GetValue<string>("MessageContentType")
                );
        }
        public string QueueUrl { get; }
        public IDictionary<string, string> MappingTypes { get; }
        public int InstanceCount { get; }
    }
}