﻿using Mixer.Base.Model.User;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.ViewModel
{
    [DataContract]
    public class UserViewModel : IEquatable<UserViewModel>
    {
        [DataMember]
        public uint ID { get; set; }

        [DataMember]
        public string UserName { get; set; }

        public UserViewModel() { }

        public UserViewModel(uint id, string username)
        {
            this.ID = id;
            this.UserName = username;
        }

        public UserViewModel(UserModel user) : this(user.id, user.username) { }

        public UserModel GetModel()
        {
            return new UserModel()
            {
                id = this.ID,
                username = this.UserName,
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is UserViewModel)
            {
                return this.Equals((UserViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserViewModel other) { return this.ID.Equals(other.ID); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return this.UserName; }
    }
}
