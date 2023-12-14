namespace Kafka.Transfer.App.DataSources.Kafka;

using Confluent.Kafka;

public class KafkaConsumerSourceOptions
{
    public long? Offset { get; set; }
    public string Topic { get; set; }

    /// <summary>
    /// The input speed of the kafka consumer has shown that TPL Dataflow library
    /// Doesn't properly perform in between execution of the transform block and or target block.
    /// Therefore a manual hard Pause has been implemented per batch of messages.
    /// This ensures optimal scheduling of the blocks and executes in true Async fashion.
    /// 
    /// default: '10'
    /// importance: high
    /// </summary>
    public int PauseTriggerMessageCount { get; set; } = 10;
    
    /// <summary>
    /// The interval time in ms for 'PauseTriggerMessageCount'
    /// 
    /// default: '100'
    /// importance: high
    /// </summary>
    public int PauseTriggerInterval { get; set; } = 100;

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

    #region Consumer Config

    /// <summary>
    /// [dotnet.consumer.consume.result.fields] A comma separated list of fields
    /// that may be optionally set in <see cref="T:Confluent.Kafka.ConsumeResult`2" />
    /// objects returned by the
    /// <see cref="M:Confluent.Kafka.Consumer`2.Consume(System.TimeSpan)" />
    /// method. Disabling fields that you do not require will improve
    /// throughput and reduce memory consumption. Allowed values:
    /// headers, timestamp, topic, all, none
    /// 
    /// default: all
    /// importance: low
    /// </summary>
    public string ConsumeResultFields { get; set; } = "all";

    /// <summary>
    /// [group.id] Client group id string.
    /// All clients sharing the same 'group.id' belong to the same group.
    /// 
    /// default: ''
    /// importance: high
    /// </summary>
    public string GroupId { get; set; } = string.Empty;

    /// <summary>
    /// [group.instance.id] Enable static group membership.
    /// Static group members are able to leave and rejoin a group within the configured `session.timeout.ms`
    /// without prompting a group rebalance. This should be used in combination with a larger `session.timeout.ms`
    /// to avoid group rebalances caused by transient unavailability (e.g. process restarts).
    /// Requires broker version &gt;= 2.3.0.
    /// 
    /// default: ''
    /// importance: medium
    /// </summary>
    public string GroupInstanceId { get; set; } = string.Empty;

    /// <summary>
    /// [partition.assignment.strategy] The name of one or more partition assignment strategies.
    /// The elected group leader will use a strategy supported by all members
    /// of the group to assign partitions to group members. If there is more
    /// than one eligible strategy, preference is determined by the order of
    /// this list (strategies earlier in the list have higher priority).
    /// Cooperative and non-cooperative (eager) strategies must not be mixed.
    /// Available strategies: range, roundrobin, cooperative-sticky.
    /// 
    /// default: range,roundrobin
    /// importance: medium
    /// </summary>
    public Confluent.Kafka.PartitionAssignmentStrategy PartitionAssignmentStrategy { get; set; } =
        Confluent.Kafka.PartitionAssignmentStrategy.RoundRobin;
    
    /// <summary>
    /// [session.timeout.ms] Client group session and failure detection timeout.
    /// The consumer sends periodic heartbeats (heartbeat.interval.ms)
    /// to indicate its liveness to the broker. If no hearts are received
    /// by the broker for a group member within the session timeout,
    /// the broker will remove the consumer from the group and trigger a rebalance.
    /// The allowed range is configured with the **broker** configuration
    /// properties `group.min.session.timeout.ms` and `group.max.session.timeout.ms`.
    /// Also see `max.poll.interval.ms`.
    /// 
    /// default: 45000
    /// importance: high
    /// </summary>
    public int SessionTimeoutMs { get; set; } = 45000;

    /// <summary>
    /// [heartbeat.interval.ms] Group session keepalive heartbeat interval.
    /// 
    /// default: 3000
    /// importance: low
    /// </summary>
    public int? HeartbeatIntervalMs { get; set; } = 3000;

    /// <summary>
    /// [group.protocol.type] Group protocol type. NOTE: Currently, the only supported group protocol type is `consumer`.
    /// 
    /// default: consumer
    /// importance: low
    /// </summary>
    public string GroupProtocolType { get; set; } = "consumer";

    /// <summary>
    /// [coordinator.query.interval.ms] How often to query for the current client group coordinator.
    /// If the currently assigned coordinator is down the configured query interval will be
    /// divided by ten to more quickly recover in case of coordinator reassignment.
    /// 
    /// default: 600000
    /// importance: low
    /// </summary>
    public int? CoordinatorQueryIntervalMs { get; set; } = 600000;

