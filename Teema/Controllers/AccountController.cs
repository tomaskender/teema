using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Teema.Models;

namespace Teema.Controllers {
    public class AccountController : Controller {

        TeemaDBEntities entities = new TeemaDBEntities();
        private const string characters = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // GET: User
        [AllowAnonymous]
        public ActionResult ShowUser(string username) {
            if (username == null) {
                if (User.Identity.IsAuthenticated)
                    return RedirectToAction("ShowUser", new { username = User.Identity.Name });
                else return RedirectToAction("Index", "Home");
            }

            bool userExists = entities.Users.Where(u => u.Username == username).Count() > 0;
            if (userExists)
                return View("ShowUser", new AccountShowModel(entities.Users.Where(u => u.Username == username).First().Id));
            else return View("Error");
        }

        [AllowAnonymous]
        public ActionResult Login(string ReturnUrl) {
            ViewBag.ReturnUrl = ReturnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Models.AccountLoginModel logModel, string ReturnUrl) {
            if (ModelState.IsValid) {
                List<User> matches = entities.Users.Where(u => u.Username == logModel.Username && u.Password == logModel.Password).ToList();
                if (matches.Count() > 0 && matches.FirstOrDefault().IsVerified) {
                    FormsAuthentication.SetAuthCookie(logModel.Username, false);

                    //if ReturnUrl is specified, the user is redirected back to the original page, otherwise he's redirected to his profile
                    if (ReturnUrl != null)
                        return Redirect(ReturnUrl);
                    else if (logModel.ReturnUrl != null)
                        return Redirect(logModel.ReturnUrl);
                    else return RedirectToAction("ShowUser");
                } else {
                    if (matches.Count() == 0)
                        ModelState.AddModelError("Username", "There is no account with such password");
                    else if (!matches.First().IsVerified)
                        ModelState.AddModelError("Username", "This account has not been verified yet.");

                    return View(logModel);
                }
            } else return View(logModel);
        }

