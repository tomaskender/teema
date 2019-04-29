using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teema.Models {
    public class SubscriptionListModel {
        public SubscriptionListModel() { Subscriptions = new List<string>(); }
        public SubscriptionListModel(int userId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            Subscriptions = new List<string>();
            foreach(Subscription subscription in entities.Subscriptions.Where(s => s.UserId == userId)) {
                Subscriptions.Add(entities.Teemas.First(t=>t.Id==subscription.TeemaId).Name);
            }
        }
        public List<string> Subscriptions { get; }
    }

    public class ConfirmSubscription {
        public ConfirmSubscription(string teema) {
            Teema = teema;
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                TeemaDBEntities entities = new TeemaDBEntities();
                IsSubscribed = entities.Subscriptions.Any(s => s.User.Username == HttpContext.Current.User.Identity.Name && s.Teema.Name == teema);
            }
        }
        public string Teema { get; }
        public bool IsSubscribed { get; }
    }
}