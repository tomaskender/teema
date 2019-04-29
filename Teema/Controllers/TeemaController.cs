using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teema.Models;

namespace Teema.Controllers {
    public class TeemaController : Controller {
        TeemaDBEntities entities = new TeemaDBEntities();

        [AllowAnonymous]
        public ActionResult Index(bool? showSubscriptions, int? page) {
            if (showSubscriptions == null)
                showSubscriptions = false;
            return View(new ThreadListModel((bool)showSubscriptions, page != null ? (int)page : 1));
        }

        [AllowAnonymous]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Viewer)]
        public ActionResult Show(string teema, int? page) {
            TeemaShowModel model = new TeemaShowModel(entities.Teemas.First(t => t.Name == teema).Id, page != null ? (int)page : 1);
            return View("Show", model);

        }

        [Authorize]
        public ActionResult Create() {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(TeemaCreateModel createModel) {
            if (ModelState.IsValid) {
                Teema createdTeema = new Teema {
                    Name = createModel.Name,
                    AnyoneCanView = createModel.AnyoneCanView,
                    AnyoneCanPost = createModel.AnyoneCanPost
                };
                entities.Teemas.Add(createdTeema);
                entities.SaveChanges();

                int teemaId = entities.Teemas.First(t => t.Name == createdTeema.Name).Id;
                int userId = entities.Users.First(u => u.Username == User.Identity.Name).Id;

                entities.TeemaAccesses.Add(new TeemaAccess {
                    TeemaId = teemaId,
                    UserId = userId,
                    RoleId = (int)TeemaRoles.Owner,
                });
                entities.SaveChanges();

                return RedirectToAction("Show", new { teema = createModel.Name });
            } else return PartialView(createModel);
        }

        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Viewer)]
        public ActionResult ConfirmSubscription(string teema) {
            return PartialView(new ConfirmSubscription(teema));
        }

        [HttpPost]
        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Viewer)]
        [ValidateAntiForgeryToken]
        public ActionResult Subscribe(string teema) {
            if (!entities.Subscriptions.Any(s => s.User.Username == User.Identity.Name && s.Teema.Name == teema)) {
                entities.Subscriptions.Add(new Subscription() {
                    UserId = entities.Users.First(u => u.Username == User.Identity.Name).Id,
                    TeemaId = entities.Teemas.First(t => t.Name == teema).Id,
                    Date = DateTime.Now
                });
                entities.SaveChanges();
            }
            return new EmptyResult();
        }

        [HttpPost]
        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Viewer)]
        [ValidateAntiForgeryToken]
        public ActionResult Unsubscribe(string teema) {
            if (entities.Subscriptions.Any(s => s.User.Username == User.Identity.Name && s.Teema.Name == teema)) {
                entities.Subscriptions.Remove(entities.Subscriptions.First(s => s.User.Username == User.Identity.Name && s.Teema.Name == teema));
                entities.SaveChanges();
            }
            return new EmptyResult();
        }

        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Admin)]
        public ActionResult EditRoles(string teema) {
            ViewBag.teema = teema;
            int myRole = 0;
            if (entities.TeemaAccesses.Any(ta => ta.User.Username == User.Identity.Name))
                myRole = entities.TeemaAccesses.Single(ta => ta.Teema.Name == teema && ta.User.Username == User.Identity.Name).RoleId;
            int teemaId = entities.Teemas.Single(t => t.Name == teema).Id;
            List<AccountRoleModel> roleModel = new List<AccountRoleModel>();
            foreach (TeemaAccess access in entities.TeemaAccesses.Where(ta => ta.TeemaId == teemaId)) {
                roleModel.Add(new AccountRoleModel() {
                    Username = entities.Users.Find(access.UserId).Username,
                    Role = (TeemaRoles)access.RoleId,
                    HasLowerStatus = myRole >= access.RoleId
                });
            }
            return PartialView(roleModel.OrderBy(r => r.Username).OrderByDescending(r => r.Role).ToList());
        }

        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Admin)]
        [HttpPost]
        public ActionResult EditRoles(string teema, List<AccountRoleModel> model) {
            if (ModelState.IsValid) {
                TeemaDBEntities entities = new TeemaDBEntities();

                List<AccountRoleModel> originalModels = new List<AccountRoleModel>();
                int teemaId = entities.Teemas.First(t => t.Name == teema).Id;

                foreach (TeemaAccess ta in entities.TeemaAccesses.Where(ta => ta.TeemaId == teemaId)) {
                    originalModels.Add(new AccountRoleModel() { Username = ta.User.Username, Role = (TeemaRoles)ta.RoleId });
                }

                List<AccountRoleModel> toRemove = new List<AccountRoleModel>();
                foreach (AccountRoleModel roleModel in originalModels) {
                    if (!model.Any(m => m.Username == roleModel.Username && m.Role == roleModel.Role)) {
                        toRemove.Add(roleModel);
                    }
                }

                List<AccountRoleModel> toAdd = new List<AccountRoleModel>();
                foreach (AccountRoleModel roleModel in model) {
                    if (!originalModels.Any(o => o.Username == roleModel.Username && o.Role == roleModel.Role)) {
                        toAdd.Add(roleModel);
                    }
                }

                //deletes roles that have been removed
                if (toRemove.Count() > 0) {
                    foreach (AccountRoleModel role in toRemove) {
                        int userId = entities.Users.First(u => u.Username == role.Username).Id;
                        TeemaAccess access = entities.TeemaAccesses.First(ta => ta.TeemaId == teemaId && ta.UserId == userId);
                        entities.TeemaAccesses.Remove(access);
                    }
                    entities.SaveChanges();
                }

                //adds roles that have been added to the original model
                if (toAdd.Count() > 0) {
                    int myRoleId = entities.TeemaAccesses.Single(ta => ta.Teema.Name == teema && ta.User.Username == User.Identity.Name).RoleId;
                    foreach (AccountRoleModel role in toAdd) {
                        if (role.Role != TeemaRoles.Undefined) {
                            if (myRoleId < (int)role.Role) {
                                role.Role = (TeemaRoles)myRoleId;
                            }
                            int userId = entities.Users.First(u => u.Username == role.Username).Id;
                            entities.TeemaAccesses.Add(new TeemaAccess() {
                                UserId = userId,
                                TeemaId = teemaId,
                                RoleId = (int)role.Role,
                            });
                        }
                    }
                    entities.SaveChanges();
                }
            }
            return RedirectToAction("EditRoles", new { teema = teema });
        }

        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Admin)]
        public ActionResult AddRole(string teema) {
            ViewBag.teema = teema;
            return PartialView();
        }

        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRole(string teema, AccountRoleModel model) {
            if (ModelState.IsValid) {
                int myRoleId = entities.TeemaAccesses.Single(ta => ta.Teema.Name == teema && ta.User.Username == User.Identity.Name).RoleId;
                if (model.Role != TeemaRoles.Undefined) {
                    if (myRoleId < (int)model.Role) {
                        model.Role = (TeemaRoles)myRoleId;
                    }

                    if (!entities.TeemaAccesses.Any(ta => ta.Teema.Name == teema && ta.User.Username == model.Username)) {
                        TeemaAccess ta = new TeemaAccess() {
                            TeemaId = entities.Teemas.Single(t => t.Name == teema).Id,
                            UserId = entities.Users.Single(u => u.Username == model.Username).Id,
                            RoleId = (int)model.Role
                        };
                        entities.TeemaAccesses.Add(ta);
                        entities.SaveChanges();
                    }


                }

                model.Username = null;
                model.Role = TeemaRoles.Undefined;
            }
            return PartialView(model);
        }

        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Admin)]
        public ActionResult Settings(string teema) {
            ViewBag.teema = teema;
            return PartialView(new TeemaSettingsModel(entities.Teemas.First(t => t.Name == teema).Id));
        }

        [HttpPost]
        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Admin)]
        [ValidateAntiForgeryToken]
        public ActionResult Settings(string teema, TeemaSettingsModel model) {
            if (ModelState.IsValid) {
                Teema selectedTeema = entities.Teemas.First(t => t.Name == teema);
                selectedTeema.Description = model.Description;
                selectedTeema.AnyoneCanView = model.AnyoneCanView;
                selectedTeema.AnyoneCanPost = model.AnyoneCanPost;
                entities.SaveChanges();
            }
            return PartialView("Settings", model);
        }
    }
}