    /// <summary>
    /// [max.poll.interval.ms] Maximum allowed time between calls to consume messages
    /// (e.g., rd_kafka_consumer_poll()) for high-level consumers. If this interval is
    /// exceeded the consumer is considered failed and the group will rebalance in order to
    /// reassign the partitions to another consumer group member. Warning: Offset commits may
    /// be not possible at this point. Note: It is recommended to set `enable.auto.offset.store=false`
    /// for long-time processing applications and then explicitly store offsets (using offsets_store())
    /// *after* message processing, to make sure offsets are not auto-committed prior to
    /// processing has finished. The interval is checked two times per second.
    /// See KIP-62 for more information.
    /// 
    /// default: 300000
    /// importance: high
    /// </summary>
    public int MaxPollIntervalMs { get; set; } = 300000;

    /// <summary>
    /// [auto.offset.reset] Action to take when there is no initial offset in offset
    /// store or the desired offset is out of range: 'smallest','earliest' -
    /// automatically reset the offset to the smallest offset, 'largest','latest' -
    /// automatically reset the offset to the largest offset, 'error' -
    /// trigger an error (ERR__AUTO_OFFSET_RESET) which is retrieved
    /// by consuming messages and checking 'message-&gt;err'.
    /// 
    /// default: largest
    /// importance: high
    /// </summary>
    public Confluent.Kafka.AutoOffsetReset? AutoOffsetReset { get; set; } = Confluent.Kafka.AutoOffsetReset.Latest;

    /// <summary>
    /// [enable.auto.commit] Automatically and periodically commit offsets in the background.
    /// Note: setting this to false does not prevent the consumer from fetching previously committed start offsets.
    /// To circumvent this behaviour set specific start offsets per partition in the call to assign().
    /// 
    /// default: true
    /// importance: high
    /// </summary>
    public bool EnableAutoCommit { get; set; } = true;

    /// <summary>
    /// [auto.commit.interval.ms] The frequency in milliseconds that the consumer offsets
    /// are committed (written) to offset storage. (0 = disable). This setting is used
    /// by the high-level consumer.
    /// 
    /// default: 5000
    /// importance: medium
    /// </summary>
    public int AutoCommitIntervalMs { get; set; } = 5000;

    /// <summary>
    /// [enable.auto.offset.store] Automatically store offset of last message provided to application.
    /// The offset store is an in-memory store of the next offset to (auto-)commit for each partition.
    /// 
    /// default: true
    /// importance: high
    /// </summary>
    public bool EnableAutoOffsetStore { get; set; } = true;

    /// <summary>
    /// [queued.min.messages] Minimum number of messages per topic+partition librdkafka
    /// tries to maintain in the local consumer queue.
    /// 
    /// default: 100000
    /// importance: medium
    /// </summary>
    public int QueuedMinMessages { get; set; } = 100000;

    /// <summary>
    /// [queued.max.messages.kbytes] Maximum number of kilobytes of queued pre-fetched messages
    /// in the local consumer queue. If using the high-level consumer this setting applies
    /// to the single consumer queue, regardless of the number of partitions.
    /// When using the legacy simple consumer or when separate partition queues are used
    /// this setting applies per partition. This value may be overshot by fetch.message.max.bytes.
    /// This property has higher priority than queued.min.messages.
    /// 
    /// default: 65536
    /// importance: medium
    /// </summary>
    public int QueuedMaxMessagesKbytes { get; set; } = 65536;

    /// <summary>
    /// [fetch.wait.max.ms] Maximum time the broker may wait to fill the Fetch response with fetch.min.bytes of messages.
    /// 
    /// default: 500
    /// importance: low
    /// </summary>
    public int FetchWaitMaxMs { get; set; } = 500;

    /// <summary>
    /// [fetch.queue.backoff.ms] How long to postpone the next fetch request for a topic+partition
    /// in case the current fetch queue thresholds (queued.min.messages or queued.max.messages.kbytes)
    /// have been exceded. This property may need to be decreased if the queue thresholds are set low and
    /// the application is experiencing long (~1s) delays between messages.
    /// Low values may increase CPU utilization.
    /// 
    /// default: 1000
    /// importance: medium
    /// </summary>
    public int FetchQueueBackoffMs { get; set; } = 1000;

