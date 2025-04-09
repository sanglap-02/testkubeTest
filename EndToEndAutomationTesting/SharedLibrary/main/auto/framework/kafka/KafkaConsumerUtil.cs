using Confluent.Kafka;
using System.Text;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.auto.framework.kafka
{
    public class KafkaConsumerUtil
    {
        private readonly ILogging _logging;
        public KafkaConsumerUtil(ILogging logging) => _logging = logging;

        public List<Dictionary<string, string>> GetKafkaMessages(string bootstrapServers, string groupId,
            string topic, int noOfMessages)
        {
            _logging.LogInformation("KafkaConsumerUtil - GetKafkaMessages");

            var topicMessages = new List<Dictionary<string, string>>();
            var retryCount = 3;

            while (retryCount > 0)
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = bootstrapServers,
                    GroupId = groupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                using var consumer = new ConsumerBuilder<string, string>(config)
                    .SetKeyDeserializer(Deserializers.Utf8)
                    .SetValueDeserializer(Deserializers.Utf8)
                    .SetErrorHandler((_, e) => _logging.LogInformation($"Error: {e.Reason}"))
                    .Build();

                try
                {
                    var partition = new TopicPartition(topic, 0);
                    consumer.Assign(partition);

                    var endOffsets = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(60));
                    var actualEndOffset = endOffsets.High.Value;

                    _logging.LogInformation("****actualEndOffset**** :: " + actualEndOffset);

                    var offsetToReadFrom = actualEndOffset - noOfMessages;
                    if (offsetToReadFrom < 0) offsetToReadFrom = 0;

                    consumer.Seek(new TopicPartitionOffset(partition, offsetToReadFrom));

                    var numberOfMessagesReadSoFar = 0;
                    var keepOnReading = true;

                    while (keepOnReading)
                    {
                        var records = consumer.Consume(TimeSpan.FromMilliseconds(100));
                        if (records == null)
                        {
                            _logging.LogInformation("No record fetched.");
                            continue;
                        }

                        if (records.Message == null)
                        {
                            _logging.LogInformation("Message is null, skipping this record.");
                            continue;
                        }

                        var messageTemp = new Dictionary<string, string>
                        {
                            { "Value", records.Message.Value ?? "null" },
                            { "Key", records.Message.Key ?? "null" },
                            { "Partition", records.Partition.Value.ToString() },
                            { "Offset", records.Offset.Value.ToString() }
                        };

                        var typeOfMessageHeader = records.Message.Headers
                            ?.LastOrDefault(h => h.Key == "type-of-message")
                            ?.GetValueBytes();
                        if (typeOfMessageHeader != null)
                        {
                            string typeOfMessage = Encoding.UTF8.GetString(typeOfMessageHeader);
                            messageTemp.Add("type-of-message", typeOfMessage);
                        }

                        topicMessages.Add(messageTemp);

                        _logging.LogInformation($"Partition: {records.Partition}, Offset: {records.Offset}");

                        numberOfMessagesReadSoFar++;

                        if (numberOfMessagesReadSoFar >= noOfMessages)
                        {
                            keepOnReading = false;
                        }
                    }

                    _logging.LogInformation("Exiting the Consumer.");
                    if (topicMessages.Count == 0)
                    {
                        _logging.LogInformation($"No New Message in Kafka Topic {topic}");
                        return null;
                    }
                    else
                    {
                        _logging.LogInformation($"New Message Found in {topic}");
                        return topicMessages;
                    }
                }
                catch (KafkaException ex)
                {
                    _logging.LogInformation($"KafkaException occurred: {ex.Message} (Error Code: {ex.Error.Code})");

                    retryCount--;
                    if (retryCount > 0)
                    {
                        _logging.LogInformation("Retrying Kafka consumer operation...");
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        _logging.LogInformation("Maximum retries reached. Exiting.");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    _logging.LogError("An unexpected error occurred", ex);

                    retryCount--;
                    if (retryCount > 0)
                    {
                        _logging.LogInformation("Retrying Kafka consumer operation...");
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        _logging.LogError("Maximum retries reached. Exiting.", ex);
                        return null;
                    }
                }
            }

            return null;
        }

        public Dictionary<string, string> GetLastKafkaMessage(string bootstrapServers, string groupId,
            string topic)
        {
            _logging.LogInformation("KafkaConsumerUtil - GetLastKafkaMessage");

            var lastMessage = new Dictionary<string, string>();
            var retryCount = 3;

            while (retryCount > 0)
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = bootstrapServers,
                    GroupId = groupId,
                    AutoOffsetReset = AutoOffsetReset.Latest,
                    EnableAutoCommit = false,
                    Debug = "all"
                };

                try
                {
                    using var consumer = new ConsumerBuilder<string, string>(config)
                        .SetKeyDeserializer(Deserializers.Utf8)
                        .SetValueDeserializer(Deserializers.Utf8)
                        .SetErrorHandler((_, e)
                            => _logging.LogInformation(
                                $"Error: {e.Reason} (IsLocalError: {e.IsLocalError}, Code: {e.Code})"))
                        .Build();

                    var partition = new TopicPartition(topic, 0);
                    consumer.Assign(partition);

                    var endOffsets = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(60));
                    var actualEndOffset = endOffsets.High;

                    _logging.LogInformation("****actualEndOffset**** :: " + actualEndOffset);

                    if (actualEndOffset == endOffsets.Low)
                    {
                        _logging.LogInformation("No messages in the partition to consume.");
                        return null;
                    }

                    var offsetToReadFrom = actualEndOffset - 1;
                    if (offsetToReadFrom < endOffsets.Low)
                    {
                        _logging.LogInformation(
                            $"Offset {offsetToReadFrom} is lower than the earliest available offset {endOffsets.Low}. Adjusting to {endOffsets.Low}.");
                        offsetToReadFrom = endOffsets.Low;
                    }

                    consumer.Seek(new TopicPartitionOffset(partition, offsetToReadFrom));

                    var record = consumer.Consume(TimeSpan.FromSeconds(10));
                    if (record != null)
                    {
                        lastMessage["Value"] = record.Message.Value;
                        lastMessage["Partition"] = record.Partition.Value.ToString();
                        lastMessage["Offset"] = record.Offset.Value.ToString();

                        var typeOfMessageHeader = record.Message.Headers?.LastOrDefault(h => h.Key == "type-of-message")
                            ?.GetValueBytes();
                        if (typeOfMessageHeader != null)
                        {
                            var typeOfMessage = Encoding.UTF8.GetString(typeOfMessageHeader);
                            lastMessage["type-of-message"] = typeOfMessage;
                        }

                        _logging.LogInformation($"Partition: {record.Partition}, Offset: {record.Offset}");
                        _logging.LogInformation("New Message Found.");
                    }
                    else
                    {
                        _logging.LogInformation($"No New Message in Kafka Topic {topic}");
                    }

                    return lastMessage.Count > 0 ? lastMessage : null;
                }
                catch (KafkaException ex)
                {
                    if (ex.Error.Code == ErrorCode.Local_State)
                    {
                        _logging.LogInformation(
                            $"KafkaConsumer entered an erroneous state: {ex.Message} (Error Code: {ex.Error.Code})");
                        retryCount--;
                        Thread.Sleep(1000);

                        if (retryCount > 0)
                        {
                            _logging.LogInformation("Retrying the Kafka consumer initialization...");
                        }
                        else
                        {
                            _logging.LogInformation("Maximum retries reached. Shutting down the consumer.");
                            throw;
                        }
                    }
                    else
                    {
                        _logging.LogInformation($"KafkaException occurred: {ex.Message} (Error Code: {ex.Error.Code})");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logging.LogError("An unexpected error occurred", ex);
                    throw;
                }
            }

            return null;
        }

        public string GetKafkaMessageCount(string bootstrapServers, string groupId, string topic)
        {
            _logging.LogInformation("KafkaConsumerUtil - GetKafkaMessageCount");

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(config)
                .SetKeyDeserializer(Deserializers.Utf8)
                .SetValueDeserializer(Deserializers.Utf8)
                .SetErrorHandler((_, e) => _logging.LogInformation($"Error: {e.Reason}"))
                .Build();

            var messageCount = 0;

            try
            {
                var partition = new TopicPartition(topic, 0);
                consumer.Assign(partition);

                var endOffsets = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(60));
                var actualEndOffset = endOffsets.High.Value;

                _logging.LogInformation("****actualEndOffset**** :: " + actualEndOffset);

                var offsetToReadFrom = 0L;

                consumer.Seek(new TopicPartitionOffset(partition, offsetToReadFrom));

                var keepOnReading = true;

                while (keepOnReading)
                {
                    var record = consumer.Consume(TimeSpan.FromMilliseconds(100));
                    if (record == null)
                        continue;

                    messageCount++;

                    _logging.LogInformation($"Partition: {record.Partition}, Offset: {record.Offset}");

                    if (record.Offset == actualEndOffset - 1)
                    {
                        keepOnReading = false;
                    }
                }

                _logging.LogInformation("Exiting the Consumer.");

                _logging.LogInformation($"Total messages found in {topic}: {messageCount}");
                return messageCount.ToString();
            }
            catch (Exception ex)
            {
                _logging.LogError("An error occurred", ex);
                return "Error";
            }
        }
    }
}