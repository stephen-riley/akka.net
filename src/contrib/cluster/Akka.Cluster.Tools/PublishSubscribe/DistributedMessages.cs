﻿//-----------------------------------------------------------------------
// <copyright file="DistributedMessages.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Akka.Actor;
using Akka.Event;

namespace Akka.Cluster.Tools.PublishSubscribe
{
    [Serializable]
    public sealed class Put : IEquatable<Put>
    {
        public Put(IActorRef @ref)
        {
            Ref = @ref;
        }

        public IActorRef Ref { get; }

        public bool Equals(Put other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;
            return Equals(Ref, other.Ref);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Put);
        }

        public override int GetHashCode()
        {
            return (Ref != null ? Ref.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Put<ref:{0}>", Ref);
        }
    }

    [Serializable]
    public sealed class Remove : IEquatable<Remove>
    {
        public Remove(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public bool Equals(Remove other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;
            return Equals(Path, other.Path);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Remove);
        }

        public override int GetHashCode()
        {
            return (Path != null ? Path.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("Remove<path:{0}>", Path);
        }
    }

    [Serializable]
    public sealed class Subscribe : IEquatable<Subscribe>
    {
        public Subscribe(string topic, IActorRef @ref) : this(topic, null, @ref)
        {
        }

        public Subscribe(string topic, string group, IActorRef @ref)
        {
            if (string.IsNullOrEmpty(topic))
                throw new ArgumentException("topic must be defined");

            Topic = topic;
            Group = group;
            Ref = @ref;
        }

        public string Topic { get; }

        public string Group { get; }

        public IActorRef Ref { get; }

        public bool Equals(Subscribe other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;
            return Equals(Topic, other.Topic) &&
                   Equals(Group, other.Group) &&
                   Equals(Ref, other.Ref);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Subscribe);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Topic != null ? Topic.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Group != null ? Group.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Ref != null ? Ref.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Subscribe<topic:{0}, group:{1}, ref:{2}>", Topic, Group, Ref);
        }
    }

    [Serializable]
    public sealed class Unsubscribe : IEquatable<Unsubscribe>
    {
        public Unsubscribe(string topic, IActorRef @ref) : this(topic, null, @ref)
        {
        }

        public Unsubscribe(string topic, string @group, IActorRef @ref)
        {
            if (string.IsNullOrEmpty(topic)) throw new ArgumentException("topic must be defined");

            Topic = topic;
            Group = @group;
            Ref = @ref;
        }

        public string Topic { get; }

        public string Group { get; }

        public IActorRef Ref { get; }

        public bool Equals(Unsubscribe other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;
            return Equals(Topic, other.Topic) &&
                   Equals(Group, other.Group) &&
                   Equals(Ref, other.Ref);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Unsubscribe);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Topic != null ? Topic.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Group != null ? Group.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Ref != null ? Ref.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Unsubscribe<topic:{0}, group:{1}, ref:{2}>", Topic, Group, Ref);
        }
    }

    [Serializable]
    public sealed class SubscribeAck : IEquatable<SubscribeAck>, IDeadLetterSuppression
    {
        public SubscribeAck(Subscribe subscribe)
        {
            Subscribe = subscribe;
        }

        public Subscribe Subscribe { get; }

        public bool Equals(SubscribeAck other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;
            return Equals(Subscribe, other.Subscribe);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SubscribeAck);
        }

        public override int GetHashCode()
        {
            return (Subscribe != null ? Subscribe.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("SubscribeAck<{0}>", Subscribe);
        }
    }

    [Serializable]
    public sealed class UnsubscribeAck : IEquatable<UnsubscribeAck>
    {
        public UnsubscribeAck(Unsubscribe unsubscribe)
        {
            Unsubscribe = unsubscribe;
        }

        public Unsubscribe Unsubscribe { get; }

        public bool Equals(UnsubscribeAck other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;
            return Equals(Unsubscribe, other.Unsubscribe);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UnsubscribeAck);
        }

        public override int GetHashCode()
        {
            return (Unsubscribe != null ? Unsubscribe.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("UnsubscribeAck<{0}>", Unsubscribe);
        }
    }

    [Serializable]
    public sealed class Publish : IDistributedPubSubMessage, IEquatable<Publish>
    {
        public Publish(string topic, object message, bool sendOneMessageToEachGroup = false)
        {
            Topic = topic;
            Message = message;
            SendOneMessageToEachGroup = sendOneMessageToEachGroup;
        }

        public string Topic { get; }

        public object Message { get; }

        public bool SendOneMessageToEachGroup { get; }

        public bool Equals(Publish other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;
            return Equals(Topic, other.Topic) &&
                   Equals(SendOneMessageToEachGroup, other.SendOneMessageToEachGroup) &&
                   Equals(Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Publish);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Topic != null ? Topic.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SendOneMessageToEachGroup.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Publish<topic:{0}, sendOneToEachGroup:{1}, message:{2}>", Topic, SendOneMessageToEachGroup, Message);
        }
    }

    [Serializable]
    public sealed class Send : IDistributedPubSubMessage, IEquatable<Send>
    {
        public Send(string path, object message, bool localAffinity = false)
        {
            Path = path;
            Message = message;
            LocalAffinity = localAffinity;
        }

        public string Path { get; }

        public object Message { get; }

        public bool LocalAffinity { get; }

        public bool Equals(Send other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;
            return Equals(Path, other.Path) &&
                   Equals(LocalAffinity, other.LocalAffinity) &&
                   Equals(Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Send);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ LocalAffinity.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Send<path:{0}, localAffinity:{1}, message:{2}>", Path, LocalAffinity, Message);
        }
    }

    [Serializable]
    public sealed class SendToAll : IDistributedPubSubMessage, IEquatable<SendToAll>
    {
        public SendToAll(string path, object message, bool allButSelf = false)
        {
            Path = path;
            Message = message;
            AllButSelf = allButSelf;
        }

        public string Path { get; }

        public object Message { get; }

        public bool AllButSelf { get; }

        public bool Equals(SendToAll other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;
            return Equals(AllButSelf, other.AllButSelf) &&
                   Equals(Path, other.Path) &&
                   Equals(Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SendToAll);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ AllButSelf.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("SendToAll<path:{0}, allButSelf:{1}, message:{2}>", Path, AllButSelf, Message);
        }
    }

    [Serializable]
    public sealed class GetTopics : IEquatable<GetTopics>
    {
        public static readonly GetTopics Instance = new GetTopics();
        private GetTopics() { }

        public bool Equals(GetTopics other)
        {
            if (other == null) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GetTopics);
        }

        public override string ToString()
        {
            return "GetTopics<>";
        }
    }

    [Serializable]
    public sealed class CurrentTopics : IEquatable<CurrentTopics>
    {
        public CurrentTopics(string[] topics)
        {
            Topics = topics ?? new string[0];
        }

        public string[] Topics { get; }

        public bool Equals(CurrentTopics other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;

            return Equals(Topics, other.Topics);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CurrentTopics);
        }

        public override int GetHashCode()
        {
            return (Topics != null ? Topics.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("CurrentTopics<{0}>", string.Join(",", Topics));
        }
    }
}