        [AllowAnonymous]
        public ActionResult Register() {
            return View(new AccountRegisterModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Models.AccountRegisterModel regModel) {
            if (ModelState.IsValid && entities.Users.Where(u => (u.Username == regModel.Username) || (u.Email == regModel.Email)).Count() == 0) {
                User user = new User() {
                    Username = regModel.Username,
                    Email = regModel.Email,
                    Password = regModel.Password,
                    CountryId = entities.Countries.First(c => c.Name == regModel.Country).Id,
                    Registered = DateTime.Now,
                    //IsVerified ma zostat false az do potvrdenia cez email, ale z hostingu neviem poslat mail
                    IsVerified = true
                };
                entities.Users.Add(user);
                entities.SaveChanges();
                Random random = new Random();
                string code;
                do {
                    code = "";
                    for (int i = 0; i < 10; i++) {
                        code += characters[random.Next(0, characters.Length)];
                    }
                } while (entities.Verifications.Any(v => v.Code == code));
                Verification verification = new Verification() { UserId = user.Id, Code = code };
                entities.Verifications.Add(verification);
                entities.SaveChanges();

                //email verification
                /*MailMessage message = new MailMessage {
                    Subject = "Welcome to Teema.Online",
                    Body = String.Format("Dear {0}, welcome to Teema! Before we get started, please verify your account by clicking on <a href='{1}'>this link</a>.", user.Username, verification.Code),
                    IsBodyHtml = true
                };
                message.To.Add(user.Email);
                SmtpClient smtp = new SmtpClient();
                smtp.Send(message);*/

                return RedirectToAction("Index", "Teema");
            } else {
                if (entities.Users.Any(u => u.Username == regModel.Username)) {
                    ModelState.AddModelError("Username", "This account already exists.");
                } else if (entities.Users.Any(u => u.Email == regModel.Email)) {
                    ModelState.AddModelError("Email", "This email is already registered.");
                }
                return View(regModel);
            }
        }

        [AllowAnonymous]
        public ActionResult VerifyAccount(string id) {
            if (id != null) {
                Verification verification = entities.Verifications.Where(v => v.Code == id).FirstOrDefault();
                if (verification.UserId != null) {
                    User user = entities.Users.First(u => u.Id == verification.UserId);
                    user.IsVerified = true;
                    entities.Verifications.Remove(verification);
                    entities.SaveChanges();
                    return View((object)user.Username);
                }
            }
            return RedirectToAction("Index", "Teema");
        }

        [AllowAnonymous]
        public ActionResult Logout(string ReturnUrl) {
            FormsAuthentication.SignOut();
            if (ReturnUrl != null)
                return Redirect(ReturnUrl);
            else return RedirectToAction("Index", "Teema");
        }

        [AllowAnonymous]
        public ActionResult ShowProfile(string username) {
            int userId = entities.Users.First(u => u.Username == username).Id;
            return PartialView(new AccountProfileModel(userId));
        }

        [AllowAnonymous]
        public ActionResult ShowSubscriptions(string username) {
            int userId = entities.Users.First(u => u.Username == username).Id;
            bool isFriendsWith = new AccountShowModel(userId).IsFriendsWith;

            if (!entities.Users.First(u => u.Id == userId).HasPrivateProfile || isFriendsWith || username == User.Identity.Name)
                return PartialView(new SubscriptionListModel(userId));
            else return PartialView(new SubscriptionListModel());
        }

        [AllowAnonymous]
        public ActionResult ShowFollows(string username) {
            int userId = entities.Users.First(u => u.Username == username).Id;
            bool isFriendsWith = new AccountShowModel(userId).IsFriendsWith;

            if (!entities.Users.First(u => u.Id == userId).HasPrivateProfile || isFriendsWith || username == User.Identity.Name)
                return PartialView(new FollowListModel(userId));
            else return PartialView(new FollowListModel());
        }

        [Authorize]
        public ActionResult ShowOptions() {
            return PartialView(new AccountOptionsModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveOptions(Models.AccountOptionsModel model, HttpPostedFileBase avatar) {
            if (ModelState.IsValid) {
                User user = entities.Users.First(u => u.Username == HttpContext.User.Identity.Name);
                if (avatar != null) {
                    if (avatar.ContentLength > 3072) {
                        ModelState.AddModelError("avatar", "Avatar file mustn't be larger than 3kB.");
                    } else {
                        System.IO.MemoryStream target = new System.IO.MemoryStream();
                        avatar.InputStream.CopyTo(target);
                        byte[] avatarBytes = target.ToArray();
                        user.Avatar = avatarBytes;
                    }
                }
                user.Description = model.Description;
                user.HasPrivateProfile = model.HasPrivateProfile;
                if (model.NewPassword != null) {
                    user.Password = model.NewPassword;
                }
                entities.SaveChanges();
            }
            return PartialView("ShowOptions", model);
        }

        [AllowAnonymous]
        public ActionResult ConfirmUserFollow(string username) {
            return PartialView(new ConfirmUserFollow(username));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Follow(string target) {
            int FollowerId = entities.Users.Where(u => u.Username == User.Identity.Name).First().Id;
            int FollowedId = entities.Users.Where(u => u.Username == target).First().Id;

            if (entities.Follows.Where(f => f.FollowerId == FollowerId && f.FollowedId == FollowedId).Count() == 0) {
                //user is not following the target user yet -> adding the follow record
                entities.Follows.Add(new Follow() { FollowerId = FollowerId, FollowedId = FollowedId, Date = DateTime.Now });
                entities.SaveChanges();
                return RedirectToAction("ShowUser", new { username = target });
            } else return View("Error");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Unfollow(string target) {
            int FollowerId = entities.Users.First(u => u.Username == User.Identity.Name).Id;
            int FollowedId = entities.Users.First(u => u.Username == target).Id;

            if (entities.Follows.Where(f => f.FollowerId == FollowerId && f.FollowedId == FollowedId).Count() > 0) {

                //user is already following the target user -> removing the follow record
                Follow follow = entities.Follows.First(f => f.FollowerId == FollowerId && f.FollowedId == FollowedId);
                entities.Follows.Remove(follow);
                entities.SaveChanges();
                return RedirectToAction("ShowUser", new { username = target });
            } else return View("Error");
        }

        [AllowAnonymous]
        public ActionResult GetAvatar(string username) {
            byte[] pic = entities.Users.Where(u => u.Username == username).Select(u => u.Avatar).FirstOrDefault();
            if (pic != null)
                return File(pic, "image/png");
            else {
                string dir = Server.MapPath("/Content/Images");
                string path = System.IO.Path.Combine(dir, "default_avatar.png");
                return File(path, "image/png");
            }
        }
    }
}