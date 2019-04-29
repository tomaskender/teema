using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Teema.Controllers {
    public class NotificationController : Controller {
        // GET: Notification
        [Authorize]
        public ActionResult ReadNotification(int notificationId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            NotificationGroup notification = entities.NotificationGroups.Find(notificationId);
            if (notification.User.Username == User.Identity.Name) {
                notification.Seen = true;
                entities.SaveChangesAsync();
            }
            return new EmptyResult();
        }
    }
}