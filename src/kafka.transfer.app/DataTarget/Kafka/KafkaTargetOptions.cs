using System.Net;
using Confluent.Kafka;

namespace Kafka.Transfer.App.DataTarget.Kafka;

public class KafkaTargetOptions
{
    #region Client Config

    /// <summary>
    /// [bootstrap.servers] Initial list of brokers as a CSV list of broker host or host:port.
    /// The application may also use `rd_kafka_brokers_add()` to add brokers during runtime.
    ///
    /// default: ''
    /// importance: high
    /// </summary>
    public string BootstrapServers { get; set; } = String.Empty;

    /// <summary> 
    /// [sasl.username] SASL username for use with the PLAIN and SASL-SCRAM-.. mechanisms
    /// 
    /// default: ''
    /// importance: high
    /// </summary>
    public string SaslUsername { get; set; } = string.Empty;

    /// <summary>
    /// [sasl.password] SASL password for use with the PLAIN and SASL-SCRAM-.. mechanism
    /// 
    /// default: ''
    /// importance: high
    /// </summary>
    public string SaslPassword { get; set; } = string.Empty;

    #endregion

    public string Topic { get; set; }
    
    public bool UseSinglePartition { get; set; }

    public bool EnableIdempotence { get; set; } = true;

    internal ProducerConfig CreateProduceConfig()
    {
        return new ProducerConfig
        {
            //Client Config
            SaslUsername = SaslUsername,
            SaslPassword = SaslPassword,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            BootstrapServers = BootstrapServers,

            // Producer Config
            ClientId = Dns.GetHostName(),
            EnableIdempotence = EnableIdempotence,
            // https://thecloudblog.net/post/building-reliable-kafka-producers-and-consumers-in-net/
            EnableDeliveryReports = true,

            // Receive acknowledgement from all sync replicas
            Acks = Acks.All,
        };
    }
}