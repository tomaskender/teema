using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teema;
using Teema.Models;

namespace System.Web.Mvc {
    public class AuthorizeTeemaAccess : ActionFilterAttribute {

        public TeemaRoles RequiredRole { get; set; }

        public override void OnActionExecuting(ActionExecutingContext actionContext) {
            TeemaDBEntities entities = new TeemaDBEntities();
            if (actionContext.ActionParameters.Where(p=>p.Key=="teema" && p.Value != null).Count() ==0) {
                actionContext.Result = new ViewResult {
                    ViewName = "~/Views/Shared/Error.cshtml",
                    ViewData = new ViewDataDictionary() { Model = "Teema parameter is not defined!" }
                };
            } else {
                string teemaName = actionContext.ActionParameters["teema"] as string;
                if (entities.Teemas.Where(t => t.Name == teemaName).Count() > 0) {
                    Teema.Teema SelectedTeema = entities.Teemas.First(t => t.Name == teemaName);

                    if (!((RequiredRole == TeemaRoles.Viewer && SelectedTeema.AnyoneCanView) ||
                        (RequiredRole == TeemaRoles.Poster && SelectedTeema.AnyoneCanPost))) {

                        if (HttpContext.Current.User.Identity.IsAuthenticated) {
                            int UserId = entities.Users.First(u => u.Username == HttpContext.Current.User.Identity.Name).Id;
                            if (RequiredRole != TeemaRoles.Undefined) {
                                if (entities.TeemaAccesses.Where(ta => ta.TeemaId == SelectedTeema.Id && ta.UserId == UserId && ta.RoleId >= (int)RequiredRole).Count()==0) {
                                    actionContext.Result = new ViewResult {
                                        ViewName = "~/Views/Shared/Error.cshtml",
                                        ViewData = new ViewDataDictionary() { Model = "You don't have the required role in " + teemaName + " to do this!" }
                                    };
                                }
                            } else {
                                if (entities.TeemaAccesses.Where(ta => ta.TeemaId == SelectedTeema.Id && ta.UserId == UserId).Count() == 0) {
                                    actionContext.Result = new ViewResult {
                                        ViewName = "~/Views/Shared/Error.cshtml",
                                        ViewData = new ViewDataDictionary() { Model = "TeemaRole is set to undefined!" }
                                    };
                                }
                            }
                        } else {
                            actionContext.Result = new RedirectToRouteResult(new Web.Routing.RouteValueDictionary(new { controller = "Account", action = "Login", ReturnUrl = HttpContext.Current.Request.Url.AbsolutePath }));
                        }
                    }
                } else {
                    actionContext.Result = new ViewResult {
                        ViewName = "~/Views/Shared/Error.cshtml",
                        ViewData = new ViewDataDictionary() { Model = "There is no teema called " + teemaName + "!" }
                    };
                }
            }
            base.OnActionExecuting(actionContext);
        }
    }
}