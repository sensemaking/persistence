using System;

namespace Sensemaking.Domain
{
    public struct User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public bool IsSystem { get; private set; }
        public bool IsHuman => !IsSystem;

        public User(Guid id, string name, bool isSystem = false)
        {
            Id = id;
            Name = name;
            IsSystem = isSystem;
        }

        #region Equality

        public static bool operator ==(User @this, User that)
        {
            return @this.Equals(that);
        }

        public static bool operator !=(User @this, User that)
        {
            return !(@this == that);
        }

        public bool Equals(User that)
        {
            return Id == that.Id;
        }

        public override bool Equals(object? that)
        {
            return that is User user && Equals(user);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = (int)2166136261;
                hash = (hash * 16777619) ^ Id.GetHashCode();
                return hash;
            }
        }

        #endregion
    }

    public static class UserExtensions
    {
        public static bool IsHumanOrCreatingSystemUser(this User user, User other)
        {
            return user.IsHuman || (user.IsSystem && user == other);
        }
    }
}