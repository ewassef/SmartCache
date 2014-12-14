using System;

namespace Cache
{
    public interface IPublisher
    {
        string PublishingServiceName { get; }
        void Notify(Type type, object key);
    }
}