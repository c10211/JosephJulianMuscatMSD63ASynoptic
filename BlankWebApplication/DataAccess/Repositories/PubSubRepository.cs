using JosephJulianMuscatMSD63ASynoptic.DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;

namespace JosephJulianMuscatMSD63ASynoptic.DataAccess.Repositories
{
    public class PubSubRepository : IPubSubRepository
    {
        public void PublishMessage(string msg, string hasImage)
        {
            TopicName topicName = TopicName.FromProjectTopic("jjmsynoptic", "jjmsynoptictopic");

            Task<PublisherClient> t = PublisherClient.CreateAsync(topicName);
            t.Wait();
            PublisherClient publisher = t.Result;

            var pubsubMessage = new PubsubMessage
            {
                // The data is any arbitrary ByteString. Here, we're using text.
                Data = ByteString.CopyFromUtf8(msg),
                // The attributes provide metadata in a string-to-string dictionary.
                Attributes =
                {
                    { "hasImage", hasImage }
                }
            };
            Task<string> t2 = publisher.PublishAsync(pubsubMessage);
            t2.Wait();
            string message = t2.Result;

            Console.WriteLine($"Published message {message}");

            // Congratulations, your message has been published
        }

        public string PullMessage()
        {
            SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription("jjmsynoptic", "jjmsynoptictopic-sub");
            SubscriberServiceApiClient subscriberClient = SubscriberServiceApiClient.Create();
            string m = "";

            try
            {
                PullResponse response = subscriberClient.Pull(subscriptionName, returnImmediately: true, maxMessages: 1);

                if(response.ReceivedMessages.Count > 0)
                {
                    var msg = response.ReceivedMessages.FirstOrDefault();
                    if (msg != null)
                    {
                        m = msg.Message.Data.ToStringUtf8();
                    }
                    subscriberClient.Acknowledge(subscriptionName, new List<string>() { msg.AckId });
                }
            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.Unavailable)
            {
                Console.WriteLine("Too many pull requests\n" + ex.ToString());
                throw new Exception();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception();
            }

            return m;
        }
    }
}