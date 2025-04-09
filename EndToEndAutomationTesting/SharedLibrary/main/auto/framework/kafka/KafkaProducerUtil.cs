using Confluent.Kafka;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.auto.framework.kafka
{
    public class KafkaProducerUtil
    {
        private readonly ILogging _logging;
        public KafkaProducerUtil(ILogging logging) => _logging = logging;
        
        public  bool PushKafkaMessage(string bootstrapServers, string topic, string message)
        {
            _logging.LogInformation("KafkaProducerUtil - PushKafkaMessage");

            try
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = bootstrapServers,
                    Acks = Acks.All
                };

                using (var producer = new ProducerBuilder<Null, string>(config).Build())
                { 
                    try
                    {
                        var deliveryReport = producer.ProduceAsync(topic, new Message<Null, string>
                        {
                            Value = message
                        }).Result;

                        _logging.LogInformation($"Message '{deliveryReport.Value}' sent to '{deliveryReport.TopicPartitionOffset}'");
                        return true;
                    }
                    catch (ProduceException<Null, string> ex)
                    {
                        _logging.LogError($"Failed to deliver message: {ex.Message} [{ex.Error.Code}]", ex);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error to data in to Kafka.\n" + e);
                return false;
            }
        }

        public async Task<int> CountKafkaMessage(string bootstrapServers, string topic)
        {
            _logging.LogInformation("KafkaProducerUtil - CountKafkaMessage");

            try
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = bootstrapServers,
                    GroupId = "kafka-message-counter",
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                using var consumer = new ConsumerBuilder<Ignore, Ignore>(config).Build();
                consumer.Subscribe(topic);

                // Give the consumer some time to get the assignment
                await Task.Delay(5000);

                var partitions = consumer.Assignment;
                if (!partitions.Any())
                {
                    _logging.LogInformation($"Topic {topic} not found or no partitions assigned.");
                    return -1;
                }

                long messageCount = 0;
                foreach (var partition in partitions)
                {
                    var watermarkOffsets = consumer.QueryWatermarkOffsets(partition, TimeSpan.FromSeconds(30));
                    messageCount += watermarkOffsets.High.Value - watermarkOffsets.Low.Value;
                }

                consumer.Unsubscribe();

                _logging.LogInformation($"The total number of messages in topic '{topic}' is {messageCount}.");

                return (int)messageCount;
            }
            catch (KafkaException ex)
            {
                _logging.LogError($"Failed to count messages: {ex.Message} [{ex.Error.Code}]", ex);
                return -1;
            }
            catch (Exception ex)
            {
                _logging.LogError("Failed to count messages", ex);
                return -1;
            }
        }
    }
}