    /// <summary>
    /// [max.partition.fetch.bytes] Initial maximum number of bytes per topic+partition to
    /// request when fetching messages from the broker. If the client encounters a message
    /// larger than this value it will gradually try to increase it
    /// until the entire message can be fetched.
    /// 
    /// default: 1048576
    /// importance: medium
    /// </summary>
    public int MaxPartitionFetchBytes { get; set; } = 1048576;

    /// <summary>
    /// [fetch.max.bytes] Maximum amount of data the broker shall return for a Fetch request.
    /// Messages are fetched in batches by the consumer and if the first message batch in
    /// the first non-empty partition of the Fetch request is larger than this value,
    /// then the message batch will still be returned to ensure the consumer can make progress.
    /// The maximum message batch size accepted by the broker is defined via `message.max.bytes`
    /// (broker config) or `max.message.bytes` (broker topic config). `fetch.max.bytes` is automatically
    /// adjusted upwards to be at least `message.max.bytes` (consumer config).
    /// 
    /// default: 52428800
    /// importance: medium
    /// </summary>
    public int FetchMaxBytes { get; set; } = 52428800;

    /// <summary>
    /// [fetch.min.bytes] Minimum number of bytes the broker responds with. If fetch.wait.max.ms expires
    /// the accumulated data will be sent to the client regardless of this setting.
    /// 
    /// default: 1
    /// importance: low
    /// </summary>
    public int FetchMinBytes { get; set; } = 1;

    /// <summary>
    /// [fetch.error.backoff.ms] How long to postpone the next fetch request for a topic+partition in case of a fetch error.
    /// 
    /// default: 500
    /// importance: medium
    /// </summary>
    public int FetchErrorBackoffMs { get; set; } = 500;

    /// <summary>
    /// [isolation.level] Controls how to read messages written transactionally: `read_committed` -
    /// only return transactional messages which have been committed. `read_uncommitted` -
    /// return all messages, even transactional messages which have been aborted.
    /// 
    /// default: read_committed
    /// importance: high
    /// </summary>
    public Confluent.Kafka.IsolationLevel? IsolationLevel { get; set; } = Confluent.Kafka.IsolationLevel.ReadCommitted;

    /// <summary>
    /// [enable.partition.eof] Emit RD_KAFKA_RESP_ERR__PARTITION_EOF event
    /// whenever the consumer reaches the end of a partition.
    /// 
    /// default: false
    /// importance: low
    /// </summary>
    public bool EnablePartitionEof { get; set; } = false;

    /// <summary>
    /// [check.crcs] Verify CRC32 of consumed messages, ensuring no on-the-wire or on-disk corruption
    /// to the messages occurred. This check comes at slightly increased CPU usage.
    /// 
    /// default: false
    /// importance: medium
    /// </summary>
    public bool CheckCrcs { get; set; } = false;
    
    #endregion
    
    internal ConsumerConfig CreateConsumerConfig()
    {
        return new ConsumerConfig
        {
            //Client Config
            SaslUsername = SaslUsername,
            SaslPassword = SaslPassword,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            AutoOffsetReset = AutoOffsetReset,
            BootstrapServers = BootstrapServers,

            // Consumer Config
            ConsumeResultFields = ConsumeResultFields,
            GroupId = GroupId,
            GroupInstanceId = GroupInstanceId, //Not tested
            
            PartitionAssignmentStrategy = PartitionAssignmentStrategy,
            SessionTimeoutMs = SessionTimeoutMs,
            HeartbeatIntervalMs = HeartbeatIntervalMs,
            GroupProtocolType = GroupProtocolType,
            CoordinatorQueryIntervalMs = CoordinatorQueryIntervalMs,
            EnableAutoCommit = EnableAutoCommit, // tested with false,
            MaxPollIntervalMs = MaxPollIntervalMs,

            //Not tested
            AutoCommitIntervalMs = AutoCommitIntervalMs,
            EnableAutoOffsetStore = EnableAutoOffsetStore,
            QueuedMinMessages = QueuedMinMessages,
            QueuedMaxMessagesKbytes = QueuedMaxMessagesKbytes,
            FetchWaitMaxMs = FetchWaitMaxMs,
            FetchQueueBackoffMs = FetchQueueBackoffMs,
            MaxPartitionFetchBytes = MaxPartitionFetchBytes,
            FetchMaxBytes = FetchMaxBytes,
            FetchMinBytes = FetchMinBytes,
            FetchErrorBackoffMs = FetchErrorBackoffMs,
            IsolationLevel = IsolationLevel,
            EnablePartitionEof = EnablePartitionEof,
            CheckCrcs = CheckCrcs
        };
    }
}